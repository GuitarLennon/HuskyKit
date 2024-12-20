﻿using HuskyKit.Sql.Columns;
using System.Text;

namespace HuskyKit.Sql.Sources
{

    public class PredicateClause(string predicate) : ISqlExpression
    {
        public virtual string Predicate => predicate;

        public string GetSqlExpression(BuildContext context)
        {
            return predicate;
        }
    }

    public class DynamicPredicate(
        Func<SqlBuilder, IEnumerable<ISqlColumn>> LeftHand,
        Func<SqlBuilder, IEnumerable<ISqlColumn>> RightHand
    ) : ISqlExpression
    {
        public string GetSqlExpression(BuildContext context)
        {
            var left = RightHand(context.RenderedTables.First(x => x.Alias == context.TableAlias.ElementAt(1)));
            var right = LeftHand(context.RenderedTables.First(x => x.Alias == context.CurrentTableAlias));
            List<string> predicates = [];

            if (left.Count() != right.Count())
                throw new InvalidOperationException("Diferente número de columnas");

            for (var i = 0; i < left.Count(); i++)
            {
                predicates.Add($"{left.ElementAt(i).GetSqlExpression(context)} AND {right.ElementAt(i).GetSqlExpression(context)}");
            }

            return string.Join(" AND ", [.. predicates]);
        }
    }

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
         
        public string GetSqlExpression(BuildContext context)
        {
            context.Indent(Alias);

            var returning = string.Format(Predicate.GetSqlExpression(context), context.TableAlias);

            context.Unindent();

            return returning;
        }
    }
}
