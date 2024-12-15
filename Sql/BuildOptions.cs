namespace HuskyKit.Sql
{

    /// <summary>
    /// Represents options used during the building of a SQL query, including pagination and JSON formatting.
    /// </summary>
    public class BuildOptions
    {
        /// <summary>
        /// Gets or sets the number of rows to skip (used for pagination).
        /// </summary>
        public int? Skip { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of rows to return (used for pagination).
        /// </summary>
        public int? Length { get; set; }

        /// <summary>
        /// Gets or sets the JSON formatting options for the query.
        /// </summary>
        public ForJsonOptions? ForJson { get; set; }

        /// <summary>
        /// Gets or sets the string used for indentation in SQL queries.
        /// </summary>
        public string Indentation { get; set; } = "  ";

        /// <summary>
        /// Creates a copy of the current BuildOptions with optional overrides.
        /// </summary>
        /// <param name="ForJson">The JSON formatting options to use in the clone.</param>
        /// <param name="Skip">The number of rows to skip (optional).</param>
        /// <param name="Length">The maximum number of rows to return (optional).</param>
        /// <returns>A new BuildOptions instance with the specified overrides.</returns>
        public BuildOptions Clone(ForJsonOptions? ForJson, int? Skip = null, int? Length = null)
        {
            var g = (BuildOptions)MemberwiseClone();
            g.Indentation = Indentation;
            g.ForJson = ForJson;
            g.Skip = Skip ?? g.Skip;
            g.Length = Length ?? g.Length;
            return g;
        }
    }
}
