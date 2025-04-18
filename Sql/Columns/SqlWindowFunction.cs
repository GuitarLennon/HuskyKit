using HuskyKit.Sql;

namespace HuskyKit.Sql.Columns
{
    /// <summary>
    /// Representa una función de ventana SQL y genera expresiones SQL para SELECT, GROUP BY, y otras cláusulas.
    /// </summary>
    public class WindowFunctionBuilder(string function) : ISqlColumn
    {
        private readonly string _function = function ?? throw new ArgumentNullException(nameof(function));
        private readonly List<string> _partitionBy = [];
        private readonly List<OrderByClause> _orderBy = [];

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
        public WindowFunctionBuilder OrderBy(params OrderByClause[] columns)
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
        public override string GetSqlExpression(BuildContext context, int targetIndex = 0)
        {
            var partitionClause = _partitionBy.Count != 0
                ? $"PARTITION BY {string.Join(", ", _partitionBy.Select(x => $"[{x}]"))} "
                : "";

            //TODO: Fix this
            var orderClause = _orderBy.Count != 0
                ? $"ORDER BY {string.Join(", ", _orderBy.Select(x => x.GetSqlExpression(context) ))} "
                : "";

            return $"{_function}() OVER ({partitionClause}{orderClause.TrimEnd()})";
        }

        /// <inheritdoc />
        public override string GetSelectExpression(BuildContext context)
        {
            return $"{GetSqlExpression(context)} AS [{Name}]";
        }

    }
}
