using System.Text;

namespace HuskyKit.Sql
{
    public class SqlQueryColumn : SqlColumnAbstract
    {
        public SqlBuilder SqlBuilder { get; set; }

        public bool ForJson { get; set; }

        public SqlQueryColumn(
            SqlBuilder sqlBuilder, 
            string AsAlias, 
            bool aggregate = false, 
            ColumnOrder order = default,
            bool forJson = false
            ) 
        {
            SqlBuilder = sqlBuilder;
            raw_name = null;
            show_name = AsAlias;
            Order = order;
            Aggregate = aggregate;
            ForJson = forJson;
        }


        public override string GetSqlExpression(string TableAlias, SqlBuilder.BuildOptions options)
        {
            var sb = new StringBuilder();

            sb.AppendLine("(");

            options.Indent(2);

            sb.AppendLine(SqlBuilder.Build(options.Clone(ForJson)));

            sb.Append(options.IndentToken);
            
            sb.Append(')');

            options.Unindent(2);

            return sb.ToString();
        }

        public override string GetGroupByExpression(string TableAlias, SqlBuilder.BuildOptions options)
        {
            return string.Empty;
            //throw new NotImplementedException();
        }

        public override string GetWhereExpression(string TableAlias, string predicate, SqlBuilder.BuildOptions options)
        {
            return "(" + SqlBuilder.Build(options.Clone(ForJson)) + ")";
        }

        public override string GetSelectExpression(string TableAlias, SqlBuilder.BuildOptions options)
            =>
            (show_name == raw_name) ? GetSqlExpression(TableAlias, options) :
            $"{GetSqlExpression(TableAlias, options)} AS [{Name}]";
    }
}
