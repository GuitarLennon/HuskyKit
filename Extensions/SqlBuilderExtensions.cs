using System.Collections;
using System.Data.SqlTypes;
using System.Dynamic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Linq;
using HuskyKit;
using HuskyKit.Sql.Columns;
using HuskyKit.Sql.Sources;

namespace HuskyKit.Sql
{
    public enum SQLOperator
    {
        AutoEquals,
        AutoDiffers,
        Equals,
        GreaterThan,
        LessThan,
        GreaterThanOrEqualTo,
        LessThanOrEqualTo,
        NotEqualTo,
        NotLessThan,
        NotGreaterThan,
        ALL,
        AND,
        ANY,
        BETWEEN,
        EXISTS,
        IN,
        LIKE,
        NOT,
        OR,
        SOME,
        IS,
        ISNOT
    }

    public enum DataType
    {
        Null,
        Time,
        Date,
        DateTime,
        Integer,
        Fractionary,
        Binary,
        String,
        Unicode,
        Misc
    }

    public static partial class SqlBuilderExtensions
    {

        public static string GetOperator(this SQLOperator @operator, bool isArray = false, bool isNull = false)
            => @operator switch
            {
                SQLOperator.AutoEquals => isNull ? "IS" : isArray ? "IN" : "=",
                SQLOperator.AutoDiffers => isNull ? "IS NOT" : isArray ? "NOT IN" : "!=",
                SQLOperator.Equals => "=",
                SQLOperator.GreaterThan => ">",
                SQLOperator.LessThan => "<",
                SQLOperator.GreaterThanOrEqualTo => ">",
                SQLOperator.LessThanOrEqualTo => "<",
                SQLOperator.NotEqualTo => "<>",
                SQLOperator.NotLessThan => "!<",
                SQLOperator.NotGreaterThan => "!<",
                SQLOperator.ISNOT => "IS NOT",
                _ => @operator.ToString() + "",
            };

        public static string GetOperatorPredicate(this SQLOperator @operator, object? @object)
            => GetOperatorPredicate(@operator, @object is not string && @object is IEnumerable, @object is null);

        public static string GetOperatorPredicate(this SQLOperator @operator, bool isArray = false, bool isNull = false)
            => @operator switch
            {
                SQLOperator.AutoEquals => isNull ? "IS {0}" : isArray ? "IN ({0})" : "= {0}",
                SQLOperator.AutoDiffers => isNull ? "IS NOT {0}" : isArray ? "NOT IN ({0})" : "!= {0}",
                SQLOperator.Equals => "= {0}",
                SQLOperator.GreaterThan => "> {0}",
                SQLOperator.LessThan => "< {0}",
                SQLOperator.GreaterThanOrEqualTo => "> {0}",
                SQLOperator.LessThanOrEqualTo => "< {0}",
                SQLOperator.NotEqualTo => "<> {0}",
                SQLOperator.NotLessThan => "!< {0}",
                SQLOperator.NotGreaterThan => "!< {0}",
                SQLOperator.ISNOT => "IS NOT {0}",
                _ => @operator.ToString() + " {0}",
            };

        public static string BuildForJson(this SqlBuilder sqlBuilder, ForJsonOptions forJsonOptions)
        {
            var new_builder = SqlBuilder
                .With(sqlBuilder.WithTables.Select(x => x.Value).ToArray())
                .Select(
                    sqlBuilder.AsColumn("TEXT", forJson: forJsonOptions)
                );


            return new_builder.Build();
        }

        public static string PrintParameter(this KeyValuePair<string, object?> Parameter)
        {
            string value = Parameter.Value?.ToString() ?? "null";

            if (Parameter.Value is null || Parameter.Value is bool)
            { }
            else if (Parameter.Value is int)
            { }
            else if (Parameter.Value is long)
            { }
            else if (Parameter.Value is float || Parameter.Value is double)
            { }
            else if (Parameter.Value is DateTime d)
            {
                value = $"DATETIMEFROMPARTS({d.Year}, {d.Month}, {d.Day}, {d.Hour}, {d.Minute}, {d.Second}, {d.Nanosecond})";
            }
            else
            {
                value = $"'{Parameter.Value}'";
            }


            return value;
        }

        public static string PrintParameterAsDeclare(this KeyValuePair<string, object?> Parameter)
        {
            string type;
            string value = Parameter.Value?.ToString() ?? "null";

            if (Parameter.Value is null || Parameter.Value is bool)
                type = "bit";
            else if (Parameter.Value is int)
                type = "int";
            else if (Parameter.Value is long)
                type = "long";
            else if (Parameter.Value is float || Parameter.Value is double)
                type = "float";
            else if (Parameter.Value is DateTime d)
            {
                type = "datetime";
                value = $"DATETIMEFROMPARTS({d.Year}, {d.Month}, {d.Day}, {d.Hour}, {d.Minute}, {d.Second}, {d.Nanosecond})";
            }
            else
            {
                type = $"NVARCHAR({Parameter.Value?.ToString()?.Length ?? 255})";
                value = $"'{Parameter.Value}'";
            }


            return $"DECLARE @{Parameter.Key} {type} = {value};";
        }

        public static string GetParameterValue(this object? Value) =>
            Value is null ? "NULL" : Value is string or DateTime
                ? $"'{Value}'"
                    : Value is IEnumerable e
                        ? !e.Cast<object>().Any() ? "(SELECT NULL)"
                            : $"({string.Join(',', e.Cast<object>().Select(x => x == null ? "null" : x is string or DateTime ? $"'{x}'" : x.ToString()).ToArray())})"
                        : Value.ToString()!;




        /// <summary>
        /// Creates an aggregate SQL column with an alias and optional ordering.
        /// </summary>
        public static SqlColumn Aggregate(this object expression, string alias, ColumnOrder? order = default)
            => new(expression, alias, true, order);


        /// <summary>
        /// Converts a SqlBuilder into a SQL query column with alias and ordering.
        /// </summary>
        public static SqlQueryColumn AsColumn(this SqlBuilder sqlBuilder, string alias, ColumnOrder? order, ForJsonOptions? forJson = default)
            => new(sqlBuilder, alias, order, forJson);

        /// <summary>
        /// Converts a SqlBuilder into a SQL query column with alias.
        /// </summary>
        public static SqlQueryColumn AsColumn(
            this SqlBuilder sqlBuilder,
            string alias,
            ForJsonOptions? forJson)
            => new(sqlBuilder, alias, default, forJson);

        /// <summary>
        /// Converts a SqlBuilder into a SQL query column with alias.
        /// </summary>
        public static SqlQueryColumn AsValueColumn(
            this SqlBuilder sqlBuilder,
            ISqlColumn valueColumn,
            string columAlias)
        {
            var new_Target = SqlBuilder.Select(valueColumn)
                .Where([.. sqlBuilder.LocalWhereConditions])
                .OrderBy([.. sqlBuilder.OrderByClauses]);

            if (sqlBuilder.From != null)
                new_Target.From(sqlBuilder.From)
                    .Joins.AddRange(sqlBuilder.Joins);

            SqlQueryColumn sqlColumn = new(
                new_Target,
                columAlias,
                length: 1
            );

            return sqlColumn;
        }



        /// <summary>
        /// Converts a SqlBuilder into a SQL query column with alias, ordering, pagination, and JSON options.
        /// </summary>
        public static SqlQueryColumn AsColumn(this SqlBuilder sqlBuilder, string alias, ColumnOrder? order = default,
                                              int? skip = null, int? take = null, ForJsonOptions? forJson = default)
        {
            SqlQueryColumn sqlColumn =
                new(
                    SqlBuilder.With(sqlBuilder.WithTables.Select(x => x.Value).ToArray())
                              .Select(sqlBuilder.Columns.Select(x => x.Column).ToArray())
                              .From(sqlBuilder.From!)
                              .Join(sqlBuilder.Joins.Select(x =>
                                new TableJoin(x.JoinType, x.SqlSource,
                                    predicate: x.Predicate,
                                    columns: [.. x.Columns])
                               ).ToArray())
                              .Where(sqlBuilder.WhereConditions),
                    //sqlBuilder.AsSubquery(alias),
                    alias,
                    order,
                    forJson,
                    skip,
                    take);

            return sqlColumn;
        }

        /// <summary>
        /// Creates a SQL query column for counting rows in a subquery.
        /// </summary>
        public static SqlQueryColumn AsColumnCount(this SqlBuilder sqlBuilder, string countExpression, string columnName, int? skip = null, int? take = null, ColumnOrder? order = default)
        {
            var subquery = sqlBuilder.AsSubquery()
                .CleanSelect(Funciones.Count(countExpression));

            return new SqlQueryColumn(subquery, columnName, order, null, skip, take);
        }

        /// <summary>
        /// Creates a SQL query column for counting rows in a subquery.
        /// </summary>
        public static SqlQueryColumn AsColumnCount(this SqlBuilder sqlBuilder, string alias, int? skip = null, int? take = null, ColumnOrder? order = default)
        {
            var subquery = sqlBuilder.AsSubquery()
                .CleanSelect(Funciones.Count());

            return new SqlQueryColumn(subquery, alias, order, null, skip, take);
        }

        /// <summary>
        /// Wraps the builder as a subquery with a given alias.
        /// </summary>
        public static SqlBuilder AsSubquery(
            this SqlBuilder builder, 
            string? alias = null
        )
        {
            if (!builder.Columns.Any())
                throw new InvalidOperationException("No se declararon columnas");

            var _builder = new SqlBuilder (builder, alias);

            return _builder;
        }

        /// <summary>
        /// Wraps the builder as a subquery with a given alias and adds table joins.
        /// </summary>
        public static SqlBuilder AsSubquery(this SqlBuilder builder, string? alias = null, params TableJoin[] joins)
        {
            var result = new SqlBuilder(builder, alias);
            if (joins != null && joins.Length > 0)
                result.Joins.AddRange(joins);

            return result;
        }

        /// <summary>
        /// Replaces all columns in the SELECT clause with new ones.
        /// </summary>
        public static SqlBuilder CleanSelect(this SqlBuilder sqlBuilder, params ISqlColumn[] columns)
        {
            sqlBuilder.TableColumns.Clear();
            sqlBuilder.TableColumns.AddRange(columns);

            if (sqlBuilder.Columns.Count() == 0)
                throw new 
                    InvalidOperationException("No se ha seleccionado ninguna columna");

            return sqlBuilder;
        }

        /// <summary>
        /// Adds a table join to the SQL query.
        /// </summary>
        public static SqlBuilder Join(this SqlBuilder sqlBuilder, JoinTypes joinType, ISqlSource joinTable, string predicate, params ISqlColumn[] columns)
        {
            var tableJoin = new TableJoin(joinType, joinTable, predicate, [.. columns]);
            sqlBuilder.Joins.Add(tableJoin);
            return sqlBuilder;
        }



        /// <summary>
        /// Adds a table join to the SQL query.
        /// </summary>
        public static SqlBuilder Join(this SqlBuilder sqlBuilder, JoinTypes joinType, ISqlSource joinTable,
                                      Func<SqlBuilder, IEnumerable<ISqlColumn>> columnSelector,
                                      params ISqlColumn[] columns)
        {
            var tableJoin = new TableJoin(joinType, joinTable, columnSelector, columnSelector, [.. columns]);
            sqlBuilder.Joins.Add(tableJoin);
            return sqlBuilder;
        }

        public static SqlBuilder Join(this SqlBuilder sqlBuilder, JoinTypes joinType, SqlTable joinTable,
                              Func<SqlBuilder, IEnumerable<ISqlColumn>> columnSelector,
                              params ISqlColumn[] columns)
            => Join(sqlBuilder, joinType, (ISqlSource)joinTable, columnSelector, columns);


        public static SqlBuilder Join(this SqlBuilder sqlBuilder, JoinTypes joinType, SqlTable joinTable,
                              Func<SqlBuilder, ISqlColumn> columnSelector,
                              params ISqlColumn[] columns)
            => Join(sqlBuilder, joinType, (ISqlSource)joinTable, (SqlBuilder x) => [columnSelector(x)], columns);


        public static SqlBuilder InnerJoin(this SqlBuilder sqlBuilder, SqlTable joinTable,
                            Func<SqlBuilder, ISqlColumn> columnSelector,
                            params ISqlColumn[] columns)
            => Join(sqlBuilder, JoinTypes.INNER, (ISqlSource)joinTable, (SqlBuilder x) => [columnSelector(x)], columns);

        public static SqlBuilder LeftJoin(this SqlBuilder sqlBuilder, SqlTable joinTable,
                            Func<SqlBuilder, ISqlColumn> columnSelector,
                            params ISqlColumn[] columns)
            => Join(sqlBuilder, JoinTypes.LEFT, (ISqlSource)joinTable, (SqlBuilder x) => [columnSelector(x)], columns);


        /// <summary>
        /// Adds a table join to the SQL query.
        /// </summary>
        public static SqlBuilder Join(this SqlBuilder sqlBuilder, JoinTypes joinType, ISqlSource joinTable,
                                      Func<SqlBuilder, IEnumerable<ISqlColumn>> leftHand,
                                      Func<SqlBuilder, IEnumerable<ISqlColumn>> rightHand, params ISqlColumn[] columns)
        {
            var tableJoin = new TableJoin(joinType, joinTable, leftHand, rightHand, [.. columns]);
            sqlBuilder.Joins.Add(tableJoin);
            return sqlBuilder;
        }


        /// <summary>
        /// Adds a table join to the SQL query.
        /// </summary>
        public static SqlBuilder Join(this SqlBuilder sqlBuilder, JoinTypes joinType, ISqlSource joinTable,
                                      Func<SqlBuilder, ISqlColumn> leftHand,
                                      Func<SqlBuilder, ISqlColumn> rightHand, params ISqlColumn[] columns)
        {
            var tableJoin = new TableJoin(joinType, joinTable, (SqlBuilder x) => [leftHand(x)], (SqlBuilder x) => [rightHand(x)], [.. columns]);
            sqlBuilder.Joins.Add(tableJoin);
            return sqlBuilder;
        }

        /*
        [Obsolete]
        public static SqlBuilder Join(this SqlBuilder sqlBuilder, JoinTypes joinType, SqlTable joinTable, string predicate, params ISqlColumn[] columns)
                    => sqlBuilder.Join(joinType, joinTable, predicate, columns);

        public static SqlBuilder Join(this SqlBuilder sqlBuilder, JoinTypes joinType, SqlBuilder subQuery, string? predicate, params ISqlColumn[] columns)
        {
            sqlBuilder.LocalWithTables.Add(subQuery);
            var tableJoin = new TableJoin(joinType, subQuery.Alias, subQuery.Alias, predicate, columns);
            sqlBuilder.Joins.Add(tableJoin);
            return sqlBuilder;
        }
        */

        /// <summary>
        /// Orders a column with an index and direction.
        /// </summary>
        public static SqlColumn OrderBy(this string columnName, int index, OrderDirection order = OrderDirection.ASC)
            => new(columnName, columnName, false, new ColumnOrder(index, order));

        //public static SqlBuilder OrderBy(this SqlBuilder sqlBuilder, params ColumnOrder[] values)
        //{
        //    for (int i = 0; i < values.Length; i++)
        //    {
        //        var (Alias, Column) = sqlBuilder.ColumnsWithAlias.FirstOrDefault(x => x.TableAlias == values[i]..);

        //        if (Column != null)
        //            Column.Order = (i, Column.Name ?? throw new ArgumentNullException(nameof(values)), values[i].Direction);
        //    }
        //    return sqlBuilder;
        //}

        /// <summary>
        /// Adds columns to the SELECT clause.
        /// </summary>
        public static SqlBuilder Select(this SqlBuilder sqlBuilder, params ISqlColumn[] columns)
        {
            sqlBuilder.TableColumns.AddRange(columns);
            return sqlBuilder;
        }


        /// <summary>
        /// Selects all columns.
        /// </summary>
        public static SqlBuilder SelectWhere(this SqlBuilder sqlBuilder, Func<ISqlColumn, bool> selector)
        {
            var columns = sqlBuilder.TableColumns.Where(selector).ToArray();

            sqlBuilder.CleanSelect(columns);

            return sqlBuilder;
        }


        /// <summary>
        /// Selects all columns.
        /// </summary>
        public static SqlBuilder SelectAll(this SqlBuilder sqlBuilder, bool AllColumns = true)
        {
            sqlBuilder.TableColumns.Add(new SqlWildCardColumn(AllColumns));
            return sqlBuilder;
        }

        /// <summary>
        /// Creates a SQL column with an alias, optional aggregation, and ordering.
        /// </summary>
        public static SqlColumn As(this object? expression, string alias, bool aggregate = false, ColumnOrder? order = default)
            => new(expression, alias, aggregate, order);


        /// <summary>
        /// Creates a SQL column with an alias, optional aggregation, and ordering.
        /// </summary>
        public static SqlColumn Aggregate(this string expression, string? alias = null)
            => new(expression, alias ?? expression, true);

        /// <summary>
        /// Conditionally adds columns to the SELECT clause.
        /// </summary>
        public static SqlBuilder SelectIf(this SqlBuilder sqlBuilder, bool condition, params ISqlColumn[] columns)
        {
            if (condition)
                sqlBuilder.TableColumns.AddRange(columns);
            return sqlBuilder;
        }

        /// <summary>
        /// Adds WHERE conditions to the SQL query.
        /// </summary>
        public static SqlBuilder Where(this SqlBuilder sqlBuilder, params string[] conditions)
        {
            foreach (var condition in conditions)
            {
                sqlBuilder.LocalWhereConditions.Add((BuildContext x) => string.Format(condition, x.CurrentTableAlias));
            }
            return sqlBuilder;
        }

        public static SqlBuilder Where(this SqlBuilder sqlBuilder, params Func<BuildContext, string>[] conditions)
        {
            foreach (var condition in conditions)
            {
                sqlBuilder.LocalWhereConditions.Add(condition);
            }
            return sqlBuilder;
        }

        public static SqlBuilder Where(this SqlBuilder sqlBuilder, bool applyCondition, params string[] conditions)
        {
            if (applyCondition)
                foreach (var condition in conditions)
                {
                    sqlBuilder.LocalWhereConditions.Add((BuildContext x) => condition);
                }
            return sqlBuilder;
        }

        /// <summary>
        /// Sets or replaces the source from this table
        /// </summary>
        /// <param name="sqlBuilder">this sql builder</param>
        /// <param name="source">new source to replace from</param>
        /// <returns></returns>
        public static SqlBuilder From(ISqlSource source, string? Alias = null)
        {
            var sqlBuilder = new SqlBuilder(Alias ?? source.Alias)
            {
                From = source
            };

            return sqlBuilder;
        }

        /// <summary>
        /// Sets or replaces the source from this table
        /// </summary>
        /// <param name="sqlBuilder">this sql builder</param>
        /// <param name="source">new source to replace from</param>
        /// <returns></returns>
        public static SqlBuilder From(this SqlBuilder sqlBuilder, ISqlSource source, string? Alias = null)
        {
            sqlBuilder.Alias = Alias ?? source.Alias;
            sqlBuilder.From = source;
            //sqlBuilder.From.Alias = sqlBuilder.Alias;


            return sqlBuilder;
        }

        /// <summary>
        /// Sets or replaces the source from this table
        /// </summary>
        /// <param name="sqlBuilder">this sql builder</param>
        /// <param name="source">new source to replace from</param>
        /// <returns></returns>
        public static SqlBuilder From(this SqlBuilder sqlBuilder, SqlTable source, string? Alias = null)
        {
            sqlBuilder.From = source;
            sqlBuilder.Alias = Alias ?? source.Alias;
            return sqlBuilder;
        }


        /// <summary>
        /// Sets or replaces the source from this table
        /// </summary>
        /// <param name="sqlBuilder">this sql builder</param>
        /// <param name="source">new source to replace from</param>
        /// <returns></returns>
        public static SqlBuilder From(this SqlBuilder sqlBuilder, string sourceAlias)
        {
            sqlBuilder.From = new SqlTable(sourceAlias);
            sqlBuilder.Alias = sourceAlias;
            return sqlBuilder;
        }


        /// <summary>
        /// Sets or replaces the source from this table
        /// </summary>
        /// <param name="sqlBuilder">this sql builder</param>
        /// <param name="source">new source to replace from</param>
        /// <returns></returns>
        public static SqlBuilder FromIndexedView(this SqlBuilder sqlBuilder, SqlTable source, string? Alias = null)
        {
            sqlBuilder.From = source;
            sqlBuilder.Alias = Alias ?? source.Alias;
            source.NoExpand = true;
            return sqlBuilder;
        }
        //WITH (NOEXPAND)


        /// <summary>
        /// Sets or replaces the top property
        /// </summary>
        /// <param name="sqlBuilder">this sql builder</param>
        /// <param name="top">new value to replace from. Null removes the top</param>
        /// <returns></returns>
        public static SqlBuilder Top(this SqlBuilder sqlBuilder, int? top)
        {
            sqlBuilder.Length = top;
            return sqlBuilder;
        }


        /// <summary>
        /// Sets or replaces the top property
        /// </summary>
        /// <param name="sqlBuilder">this sql builder</param>
        /// <param name="top">new value to replace from. Null removes the top</param>
        /// <returns></returns>
        public static SqlBuilder Offset(this SqlBuilder sqlBuilder, int? skip)
        {
            sqlBuilder.Skip = skip;
            return sqlBuilder;
        }


    }
}
