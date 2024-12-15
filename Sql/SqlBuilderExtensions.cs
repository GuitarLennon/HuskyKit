using System.Runtime.CompilerServices;

namespace HuskyKit.Sql
{
    /// <summary>
    /// Provides extension methods for building SQL queries using the SqlBuilder.
    /// </summary>
    public static partial class SqlBuilderExtensions
    {

        /// <summary>
        /// Creates an aggregate SQL column with an alias and optional ordering.
        /// </summary>
        public static SqlColumn Aggregate(this object expression, string alias, ColumnOrder order = default)
            => new(expression, alias, true, order);


        /// <summary>
        /// Converts a SqlBuilder into a SQL query column with alias and ordering.
        /// </summary>
        public static SqlQueryColumn AsColumn(this SqlBuilder sqlBuilder, string alias, ColumnOrder order, ForJsonOptions? forJson = default)
            => new(sqlBuilder, alias, order, forJson);

        /// <summary>
        /// Converts a SqlBuilder into a SQL query column with alias.
        /// </summary>
        public static SqlQueryColumn AsColumn(this SqlBuilder sqlBuilder, string alias, ForJsonOptions? forJson)
            => new(sqlBuilder, alias, default, forJson);

        /// <summary>
        /// Converts a SqlBuilder into a SQL query column with alias, ordering, pagination, and JSON options.
        /// </summary>
        public static SqlQueryColumn AsColumn(this SqlBuilder sqlBuilder, string alias, ColumnOrder order = default, int? skip = null, int? take = null, ForJsonOptions? forJson = default)
            => new(sqlBuilder, alias, order, forJson, skip, take);

        /// <summary>
        /// Creates a SQL query column for counting rows in a subquery.
        /// </summary>
        public static SqlQueryColumn AsColumnCount(this SqlBuilder sqlBuilder, string alias, int? skip = null, int? take = null, ColumnOrder order = default)
        {
            var subquery = sqlBuilder.AsSubquery()
                .CleanSelect(Funciones.Count(alias));

            return new SqlQueryColumn(subquery, alias, order, null, skip, take);
        }

        /// <summary>
        /// Wraps the builder as a subquery with a given alias.
        /// </summary>
        public static SqlBuilder AsSubquery(this SqlBuilder builder, string alias = "sq")
            => new(builder, alias);

        /// <summary>
        /// Wraps the builder as a subquery with a given alias and adds table joins.
        /// </summary>
        public static SqlBuilder AsSubquery(this SqlBuilder builder, string alias = "sq", params TableJoin[] joins)
        {
            var result = new SqlBuilder(builder, alias);
            if (joins != null && joins.Length > 0)
                result.Joins.AddRange(joins);

            return result;
        }

        /// <summary>
        /// Replaces all columns in the SELECT clause with new ones.
        /// </summary>
        public static SqlBuilder CleanSelect(this SqlBuilder sqlBuilder, params SqlColumnAbstract[] columns)
        {
            sqlBuilder.TableColumns.Clear();
            sqlBuilder.TableColumns.AddRange(columns);
            return sqlBuilder;
        }

        public static SqlBuilder From(this SqlBuilder sqlBuilder, ISqlSource source)
        {
            sqlBuilder.From = source;
            return sqlBuilder;
        }

        /// <summary>
        /// Adds a general filter condition to the SQL query.
        /// </summary>
        public static SqlBuilder GeneralFilter(this SqlBuilder sqlBuilder, string filter)
        {
            var conditions = sqlBuilder.ColumnsWithAlias
                .Select(x => $"[{x.Alias}].[{x.Column.Name}] LIKE '%{filter}%'")
                .Aggregate((a, b) => $"{a} OR {b}");

            sqlBuilder.WhereConditions.Add($"({conditions})");
            return sqlBuilder;
        }

        public static SqlBuilder Join(this SqlBuilder builder, JoinTypes type, (string Schema, string Table) table, string predicate)
                                                                                                   => builder.Join(type, (table.Schema, table.Table, null), predicate);


        /// <summary>
        /// Adds a table join to the SQL query.
        /// </summary>
        public static SqlBuilder Join(this SqlBuilder sqlBuilder, JoinTypes joinType, (string Schema, string Table, string? Alias) joinTable, string predicate, params SqlColumnAbstract[] columns)
        {
            var tableJoin = new TableJoin(joinType, (joinTable.Schema, joinTable.Table), joinTable.Alias, predicate, columns);
            sqlBuilder.Joins.Add(tableJoin);
            return sqlBuilder;
        }

        public static SqlBuilder Join(this SqlBuilder sqlBuilder, JoinTypes joinType, (string Schema, string Table) joinTable, string predicate, params SqlColumnAbstract[] columns)
                    => sqlBuilder.Join(joinType, (joinTable.Schema, joinTable.Table, null), predicate, columns);

        public static SqlBuilder Join(this SqlBuilder sqlBuilder, JoinTypes joinType, SqlBuilder subQuery, string? predicate, params SqlColumnAbstract[] columns)
        {
            sqlBuilder.WithTables.Add(subQuery);
            var tableJoin = new TableJoin(joinType, subQuery.Alias, subQuery.Alias, predicate, columns);
            sqlBuilder.Joins.Add(tableJoin);
            return sqlBuilder;
        }

        /// <summary>
        /// Orders a column with an index and direction.
        /// </summary>
        public static SqlColumn OrderBy(this string columnName, int index, OrderDirection order = OrderDirection.ASC)
            => new(columnName, columnName, false, new ColumnOrder { Index = index, Direction = order });

        //    return sqlBuilder.Join(joinType, other, predicate, columns);
        //}
        public static SqlBuilder OrderBy(this SqlBuilder sqlBuilder, params ColumnOrder[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                var (Alias, Column) = sqlBuilder.ColumnsWithAlias.FirstOrDefault(x => x.Alias == values[i].Column);

                if (Column != null)
                    Column.Order = (i, Column.Name ?? throw new ArgumentNullException(nameof(values)), values[i].Direction);
            }
            return sqlBuilder;
        }

        /// <summary>
        /// Adds columns to the SELECT clause.
        /// </summary>
        public static SqlBuilder Select(this SqlBuilder sqlBuilder, params SqlColumnAbstract[] columns)
        {
            sqlBuilder.TableColumns.AddRange(columns);
            return sqlBuilder;
        }

        /// <summary>
        /// Selects all columns.
        /// </summary>
        public static SqlBuilder SelectAll(this SqlBuilder sqlBuilder)
        {
            sqlBuilder.TableColumns.Add(SqlColumn.All());
            return sqlBuilder;
        }

        /// <summary>
        /// Creates a SQL column with an alias, optional aggregation, and ordering.
        /// </summary>
        public static SqlColumn As(this object expression, string alias, bool aggregate = false, ColumnOrder order = default)
            => new(expression, alias, aggregate, order);

        /// <summary>
        /// Conditionally adds columns to the SELECT clause.
        /// </summary>
        public static SqlBuilder SelectIf(this SqlBuilder sqlBuilder, bool condition, params SqlColumnAbstract[] columns)
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
            sqlBuilder.WhereConditions.AddRange(conditions);
            return sqlBuilder;
        }

        //public static SqlBuilder NaturalJoin(this SqlBuilder sqlBuilder, (string Schema, string Table, string? Alias) other, JoinTypes joinType = JoinTypes.INNER, params SqlColumn[] columns)
        //{
        //    var options = new BuildOptions();
        public static SqlBuilder Where(this SqlBuilder sqlBuilder, bool applyCondition, params string[] conditions)
        {
            if (applyCondition)
                sqlBuilder.WhereConditions.AddRange(conditions);
            return sqlBuilder;
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

            var result = new SqlBuilder(string.Empty);

            result.WithTables.AddRange(subqueries);

            return result;
        }

    }
}
