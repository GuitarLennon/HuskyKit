using System.Text;

namespace HuskyKit.Sql.Sources
{
    public class SetOperation(SetOperator setOperation, SqlBuilder sqlBuilder) : ISqlExpression
    {
        public SetOperator SetOperator { get; } = setOperation;

        public SqlBuilder SqlSource { get; } = sqlBuilder;

        public string GetSqlExpression(BuildContext context, int targetindex = 0)
        {
            StringBuilder sb = new();

            sb.Append(context.IndentToken);
            sb.AppendLine(SetOperator.ToString().Replace('_', ' '));

            //sb.Append(context.IndentToken);

            sb.AppendLine(SqlSource.Build(context));

            return sb.ToString();
        }
    }
}
