using HuskyKit.Extensions;
using HuskyKit.Sql.Columns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HuskyKit.Sql.Sources
{


    public partial class SqlBuilder : ISqlSource
    {
        /// <summary>
        /// Builds the SQL query string based on the provided options.
        /// </summary>
        /// <param name="options">The build options for the query.</param>
        /// <returns>The SQL query string.</returns>
        public string Build(BuildOptions? options = default)
        {
            var context = new BuildContext(this, options ?? new BuildOptions());

#if DEBUG
            //Console.WriteLine($"Iniciando Build con options: <{options}> y alias: <{context}>");
#endif

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
                throw new InvalidOperationException($"No columns specified in [{From_Source}] [{Alias}]");

            var sb = new StringBuilder().DebugComment($"Build({ID}, Alias:{Alias}, Depth:{context.Depth})");

            if (context.Depth == 1)
            {
                var nestedTables = GetNestedBuilders(true).ToHashSet();

                DetermineQueryOptions(context, sb, nestedTables);

                DetermineWith(sb, context, nestedTables);
            }


            var (topSql, orderSqlWriter) = DetermineOffset(context);

            var fromWriter = DetermineFrom(context);

            DetermineSelect(sb, topSql, context);

            fromWriter(sb);

            DetermineWhere(sb, context);
            DetermineGroupBy(sb, context);

            DetermineSetOperation(sb, context);


            orderSqlWriter(sb);

            if (context.CurrentOptions.ForJson.HasValue)
                AppendForJson(sb, context.CurrentOptions.ForJson.Value, context.CurrentOptions.Indentation);

            if (!string.IsNullOrWhiteSpace(QueryOptions))
                sb.Append(context.IndentToken)
                    .DebugComment("Query options")
                    .Append(QueryOptions);

            return sb.ToString();
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

        protected IEnumerable<SqlBuilder> GetNestedBuilders(bool includeQueryColumns)
        {
            foreach (var table in LocalWithTables)
            {
                foreach (var subtable in table.GetNestedBuilders(includeQueryColumns))
                    if (subtable.From_Source != null)
                        yield return subtable;

                if (table.From_Source != null)
                    yield return table;
            }

            GetNestedSubqueries(includeQueryColumns);
        }

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
                    sb.Comment($"{context.IndentToken}   {clause.GetSqlExpression(context)}");
                }


                //sb.Append($"ORDER BY ")
                //    .AppendLine(string.Join($",{Environment.NewLine}{context.IndentToken}       ",
                //        clauses.Select(clause => $"{clause.Expression} {clause.Direction}")))
                //    .AppendLine($"{Environment.NewLine}{context.IndentToken}OFFSET 0 ROWS");
            }
            else
            {
                // Generar cláusulas ORDER BY
                sb.Append($"ORDER BY ");
                sb.AppendLine(string.Join($",{Environment.NewLine}{context.IndentToken}       ",
                    clauses.Select(clause => clause.GetSqlExpression(context))));
            }
        }

        private static Func<StringBuilder, StringBuilder> DeterminePagination(
            BuildContext context,
            Func<StringBuilder, StringBuilder> OrderDelegate,
            BuildOptions options,
            int? skip,
            int? length,
            OrderByClause[] clauses)
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
        /// Appends the FROM clause to the query, including any joins.
        /// </summary>
        /// <param name="sb">The StringBuilder to append the clause to.</param>
        /// <param name="context">The build context for constructing the query.</param>
        private Action<StringBuilder> DetermineFrom(BuildContext context)
        {
            if (From_Source == null)
                return sb => sb.DebugComment("No from rendered");

            var renderFrom = From_Source switch
            {
                SqlBuilder sqlbuilder => RenderSqlBuilderFrom(context, sqlbuilder),
                _ => RenderSimpleFrom(context)
            };

            return sb =>
            {
                renderFrom(sb);
                RenderJoins(sb, context);
            };
        }

        private Action<StringBuilder> RenderSqlBuilderFrom(BuildContext context, SqlBuilder sqlbuilder)
        {
            bool isFirstRender = context.RenderedTables.Add(sqlbuilder);

            if (isFirstRender)
            {
                return sb =>
                {
                    sb.Append(context.IndentToken);
                    sb.DebugComment($"RenderSqlBuilderFrom({ID}->{sqlbuilder.ID}) FirstRender");

                    sb.AppendLine("FROM (");
                    using (context.Indent(sqlbuilder))
                    {
                        sb.AppendLine(sqlbuilder.Build(context));
                    }
                    sb.AppendLine($"{context.IndentToken}) AS [{sqlbuilder.Alias}]");
                };
            }
            else
            {
                return sb =>
                {
                    sb.Append(context.IndentToken);
                    sb.DebugComment($"RenderSqlBuilderFrom({ID}->{sqlbuilder.ID}) Reuse");

                    string aliasPart = sqlbuilder.Alias == context.CurrentTableAlias
                        ? $"[{sqlbuilder.Alias}]"
                        : $"[{sqlbuilder.Alias}] AS [{context.CurrentTableAlias}]";

                    sb.AppendLine($"FROM {aliasPart}");
                };
            }
        }

        private Action<StringBuilder> RenderSimpleFrom(BuildContext context)
        {
            return sb =>
            {
                sb.Append(context.IndentToken);
                sb.DebugComment($"RenderSimpleFrom({ID})");
                sb.AppendLine($"FROM {From_Source!.Build(context)}");
            };
        }

        private void RenderJoins(StringBuilder sb, BuildContext context)
        {
            foreach (var join in Joins)
            {
                using (context.Indent(join.SqlSource))
                {
                    sb.AppendLine(join.GetSqlExpression(context));
                }
            }
        }


        /// <summary>
        /// Appends the GROUP BY clause to the query based on non-aggregate columns.
        /// </summary>
        /// <param name="sb">The StringBuilder to append the clause to.</param>
        /// <param name="context">The build context for constructing the query.</param>
        private void DetermineGroupBy(StringBuilder sb, BuildContext context)
        {
            if (!Columns.Any(x => x.Column.IsAggregate))
                return;

            var groupByColumns = Columns
                .Where(c => !c.Column.IsAggregate)
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

        private bool NeedsToBeRenderedAsWith =>
            From_Source is not SqlTable ||
            Columns.Any(x =>
                x.Column is not SqlColumn sq ||
                !sq.IsMappedToColumn ||
                sq.IsAggregate
            ) ||
            SetOperations.Count > 0 ||
            Length.HasValue ||
            Skip.HasValue ||
            OrderByClauses.Any();

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
                if (!context.RenderedTables.Add(Table))
                    continue;

                //if (!Table.NeedsToBeRenderedAsWith)
                    //continue;

                string outerTableName = Table.Alias;

                if (context.RenderedTables.Any(x => x != Table && x.Alias == Table.Alias))
                {   //This CTE name has been used
                    sb.Comment($"Table with alias {Table.Alias} already rendered");
                    outerTableName = $"{Table.Alias}-{Table.ID}";
                    sb.Comment($"Renaming to {Table.Alias}");
                }

                if (i++ > 0) sb.Append(',');

                sb.AppendLine($"[{outerTableName}] AS (");

                sb.DebugComment($"NeedsToBeRenderedAsWith: {Table.NeedsToBeRenderedAsWith}");

                using (context.Indent(Table))
                {
                    sb.AppendLine($"{Table.Build(context)}");

                    Table.Alias = outerTableName;
                }

                sb.AppendLine(")");
            }

        }

        private IEnumerable<SqlBuilder> GetNestedSubqueries(bool includeQueryColumns)
        {
            foreach (ISqlColumn SqlColumn in TableColumns)
            {
                if (SqlColumn is SqlQueryColumn query)
                {
                    foreach (var subtable in query.SqlBuilder.GetNestedBuilders(includeQueryColumns))
                    {
                        if (subtable.From_Source != null /* && subtable.WhereConditions.Any() */)
                            yield return subtable;
                    }
                    //if (includeQueryColumns)
                    yield return query.SqlBuilder;
                }
            }
        }

        public SqlBuilder Join(params TableJoin[] tableJoins)
        {
            Joins.AddRange(tableJoins);

            return this;
        }
    }
}
