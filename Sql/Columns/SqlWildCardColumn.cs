namespace HuskyKit.Sql.Columns
{
    public class SqlWildCardColumn : ISqlColumn
    {
        private readonly string Expression;
        public SqlWildCardColumn(bool AllColumns = false)
        {
            Order.Reset();
            Aggregate = false;
            Expression = AllColumns ? "*" : "[{0}].*";
        }

        public override string GetSelectExpression(BuildContext context)
        {
            return string.Format(Expression, context.CurrentTableAlias);
        }

        public override string GetSqlExpression(BuildContext context, int targetIndex = 0)
        {
            return string.Format(Expression, context.TableAlias.ElementAt(targetIndex));
        }
    }
}
