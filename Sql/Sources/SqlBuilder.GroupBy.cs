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
    // Propiedades
    public partial class SqlBuilder : ISqlSource
    {


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
                .Select(c => c.Column.GetGroupByExpression(context));

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

    }
}
