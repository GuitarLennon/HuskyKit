using System.Text;

/// <summary>
/// Core class for building SQL queries dynamically.
/// </summary>
namespace HuskyKit.Sql
{
    public partial class SqlBuilder : ISqlSource
    {
        private string? m_alias;

        /// <summary>
        /// Initializes a new instance of the SqlBuilder class with a source, alias, and optional columns.
        /// </summary>
        /// <param name="source">The SQL source object used to build the query.</param>
        /// <param name="alias">The alias for the SQL source in the query.</param>
        /// <param name="columns">Optional array of SqlColumn objects to include in the query.</param>
        internal SqlBuilder(ISqlSource source, string alias, params SqlColumn[] columns)
        {
            From = source;
            Alias = alias;

            if (source is SqlBuilder builder)
            {
                WithTables.Add(builder);
                TableColumns.AddRange(builder.ColumnsWithAlias.Select(c => new SqlColumn(rawName: c.Column.Name ?? throw new InvalidOperationException())));
            }

            TableColumns.AddRange(columns);
        }

        /// <summary>
        /// Initializes a new instance of the SqlBuilder class with an SQL source.
        /// </summary>
        /// <param name="sqlSource">The SQL source object.</param>
        internal SqlBuilder(ISqlSource sqlSource)
        {
            Alias = sqlSource.Alias;
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

        /// <summary>
        /// Gets or sets the alias for the SQL source.
        /// </summary>
        public string Alias { get => m_alias ?? GetHashCode().ToString(); set => m_alias = value; }

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
        /// Gets or sets the pre-query options (e.g., additional SQL clauses).
        /// </summary>
        public string? PreQueryOptions { get; set; }

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
        public List<SqlColumnAbstract> TableColumns { get; internal set; } = [];

        /// <summary>
        /// Gets the list of WHERE conditions applied to the query.
        /// </summary>
        public List<string> WhereConditions { get; } = [];

        /// <summary>
        /// Gets the columns with their associated aliases.
        /// </summary>
        internal IEnumerable<(string Alias, SqlColumnAbstract Column)> ColumnsWithAlias =>
            Joins.SelectMany(j => j.Columns.Select(c => (j.Alias, Column: c)))
                 .Union(TableColumns.Select(c => (Alias, Column: c)));

        /// <summary>
        /// Gets the list of subqueries used in the query.
        /// </summary>
        internal List<SqlBuilder> WithTables { get; } = [];

        /// <summary>
        /// Builds the SQL query string based on the provided options.
        /// </summary>
        /// <param name="options">The build options for the query.</param>
        /// <returns>The SQL query string.</returns>
        public string Build(BuildOptions? options = default)
        {
            var context = new BuildContext(options ?? new BuildOptions());

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
            if (!ColumnsWithAlias.Any())
                throw new InvalidOperationException($"No columns specified in [{From}] [{Alias}]");

            var sb = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(PreQueryOptions))
                sb.AppendLine(PreQueryOptions);

            DetermineWith(sb, context);

            var (topSql, orderSql) = DetermineOffset(context);

            DetermineSelect(sb, topSql, context);
            DetermineFrom(sb, context);
            DetermineWhere(sb);
            DetermineGroupBy(sb, context);

            if (!string.IsNullOrWhiteSpace(orderSql))
                sb.Append(orderSql);

            if (context.CurrentOptions.ForJson.HasValue)
                AppendForJson(sb, context.CurrentOptions.ForJson.Value, context.CurrentOptions.Indentation);

            if (!string.IsNullOrWhiteSpace(QueryOptions))
                sb.Append(context.IndentToken).Append(QueryOptions);

            return sb.ToString();
        }

        /// <summary>
        /// Returns the SQL query string representation of the SqlBuilder object.
        /// </summary>
        /// <returns>The SQL query string.</returns>
        public override string ToString() => Build();


        /// <summary>
        /// Recursively retrieves all SqlBuilder instances used in WITH clauses, including the current instance.
        /// </summary>
        /// <returns>An enumerable of SqlBuilder instances.</returns>
        protected IEnumerable<SqlBuilder> GetWithTableBuilders()
            => WithTables.SelectMany(t => t.GetWithTableBuilders()).Append(this);

        /// <summary>
        /// Appends a FOR JSON clause to the query with the specified options.
        /// </summary>
        /// <param name="sb">The StringBuilder to append the clause to.</param>
        /// <param name="forJson">The JSON options to apply (e.g., INCLUDE NULL VALUES).</param>
        /// <param name="indent">The indentation string for formatting the JSON output.</param>
        private static void AppendForJson(StringBuilder sb, ForJsonOptions forJson, string indent)
        {
            string options = string.Empty;

            if (forJson.HasFlag(ForJsonOptions.INCLUDE_NULL_VALUES))
                options += " INCLUDE NULL VALUES";

            if (forJson.HasFlag(ForJsonOptions.WITHOUT_ARRAY_WRAPPER))
                options += " INCLUDE NULL VALUES";

            if (options.Length > 0)
                options = ", " + options;

            if (forJson.HasFlag(ForJsonOptions.PATH))
                sb.AppendLine($"{indent}FOR JSON PATH{options}");
            else
                sb.AppendLine($"{indent}FOR JSON AUTO{options}");
        }

        /// <summary>
        /// Appends the FROM clause to the query, including any joins.
        /// </summary>
        /// <param name="sb">The StringBuilder to append the clause to.</param>
        /// <param name="context">The build context for constructing the query.</param>
        private void DetermineFrom(StringBuilder sb, BuildContext context)
        {
            if (From != null)
            {
                sb.AppendLine(From.Build(context));

                foreach (var join in Joins)
                {
                    sb.AppendLine(join.GetSqlExpression(Alias, context.IndentToken));
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
            var groupByColumns = ColumnsWithAlias
                .Where(c => !c.Column.Aggregate)
                .Select(c => c.Column.GetGroupByExpression(c.Alias, context));

            if (!groupByColumns.Any()) return;

            sb.Append("GROUP BY ");
            sb.Append(string.Join(", ", groupByColumns));
        }

        /// <summary>
        /// Determines and constructs the OFFSET and ORDER BY clauses for the query.
        /// </summary>
        /// <param name="context">The build context for constructing the query.</param>
        /// <returns>A tuple containing the TOP SQL clause and the ORDER BY SQL clause.</returns>
        private (string topSql, string orderSql) DetermineOffset(BuildContext context)
        {
            var topSql = string.Empty;
            var orderSql = string.Empty;
            var options = context.CurrentOptions;

            if (ColumnsWithAlias.Any(c => c.Column.Order.Direction != OrderDirection.NONE))
            {
                orderSql = "ORDER BY " + string.Join(", ", ColumnsWithAlias
                    .Where(c => c.Column.Order.Direction != OrderDirection.NONE)
                    .OrderBy(c => c.Column.Order.Index)
                    .Select(c => c.Column.GetOrderByExpression(c.Alias, context)));

                if (options.Skip.HasValue)
                {
                    orderSql += options.Length.HasValue
                        ? $" OFFSET {options.Skip.Value} ROWS FETCH NEXT {options.Length.Value} ROWS ONLY"
                        : $" OFFSET {options.Skip.Value} ROWS";
                }
                else if (options.Length.HasValue)
                {
                    topSql = $"TOP({options.Length.Value})";
                }
            }
            else if (options.Length.HasValue)
            {
                topSql = $"TOP({options.Length.Value})";
            }

            return (topSql, orderSql);
        }

        /// <summary>
        /// Appends the SELECT clause to the query, including optional TOP clause.
        /// </summary>
        /// <param name="sb">The StringBuilder to append the clause to.</param>
        /// <param name="topSql">The TOP clause for limiting the number of rows.</param>
        /// <param name="context">The build context for constructing the query.</param>
        private void DetermineSelect(StringBuilder sb, string topSql, BuildContext context)
        {
            sb.Append("SELECT ");

            if (!string.IsNullOrEmpty(topSql))
                sb.AppendLine(topSql);

            sb.Append(string.Join(", ", ColumnsWithAlias.Select(c => c.Column.GetSelectExpression(c.Alias, context))));
        }

        /// <summary>
        /// Appends the WHERE clause to the query based on the specified conditions.
        /// </summary>
        /// <param name="sb">The StringBuilder to append the clause to.</param>
        private void DetermineWhere(StringBuilder sb)
        {
            if (WhereConditions.Count == 0) return;

            sb.Append("WHERE ");
            sb.Append(string.Join(" AND ", WhereConditions.Select(c => string.Format(c, Alias))));
        }

        /// <summary>
        /// Appends the WITH clause to the query for CTEs (Common Table Expressions).
        /// </summary>
        /// <param name="sb">The StringBuilder to append the clause to.</param>
        /// <param name="context">The build context for constructing the query.</param>
        private void DetermineWith(StringBuilder sb, BuildContext context)
        {
            if (context.Depth > 1)
                return;

            var withTables = GetWithTableBuilders().ToHashSet();

            if (withTables.Count == 0) return;

            sb.Append(";WITH ");

            int i = 0;
            foreach (var Table in withTables)
            {
                if (i++ > 0) sb.Append(',');

                sb.AppendLine($"[{Table.Alias}] AS (");

                context.Indent();

                sb.AppendLine($"{Table.Build(context)})");

                context.Unindent();

                sb.AppendLine(")");
            }
        }
    }
}
