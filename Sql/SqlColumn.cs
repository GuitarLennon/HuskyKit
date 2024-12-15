using System.Linq.Expressions;
using System.Text;

namespace HuskyKit.Sql
{
    /// <summary>
    /// Representa una columna en una consulta SQL, proporcionando soporte para alias, agregados, y ordenación.
    /// </summary>
    public class SqlColumn : SqlColumnAbstract
    {
        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="SqlColumn"/> utilizando solo el nombre original de la columna.
        /// </summary>
        /// <param name="rawName">El nombre de la columna tal como aparece en la base de datos.</param>
        public SqlColumn(string rawName)
        {
            raw_name = rawName;
            show_name = rawName;
            Expression = $"[{{0}}].[{rawName}]";
        }

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="SqlColumn"/> con un nombre, un alias, y opciones adicionales.
        /// </summary>
        /// <param name="rawName">El nombre de la columna tal como aparece en la base de datos.</param>
        /// <param name="AsAlias">El alias que se utilizará para la columna en la consulta SQL.</param>
        /// <param name="aggregate">Indica si la columna es una expresión de agregado (por ejemplo, SUM, AVG).</param>
        /// <param name="order">El orden de la columna en la cláusula ORDER BY.</param>
        public SqlColumn(string rawName, string AsAlias, bool aggregate = false, ColumnOrder order = default)
        {
            raw_name = rawName;
            show_name = AsAlias;
            Expression = $"[{{0}}].[{rawName}]";
            Order = order;
            Aggregate = aggregate;
        }

        /// <summary>
        /// Constructor interno que inicializa una columna utilizando una expresión y opciones avanzadas.
        /// </summary>
        /// <param name="expression">La expresión que define la columna (por ejemplo, una operación o función).</param>
        /// <param name="AsAlias">El alias que se utilizará para la columna en la consulta SQL.</param>
        /// <param name="aggregate">Indica si la columna es una expresión de agregado.</param>
        /// <param name="order">El orden de la columna en la cláusula ORDER BY.</param>
        internal SqlColumn(object? expression, string AsAlias, bool aggregate = false, ColumnOrder order = default)
        {
            raw_name = null;
            show_name = AsAlias;
            Aggregate = aggregate;
            Order = order;
            Expression = expression?.ToString() ?? $"null";
        }

        /// <summary>
        /// Obtiene una instancia especial que representa todas las columnas (`SELECT *`).
        /// </summary>
        /// <returns>Una instancia de <see cref="SqlColumn"/> que selecciona todas las columnas.</returns>
        public static SqlColumn All()
        {
            return new("*");
        }

        /// <summary>
        /// Obtiene o establece la expresión SQL asociada a esta columna.
        /// </summary>
        public string Expression { get; set; }

        /// <summary>
        /// Genera la expresión SQL de la columna para la cláusula SELECT.
        /// </summary>
        /// <param name="TableAlias">El alias de la tabla al que pertenece la columna.</param>
        /// <param name="context">El contexto de construcción de la consulta SQL.</param>
        /// <returns>Una cadena que representa la expresión SQL para la cláusula SELECT.</returns>
        public override string GetSelectExpression(string TableAlias, BuildContext context)
            => (show_name == raw_name)
                ? GetSqlExpression(TableAlias, context)
                : $"{GetSqlExpression(TableAlias, context)} AS [{Name}]";

        /// <summary>
        /// Genera la expresión SQL de la columna para la cláusula GROUP BY.
        /// </summary>
        /// <param name="TableAlias">El alias de la tabla al que pertenece la columna.</param>
        /// <param name="context">El contexto de construcción de la consulta SQL.</param>
        /// <returns>Una cadena que representa la expresión SQL para la cláusula GROUP BY.</returns>
        public override string GetGroupByExpression(string TableAlias, BuildContext context)
            => string.Format(Expression, TableAlias);

        /// <summary>
        /// Genera la expresión SQL de la columna para la cláusula WHERE.
        /// </summary>
        /// <param name="TableAlias">El alias de la tabla al que pertenece la columna.</param>
        /// <param name="predicate">El predicado a aplicar en la condición (por ejemplo, `= 1`, `LIKE '%value%'`).</param>
        /// <param name="context">El contexto de construcción de la consulta SQL.</param>
        /// <returns>Una cadena que representa la expresión SQL para la cláusula WHERE.</returns>
        public override string GetWhereExpression(string TableAlias, string predicate, BuildContext context)
            => $"CONVERT(varchar, {string.Format(Expression, TableAlias)}) {predicate}";

        /// <summary>
        /// Genera la expresión SQL de la columna.
        /// </summary>
        /// <param name="TableAlias">El alias de la tabla al que pertenece la columna.</param>
        /// <param name="context">El contexto de construcción de la consulta SQL.</param>
        /// <returns>Una cadena que representa la expresión SQL de la columna.</returns>
        public override string GetSqlExpression(string TableAlias, BuildContext context)
            => string.Format(Expression, TableAlias);

        /// <summary>
        /// Devuelve una representación en cadena de la columna.
        /// </summary>
        /// <returns>Una cadena que representa la columna, incluyendo su alias si se define.</returns>
        public override string ToString()
        {
            return string.IsNullOrWhiteSpace(Name)
                ? Expression
                : $"{Expression} AS {Name}";
        }

      

        /// <summary>
        /// Creates a SQL column with an alias, optional aggregation, and ordering.
        /// </summary>
        public SqlColumn As(string alias)
        {
            show_name = alias;
            return this;
        }
    }
}
