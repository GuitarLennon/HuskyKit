namespace HuskyKit.Sql
{
    /// <summary>
    /// Represents the options available for formatting JSON output in SQL queries using the FOR JSON clause.
    /// This enumeration supports bitwise combination of its values.
    /// </summary>
    [Flags]
    public enum ForJsonOptions : short
    {
        /// <summary>
        /// Default behavior for FOR JSON, automatically mapping column names to JSON keys.
        /// </summary>
        AUTO = 0x0, // Binary: 0000

        /// <summary>
        /// Specifies that column names should be interpreted as JSON paths, allowing nested JSON structures.
        /// </summary>
        PATH = 1 << 0, // Binary: 0001

        /// <summary>
        /// Omits the outer array wrapper in the resulting JSON. This is useful when only a single object or value is expected.
        /// </summary>
        WITHOUT_ARRAY_WRAPPER = 1 << 1, // Binary: 0010

        /// <summary>
        /// Includes columns with NULL values in the JSON output, explicitly showing them as null.
        /// </summary>
        INCLUDE_NULL_VALUES = 1 << 2 // Binary: 0100
    }
}
