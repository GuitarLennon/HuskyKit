namespace HuskyKit.Sql
{
    /// <summary>
    /// Representa una función de ventana SQL y genera expresiones SQL para SELECT, GROUP BY, y otras cláusulas.
    /// </summary>
    public class WindowFunctionBuilder(string function) : SqlColumnAbstract
    {
        private readonly string _function = function ?? throw new ArgumentNullException(nameof(function));
        private readonly List<string> _partitionBy = [];
        private readonly List<ColumnOrder> _orderBy = [];

        /// <summary>
        /// Define las columnas para la cláusula PARTITION BY.
        /// </summary>
        /// <param name="columns">Una lista de columnas.</param>
        /// <returns>El mismo <see cref="WindowFunctionBuilder"/> para encadenamiento.</returns>
        public WindowFunctionBuilder PartitionBy(params string[] columns)
        {
            _partitionBy.AddRange(columns);
            return this;
        }

        /// <summary>
        /// Define las columnas para la cláusula ORDER BY.
        /// </summary>
        /// <param name="columns">Una lista de columnas o tuplas (columna, dirección).</param>
        /// <returns>El mismo <see cref="WindowFunctionBuilder"/> para encadenamiento.</returns>
        public WindowFunctionBuilder OrderBy(params ColumnOrder[] columns)
        {
            foreach (var column in columns)
            {
                _orderBy.Add(column);   
            }
            return this;
        }

        /// <summary>
        /// Define un alias para la función de ventana.
        /// </summary>
        /// <param name="alias">El alias para la columna generada.</param>
        /// <returns>El mismo <see cref="WindowFunctionBuilder"/> para encadenamiento.</returns>
        public WindowFunctionBuilder As(string alias)
        {
            show_name = alias;
            return this;
        }

        /// <inheritdoc />
        public override string GetSqlExpression(string TableAlias, BuildContext context)
        {
            var partitionClause = _partitionBy.Count != 0
                ? $"PARTITION BY {string.Join(", ", _partitionBy.Select(x => $"[{x}]"))} "
                : "";

            var orderClause = _orderBy.Count != 0
                ? $"ORDER BY {string.Join(", ", _orderBy.Select(x => $"[{x.Column}] {x.Direction}"))} "
                : "";

            return $"{_function}() OVER ({partitionClause}{orderClause.TrimEnd()})";
        }

        /// <inheritdoc />
        public override string GetGroupByExpression(string TableAlias, BuildContext context)
        {
            throw new NotSupportedException("GROUP BY is not applicable for window functions.");
        }

        /// <inheritdoc />
        public override string GetOrderByExpression(string TableAlias, BuildContext context)
        {
            throw new NotSupportedException("ORDER BY is not directly applicable for window functions.");
        }

        /// <inheritdoc />
        public override string GetSelectExpression(string TableAlias, BuildContext context)
        {
            return $"{GetSqlExpression(TableAlias, context)} AS [{Name}]";
        }

        /// <inheritdoc />
        public override string GetWhereExpression(string TableAlias, string predicate, BuildContext context)
        {
            throw new NotSupportedException("WHERE is not applicable for window functions.");
        }
    }
}
