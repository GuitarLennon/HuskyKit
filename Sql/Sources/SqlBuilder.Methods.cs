using HuskyKit.Extensions;
using HuskyKit.Sql.Columns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HuskyKit.Sql.Sources
{


    public partial class SqlBuilder : ISqlSource
    {
        private bool NeedsToBeRenderedAsWith =>
                    From_Source is not SqlTable ||
                    Columns.Any(x =>
                        x.Column is not SqlColumn sq ||
                        !sq.IsMappedToColumn ||
                        sq.IsAggregate
                    ) ||
                    SetOperations.Count > 0 ||
                    Length.HasValue ||
                    Skip.HasValue ||
                    OrderByClauses.Any();

        /// <summary>
        /// Builds the SQL query string based on the provided options.
        /// </summary>
        /// <param name="options">The build options for the query.</param>
        /// <returns>The SQL query string.</returns>
        public string Build(BuildOptions? options = default)
        {
            var context = new BuildContext(this, options ?? new BuildOptions());
 
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
            if (!Columns.Any())
                throw new InvalidOperationException($"No columns specified in [{From_Source}] [{Alias}]");

            var sb = new StringBuilder().DebugComment($"Build({ID}, Alias:{Alias}, Depth:{context.Depth})");

            if (context.Depth == 1)
            {
                var nestedTables = GetNestedBuilders(true).ToHashSet();

                DetermineQueryOptions(context, sb, nestedTables);

                DetermineWith(sb, context, nestedTables);
            }


            var (topSql, orderSqlWriter) = _DetermineOffset(context);

            var fromWriter = DetermineFrom(context);

            DetermineSelect(sb, topSql, context);

            fromWriter(sb);

            _DetermineWhere(sb, context);
            DetermineGroupBy(sb, context);

            _DetermineSetOperation(sb, context);


            orderSqlWriter(sb);

            if (context.CurrentOptions.ForJson.HasValue)
                AppendForJson(sb, context.CurrentOptions.ForJson.Value, context.CurrentOptions.Indentation);

            if (!string.IsNullOrWhiteSpace(QueryOptions))
                sb.Append(context.IndentToken)
                    .DebugComment("Query options")
                    .Append(QueryOptions);

            return sb.ToString();
        }

        /// <summary>
        /// Appends a FOR JSON clause to the query with the specified options.
        /// </summary>
        /// <param name="sb">The StringBuilder to append the clause to.</param>
        /// <param name="forJson">The JSON options to apply (e.g., INCLUDE NULL VALUES).</param>
        /// <param name="indent">The indentation string for formatting the JSON output.</param>
        private static void AppendForJson(StringBuilder sb, ForJsonOptions forJson, string indent)
        {
            sb.Append(indent);
            sb.DebugComment($"{nameof(AppendForJson)}({forJson})");

            string options = string.Empty;

            if (forJson.HasFlag(ForJsonOptions.INCLUDE_NULL_VALUES))
                options += " INCLUDE_NULL_VALUES";

            if (forJson.HasFlag(ForJsonOptions.WITHOUT_ARRAY_WRAPPER))
                options += " WITHOUT_ARRAY_WRAPPER";

            if (options.Length > 0)
                options = ", " + options;

            if (forJson.HasFlag(ForJsonOptions.PATH))
                sb.AppendLine($"FOR JSON PATH{options}");
            else
                sb.AppendLine($"FOR JSON AUTO{options}");
        }

        private static void DetermineQueryOptions(BuildContext context, StringBuilder sb, HashSet<SqlBuilder> nestedTables)
        {
            var nestedTablesOptions = nestedTables.SelectMany(x => x.PreQueryOptions).Distinct().ToList();

            if (nestedTablesOptions.Count > 0)
            {
                sb.Append(context.IndentToken)
                    .DebugComment("Prequery options");

                foreach (var item in nestedTablesOptions)
                    sb.AppendLine(item);
            }
        }



    }
}
