using HuskyKit.Sql.Columns;
using System.Text;

namespace HuskyKit.Sql.Sources
{
    /// <summary>
    /// Represents a join between tables in an SQL query, including the join type, target table, and optional columns.
    /// </summary>
    public class TableJoin : ISqlExpression
    {
        /// <summary>
        /// Gets or sets the ON condition for the join.
        /// </summary>
        public ISqlExpression Predicate { get; set; }

        /// <summary>
        /// Gets or sets the alias for the target table.
        /// </summary>
        public string Alias => SqlSource.Alias;

        public ISqlSource SqlSource { get; set; }


        /// <summary>
        /// Gets the list of columns included in the join.
        /// </summary>
        public List<ISqlColumn> Columns { get; protected set; } = [];

        /// <summary>
        /// Gets or sets the type of join (e.g., INNER JOIN, LEFT JOIN).
        /// </summary>
        public JoinTypes JoinType { get; set; }


        public TableJoin(JoinTypes joinTypes,
                         ISqlSource sqlSource,
                         Func<SqlBuilder, IEnumerable<ISqlColumn>> leftHand,
                         Func<SqlBuilder, IEnumerable<ISqlColumn>> rightHand,
                         params ISqlColumn[] columns)
        {
            this.JoinType = joinTypes;
            this.SqlSource = sqlSource;
            Predicate = new DynamicPredicate(leftHand, rightHand);
            Columns = [.. columns];
        }

        public TableJoin(JoinTypes joinTypes,
                       ISqlSource sqlSource,
                       Func<SqlBuilder, IEnumerable<ISqlColumn>> columnSelector,
                       params ISqlColumn[] columns)
        {
            this.JoinType = joinTypes;
            this.SqlSource = sqlSource;
            Predicate = new DynamicPredicate(columnSelector, columnSelector);
            Columns = [.. columns];
        }

        public TableJoin(JoinTypes joinTypes,
                 ISqlSource sqlSource,
                 string predicate,
                 params ISqlColumn[] columns)
        {
            this.JoinType = joinTypes;
            this.SqlSource = sqlSource;
            Predicate = new PredicateClause(predicate);
            Columns = [.. columns];
        }

        public string GetSqlExpression(BuildContext context, int targetindex = 0)
        {
            string returning = $"{context.IndentToken}{JoinType} JOIN ";

            using (context.Indent(SqlSource))
            {
                if (SqlSource is SqlBuilder b)
                    if (context.RenderedTables.Add(b))
                    {
                        returning += $"[{Alias}]";
                    }
                    else
                    {
                        returning += $"({b.Build(context)}) as [{Alias}]";
                    }
                else
                {
                    returning += SqlSource.Build(context);
                }

                returning += " ON " + string.Format(Predicate.GetSqlExpression(context, targetindex), context.TableAlias);

            }

            return returning;
        }
    }
}
