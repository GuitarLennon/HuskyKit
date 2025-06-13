using HuskyKit.Extensions;
using HuskyKit.Sql.Columns;
using System.Text;

namespace HuskyKit.Sql.Sources
{
    public partial class SqlBuilder : ISqlSource
    {

       
        public SqlBuilder Join(params TableJoin[] tableJoins)
        {
            Joins.AddRange(tableJoins);

            return this;
        }

        protected IEnumerable<SqlBuilder> GetNestedBuilders(bool includeQueryColumns)
        {
            foreach (var table in LocalWithTables)
            {
                foreach (var subtable in table.GetNestedBuilders(includeQueryColumns))
                    if (subtable.From_Source != null)
                        yield return subtable;

                if (table.From_Source != null)
                    yield return table;
            }

            GetNestedSubqueries(includeQueryColumns);
        }

        /// <summary>
        /// Appends the FROM clause to the query, including any joins.
        /// </summary>
        /// <param name="sb">The StringBuilder to append the clause to.</param>
        /// <param name="context">The build context for constructing the query.</param>
        private Action<StringBuilder> DetermineFrom(BuildContext context)
        {
            if (From_Source == null)
                return sb => sb.DebugComment("No from rendered");

            var renderFrom = From_Source switch
            {
                SqlBuilder sqlbuilder => RenderSqlBuilderFrom(context, sqlbuilder),
                _ => RenderSimpleFrom(context)
            };

            return sb =>
            {
                renderFrom(sb);
                RenderJoins(sb, context);
            };
        }

        /// <summary>
        /// Appends the WITH clause to the query for CTEs (Common Table Expressions).
        /// </summary>
        /// <param name="sb">The StringBuilder to append the clause to.</param>
        /// <param name="context">The build context for constructing the query.</param>
        private void DetermineWith(StringBuilder sb, BuildContext context, HashSet<SqlBuilder> withTables)
        {
            withTables.Remove(this);

            if (withTables.Count == 0) return;

            sb.DebugComment(nameof(DetermineWith));

            sb.Append(";WITH ");

            int i = 0;
            foreach (var Table in withTables)
            {

                //This table has not been rendered
                if (!context.RenderedTables.Add(Table))
                    continue;

                //if (!Table.NeedsToBeRenderedAsWith)
                //continue;

                string outerTableName = Table.Alias;

                if (context.RenderedTables.Any(x => x != Table && x.Alias == Table.Alias))
                {   //This CTE name has been used
                    sb.Comment($"Table with alias {Table.Alias} already rendered");
                    outerTableName = $"{Table.Alias}-{Table.ID}";
                    sb.Comment($"Renaming to {Table.Alias}");
                }

                if (i++ > 0) sb.Append(',');

                sb.AppendLine($"[{outerTableName}] AS (");

                sb.DebugComment($"NeedsToBeRenderedAsWith: {Table.NeedsToBeRenderedAsWith}");

                using (context.Indent(Table))
                {
                    sb.AppendLine($"{Table.Build(context)}");

                    Table.Alias = outerTableName;
                }

                sb.AppendLine(")");
            }

        }

        private IEnumerable<SqlBuilder> GetNestedSubqueries(bool includeQueryColumns)
        {
            foreach (ISqlColumn SqlColumn in TableColumns)
            {
                if (SqlColumn is SqlQueryColumn query)
                {
                    foreach (var subtable in query.SqlBuilder.GetNestedBuilders(includeQueryColumns))
                    {
                        if (subtable.From_Source != null /* && subtable.WhereConditions.Any() */)
                            yield return subtable;
                    }
                    //if (includeQueryColumns)
                    yield return query.SqlBuilder;
                }
            }
        }

        private void RenderJoins(StringBuilder sb, BuildContext context)
        {
            foreach (var join in Joins)
            {
                using (context.Indent(join.SqlSource))
                {
                    sb.AppendLine(join.GetSqlExpression(context));
                }
            }
        }

        private Action<StringBuilder> RenderSimpleFrom(BuildContext context)
        {
            return sb =>
            {
                sb.Append(context.IndentToken);
                sb.DebugComment($"RenderSimpleFrom({ID})");
                sb.AppendLine($"FROM {From_Source!.Build(context)}");
            };
        }

        private Action<StringBuilder> RenderSqlBuilderFrom(BuildContext context, SqlBuilder sqlbuilder)
        {
            bool isFirstRender = context.RenderedTables.Add(sqlbuilder);

            if (isFirstRender)
            {
                return sb =>
                {
                    sb.Append(context.IndentToken);
                    sb.DebugComment($"RenderSqlBuilderFrom({ID}->{sqlbuilder.ID}) FirstRender");

                    sb.AppendLine("FROM (");
                    using (context.Indent(sqlbuilder))
                    {
                        sb.AppendLine(sqlbuilder.Build(context));
                    }
                    sb.AppendLine($"{context.IndentToken}) AS [{sqlbuilder.Alias}]");
                };
            }
            else
            {
                return sb =>
                {
                    sb.Append(context.IndentToken);
                    sb.DebugComment($"RenderSqlBuilderFrom({ID}->{sqlbuilder.ID}) Reuse");

                    string aliasPart = sqlbuilder.Alias == context.CurrentTableAlias
                        ? $"[{sqlbuilder.Alias}]"
                        : $"[{sqlbuilder.Alias}] AS [{context.CurrentTableAlias}]";

                    sb.AppendLine($"FROM {aliasPart}");
                };
            }
        }
    }
}
