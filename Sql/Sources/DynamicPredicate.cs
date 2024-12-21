using HuskyKit.Sql.Columns;

namespace HuskyKit.Sql.Sources
{
    public class DynamicPredicate(
        Func<SqlBuilder, IEnumerable<ISqlColumn>> LeftHand,
        Func<SqlBuilder, IEnumerable<ISqlColumn>> RightHand
    ) : ISqlExpression
    {
        public string GetSqlExpression(BuildContext context, int targetIndex)
        {
            var rightSqlBuilder = context.CurrentSource is SqlBuilder a ? a : new SqlBuilderResolver(context.CurrentTableAlias);
            var leftSqlBuilder = context.Sources.ElementAtOrDefault(1) is SqlBuilder b ? b : new SqlBuilderResolver(context.CurrentTableAlias);

            var leftColumns = LeftHand(leftSqlBuilder);
            var rightColumns = RightHand(rightSqlBuilder);

            List<string> predicates = [];

            if (leftColumns.Count() != rightColumns.Count())
                throw new InvalidOperationException("Diferente número de columnas");

            for (var i = 0; i < leftColumns.Count(); i++)
            {
                var left_predicate = leftColumns
                    .ElementAt(i)
                    .GetSqlExpression(context, targetIndex);

                var right_predicate = rightColumns
                    .ElementAt(i)
                    .GetSqlExpression(context, targetIndex + 1);

                predicates.Add($"{left_predicate} = {right_predicate}");
            }

            return string.Join(" AND ", [.. predicates]);
        }
    }
}
