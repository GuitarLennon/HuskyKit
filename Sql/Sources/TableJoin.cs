using HuskyKit.Sql.Columns;

namespace HuskyKit.Sql.Sources
{
    /// <summary>
    /// Represents a join between tables in an SQL query, including the join type, target table, and optional columns.
    /// </summary>
    public class TableJoin
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TableJoin"/> class.
        /// </summary>
        /// <param name="joinTypes">The type of join (e.g., INNER, LEFT).</param>
        /// <param name="qualifiedTarget">The fully qualified name of the target table (e.g., [Schema].[Table]).</param>
        /// <param name="alias">The alias for the target table. If null, a hash code is used as the alias.</param>
        /// <param name="predicate">The ON condition for the join.</param>
        /// <param name="columns">Optional columns to include in the join.</param>
        public TableJoin(JoinTypes joinTypes, string qualifiedTarget, string? alias = null, string? predicate = null, IEnumerable<ISqlColumn>? columns = null)
        {
            JoinType = joinTypes;
            Target = qualifiedTarget;
            Predicate = predicate;
            Alias = alias ?? GetHashCode().ToString();
            if (columns != null)
                Columns.AddRange(columns);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TableJoin"/> class using a schema and table name.
        /// </summary>
        /// <param name="joinTypes">The type of join (e.g., INNER, LEFT).</param>
        /// <param name="rawTable">A tuple containing the schema and table name.</param>
        /// <param name="alias">The alias for the target table. If null, the table name is used as the alias.</param>
        /// <param name="predicate">The ON condition for the join.</param>
        /// <param name="columns">Optional columns to include in the join.</param>
        public TableJoin(JoinTypes joinTypes, (string Schema, string Name) rawTable, string? alias = null, string? predicate = null, IEnumerable<ISqlColumn>? columns = null)
            : this(joinTypes, $"[{rawTable.Schema}].[{rawTable.Name}]", alias ?? rawTable.Name, predicate, columns)
        {
        }

        /// <summary>
        /// Gets or sets the alias for the target table.
        /// </summary>
        public string Alias { get; set; }

        /// <summary>
        /// Gets the list of columns included in the join.
        /// </summary>
        public List<ISqlColumn> Columns { get; protected set; } = new();

        /// <summary>
        /// Generates the SQL expression for the join, including the JOIN type and ON condition.
        /// </summary>
        /// <param name="fromAlias">The alias of the source table.</param>
        /// <param name="indent">Optional indentation for formatting the SQL output.</param>
        /// <returns>The SQL expression for the join.</returns>
        public string GetSqlExpression(string fromAlias, string? indent = null) =>
            $"{indent}  {JoinType.ToString().Replace('_', ' ')} JOIN " +
            (Target == Alias ? $"[{Alias}]" : $"{Target} AS [{Alias}]") +
            (!string.IsNullOrWhiteSpace(Predicate) ? $"\n{indent}    ON {Predicate}" : string.Empty);

        /// <summary>
        /// Gets or sets the type of join (e.g., INNER JOIN, LEFT JOIN).
        /// </summary>
        public JoinTypes JoinType { get; set; }

        /// <summary>
        /// Gets or sets the ON condition for the join.
        /// </summary>
        public string? Predicate { get; set; }

        /// <summary>
        /// Gets or sets the fully qualified name of the target table.
        /// </summary>
        public string Target { get; set; }

        /// <summary>
        /// Returns the SQL expression for the join as a string.
        /// </summary>
        /// <returns>The SQL expression for the join.</returns>
        public override string ToString() => GetSqlExpression("{{From Table}}");
    }
}
