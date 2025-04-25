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

            IsAggregate = false;
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
        public override string GetSqlExpression(BuildContext context, int targetIndex = 0)
        {
            var sb = new StringBuilder();

            sb.AppendLine("(");

            sb.DebugComment($"{nameof(SqlQueryColumn)}.{nameof(GetSqlExpression)} (CurrentTableAlias:{context.CurrentTableAlias}, Name:{Name})");

            using (context.Indent(SqlBuilder, context.CurrentOptions.Clone(ForJson, Skip, Length)))
            {
                sb.Append(SqlBuilder.Build(context)).Replace("\n", "\n   ");
            };

            sb.Append(context.IndentToken);

            sb.DebugComment($"-->{nameof(SqlQueryColumn)}.{nameof(GetSqlExpression)}");

            sb.Append(')');

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
            using (context.Indent(SqlBuilder, context.CurrentOptions.Clone(ForJson)))
            {
                var sql = "(" + SqlBuilder.Build(context) + ")";

                return sql;
            }
        }

        /// <summary>
        /// Generates the SQL SELECT expression for the column, including the alias if specified.
        /// </summary>
        /// <param name="TableAlias">The alias of the table containing the column.</param>
        /// <param name="context">The build context for constructing the query.</param>
        /// <returns>The SELECT SQL expression for the column.</returns>
        public override string GetSelectExpression(BuildContext context)
            => $"{GetSqlExpression(context)} AS [{Name}]";


        public SqlQueryColumn JoinEnvolvingTable(
           Func<SqlBuilder, ISqlColumn> ColumnSelector
        )
        {
            return JoinEnvolvingTable(
                (SqlBuilder x) => [ColumnSelector(x)],
                (SqlBuilder x) => [ColumnSelector(x)]
            );
        }

        public SqlQueryColumn JoinEnvolvingTable(
           Func<SqlBuilder, ISqlColumn> LeftHand,
           Func<SqlBuilder, ISqlColumn> RightHand
        )
        {
            return JoinEnvolvingTable(
                (SqlBuilder x) => [LeftHand(x)],
                (SqlBuilder x) => [LeftHand(x)]
            );
        }

        public SqlQueryColumn JoinEnvolvingTable(
            Func<SqlBuilder, IEnumerable<ISqlColumn>> innerSelector,
            Func<SqlBuilder, IEnumerable<ISqlColumn>> outerSelector
        )
        {
            SqlBuilder.LocalWhereConditions.Add((BuildContext context) =>
            {
                var outerSqlBuilder = context.Sources.ElementAtOrDefault(1) is SqlBuilder b ? b : 
                                    new SqlBuilderResolver(context.CurrentTableAlias);

                var innerColumns = innerSelector(SqlBuilder);
                var outerColumns = outerSelector(outerSqlBuilder);

                List<string> predicates = [];

                if (innerColumns.Count() != outerColumns.Count())
                    throw new InvalidOperationException("Diferente número de columnas");

                for (var i = 0; i < innerColumns.Count(); i++)
                {
                    var left_predicate = innerColumns
                        .ElementAt(i)
                        .GetSqlExpression(context, 0);

                    var right_predicate = outerColumns  
                        .ElementAt(i)
                        .GetSqlExpression(context, 0 + 1);

                    predicates.Add($"{left_predicate} = {right_predicate}");
                }

                return string.Join(" AND ", [.. predicates]);
            });
            return this;
        }
    }
}
