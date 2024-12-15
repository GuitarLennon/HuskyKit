using System.Text;

namespace HuskyKit.Sql
{
    /// <summary>
    /// Represents a column derived from a SQL subquery, providing methods to generate SQL expressions.
    /// </summary>
    public class SqlQueryColumn : SqlColumnAbstract
    {
        /// <summary>
        /// Gets or sets the <see cref="SqlBuilder"/> used to build the subquery for this column.
        /// </summary>
        public SqlBuilder SqlBuilder { get; set; }

        /// <summary>
        /// Gets or sets the JSON formatting options for the subquery.
        /// </summary>
        public ForJsonOptions? ForJson { get; set; }

        /// <summary>
        /// Gets or sets the number of rows to skip in the subquery.
        /// </summary>
        public int? Skip { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of rows to return in the subquery.
        /// </summary>
        public int? Length { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlQueryColumn"/> class.
        /// </summary>
        /// <param name="sqlBuilder">The <see cref="SqlBuilder"/> instance representing the subquery.</param>
        /// <param name="AsAlias">The alias for the column in the query.</param>
        /// <param name="order">The order configuration for the column.</param>
        /// <param name="forJson">Optional JSON formatting options.</param>
        /// <param name="skip">Optional number of rows to skip in the subquery.</param>
        /// <param name="length">Optional maximum number of rows to return in the subquery.</param>
        public SqlQueryColumn(
            SqlBuilder sqlBuilder,
            string AsAlias,
            ColumnOrder order = default,
            ForJsonOptions? forJson = null,
            int? skip = null,
            int? length = null
        )
        {
            SqlBuilder = sqlBuilder;
            raw_name = null;
            show_name = AsAlias;
            Order = order;
            Aggregate = false;
            ForJson = forJson;
            Skip = skip;
            Length = length;
        }

        /// <summary>
        /// Generates the SQL expression for the column, including the subquery and formatting options.
        /// </summary>
        /// <param name="TableAlias">The alias of the table containing the column.</param>
        /// <param name="context">The build context for constructing the query.</param>
        /// <returns>The SQL expression for the column.</returns>
        public override string GetSqlExpression(string TableAlias, BuildContext context)
        {
            var sb = new StringBuilder();

            sb.AppendLine("(");

            var options = context.CurrentOptions.Clone(ForJson, Skip, Length);

            context.Indent(options);

            sb.AppendLine(SqlBuilder.Build(context));

            sb.Append(context.IndentToken);

            sb.Append(')');

            context.Unindent();

            return sb.ToString();
        }

        /// <summary>
        /// Returns an empty string as grouping is not applicable for subquery columns.
        /// </summary>
        /// <param name="TableAlias">The alias of the table containing the column.</param>
        /// <param name="options">The build context for constructing the query.</param>
        /// <returns>An empty string.</returns>
        public override string GetGroupByExpression(string TableAlias, BuildContext options)
        {
            return string.Empty;
        }

        /// <summary>
        /// Generates the SQL WHERE expression for the column based on a predicate.
        /// </summary>
        /// <param name="TableAlias">The alias of the table containing the column.</param>
        /// <param name="predicate">The predicate to apply in the WHERE clause.</param>
        /// <param name="context">The build context for constructing the query.</param>
        /// <returns>The WHERE SQL expression for the column.</returns>
        public override string GetWhereExpression(string TableAlias, string predicate, BuildContext context)
        {
            context.Indent(context.CurrentOptions.Clone(ForJson));

            var sql = "(" + SqlBuilder.Build(context) + ")";

            context.Unindent();

            return sql;
        }

        /// <summary>
        /// Generates the SQL SELECT expression for the column, including the alias if specified.
        /// </summary>
        /// <param name="TableAlias">The alias of the table containing the column.</param>
        /// <param name="context">The build context for constructing the query.</param>
        /// <returns>The SELECT SQL expression for the column.</returns>
        public override string GetSelectExpression(string TableAlias, BuildContext context)
            =>
            (show_name == raw_name) ? GetSqlExpression(TableAlias, context) :
            $"{GetSqlExpression(TableAlias, context)} AS [{Name}]";
    }
}
