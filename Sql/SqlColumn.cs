using System.Linq.Expressions;
using System.Text;

namespace HuskyKit.Sql
{
    //public class SqlColumn<T> : SqlColumn
    //    where T : struct, IComparable<T>
    //{
    //    public SqlColumn(T expression, string AsAlias, bool aggregate = false, ColumnOrder order = default)
    //        : base(null, null)
    //    {
    //        raw_name = "(Expression)";
    //        show_name = AsAlias;
    //        Expression = expression?.ToString() ?? $"null";
    //        Order = order;
    //        Aggregate = aggregate;
    //    }
    //}

    public class SqlColumn : SqlColumnAbstract
    {
        public SqlColumn(string rawName)
        {
            raw_name = rawName;
            show_name = rawName;
            Expression = $"[{{0}}].[{rawName}]";
        }

        public SqlColumn(string rawName, string AsAlias, bool aggregate = false, ColumnOrder order = default)
        {
            raw_name = rawName;
            show_name = AsAlias;
            Expression = $"[{{0}}].[{rawName}]";
            Order = order;
            Aggregate = aggregate;
        }

        private SqlColumn()
        {
            Expression = "[{0}].*";
        }

        public static SqlColumn All()
        {
            return new();
        }

        internal SqlColumn(object? expression, string AsAlias, bool aggregate = false, ColumnOrder order = default)
        {
            raw_name = null;
            show_name = AsAlias;
            Aggregate = aggregate;
            Order = order;
            Expression = expression?.ToString() ?? $"null";

        }

        public string Expression { get; set; }

      


        public override string GetSqlExpression(string TableAlias, SqlBuilder.BuildOptions options)
            => string.Format(Expression, TableAlias);

        public override string GetGroupByExpression(string TableAlias, SqlBuilder.BuildOptions options)
            => string.Format(Expression, TableAlias);

        public override string GetWhereExpression(string TableAlias, string predicate, SqlBuilder.BuildOptions options)
            => $"CONVERT(varchar, {string.Format(Expression, TableAlias)}) {predicate}";

        public override string ToString()
        {
            return string.IsNullOrWhiteSpace(Name) ? Expression : $"{Expression} AS {Name}";
        }


        public override string GetSelectExpression(string TableAlias, SqlBuilder.BuildOptions options)
            =>
            (show_name == raw_name) ? GetSqlExpression(TableAlias, options) :
            $"{GetSqlExpression(TableAlias, options)} AS [{Name}]";
    }
}
