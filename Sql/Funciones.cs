namespace HuskyKit.Sql
{
    /// <summary>
    /// Proporciona una colección de funciones SQL utilizables dentro de <see cref="SqlBuilder"/>.
    /// </summary>
    public static class Funciones
    {
        /// <summary>
        /// Genera la expresión SQL `GetDate()` para obtener la fecha y hora actual.
        /// </summary>
        /// <returns>Un <see cref="SqlColumn"/> representando `GetDate()`.</returns>
        public static SqlColumn GetDate() => "GetDate()".As("GetDate", false);

        /// <summary>
        /// Genera la expresión SQL `Count(*)`.
        /// </summary>
        /// <returns>Un <see cref="SqlColumn"/> representando `Count(*)`.</returns>
        public static SqlColumn Count() => "Count(*)".As("Count", true);

        /// <summary>
        /// Genera la expresión SQL `Count(columna)`.
        /// </summary>
        /// <param name="expression">El nombre de la columna para aplicar `Count`.</param>
        /// <returns>Un <see cref="SqlColumn"/> representando `Count(columna)`.</returns>
        public static SqlColumn Count(this string expression) =>
            $"Count([{expression}])".As($"Count_{expression}", true);

        /// <summary>
        /// Genera la expresión SQL `Round(expression, precision)`.
        /// </summary>
        /// <param name="expression">La expresión a redondear.</param>
        /// <param name="precision">El número de decimales para el redondeo.</param>
        /// <returns>Un <see cref="SqlColumn"/> representando la expresión `Round(expression, precision)`.</returns>
        public static SqlColumn Round(this string expression, int precision) =>
            $"Round({expression}, {precision})".As($"Round_{expression}", false);

        /// <summary>
        /// Genera la expresión SQL `String_agg(expression, separator)`.
        /// </summary>
        /// <param name="expression">La expresión o columna a concatenar.</param>
        /// <param name="separator">El separador entre los valores concatenados.</param>
        /// <param name="alias">El alias para la expresión generada.</param>
        /// <returns>Un <see cref="SqlColumn"/> representando `String_agg(expression, separator)`.</returns>
        public static SqlColumn StringAgg(this string expression, string separator = ",", string? alias = null) =>
            $"String_agg([{expression}], '{separator}')".As(alias ?? $"StringAgg_{expression}", true);

        /// <summary>
        /// Genera una expresión JSON basada en columnas.
        /// </summary>
        /// <param name="columns">Un diccionario de alias y nombres de columnas.</param>
        /// <param name="alias">El alias para el objeto JSON.</param>
        /// <returns>Un <see cref="SqlColumn"/> representando el objeto JSON.</returns>
        public static SqlColumn JsonObject(Dictionary<string, string> columns, string alias)
        {
            if (columns == null || columns.Count == 0)
                throw new ArgumentException("Columns cannot be null or empty.", nameof(columns));

            var jsonParts = columns
                .Select(x => $"'\"{x.Key}\": ' + COALESCE(Convert(varchar(max), [{x.Value}]), 'null')")
                .ToList();

            var jsonExpression = $"'{string.Join(", ", jsonParts)}'";
            return jsonExpression.As(alias, false);
        }

        /// <summary>
        /// Genera una expresión `CASE` para asignar índices basados en el valor de una columna.
        /// </summary>
        /// <param name="columnName">El nombre de la columna base.</param>
        /// <param name="columns">Los valores de la columna para los casos.</param>
        /// <param name="alias">El alias para la expresión generada.</param>
        /// <returns>Un <see cref="SqlColumn"/> representando la expresión `CASE`.</returns>
        public static SqlColumn CaseOrderIndex(string columnName, string[] columns, string alias)
        {
            if (columns == null || columns.Length == 0)
                throw new ArgumentException("Columns array cannot be null or empty.", nameof(columns));

            var cases = columns
                .Select((x, i) => $"WHEN [{columnName}] = '{x}' THEN {i}")
                .Aggregate("", (a, b) => a + " " + b);

            var caseExpression = $"CASE {cases} END";
            return caseExpression.As(alias, false);
        }

        /// <summary>
        /// Genera la expresión SQL `Min(columna)`.
        /// </summary>
        /// <param name="expression">El nombre de la columna.</param>
        /// <returns>Un <see cref="SqlColumn"/> representando `Min(columna)`.</returns>
        public static SqlColumn Min(this string expression) =>
            $"Min([{expression}])".As($"Min_{expression}", true);

        /// <summary>
        /// Genera la expresión SQL `Max(columna)`.
        /// </summary>
        /// <param name="expression">El nombre de la columna.</param>
        /// <returns>Un <see cref="SqlColumn"/> representando `Max(columna)`.</returns>
        public static SqlColumn Max(this string expression) =>
            $"Max([{expression}])".As($"Max_{expression}", true);


        /// <summary>
        /// Genera la expresión SQL `Avg(columna)`.
        /// </summary>
        /// <param name="expression">El nombre de la columna.</param>
        /// <returns>Un <see cref="SqlColumn"/> representando `Avg(columna)`.</returns>
        public static SqlColumn Avg(this string expression) =>
            $"Avg([{expression}])".As($"Avg_{expression}", true);

        /// <summary>
        /// Genera la expresión SQL `Sum(columna)`.
        /// </summary>
        /// <param name="expression">El nombre de la columna.</param>
        /// <returns>Un <see cref="SqlColumn"/> representando `Sum(columna)`.</returns>
        public static SqlColumn Sum(this string expression) =>
            $"Sum([{expression}])".As($"Sum_{expression}", true);

        /// <summary>
        /// Genera la expresión SQL `ISNULL(expression, defaultValue)`.
        /// </summary>
        /// <param name="expression">La expresión o columna a evaluar.</param>
        /// <param name="defaultValue">El valor por defecto si la expresión es NULL.</param>
        /// <returns>Un <see cref="SqlColumn"/> representando `ISNULL(expression, defaultValue)`.</returns>
        public static SqlColumn IsNull(this string expression, object defaultValue) =>
            $"ISNULL([{expression}], {defaultValue})".As($"IsNull_{expression}", false);

        /// <summary>
        /// Genera la expresión SQL `LEN(columna)`.
        /// </summary>
        /// <param name="expression">El nombre de la columna.</param>
        /// <returns>Un <see cref="SqlColumn"/> representando `LEN(columna)`.</returns>
        public static SqlColumn Len(this string expression) =>
            $"LEN([{expression}])".As($"Len_{expression}", false);

        /// <summary>
        /// Genera la expresión SQL `UPPER(columna)`.
        /// </summary>
        /// <param name="expression">El nombre de la columna.</param>
        /// <returns>Un <see cref="SqlColumn"/> representando `UPPER(columna)`.</returns>
        public static SqlColumn Upper(this string expression) =>
            $"UPPER([{expression}])".As($"Upper_{expression}", false);

        /// <summary>
        /// Genera la expresión SQL `LOWER(columna)`.
        /// </summary>
        /// <param name="expression">El nombre de la columna.</param>
        /// <returns>Un <see cref="SqlColumn"/> representando `LOWER(columna)`.</returns>
        public static SqlColumn Lower(this string expression) =>
            $"LOWER([{expression}])".As($"Lower_{expression}", false);

        /// <summary>
        /// Genera la expresión SQL `SUBSTRING(columna, start, length)`.
        /// </summary>
        /// <param name="expression">El nombre de la columna.</param>
        /// <param name="start">La posición inicial (1-indexed).</param>
        /// <param name="length">La cantidad de caracteres a extraer.</param>
        /// <returns>Un <see cref="SqlColumn"/> representando `SUBSTRING(columna, start, length)`.</returns>
        public static SqlColumn Substring(this string expression, int start, int length) =>
            $"SUBSTRING([{expression}], {start}, {length})".As($"Substring_{expression}", false);

        /// <summary>
        /// Genera la expresión SQL `COALESCE(expression1, expression2, ...)`.
        /// </summary>
        /// <param name="expressions">Un conjunto de expresiones para evaluar.</param>
        /// <param name="alias">El alias de la columna generada.</param>
        /// <returns>Un <see cref="SqlColumn"/> representando `COALESCE(expression1, expression2, ...)`.</returns>
        public static SqlColumn Coalesce(this IEnumerable<string> expressions, string alias)
        {
            if (expressions == null || !expressions.Any())
                throw new ArgumentException("Expressions cannot be null or empty.", nameof(expressions));

            var expressionList = string.Join(", ", expressions.Select(e => $"[{e}]"));
            return $"COALESCE({expressionList})".As(alias, false);
        }

    }
}
