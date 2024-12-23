using HuskyKit.Extensions;
using HuskyKit.Sql;
using HuskyKit.Sql.Columns;
using System.Collections;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Linq.Expressions;
using System.Net.Http.Headers;
using System.Text;

/// <summary>
/// Core class for building SQL queries dynamically.
/// </summary>
namespace HuskyKit.Sql.Sources
{
    public partial class SqlBuilder : ISqlSource
    {
        public virtual ISqlColumn this[string name]
        {
            get
            {
                var a = Columns.FirstOrDefault(x => x.Column.Name.Equals(name)).Column;
                if (a != null) return a;

                if (From is SqlBuilder b)
                    return b.Columns.First(x => x.Column.Name.Equals(name)).Column;

                throw new InvalidOperationException();
            }

        }

        public IDictionary<string, object?> LocalParameters { get; set; } = new Dictionary<string, object?>();

        public IReadOnlyDictionary<string, object?> Parameters =>
            new ReadOnlyDictionary<string, object?>(
                GetNestedBuilders(true)
                .Distinct()
                .SelectMany(x => x.LocalParameters)
                .ToDictionary(x => x.Key, x => x.Value)
            );

        protected IEnumerable<SqlBuilder> GetNestedBuilders(bool includeQueryColumns)
        {
            foreach (var table in LocalWithTables)
            {
                foreach (var subtable in table.GetNestedBuilders(includeQueryColumns))
                    if (subtable.From != null)
                        yield return subtable;

                if (table.From != null)
                    yield return table;
            }

            GetNestedSubqueries(includeQueryColumns);
        }

        private IEnumerable<SqlBuilder> GetNestedSubqueries(bool includeQueryColumns)
        {
            foreach (ISqlColumn SqlColumn in TableColumns)
            {
                if (SqlColumn is SqlQueryColumn query)
                {
                    foreach (var subtable in query.SqlBuilder.GetNestedBuilders(includeQueryColumns))
                    {
                        if (subtable.From != null /* && subtable.WhereConditions.Any() */)
                            yield return subtable;
                    }
                    //if (includeQueryColumns)
                    yield return query.SqlBuilder;
                }
            }
        }

        #region Where

        public SqlBuilder JoinEnvolvingTable(
           Func<SqlBuilder, ISqlColumn> LeftHand,
           Func<SqlBuilder, ISqlColumn> RightHand
        )
        {
            return JoinEnvolvingTable(
                (SqlBuilder x) => [LeftHand(x)] ,
                (SqlBuilder x) => [LeftHand(x)] 
            );
        }

        public SqlBuilder JoinEnvolvingTable(
            Func<SqlBuilder, IEnumerable<ISqlColumn>> LeftHand,
            Func<SqlBuilder, IEnumerable<ISqlColumn>> RightHand
        )
        {
            LocalWhereConditions.Add((BuildContext context) =>
            {
                var rightSqlBuilder = context.CurrentSource is SqlBuilder a ? a : new SqlBuilderResolver(context.CurrentTableAlias);

                var leftSqlBuilder = context.Sources.ElementAtOrDefault(1) is SqlBuilder b ? b : new SqlBuilderResolver(context.CurrentTableAlias);

                var leftColumns = LeftHand(leftSqlBuilder);
                var rightColumns = RightHand(rightSqlBuilder);

                List<string> predicates = [];

                if (leftColumns.Count() != rightColumns.Count())
                    throw new InvalidOperationException("Diferente número de columnas");

                for (var i = 0; i < leftColumns.Count(); i++)
                {
                    var left_predicate = leftColumns
                        .ElementAt(i)
                        .GetSqlExpression(context, 0);

                    var right_predicate = rightColumns
                        .ElementAt(i)
                        .GetSqlExpression(context, 0 + 1);

                    predicates.Add($"{left_predicate} = {right_predicate}");
                }

                return string.Join(" AND ", [.. predicates]);
            });
            return this;
        }

        public SqlBuilder Where<T>(bool condition, Func<SqlBuilder, ISqlColumn> sqlColumnSelector, T? Value, SQLOperator @operator = SQLOperator.AutoEquals)
        {
            if (condition)
                Where(sqlColumnSelector, Value, @operator);

            return this;
        }

        public SqlBuilder Where<T>(Func<SqlBuilder, ISqlColumn> sqlColumnSelector, T? Value, SQLOperator @operator = SQLOperator.AutoEquals)
        {
            string sqloperator;

            if (Value is not string && Value is IEnumerable E)
            {
                sqloperator = @operator.GetOperator(true);
            }
            else
            {
                sqloperator = @operator.GetOperator(false);
            }

            LocalWhereConditions.Add((BuildContext x) =>
            {
                var g = $"{sqlColumnSelector(this).GetWhereExpression(x)} {sqloperator} {Value.GetParameterValue()}";
                return g;
            });

            return this;
        }

        public SqlBuilder WhereParameter<T>(Func<SqlBuilder, ISqlColumn> sqlColumnSelector, T? Value, SQLOperator @operator = SQLOperator.AutoEquals)
        {
            IEnumerable<object?> enumerators;
            List<string> Keys = [];

            if (Value is not string && Value is IEnumerable E)
                enumerators = E.Cast<object>();
            else
                enumerators = [Value];

            foreach (var item in enumerators)
            {
                var id = $"Pb{this.ID}i{LocalParameters.Count}";
                LocalParameters.Add(id, item);
                Keys.Add("@" + id);
            }


            var predicate = string.Format(@operator.GetOperatorPredicate(Value), string.Join(',', Keys));

            LocalWhereConditions.Add((BuildContext x) =>
                $"{sqlColumnSelector(this).GetWhereExpression(x)} {predicate}");

            return this;
        }

        public SqlBuilder WhereNot<T>(Func<SqlBuilder, ISqlColumn> sqlColumnSelector, T? Value)
        {
            var id = $"@Pb{this.ID}i{LocalParameters.Count}";
            LocalParameters.Add(id, Value);
            LocalWhereConditions.Add((BuildContext x) =>
            {
                var expression1 = string.Format(sqlColumnSelector(this).GetWhereExpression(x), x.TableAlias.ToArray());
                return $"{expression1} {SQLOperator.AutoDiffers.GetOperatorPredicate(Value)} @{id}";
            });
            return this;
        }

        public SqlBuilder WhereIsNotNull(Func<SqlBuilder, ISqlColumn> sqlColumnSelector)
        {

            LocalWhereConditions.Add((BuildContext x) =>
            {
                var expression1 = string.Format(sqlColumnSelector(this).GetWhereExpression(x), x.TableAlias.ToArray());
                return $"{expression1} IS NOT NULL";
            });
            return this;
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of the SqlBuilder class with a source, alias, and optional columns.
        /// </summary>
        /// <param name="source">The SQL source object used to build the query.</param>
        /// <param name="alias">The alias for the SQL source in the query.</param>
        /// <param name="columns">Optional array of SqlColumn objects to include in the query.</param>
        internal SqlBuilder(ISqlSource source, string? alias, params SqlColumn[] columns)
        {
            From = source;
            Alias = alias!; //Can accept null

            if (source is SqlBuilder builder)
            {
                LocalWithTables.Add(builder);
                TableColumns.AddRange(builder.Columns.Select(c => new SqlColumn(rawName: c.Column.Name ?? throw new InvalidOperationException())));
            }

            TableColumns.AddRange(columns);
        }

        /// <summary>
        /// Initializes a new instance of the SqlBuilder class with an SQL source.
        /// </summary>
        /// <param name="sqlSource">The SQL source object.</param>
        internal SqlBuilder(ISqlSource sqlSource)
        {
            Alias = $"sourceless{count++}";
            From = sqlSource;
        }

        /// <summary>
        /// Initializes a new instance of the SqlBuilder class with a raw SQL table.
        /// </summary>
        /// <param name="rawTable">The raw table object to include in the query.</param>
        internal SqlBuilder(SqlTable rawTable) : this((ISqlSource)rawTable)
        {
        }

        /// <summary>
        /// Initializes a new instance of the SqlBuilder class with an alias.
        /// </summary>
        /// <param name="alias">The alias to use for the SQL source.</param>
        internal SqlBuilder(string alias) => Alias = alias;

        private static int count = 0;

        /// <summary>
        /// Initializes a new instance of the SqlBuilder class with an alias.
        /// </summary>
        internal SqlBuilder()
        {
            //Alias = $"Alias-{count++}";
        }

        /// <summary>
        /// Gets or sets the alias for the SQL source.
        /// </summary>
        public string Alias
        {
            get
            {
                if (string.IsNullOrWhiteSpace(m_custom_alias))
                {
                    if (From is null)
                        return $"No-alias-{ID}";

                    if (From is SqlTable table)
                        return table.Alias;

                    return From.Alias;
                }
                return m_custom_alias;

            }
            set
            {
                m_custom_alias = value;
            }
        }

        private string? m_custom_alias;

        private readonly int ID = count++;

        /// <summary>
        /// Gets or sets the SQL source object used to build the query.
        /// </summary>
        public ISqlSource? From { get; set; }


        /// <summary>
        /// Gets the list of table joins included in the query.
        /// </summary>
        public List<TableJoin> Joins { get; } = [];

        /// <summary>
        /// Gets or sets the number of rows to return (used for pagination).
        /// </summary>
        public int? Length { get; set; }

        /// <summary>
        /// Gets the current <see cref="OrderByClause"/>  clauses of this query
        /// </summary>
        public IEnumerable<OrderByClause> OrderByClauses =>
            LocalOrderByClauses.Union(
                Columns
                    .Where(x => x.Column.Order.Direction != OrderDirection.NONE)
                    .OrderBy(x => x.Column.Order.Index)
                    .Select(x => new OrderByClause(x.Column, x.TableAlias)
                )
            );

        /// <summary>
        /// Gets or sets the pre-query options (e.g., additional SQL clauses).
        /// </summary>
        public ICollection<string> PreQueryOptions { get; } = [];

        /// <summary>
        /// Gets or sets the post-query options (e.g., additional SQL clauses).
        /// </summary>
        public string? QueryOptions { get; set; }

        /// <summary>
        /// Gets or sets the number of rows to skip (used for pagination).
        /// </summary>
        public int? Skip { get; set; }

        /// <summary>
        /// Gets the list of columns included in the query.
        /// </summary>
        public List<ISqlColumn> TableColumns { get; internal set; } = [];

        /// <summary>
        /// Gets the list of WHERE conditions applied to the query.
        /// </summary>
        public List<Func<BuildContext, string>> LocalWhereConditions { get; } = [];


        /// <summary>
        /// Gets the list of WHERE conditions applied to the query.
        /// </summary>
        public IEnumerable<Func<BuildContext, string>> WhereConditions => [.. LocalWhereConditions];


        /// <summary>
        /// Gets the columns with their associated aliases.
        /// </summary>
        public IEnumerable<(string TableAlias, ISqlColumn Column)> Columns
        {
            get
            {
                foreach (var column in TableColumns)
                {
                    yield return (From?.Alias ?? Alias, column);
                }

                foreach (var join in Joins)
                {
                    foreach (var column in join.Columns)
                    {
                        yield return (join.Alias, column);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the list of orders used in the query.
        /// </summary>
        internal HashSet<OrderByClause> LocalOrderByClauses { get; } = [];

        /// <summary>
        /// Gets the list of subqueries used in the query.
        /// </summary>
        internal List<SqlBuilder> LocalWithTables { get; } = [];

        /// <summary>
        /// Builds the SQL query string based on the provided options.
        /// </summary>
        /// <param name="options">The build options for the query.</param>
        /// <returns>The SQL query string.</returns>
        public string Build(BuildOptions? options = default)
        {
            var context = new BuildContext(this, options ?? new BuildOptions());

            var result = Build(context);

            return result.Replace(context.IndentToken, context.CurrentOptions.Indentation);
        }

        /// <summary>
        /// Builds the SQL query string using a build context.
        /// </summary>
        /// <param name="context">The build context for the query.</param>
        /// <returns>The SQL query string.</returns>
        public string Build(BuildContext context)
        {
            if (!Columns.Any())
                throw new InvalidOperationException($"No columns specified in [{From}] [{Alias}]");

            var sb = new StringBuilder().DebugComment($"Build({ID}, Alias:{Alias}, Depth:{context.Depth})");

            if (context.Depth == 1)
            {
                var nestedTables = GetNestedBuilders(false).ToHashSet();

                DetermineQueryOptions(context, sb, nestedTables);

                DetermineWith(sb, context, nestedTables);
            }


            var (topSql, orderSqlWriter) = DetermineOffset(context);

            var fromWriter = DetermineFrom(context);

            DetermineSelect(sb, topSql, context);

            fromWriter(sb);

            DetermineWhere(sb, context);
            DetermineGroupBy(sb, context);

            orderSqlWriter(sb);

            if (context.CurrentOptions.ForJson.HasValue)
                AppendForJson(sb, context.CurrentOptions.ForJson.Value, context.CurrentOptions.Indentation);

            if (!string.IsNullOrWhiteSpace(QueryOptions))
                sb.Append(context.IndentToken)
                    .DebugComment("Query options")
                    .Append(QueryOptions);

            return sb.ToString();
        }

        private static void DetermineQueryOptions(BuildContext context, StringBuilder sb, HashSet<SqlBuilder> nestedTables)
        {
            var nestedTablesOptions = nestedTables.SelectMany(x => x.PreQueryOptions).Distinct().ToList();

            if (nestedTablesOptions.Count > 0)
            {
                sb.Append(context.IndentToken)
                    .DebugComment("Prequery options");

                foreach (var item in nestedTablesOptions)
                    sb.AppendLine(item);
            }
        }

        /// <summary>
        /// Elimina todas las cláusulas de ordenación (`ORDER BY`) actuales de la consulta SQL.
        /// </summary>
        /// <returns>El mismo <see cref="SqlBuilder"/> para permitir el encadenamiento de métodos.</returns>
        /// <remarks>
        /// Esta función también restablece la dirección e índice de orden en todas las columnas definidas en la consulta.
        /// </remarks>
        public SqlBuilder ClearOrder()
        {
            LocalOrderByClauses.Clear();
            foreach (var (_, Column) in Columns)
            {
                Column.Order.Reset();
            }

            return this;
        }

        /// <summary>
        /// Define una cláusula `ORDER BY` para las columnas especificadas con la dirección proporcionada.
        /// </summary>
        /// <param name="direction">La dirección de orden (ASC o DESC).</param>
        /// <param name="Columns">Un conjunto de nombres de columnas sobre las cuales aplicar la ordenación.</param>
        /// <returns>El mismo <see cref="SqlBuilder"/> para permitir el encadenamiento de métodos.</returns>
        /// <exception cref="InvalidOperationException">
        /// Se lanza cuando:
        /// - Alguna columna especificada no existe en la consulta.
        /// - Hay ambigüedad entre varias columnas con el mismo nombre pero diferentes alias de tabla.
        /// </exception>
        /// <remarks>
        /// - Primero, elimina cualquier orden existente llamando a <see cref="ClearOrder"/>.
        /// - Luego, intenta asignar la dirección de orden a cada columna especificada.
        /// - Si se encuentra más de una columna con el mismo nombre (y diferentes alias de tabla), se lanza una excepción.
        /// </remarks>
        public SqlBuilder OrderBy(OrderDirection direction, params string[] Columns)
        {
            ClearOrder();

            var groupJoin = Columns.GroupJoin(this.Columns, x => x, x => x.Column.Name, (x, y) => (x, y.ToArray()));

            int i = 0;
            foreach (var (name, columnsWithAlias) in groupJoin)
            {
                switch (columnsWithAlias.Length)
                {
                    case 0:
                        throw new InvalidOperationException($"La columna {name} no existe");

                    case 1:
                        columnsWithAlias[0].Column.Order.Direction = direction;
                        columnsWithAlias[0].Column.Order.Index = i++;
                        break;

                    default:
                        throw new InvalidOperationException($"Ambigüedad entre: {string.Join(",", columnsWithAlias.Select(x => $"{x.Column.Name}" + (x.TableAlias != Alias ? $" ({x.TableAlias})" : string.Empty))).ToArray()}");
                }
            }

            return this;
        }

        /// <summary>
        /// Define una cláusula `ORDER BY` para las columnas especificadas en orden ascendente (ASC).
        /// </summary>
        /// <param name="Columns">Un conjunto de nombres de columnas sobre las cuales aplicar la ordenación.</param>
        /// <returns>El mismo <see cref="SqlBuilder"/> para permitir el encadenamiento de métodos.</returns>
        /// <exception cref="InvalidOperationException">
        /// Se lanza cuando:
        /// - Alguna columna especificada no existe en la consulta.
        /// - Hay ambigüedad entre varias columnas con el mismo nombre pero diferentes alias de tabla.
        /// </exception>
        /// <remarks>
        /// - Primero, elimina cualquier orden existente llamando a <see cref="ClearOrder"/>.
        /// - Luego, asigna la dirección de orden ASC a cada columna especificada.
        /// - Si se encuentra más de una columna con el mismo nombre (y diferentes alias de tabla), se lanza una excepción.
        /// </remarks>
        public SqlBuilder OrderBy(params string[] Columns)
        {
            ClearOrder();

            var groupJoin = Columns.GroupJoin(this.Columns, x => x, x => x.Column.Name, (x, y) => (x, y.ToArray()));

            int i = 0;
            foreach (var (name, columnsWithAlias) in groupJoin)
            {
                switch (columnsWithAlias.Length)
                {
                    case 0:
                        throw new InvalidOperationException($"La columna {name} no existe");

                    case 1:
                        columnsWithAlias[0].Column.Order.Direction = OrderDirection.ASC;
                        columnsWithAlias[0].Column.Order.Index = i++;
                        break;

                    default:
                        throw new InvalidOperationException($"Ambigüedad entre: {string.Join(",", columnsWithAlias.Select(x => $"{x.Column.Name}" + (x.TableAlias != Alias ? $" ({x.TableAlias})" : string.Empty))).ToArray()}");
                }
            }

            return this;
        }

        public SqlBuilder OrderBy(params OrderByClause[] Columns)
        {
            ClearOrder();

            foreach (var column in Columns)
                LocalOrderByClauses.Add(column);

            return this;
        }


        /// <summary>
        /// Returns the SQL query string representation of the SqlBuilder object.
        /// </summary>
        /// <returns>The SQL query string.</returns>
        public override string ToString() => Build();


        public IDictionary<string, SqlBuilder> WithTables => GetNestedBuilders(false).ToDictionary(x => x.Alias, x => x);


        /// <summary>
        /// Appends a FOR JSON clause to the query with the specified options.
        /// </summary>
        /// <param name="sb">The StringBuilder to append the clause to.</param>
        /// <param name="forJson">The JSON options to apply (e.g., INCLUDE NULL VALUES).</param>
        /// <param name="indent">The indentation string for formatting the JSON output.</param>
        private static void AppendForJson(StringBuilder sb, ForJsonOptions forJson, string indent)
        {
            sb.Append(indent);
            sb.DebugComment($"{nameof(AppendForJson)}({forJson})");

            string options = string.Empty;

            if (forJson.HasFlag(ForJsonOptions.INCLUDE_NULL_VALUES))
                options += " INCLUDE_NULL_VALUES";

            if (forJson.HasFlag(ForJsonOptions.WITHOUT_ARRAY_WRAPPER))
                options += " WITHOUT_ARRAY_WRAPPER";

            if (options.Length > 0)
                options = ", " + options;

            if (forJson.HasFlag(ForJsonOptions.PATH))
                sb.AppendLine($"FOR JSON PATH{options}");
            else
                sb.AppendLine($"FOR JSON AUTO{options}");
        }

        /// <summary>
        /// Appends the FROM clause to the query, including any joins.
        /// </summary>
        /// <param name="sb">The StringBuilder to append the clause to.</param>
        /// <param name="context">The build context for constructing the query.</param>
        private Action<StringBuilder> DetermineFrom(BuildContext context)
        {
            if (From == null)
                return (StringBuilder sb) => sb.DebugComment("No from rendered");

            Action<StringBuilder> returning;

            if (From is SqlBuilder sqlbuilder)
                if (context.RenderedTables.Add(sqlbuilder))
                    returning = (StringBuilder sb) =>
                    {
                        sb.Append(context.IndentToken);

                        sb.DebugComment($"{nameof(DetermineFrom)}({ID}->{sqlbuilder.ID}).Append -> Sb.Alias: {sqlbuilder.Alias} Alias:{Alias} CurrentTableAlias:{context.CurrentTableAlias}{sqlbuilder.Alias} ");

                        sb.AppendLine($"FROM (");

                        using (context.Indent(sqlbuilder))
                        {
                            sb.AppendLine(sqlbuilder.Build(context));

                        }

                        sb.AppendLine($"{context.IndentToken}) AS [{sqlbuilder.Alias}]");
                    };
                else
                    returning = (StringBuilder sb) =>
                    {
                        sb.Append(context.IndentToken);

                        sb.DebugComment($"{nameof(DetermineFrom)}({ID}->{sqlbuilder.ID})->Sb.Alias: {sqlbuilder.Alias} Alias:{Alias} CurrentTableAlias:{context.CurrentTableAlias}");

                        sb.AppendLine($"FROM [{sqlbuilder.Alias}] AS [{context.CurrentTableAlias}]");
                    };
            else
                returning = (StringBuilder sb) =>
                {
                    sb.Append(context.IndentToken);

                    sb.DebugComment($"{nameof(DetermineFrom)}({ID})->Alias:{Alias} CurrentTableAlias:{context.CurrentTableAlias} SqlTable({From})");

                    sb.AppendLine($"FROM {From.Build(context)}");
                };

            return (StringBuilder sb) =>
            {
                returning(sb);
                foreach (var join in Joins)
                {
                    using (context.Indent(join.SqlSource))
                    {
                        sb.AppendLine(join.GetSqlExpression(context));
                    }
                }
            };
        }

        /// <summary>
        /// Appends the GROUP BY clause to the query based on non-aggregate columns.
        /// </summary>
        /// <param name="sb">The StringBuilder to append the clause to.</param>
        /// <param name="context">The build context for constructing the query.</param>
        private void DetermineGroupBy(StringBuilder sb, BuildContext context)
        {
            if (!Columns.Any(x => x.Column.Aggregate))
                return;

            var groupByColumns = Columns
                .Where(c => !c.Column.Aggregate)
                .Select(c => c.Column.GetGroupByExpression(context /*.SetCurrentTable(c.TableAlias)*/ ));

            if (!groupByColumns.Any()) return;

            sb.DebugComment(nameof(DetermineGroupBy));

            bool first = true;
            foreach (var groupByClause in groupByColumns)
            {
                sb.Append(context.IndentToken);

                if (first)
                    sb.Append("GROUP BY ");
                else
                    sb.Append("       , ");

                sb.AppendLine(groupByClause);

                first = false;
            }
        }


        /// <summary>
        /// Determines and constructs the OFFSET and ORDER BY clauses for the query.
        /// </summary>
        /// <param name="context">The build context for constructing the query.</param>
        /// <returns>A tuple containing the TOP SQL clause and the ORDER BY SQL clause.</returns>
        private (string TopSql, Func<StringBuilder, StringBuilder> OrderDelegate) DetermineOffset(BuildContext context)
        {
            // Inicialización de variables
            string TopSql = string.Empty;
            Func<StringBuilder, StringBuilder> OrderDelegate = sb => sb;

            var options = context.CurrentOptions;
            var skip = context.CurrentOptions.Skip ?? Skip;
            var length = context.CurrentOptions.Length ?? Length;

            var clauses = OrderByClauses.ToArray();

            // Determinar ORDER BY y OFFSET
            OrderDelegate = DeterminePagination(context, OrderDelegate, options, skip, length, clauses);

            if (!(skip.HasValue && skip.Value >= 0) && length.HasValue && length.Value > 0)
            {
                TopSql = $"TOP({length.Value}) ";
            }

            return (TopSql, OrderDelegate);
        }

        private static Func<StringBuilder, StringBuilder> DeterminePagination(BuildContext context, Func<StringBuilder, StringBuilder> OrderDelegate, BuildOptions options, int? skip, int? length, OrderByClause[] clauses)
        {
            if (clauses.Length > 0)
            {
                OrderDelegate = sb =>
                {
                    sb.Append(context.IndentToken);
                    sb.DebugComment($"{nameof(OrderDelegate)}()");

                    DetermineOrder(context, options, skip, length, clauses, sb);

                    DetermineOffset(skip, length, sb);

                    return sb;
                };
            }

            return OrderDelegate;
        }

        private static void DetermineOrder(BuildContext context, BuildOptions options, int? skip, int? length, OrderByClause[] clauses, StringBuilder sb)
        {
            // Validar condiciones en subtablas
            if (context.Depth > 1 &&
                (!skip.HasValue || skip.Value < 0) &&
                (!length.HasValue || length.Value < 0) &&
                !options.ForJson.HasValue)
            {
                // Encapsular advertencia y cláusulas en /* */
                sb.Comment($"No se puede utilizar ORDER BY debido a la falta de condiciones válidas.");
                sb.Comment($"-- Cláusulas encontradas:");
                foreach (var clause in clauses)
                {
                    sb.Comment($"{context.IndentToken}   {clause.Expression} {clause.Direction}");
                }
            }
            else
            {
                // Generar cláusulas ORDER BY
                sb.Append($"ORDER BY ");
                sb.AppendLine(string.Join($",{Environment.NewLine}{context.IndentToken}       ",
                    clauses.Select(clause => $"{clause.Expression} {clause.Direction}")));
            }
        }

        private static void DetermineOffset(int? skip, int? length, StringBuilder sb)
        {
            // Generar OFFSET y FETCH si es aplicable
            if (skip.HasValue && skip.Value >= 0)
            {
                sb.Append($" OFFSET {skip.Value} ROWS");

                if (length.HasValue && length.Value > 0)
                {
                    sb.Append($" FETCH NEXT {length.Value} ROWS ONLY");
                }
            }
        }




        /// <summary>
        /// Appends the SELECT clause to the query, including optional TOP clause.
        /// </summary>
        /// <param name="sb">The StringBuilder to append the clause to.</param>
        /// <param name="topSql">The TOP clause for limiting the number of rows.</param>
        /// <param name="context">The build context for constructing the query.</param>
        private void DetermineSelect(StringBuilder sb, string topSql, BuildContext context)
        {
            sb.Append(context.IndentToken);

            sb.DebugComment(nameof(DetermineSelect));

            sb.Append($"SELECT ");

            bool first = true, toprendered = false;

            if (!string.IsNullOrEmpty(topSql))
            {
                sb.AppendLine(topSql);
                toprendered = true;
            }


            foreach (var column in TableColumns)
            {
                sb.Append(!first, context.IndentToken)
                    .Append(!first, "  ,")
                    .AppendLine(column.GetSelectExpression(context));

                first = false;
            }

            foreach (var join in Joins)
                using (context.Indent(join.SqlSource))
                {
                    foreach (var column in join.Columns)
                    {
                        sb.Append(!first, context.IndentToken)
                            .Append(!first, "  ,")
                            .AppendLine(column.GetSelectExpression(context));

                        first = false;
                    }
                }
        }

        /// <summary>
        /// Appends the WHERE clause to the query based on the specified conditions.
        /// </summary>
        /// <param name="sb">The StringBuilder to append the clause to.</param>
        private void DetermineWhere(StringBuilder sb, BuildContext context)
        {
            if (!WhereConditions.Any()) return;


            bool first = true;
            foreach (var whereCondition in WhereConditions)
            {
                sb.Append(context.IndentToken);

                if (first)
                {
                    sb.DebugComment($"{nameof(DetermineWhere)}");
                    sb.Append("WHERE ");
                }
                else
                    sb.Append("  AND ");

                sb.AppendLine(string.Format(whereCondition(context), context.CurrentTableAlias));

                first = false;
            }
        }

        /// <summary>
        /// Appends the WITH clause to the query for CTEs (Common Table Expressions).
        /// </summary>
        /// <param name="sb">The StringBuilder to append the clause to.</param>
        /// <param name="context">The build context for constructing the query.</param>
        private void DetermineWith(StringBuilder sb, BuildContext context, HashSet<SqlBuilder> withTables)
        {
            withTables.Remove(this);

            if (withTables.Count == 0) return;

            sb.DebugComment(nameof(DetermineWith));

            sb.Append(";WITH ");

            int i = 0;
            foreach (var Table in withTables)
            {

                //This table has not been rendered
                if (context.RenderedTables.Add(Table))
                {
                    string outerTableName = Table.Alias;

                    if (context.RenderedTables.Any(x => x != Table && x.Alias == Table.Alias))
                    {   //This CTE name has been used
                        sb.Comment($"Table with alias {Table.Alias} already rendered");
                        outerTableName = $"{Table.Alias}-{Table.ID}";
                        sb.Comment($"Renaming to {Table.Alias}");
                    }

                    if (i++ > 0) sb.Append(',');

                    sb.AppendLine($"[{outerTableName}] AS (");

                    using (context.Indent(Table))
                    {

                        sb.AppendLine($"{Table.Build(context)}");

                        Table.Alias = outerTableName;

                    }

                    sb.AppendLine(")");
                }
            }
        }


        public static SqlBuilder Select(params ISqlColumn[] columns)
        {
            var ret = new SqlBuilder();

            ret.Select(columns);

            return ret;
        }

        /// <summary>
        /// Creates a new SqlBuilder instance with the specified subqueries added to the WITH clause.
        /// </summary>
        /// <param name="subqueries">An array of subqueries to include in the WITH clause.</param>
        /// <returns>A new SqlBuilder instance containing the specified subqueries.</returns>
        public static SqlBuilder With(params SqlBuilder[] subqueries)
        {
            if (subqueries == null || subqueries.Length == 0)
                throw new ArgumentException("At least one subquery must be provided.", nameof(subqueries));

            var result = new SqlBuilder();

            result.LocalWithTables.AddRange(subqueries);

            return result;
        }

        public static SqlBuilder SelectAll(bool selectAll = true)
        {
            return new()
            {
                TableColumns = { new SqlWildCardColumn(selectAll) }
            };
        }
    }
}
