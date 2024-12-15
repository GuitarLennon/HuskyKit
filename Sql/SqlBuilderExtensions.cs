namespace HuskyKit.Sql
{
    /// <summary>
    /// Provides extension methods for building SQL queries using the SqlBuilder.
    /// </summary>
    public static partial class SqlBuilderExtensions
    {
        /// <summary>
        /// Wraps the builder as a subquery with a given alias.
        /// </summary>
        public static SqlBuilder AsSubquery(this SqlBuilder builder, string alias = "sq")
            => new SqlBuilder(builder, alias);

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
        /// Creates an aggregate SQL column with an alias and optional ordering.
        /// </summary>
        public static SqlColumn Aggregate(this object expression, string alias, ColumnOrder order = default)
            => new(expression, alias, true, order);

        /// <summary>
        /// Creates a SQL column with an alias, optional aggregation, and ordering.
        /// </summary>
        public static SqlColumn As(this object expression, string alias, bool aggregate = false, ColumnOrder order = default)
            => new(expression, alias, aggregate, order);

        /// <summary>
        /// Converts a SqlBuilder into a SQL query column with alias and ordering.
        /// </summary>
        public static SqlQueryColumn AsColumn(this SqlBuilder sqlBuilder, string alias, ColumnOrder order, ForJsonOptions? forJson = default)
            => new SqlQueryColumn(sqlBuilder, alias, order, forJson);

        /// <summary>
        /// Converts a SqlBuilder into a SQL query column with alias.
        /// </summary>
        public static SqlQueryColumn AsColumn(this SqlBuilder sqlBuilder, string alias, ForJsonOptions? forJson)
            => new SqlQueryColumn(sqlBuilder, alias, default, forJson);

        /// <summary>
        /// Converts a SqlBuilder into a SQL query column with alias, ordering, pagination, and JSON options.
        /// </summary>
        public static SqlQueryColumn AsColumn(this SqlBuilder sqlBuilder, string alias, ColumnOrder order = default, int? skip = null, int? take = null, ForJsonOptions? forJson = default)
            => new SqlQueryColumn(sqlBuilder, alias, order, forJson, skip, take);

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
        /// Sets the source table and alias for the SQL query.
        /// </summary>
        public static SqlBuilder From(this SqlBuilder sqlBuilder, (string Schema, string Table, string Alias) from)
        {
            sqlBuilder.Table = $"[{from.Schema}].[{from.Table}]";
            sqlBuilder.Alias = from.Alias;
            return sqlBuilder;
        }

        public static SqlBuilder From(this SqlBuilder sqlBuilder, (string Schema, string Table) from)
            => sqlBuilder.From((from.Schema, from.Table, from.Table));

        public static SqlBuilder From(this SqlBuilder sqlBuilder, string table, string? alias = null)
        {
            sqlBuilder.Table = table;
            sqlBuilder.Alias = alias ?? table;
            return sqlBuilder;
        }

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
        /// Adds a natural join between two tables based on common columns.
        /// </summary>
        public static SqlBuilder NaturalJoin(this SqlBuilder sqlBuilder, SqlBuilder other, JoinTypes joinType = JoinTypes.INNER, params SqlColumnAbstract[] columns)
        {
            var options = new SqlBuilder.BuildOptions();

            var predicate = sqlBuilder.TableColumns
                .Join(other.TableColumns, x => x.Name, y => y.Name, (x, y) => $"{x.GetSqlExpression(sqlBuilder.Alias, options)} = {y.GetSqlExpression(other.Alias, options)}")
                .Aggregate((a, b) => $"{a} AND {b}");

            return sqlBuilder.Join(joinType, other, predicate, columns);
        }

        public static SqlBuilder NaturalJoin(this SqlBuilder sqlBuilder, (string Schema, string Table, string? Alias) other, JoinTypes joinType = JoinTypes.INNER, params SqlColumn[] columns)
        {
            var options = new SqlBuilder.BuildOptions();

            var otherBuilder = SqlBuilder.SelectFrom((other.Schema, other.Table, other.Alias ?? other.Table)).Select(columns);

            string predicate = joinType != JoinTypes.CROSS
                ? sqlBuilder.TableColumns
                    .Join(otherBuilder.TableColumns, x => x.Name, y => y.Name, (x, y) => $"{x.GetSqlExpression(sqlBuilder.Alias, options)} = {y.GetSqlExpression(otherBuilder.Alias, options)}")
                    .Aggregate((a, b) => $"{a} AND {b}")
                : string.Empty;

            return sqlBuilder.Join(joinType, other, predicate, columns);
        }

        /// <summary>
        /// Orders a column with an index and direction.
        /// </summary>
        public static SqlColumn OrderBy(this string columnName, int index, OrderDirection order = OrderDirection.ASC)
            => new(columnName, columnName, false, new ColumnOrder { Index = index, Direction = order });

        public static SqlBuilder OrderBy(this SqlBuilder sqlBuilder, params ColumnOrder[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                var column = sqlBuilder.ColumnsWithAlias.FirstOrDefault(x => x.Alias == values[i].Column);
                if (column.Column != null)
                    column.Column.Order = (i, column.Column.Name ?? throw new ArgumentNullException(nameof(values)), values[i].Direction);
            }
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

        /// <summary>
        /// Paginates the SQL query by setting the take and skip values.
        /// </summary>
        public static SqlBuilder Paginate(this SqlBuilder builder, int? take, int? skip)
        {
            builder.Length = take;
            builder.Skip = skip;
            return builder;
        }

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
        /// Adds columns to the SELECT clause.
        /// </summary>
        public static SqlBuilder Select(this SqlBuilder sqlBuilder, params SqlColumnAbstract[] columns)
        {
            sqlBuilder.TableColumns.AddRange(columns);
            return sqlBuilder;
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

        /// <summary>
        /// Selects all columns.
        /// </summary>
        public static SqlBuilder SelectAll(this SqlBuilder sqlBuilder)
        {
            sqlBuilder.TableColumns.Add(SqlColumn.All());
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

        public static SqlBuilder Where(this SqlBuilder sqlBuilder, bool applyCondition, params string[] conditions)
        {
            if (applyCondition)
                sqlBuilder.WhereConditions.AddRange(conditions);
            return sqlBuilder;
        }
    }
}
