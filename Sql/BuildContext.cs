using HuskyKit.Sql.Sources;
using System.Linq.Expressions;

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
        public Stack<(string tableName, BuildOptions options)> OptionList { get; } = new ([(string.Empty, options)]);

        /// <summary>
        /// Gets the current build options from the stack.
        /// </summary>
        public BuildOptions CurrentOptions => OptionList.Peek().options;

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
        public string CurrentTableAlias => OptionList.Peek().tableName;

        public IEnumerable<string> TableAlias => OptionList.Select(x => x.tableName);
        // OptionList.Count < 2 ? null :
        // OptionList.ElementAt(1).tableName;


        /*
        /// <summary>
        /// Sets the current table alias for the context.
        /// </summary>
        /// <param name="currentTableAlias">The alias of the table to set.</param>
        /// <returns>The updated BuildContext instance.</returns>
        public BuildContext SetCurrentTable(string currentTableAlias)
        {
            CurrentTableAlias = currentTableAlias;
            return this;
        }
        */

        /// <summary>
        /// Increases the indentation level by pushing the current options onto the stack.
        /// </summary>
        public void Indent(string currentTableAlias) => Indent(currentTableAlias, CurrentOptions.Clone(null));

        /// <summary>
        /// Increases the indentation level using the specified options.
        /// </summary>
        /// <param name="options">The options to push onto the stack.</param>
        public void Indent(string currentTableAlias, BuildOptions options)
        {
            if(string.IsNullOrWhiteSpace(currentTableAlias)) 
                throw new ArgumentNullException(nameof(currentTableAlias)); 
            OptionList.Push((currentTableAlias, options));
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
