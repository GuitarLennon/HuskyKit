namespace HuskyKit.Sql
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
        string Alias { get; }
    }

    /// <summary>
    /// Represents a specific SQL table, including its schema, table name, and optional alias.
    /// </summary>
    public class SqlTable : ISqlSource
    {
        /// <summary>
        /// Gets the schema of the SQL table.
        /// </summary>
        public string Schema { get; }

        /// <summary>
        /// Gets the name of the SQL table.
        /// </summary>
        public string Table { get; }

        /// <summary>
        /// Gets the alias of the SQL table used in the query.
        /// </summary>
        public string Alias { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlTable"/> class.
        /// </summary>
        /// <param name="schema">The schema name of the table. Cannot be null or empty.</param>
        /// <param name="table">The table name. Cannot be null or empty.</param>
        /// <param name="alias">An optional alias for the table. Defaults to the table name if not provided.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="schema"/> or <paramref name="table"/> is null or empty.</exception>
        public SqlTable(string schema, string table, string? alias = null)
        {
            if (string.IsNullOrWhiteSpace(schema)) throw new ArgumentException("Schema cannot be null or empty.", nameof(schema));
            if (string.IsNullOrWhiteSpace(table)) throw new ArgumentException("Table cannot be null or empty.", nameof(table));

            Schema = schema;
            Table = table;
            Alias = alias ?? table;
        }

        /// <summary>
        /// Builds the SQL representation of the table with optional build options.
        /// </summary>
        /// <param name="options">Optional build options that modify the SQL output.</param>
        /// <returns>The SQL representation of the table, including schema, table name, and alias.</returns>
        public string Build(BuildOptions? options) => $"[{Schema}].[{Table}] AS [{Alias}]";

        /// <summary>
        /// Builds the SQL representation of the table using a specific context.
        /// </summary>
        /// <param name="context">The context used to modify the SQL output.</param>
        /// <returns>The SQL representation of the table, including schema, table name, and alias.</returns>
        public string Build(BuildContext context) => $"[{Schema}].[{Table}] AS [{Alias}]";

        /// <summary>
        /// Returns a string representation of the table, which is its SQL representation.
        /// </summary>
        /// <returns>The SQL representation of the table.</returns>
        public override string ToString() => Build((BuildOptions?)null);

        /// <summary>
        /// Implicitly converts a tuple containing schema and table names into a <see cref="SqlTable"/> instance.
        /// </summary>
        /// <param name="tuple">A tuple containing the schema and table names.</param>
        /// <returns>A new <see cref="SqlTable"/> instance.</returns>
        public static implicit operator SqlTable((string Schema, string Table) tuple)
            => new(tuple.Schema, tuple.Table);

        /// <summary>
        /// Implicitly converts a tuple containing schema, table names, and alias into a <see cref="SqlTable"/> instance.
        /// </summary>
        /// <param name="tuple">A tuple containing the schema, table names, and alias.</param>
        /// <returns>A new <see cref="SqlTable"/> instance.</returns>
        public static implicit operator SqlTable((string Schema, string Table, string Alias) tuple)
            => new(tuple.Schema, tuple.Table, tuple.Alias);
    }
}
