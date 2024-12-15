namespace HuskyKit.Sql
{
    public struct ColumnOrder()
    {
        public static implicit operator ColumnOrder(OrderDirection order)
        {
            return new()
            {
                Direction = order
            };
        }

        public static implicit operator ColumnOrder(string column)
        {
            return new()
            {
                Column = column,
                Index = -1,
                Direction = OrderDirection.ASC
            };
        }

        public static implicit operator ColumnOrder((string column, OrderDirection order) args)
        {
            return new()
            {
                Column = args.column,
                Index = -1,
                Direction = args.order
            };
        }

        public static implicit operator ColumnOrder((int ix, string column, OrderDirection order) args)
        {
            return new()
            {
                Column = args.column,
                Index = args.ix,
                Direction = args.order
            };
        }

        public int Index { get; set; } = 0;
        public string Column { get; set; }
        public OrderDirection Direction { get; set; }
    }

    public enum OrderDirection
    {
        NONE, ASC, DESC
    }
}
