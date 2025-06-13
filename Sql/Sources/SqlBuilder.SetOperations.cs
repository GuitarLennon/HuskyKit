using HuskyKit.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HuskyKit.Sql.Sources
{
    public partial class SqlBuilder
    {

        private void _DetermineSetOperation(StringBuilder sb, BuildContext context)
        {
            if (SetOperations.Count == 0)
                return;

            sb.DebugComment(nameof(_DetermineSetOperation));
            
            foreach (var item in SetOperations)
            {
                using (context.Indent(item.SqlSource))
                {
                    sb.Append(item.GetSqlExpression(context));
                }
            }
        }

        public SqlBuilder UnionAll(SqlBuilder other)
        {
            SetOperations.Add(new SetOperation(SetOperator.UNION_ALL, other));

            return this;
        }

        public SqlBuilder Union(SqlBuilder other)
        {
            SetOperations.Add(new SetOperation(SetOperator.UNION, other));

            return this;
        }

        public SqlBuilder Except(SqlBuilder other)
        {
            SetOperations.Add(new SetOperation(SetOperator.EXCEPT, other));

            return this;
        }

        public SqlBuilder Intersect(SqlBuilder other)
        {
            SetOperations.Add(new SetOperation(SetOperator.INTERSECT, other));

            return this;
        }
    }
}
