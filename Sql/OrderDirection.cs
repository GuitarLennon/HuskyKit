namespace HuskyKit.Sql
{
    /// <summary>
    /// Representa un orden para una columna en una cláusula ORDER BY en SQL.
    /// Incluye el nombre de la columna, la dirección de ordenación (ASC, DESC) y un índice opcional.
    /// </summary>
    public struct ColumnOrder : IEquatable<ColumnOrder>
    {
        /// <summary>
        /// Obtiene el índice de la columna en la cláusula ORDER BY.
        /// </summary>
        /// <remarks>
        /// El índice se utiliza para determinar la prioridad de la columna en una ordenación múltiple.
        /// El valor predeterminado es -1, indicando que no se ha definido un índice.
        /// </remarks>
        public int Index { get; }

        /// <summary>
        /// Obtiene el nombre de la columna utilizada en la ordenación.
        /// </summary>
        /// <remarks>
        /// El nombre de la columna no puede ser nulo ni estar vacío.
        /// </remarks>
        public string Column { get; }

        /// <summary>
        /// Obtiene la dirección de ordenación (ASC, DESC o NONE).
        /// </summary>
        public OrderDirection Direction { get; }

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="ColumnOrder"/> con un índice, nombre de columna y dirección.
        /// </summary>
        /// <param name="index">El índice de la columna en la cláusula ORDER BY. El valor -1 indica que no se ha definido un índice.</param>
        /// <param name="column">El nombre de la columna. No puede ser nulo ni estar vacío.</param>
        /// <param name="direction">La dirección de ordenación (ASC o DESC).</param>
        /// <exception cref="ArgumentException">Si <paramref name="column"/> es nulo o está vacío.</exception>
        public ColumnOrder(int index, string column, OrderDirection direction)
        {
            if (string.IsNullOrWhiteSpace(column))
                throw new ArgumentException("Column name cannot be null or empty.", nameof(column));

            Index = index;
            Column = column;
            Direction = direction;
        }

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="ColumnOrder"/> con un nombre de columna y dirección predeterminada.
        /// </summary>
        /// <param name="column">El nombre de la columna. No puede ser nulo ni estar vacío.</param>
        /// <param name="direction">La dirección de ordenación (predeterminada: ASC).</param>
        public ColumnOrder(string column, OrderDirection direction = OrderDirection.ASC)
            : this(-1, column, direction) { }

        /// <summary>
        /// Conversión implícita de <see cref="OrderDirection"/> a <see cref="ColumnOrder"/>.
        /// </summary>
        /// <param name="order">La dirección de ordenación (ASC o DESC).</param>
        public static implicit operator ColumnOrder(OrderDirection order) =>
            new ColumnOrder(index: -1, column: null, direction: order);

        /// <summary>
        /// Conversión implícita de una cadena (nombre de columna) a <see cref="ColumnOrder"/>.
        /// </summary>
        /// <param name="column">El nombre de la columna.</param>
        /// <exception cref="ArgumentException">Si <paramref name="column"/> es nulo o está vacío.</exception>
        public static implicit operator ColumnOrder(string column) =>
            new ColumnOrder(column);

        /// <summary>
        /// Conversión implícita de una tupla con columna y dirección a <see cref="ColumnOrder"/>.
        /// </summary>
        /// <param name="args">Una tupla que contiene el nombre de la columna y la dirección de ordenación.</param>
        public static implicit operator ColumnOrder((string column, OrderDirection order) args) =>
            new ColumnOrder(args.column, args.order);

        /// <summary>
        /// Conversión implícita de una tupla con índice, columna y dirección a <see cref="ColumnOrder"/>.
        /// </summary>
        /// <param name="args">Una tupla que contiene el índice, nombre de la columna y la dirección de ordenación.</param>
        public static implicit operator ColumnOrder((int ix, string column, OrderDirection order) args) =>
            new ColumnOrder(args.ix, args.column, args.order);

        /// <summary>
        /// Devuelve una representación en cadena de la instancia de <see cref="ColumnOrder"/>.
        /// </summary>
        /// <returns>Una cadena que representa la columna, su índice (si está definido) y la dirección de ordenación.</returns>
        public override string ToString() =>
            Index >= 0 ? $"{Index}: {Column} {Direction}" : $"{Column} {Direction}";

        /// <summary>
        /// Determina si la instancia actual es igual a otra instancia de <see cref="ColumnOrder"/>.
        /// </summary>
        /// <param name="other">La otra instancia de <see cref="ColumnOrder"/> a comparar.</param>
        /// <returns><c>true</c> si las instancias son iguales; de lo contrario, <c>false</c>.</returns>
        public bool Equals(ColumnOrder other) =>
            Index == other.Index && Column == other.Column && Direction == other.Direction;

        /// <summary>
        /// Determina si la instancia actual es igual a otro objeto.
        /// </summary>
        /// <param name="obj">El objeto a comparar con esta instancia.</param>
        /// <returns><c>true</c> si el objeto es un <see cref="ColumnOrder"/> y es igual a esta instancia; de lo contrario, <c>false</c>.</returns>
        public override bool Equals(object obj) =>
            obj is ColumnOrder other && Equals(other);

        /// <summary>
        /// Calcula un código hash para esta instancia.
        /// </summary>
        /// <returns>Un código hash que representa esta instancia.</returns>
        public override int GetHashCode() =>
            HashCode.Combine(Index, Column, Direction);
    }

    /// <summary>
    /// Define las posibles direcciones de ordenación para una columna en SQL.
    /// </summary>
    public enum OrderDirection
    {
        /// <summary>
        /// No se aplica ninguna ordenación.
        /// </summary>
        NONE,

        /// <summary>
        /// Orden ascendente (ASC).
        /// </summary>
        ASC,

        /// <summary>
        /// Orden descendente (DESC).
        /// </summary>
        DESC
    }
}
