namespace HuskyKit.Sql.Columns
{
    public partial class SqlColumn 
    {
        /// <summary>
        /// Implicitly converts a string to a SqlColumnAbstract instance.
        /// </summary>
        /// <param name="column">The name of the column as a string.</param>
        /// <returns>A new SqlColumn instance.</returns>
        public static implicit operator SqlColumn(string column) 
            => new(column);

        /// <summary>
        /// Implicitly converts a tuple of (string value, string alias) to a SqlColumnAbstract instance.
        /// </summary>
        /// <param name="column">A tuple containing the column name and alias.</param>
        /// <returns>A new SqlColumn instance with the specified alias.</returns>
        public static implicit operator SqlColumn((string value, string AsAlias) column) 
            => new(column.value, column.AsAlias);

        /// <summary>
        /// Implicitly converts a tuple of (object value, string alias) to a SqlColumnAbstract instance.
        /// </summary>
        /// <param name="column">A tuple containing the column value and alias.</param>
        /// <returns>A new SqlColumn instance with the specified alias.</returns>
        public static implicit operator SqlColumn((object value, string AsAlias) column)
            => new(column.value, column.AsAlias);

        /// <summary>
        /// Implicitly converts a tuple of (object value, string alias, bool groupBy) to a SqlColumnAbstract instance.
        /// </summary>
        /// <param name="column">A tuple containing the column value, alias, and groupBy flag.</param>
        /// <returns>A SqlColumn instance configured with grouping.</returns>
        public static implicit operator SqlColumn((object value, string AsAlias, bool groupBy) column) 
            => column.value.As(column.AsAlias, column.groupBy);

    }
}
