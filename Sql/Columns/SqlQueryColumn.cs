using HuskyKit.Extensions;
using HuskyKit.Sql.Sources;
using System.Text;

namespace HuskyKit.Sql.Columns
{
    /// <summary>
    /// Represents a column derived from a SQL subquery, providing methods to generate SQL expressions.
    /// </summary>
    public class SqlQueryColumn : ISqlColumn
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
            ColumnOrder? order = default,
            ForJsonOptions? forJson = null,
            int? skip = null,
            int? length = null
        )
        {
            SqlBuilder = sqlBuilder;
            raw_name = null;
            show_name = AsAlias;
            Order.Apply(order);

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
        public override string GetSqlExpression(BuildContext context)
        {
            var sb = new StringBuilder();

            sb.AppendLine("(");

            sb.DebugComment($"{nameof(SqlQueryColumn)}.{nameof(GetSqlExpression)} (CurrentTableAlias:{context.CurrentTableAlias}, Name:{Name})");

            var options = context.CurrentOptions.Clone(ForJson, Skip, Length);

            context.Indent(SqlBuilder.Alias, options);

            sb.Append(SqlBuilder.Build(context)).Replace("\n", "\n   ");

            context.Unindent();

            sb.Append(context.IndentToken);

            sb.DebugComment($"-->{nameof(SqlQueryColumn)}.{nameof(GetSqlExpression)}");

            sb.Append(')');

            //sb.Append($") AS [{Name}]");

            return sb.ToString();
        }

        /// <summary>
        /// Generates the SQL WHERE expression for the column based on a predicate.
        /// </summary>
        /// <param name="TableAlias">The alias of the table containing the column.</param>
        /// <param name="predicate">The predicate to apply in the WHERE clause.</param>
        /// <param name="context">The build context for constructing the query.</param>
        /// <returns>The WHERE SQL expression for the column.</returns>
        public override string GetWhereExpression(BuildContext context)
        {
            context.Indent(SqlBuilder.Alias, context.CurrentOptions.Clone(ForJson));

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
        public override string GetSelectExpression(BuildContext context)
            => $"{GetSqlExpression(context)} AS [{Name}]";
    }
}
