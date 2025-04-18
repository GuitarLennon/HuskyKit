using HuskyKit.Sql.Columns;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HuskyKit.Sql.Sources
{
    public partial class SqlBuilder : ISqlSource
    {

        public SqlBuilder Where<T>(bool condition, Func<SqlBuilder, ISqlColumn> sqlColumnSelector, T? Value, SQLOperator @operator = SQLOperator.AutoEquals)
        {
            if (condition)
                Where(sqlColumnSelector, Value, @operator);

            return this;
        }

        public SqlBuilder Where(IEnumerable<Func<BuildContext, string>> WhereConditions)
        {
            LocalWhereConditions.AddRange(WhereConditions);
            return this;
        }

        public SqlBuilder Where<T>(Func<SqlBuilder, ISqlColumn> sqlColumnSelector, T? Value, SQLOperator @operator = SQLOperator.AutoEquals)
        {
            string sqloperator;

            if (Value is not string && Value is IEnumerable E)
            {
                sqloperator = @operator.GetOperator(true);
            }
            else
            {
                sqloperator = @operator.GetOperator(false);
            }

            LocalWhereConditions.Add((BuildContext x) =>
            {
                var g = $"{sqlColumnSelector(this).GetWhereExpression(x)} {sqloperator} {Value.GetParameterValue()}";
                return g;
            });

            return this;
        }

        public SqlBuilder WhereParameter<T>(Func<SqlBuilder, ISqlColumn> sqlColumnSelector, T? Value, SQLOperator @operator = SQLOperator.AutoEquals)
        {
            IEnumerable<object?> enumerators;
            List<string> Keys = [];

            if (Value is not string && Value is IEnumerable E)
                enumerators = E.Cast<object>();
            else
                enumerators = [Value];

            foreach (var item in enumerators)
            {
                var id = $"Pb{this.ID}i{LocalParameters.Count}";
                LocalParameters.Add(id, item);
                Keys.Add("@" + id);
            }


            var predicate = string.Format(@operator.GetOperatorPredicate(Value), string.Join(',', Keys));

            LocalWhereConditions.Add((BuildContext x) =>
                $"{sqlColumnSelector(this).GetWhereExpression(x)} {predicate}");

            return this;
        }

        public SqlBuilder WhereNot<T>(Func<SqlBuilder, ISqlColumn> sqlColumnSelector, T? Value)
        {
            var id = $"@Pb{this.ID}i{LocalParameters.Count}";
            LocalParameters.Add(id, Value);
            LocalWhereConditions.Add((BuildContext x) =>
            {
                var expression1 = string.Format(sqlColumnSelector(this).GetWhereExpression(x), x.TableAlias.ToArray());
                return $"{expression1} {SQLOperator.AutoDiffers.GetOperatorPredicate(Value)} @{id}";
            });
            return this;
        }

        public SqlBuilder WhereIsNotNull(Func<SqlBuilder, ISqlColumn> sqlColumnSelector)
        {

            LocalWhereConditions.Add((BuildContext x) =>
            {
                var expression1 = string.Format(sqlColumnSelector(this).GetWhereExpression(x), x.TableAlias.ToArray());
                return $"{expression1} IS NOT NULL";
            });
            return this;
        }

    }

}
