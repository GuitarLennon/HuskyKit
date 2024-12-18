using HuskyKit.Sql.Sources;

namespace HuskyKit.Sql
{
    /// <summary>
    /// Represents the context in which a SQL query is being built, including options, depth, and indentation.
    /// </summary>
    public class BuildContext(BuildOptions options)
    {
        /// <summary>
        /// Placeholder for indentation in SQL queries.
        /// </summary>
        public const string IndentationPlacement = "{}";

        /// <summary>
        /// Stack of build options used during query construction.
        /// </summary>
        public Stack<BuildOptions> OptionList { get; } = new Stack<BuildOptions>([options]);

        /// <summary>
        /// Gets the current build options from the stack.
        /// </summary>
        public BuildOptions CurrentOptions => OptionList.Peek();

        /// <summary>
        /// Gets the current depth of the query context, based on the number of options in the stack.
        /// </summary>
        public int Depth => OptionList.Count ;

        /// <summary>
        /// Token used for indentation in SQL queries, repeated based on depth.
        /// </summary>
        public string IndentToken => 
            string.Concat(Enumerable.Repeat(IndentationPlacement, Depth));

        /// <summary>
        /// Keeps track of tables that have already been rendered in the query.
        /// </summary>
        public HashSet<SqlBuilder> RenderedTables { get; protected set; } = [];

        /// <summary>
        /// Gets or sets the current table alias being used in the query.
        /// </summary>
        public string? CurrentTableAlias { get; private set; }

        /// <summary>
        /// Sets the current table alias for the context.
        /// </summary>
        /// <param name="currentTableAlias">The alias of the table to set.</param>
        /// <returns>The updated BuildContext instance.</returns>
        public BuildContext SetTable(string currentTableAlias)
        {
            CurrentTableAlias = currentTableAlias;
            return this;
        }

        /// <summary>
        /// Increases the indentation level by pushing the current options onto the stack.
        /// </summary>
        public void Indent() => Indent(CurrentOptions.Clone(null));

        /// <summary>
        /// Increases the indentation level using the specified options.
        /// </summary>
        /// <param name="options">The options to push onto the stack.</param>
        public void Indent(BuildOptions options)
        {
            OptionList.Push(options);
        }

        /// <summary>
        /// Decreases the indentation level by popping the last options from the stack.
        /// </summary>
        public void Unindent()
        {
            OptionList.TryPop(out _);
        }
    }
}
