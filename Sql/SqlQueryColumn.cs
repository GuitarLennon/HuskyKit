using System.Text;

namespace HuskyKit.Sql
{
    public class SqlQueryColumn : SqlColumnAbstract
    {
        public SqlBuilder SqlBuilder { get; set; }

        public ForJsonOptions? ForJson { get; set; }

        public int? Skip { get; set; }

        public int? Length { get; set; }

        public SqlQueryColumn(
            SqlBuilder sqlBuilder, 
            string AsAlias, 
            ColumnOrder order = default,
            ForJsonOptions? forJson = null,
            int? skip = null,
            int? length = null
            ) 
        {
            SqlBuilder = sqlBuilder;
            raw_name = null;
            show_name = AsAlias;
            Order = order;
            Aggregate = false;
            ForJson = forJson;
            Skip = skip;
            Length = length;
        }


        public override string GetSqlExpression(string TableAlias, SqlBuilder.BuildOptions options)
        {
            var sb = new StringBuilder();

            sb.AppendLine("(");

            options.Indent(2);

            var _options = options.Clone(ForJson, Skip, Length);

            sb.AppendLine(SqlBuilder.Build(_options));

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
