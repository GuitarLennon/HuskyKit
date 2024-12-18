using System;

namespace HuskyKit.Sql.Columns
{
    public class OrderByClause : IEquatable<OrderByClause>
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
            Expression = index.ToString();
            Direction = direction;
        }

        public OrderByClause(string expression, OrderDirection direction = OrderDirection.ASC)
        {
            Expression = expression;
            Direction = direction;
        }

        public OrderByClause(ISqlColumn column, string tableAlias)
        {
            Expression = column.GetOrderByExpression(tableAlias);
            Direction = column.Order.Direction;
        }

        /// <summary>
        /// Obtiene el índice de la columna en la cláusula ORDER BY.
        /// </summary>
        /// <remarks>
        /// El índice se utiliza para determinar la prioridad de la columna en una ordenación múltiple.
        /// El valor predeterminado es -1, indicando que no se ha definido un índice.
        /// </remarks>
        public string Expression { get; set; }


        /// <summary>
        /// Obtiene la dirección de ordenación (ASC, DESC o NONE).
        /// </summary>
        public OrderDirection Direction { get; set; }

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

        public static bool operator ==(OrderByClause left, OrderByClause right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(OrderByClause left, OrderByClause right)
        {
            return !(left == right);
        }
    }


    /// <summary>
    /// Representa un orden para una columna en una cláusula ORDER BY en SQL.
    /// Incluye el nombre de la columna, la dirección de ordenación (ASC, DESC) y un índice opcional.
    /// </summary>
    /// <remarks>
    /// Inicializa una nueva instancia de la clase <see cref="ColumnOrder"/> con un nombre de columna y dirección predeterminada.
    /// </remarks>
    /// <param name="column">El nombre de la columna. No puede ser nulo ni estar vacío.</param>
    /// <param name="direction">La dirección de ordenación (predeterminada: ASC).</param>
    public class ColumnOrder(int index, OrderDirection direction = OrderDirection.ASC) : IEquatable<ColumnOrder>
    {
        /// <summary>
        /// Obtiene el índice de la columna en la cláusula ORDER BY.
        /// </summary>
        /// <remarks>
        /// El índice se utiliza para determinar la prioridad de la columna en una ordenación múltiple.
        /// El valor predeterminado es -1, indicando que no se ha definido un índice.
        /// </remarks>
        public int Index { get; set; } = index;

        /// <summary>
        /// Obtiene la dirección de ordenación (ASC, DESC o NONE).
        /// </summary>
        public OrderDirection Direction { get; set; } = direction;

        /// <summary>
        /// Conversión implícita de <see cref="OrderDirection"/> a <see cref="ColumnOrder"/>.
        /// </summary>
        /// <param name="order">La dirección de ordenación (ASC o DESC).</param>
        public static implicit operator ColumnOrder(OrderDirection order) =>
            new(index: -1, direction: order);

        /// <summary>
        /// Determina si la instancia actual es igual a otra instancia de <see cref="ColumnOrder"/>.
        /// </summary>
        /// <param name="other">La otra instancia de <see cref="ColumnOrder"/> a comparar.</param>
        /// <returns><c>true</c> si las instancias son iguales; de lo contrario, <c>false</c>.</returns>
        public bool Equals(ColumnOrder? other) =>
            Index == other?.Index && /* Column == other.Column && */ Direction == other?.Direction;

        /// <summary>
        /// Determina si la instancia actual es igual a otro objeto.
        /// </summary>
        /// <param name="obj">El objeto a comparar con esta instancia.</param>
        /// <returns><c>true</c> si el objeto es un <see cref="ColumnOrder"/> y es igual a esta instancia; de lo contrario, <c>false</c>.</returns>
        public override bool Equals(object? obj) =>
            obj != null && obj is ColumnOrder other && Equals(other);

        /// <summary>
        /// Calcula un código hash para esta instancia.
        /// </summary>
        /// <returns>Un código hash que representa esta instancia.</returns>
        public override int GetHashCode() =>
            HashCode.Combine(Index, Direction);

        internal void Reset()
        {
            Index = -1;
            Direction = OrderDirection.NONE;
        }

        internal void Apply(ColumnOrder? order)
        {
            Index = order?.Index ?? -1;
            Direction = order?.Direction ?? OrderDirection.NONE;
        }

        public static bool operator ==(ColumnOrder left, ColumnOrder right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ColumnOrder left, ColumnOrder right)
        {
            return !(left == right);
        }
    }
}
