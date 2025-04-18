namespace HuskyKit.Sql.Sources
{
    /// <summary>
    /// Represents a SQL source (table, view, or subquery) in a FROM or JOIN clause.
    /// </summary>
    public interface ISqlSource
    {
        /// <summary>
        /// Converts the source into a SQL-compatible string.
        /// </summary>
        /// <param name="options">Optional build options that modify the SQL output.</param>
        /// <returns>The SQL representation of the source.</returns>
        string Build(BuildOptions? options);

        /// <summary>
        /// Internal method to build the SQL representation using a specific context.
        /// </summary>
        /// <param name="options">Contextual options for SQL building.</param>
        /// <returns>The SQL representation of the source.</returns>
        internal string Build(BuildContext options);

        /// <summary>
        /// Gets the alias associated with the source in the SQL query.
        /// </summary>
        string Alias { get; set; }

        internal bool HasAlias { get; }
    }
}
