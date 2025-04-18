using HuskyKit.Sql.Sources;

namespace HuskyKit.Sql.Columns
{
    public class OrderByClause : IEquatable<OrderByClause>, ISqlExpression
    {
        public static implicit operator OrderByClause(string orderByClause)
        {
            return new(orderByClause);
        }

        public static implicit operator OrderByClause((string orderByClause, OrderDirection order) @implicit)
        {
            return new(@implicit.orderByClause, @implicit.order);
        }

        public OrderByClause(int index, OrderDirection direction = OrderDirection.ASC)
        {
            Expression = (BuildContext _, int _) => index.ToString();
            Direction = direction;
        }

        public OrderByClause(string expression, OrderDirection direction = OrderDirection.ASC)
        {
            Expression = (BuildContext _, int _) => expression;
            Direction = direction;
        }

        public OrderByClause(ISqlColumn column)
        {
            Expression = column.GetOrderByExpression;
            Direction = column.Order.Direction;
        }

        /// <summary>
        /// Obtiene el índice de la columna en la cláusula ORDER BY.
        /// </summary>
        /// <remarks>
        /// El índice se utiliza para determinar la prioridad de la columna en una ordenación múltiple.
        /// El valor predeterminado es -1, indicando que no se ha definido un índice.
        /// </remarks>
        private Func<BuildContext, int, string> Expression { get; set; }


        /// <summary>
        /// Obtiene la dirección de ordenación (ASC, DESC o NONE).
        /// </summary>
        public OrderDirection Direction { get; }

        /// <summary>
        /// Determina si la instancia actual es igual a otra instancia de <see cref="ColumnOrder"/>.
        /// </summary>
        /// <param name="other">La otra instancia de <see cref="ColumnOrder"/> a comparar.</param>
        /// <returns><c>true</c> si las instancias son iguales; de lo contrario, <c>false</c>.</returns>
        public bool Equals(OrderByClause? other) =>
            Expression == other?.Expression && /* Column == other.Column && */ Direction == other?.Direction;

        /// <summary>
        /// Determina si la instancia actual es igual a otro objeto.
        /// </summary>
        /// <param name="obj">El objeto a comparar con esta instancia.</param>
        /// <returns><c>true</c> si el objeto es un <see cref="ColumnOrder"/> y es igual a esta instancia; de lo contrario, <c>false</c>.</returns>
        public override bool Equals(object? obj) =>
            obj != null && obj is OrderByClause other && Equals(other);

        /// <summary>
        /// Calcula un código hash para esta instancia.
        /// </summary>
        /// <returns>Un código hash que representa esta instancia.</returns>
        public override int GetHashCode() =>
            HashCode.Combine(Expression, Direction);

        public string GetSqlExpression(BuildContext context, int targetIndex = 0)
        {
            return $"{Expression(context, targetIndex)} {Direction}";
        }

        public static bool operator ==(OrderByClause left, OrderByClause right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(OrderByClause left, OrderByClause right)
        {
            return !(left == right);
        }
    }
}
