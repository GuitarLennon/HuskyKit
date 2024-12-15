namespace HuskyKit.Sql
{
    public abstract class SqlColumnAbstract
    {
        public static implicit operator SqlColumnAbstract(string column)
        {
            return new SqlColumn(column);
        }

        public static implicit operator SqlColumnAbstract((string value, string AsAlias) column)
        {
            return new SqlColumn(column.value, column.AsAlias);
        }

        public static implicit operator SqlColumnAbstract((object value, string AsAlias) column)
        {
            return new SqlColumn(column.value, column.AsAlias);
        }

        public static implicit operator SqlColumnAbstract((object value, string AsAlias, bool groupBy) column)
        {
            return column.value.As(column.AsAlias, column.groupBy);
        }


        protected string? raw_name;
        protected string? show_name;

        public bool Aggregate { get; set; }

        public string? Name { get => show_name ?? raw_name; set => show_name = value; }

        public ColumnOrder Order { get; set; }

        public abstract string GetSqlExpression(string TableAlias, SqlBuilder.BuildOptions options);

        public abstract string GetGroupByExpression(string TableAlias, SqlBuilder.BuildOptions options);

        public string GetOrderByExpression(string TableAlias, SqlBuilder.BuildOptions options)
        {
            var _format = options.IndentToken.Replace("{", "{{").Replace("}", "}}") + $"[{Name}] {Order.Direction}";
            try
            {
                return Order.Direction != OrderDirection.NONE ? string.Format(_format, TableAlias) : $"[{Name}]";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }
        public abstract string GetSelectExpression(string TableAlias, SqlBuilder.BuildOptions options);

        public abstract string GetWhereExpression(string TableAlias, string predicate, SqlBuilder.BuildOptions options);

    }
}
