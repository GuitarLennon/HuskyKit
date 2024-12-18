using System.Collections;
using System.Data.SqlTypes;
using System.Dynamic;
using System.Runtime.CompilerServices;
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

    public static partial class SqlBuilderExtensions
    {

        public static string GetOperator(this SQLOperator @operator, object? @object)
            => GetOperator(@operator, @object is not string && @object is IEnumerable, @object is null);

        public static string GetOperator(this SQLOperator @operator, bool isArray = false, bool isNull = false)
            => @operator switch
            {
                SQLOperator.AutoEquals => isNull ? "IS": isArray ? "IN" : "=",
                SQLOperator.AutoDiffers => isNull ? "IS NOT": isArray ? "NOT IN" : "!=",
                SQLOperator.Equals => "=",
                SQLOperator.GreaterThan => ">",
                SQLOperator.LessThan => "<",
                SQLOperator.GreaterThanOrEqualTo => ">",
                SQLOperator.LessThanOrEqualTo => "<",
                SQLOperator.NotEqualTo => "<>",
                SQLOperator.NotLessThan => "!<",
                SQLOperator.NotGreaterThan => "!<",
                SQLOperator.ISNOT => "IS NOT",
                _ => @operator.ToString(),
            };

        public static string BuildForJson(this SqlBuilder sqlBuilder, ForJsonOptions forJsonOptions)
        {
            var new_builder = SqlBuilder
                .With(sqlBuilder)
                .Select(
                    sqlBuilder.AsColumn("TEXT", forJson: forJsonOptions)
                );


            return new_builder.Build();
        }
    }

    /// <summary>
    /// Provides extension methods for building SQL queries using the SqlBuilder.
    /// </summary>
    public static partial class SqlBuilderExtensions
    {


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

                new(sqlBuilder.AsSubquery(alias),
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
        public static SqlBuilder AsSubquery(this SqlBuilder builder, string? alias = null)
            => new(builder, alias);

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
            return sqlBuilder;
        }

        public static SqlBuilder Join(this SqlBuilder builder, JoinTypes type, (string Schema, string Table) table, string predicate)
                                                                                                   => builder.Join(type, (table.Schema, table.Table, null), predicate);


        /// <summary>
        /// Adds a table join to the SQL query.
        /// </summary>
        public static SqlBuilder Join(this SqlBuilder sqlBuilder, JoinTypes joinType, (string Schema, string Table, string? Alias) joinTable, string predicate, params ISqlColumn[] columns)
        {
            var tableJoin = new TableJoin(joinType, (joinTable.Schema, joinTable.Table), joinTable.Alias, predicate, columns);
            sqlBuilder.Joins.Add(tableJoin);
            return sqlBuilder;
        }

        public static SqlBuilder Join(this SqlBuilder sqlBuilder, JoinTypes joinType, (string Schema, string Table) joinTable, string predicate, params ISqlColumn[] columns)
                    => sqlBuilder.Join(joinType, (joinTable.Schema, joinTable.Table, null), predicate, columns);

        public static SqlBuilder Join(this SqlBuilder sqlBuilder, JoinTypes joinType, SqlBuilder subQuery, string? predicate, params ISqlColumn[] columns)
        {
            sqlBuilder.LocalWithTables.Add(subQuery);
            var tableJoin = new TableJoin(joinType, subQuery.Alias, subQuery.Alias, predicate, columns);
            sqlBuilder.Joins.Add(tableJoin);
            return sqlBuilder;
        }

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
                sqlBuilder.LocalWhereConditions.Add((BuildContext x) => condition);
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

  
        /*
        public static SqlBuilder Where<T>(this SqlBuilder sqlBuilder, ISqlColumn sqlColumn, T? Value, SQLOperator @operator = SQLOperator.EqualsOrContains) where T : struct
        {
            var id = $"@P{sqlBuilder.Parameters.Count()}";
            sqlBuilder.LocalParameters.Add(id, Value);
            sqlBuilder.LocalWhereConditions.Add((BuildContext x) => $"{sqlColumn.GetWhereExpression(x)} {GetOperator(@operator)} @{id}");
            return sqlBuilder;
        }

        public static SqlBuilder Where<T>(this SqlBuilder sqlBuilder, ISqlColumn sqlColumn, IEnumerable<T> Value, SQLOperator @operator = SQLOperator.EqualsOrContains) where T : struct
        {
            var id = $"@P{sqlBuilder.Parameters.Count()}";
            sqlBuilder.LocalParameters.Add(id, Value);
            sqlBuilder.LocalWhereConditions.Add((BuildContext x) => $"{sqlColumn.GetWhereExpression(x)} {GetOperator(@operator, true)} @{id}");
            return sqlBuilder;
        }
        */
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
