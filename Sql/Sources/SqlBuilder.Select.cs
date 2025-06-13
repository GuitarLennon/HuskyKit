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
        /// Appends the SELECT clause to the query, including optional TOP clause.
        /// </summary>
        /// <param name="sb">The StringBuilder to append the clause to.</param>
        /// <param name="topSql">The TOP clause for limiting the number of rows.</param>
        /// <param name="context">The build context for constructing the query.</param>
        private void DetermineSelect(StringBuilder sb, string topSql, BuildContext context)
        {
            sb.Append(context.IndentToken);

            sb.DebugComment(nameof(DetermineSelect));

            sb.Append($"SELECT ");

            bool first = true;

            if (!string.IsNullOrEmpty(topSql))
            {
                sb.AppendLine(topSql);
            }
            else
            {
                sb.AppendLine();
            }


            foreach (var column in TableColumns)
            {
                if (!column.IsSelectable)
                    continue;

                sb.Append(context.IndentToken)
                    .Append(first, "   ")
                    .Append(!first, "  ,")
                    .AppendLine(column.GetSelectExpression(context));

                first = false;
            }

            foreach (var tableJoin in Joins)
                using (context.Indent(tableJoin.SqlSource))
                {
                    foreach (var column in tableJoin.Columns)
                    {
                        if (!column.IsSelectable)
                            continue;

                        sb.Append(context.IndentToken)
                            .Append(first, "   ")
                            .Append(!first, "  ,")
                            .AppendLine(column.GetSelectExpression(context));

                        first = false;
                    }
                }
        }

    }
}
