namespace HuskyKit.Sql.Sources
{
    public class PredicateClause(string predicate) : ISqlExpression
    {
        public virtual string Predicate => predicate;

        public string GetSqlExpression(BuildContext context, int index)
            => predicate;

    }
}
