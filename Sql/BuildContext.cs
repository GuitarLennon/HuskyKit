using HuskyKit.Sql.Sources;
using System.Linq.Expressions;

namespace HuskyKit.Sql
{
    /// <summary>
    /// Represents the context in which a SQL query is being built, including options, depth, and indentation.
    /// </summary>
    public class BuildContext(SqlBuilder firstBuilder, BuildOptions firstoptions)
    {

        /// <summary>
        /// Placeholder for indentation in SQL queries.
        /// </summary>
        public const string IndentationPlacement = "{}";

        /// <summary>
        /// Gets the current build options from the stack.
        /// </summary>
        public BuildOptions CurrentOptions => StackList.Peek().Options;

        /// <summary>
        /// Gets the current build options from the stack.
        /// </summary>
        public ISqlSource CurrentSource => StackList.Peek().Source;

        /// <summary>
        /// Gets or sets the current table alias being used in the query.
        /// </summary>
        public string CurrentTableAlias => CurrentSource.Alias;

        /// <summary>
        /// Checks if the current member has a valid alias
        /// </summary>
        public bool CurrentHasAlias => CurrentSource.HasAlias;


        /// <summary>
        /// Gets the current depth of the query context, based on the number of options in the stack.
        /// </summary>
        public int Depth => StackList.Count;

        /// <summary>
        /// Token used for indentation in SQL queries, repeated based on depth.
        /// </summary>
        public string IndentToken =>
            string.Concat(Enumerable.Repeat(IndentationPlacement, Depth));

        /// <summary>
        /// Stack of build options used during query construction.
        /// </summary>
        protected Stack<(ISqlSource Source, BuildOptions Options)> StackList { get; } = new([(firstBuilder, firstoptions)]);

        /// <summary>
        /// Keeps track of tables that have already been rendered in the query.
        /// </summary>
        internal HashSet<SqlBuilder> RenderedTables { get; set; } = [];

        /// <summary>
        /// Gets the table aliases
        /// </summary>
        public string[] TableAlias => StackList.Select(x => x.Source.Alias).ToArray();

        /// <summary>
        /// Gets the table sources
        /// </summary>
        public IEnumerable<ISqlSource> Sources => StackList.Select(x => x.Source);


        /// <summary>
        /// Increases the indentation level by pushing the current options onto the stack.
        /// </summary>
        public UnindentClass Indent(ISqlSource source) => Indent(source, CurrentOptions.Clone(null));

        /// <summary>
        /// Increases the indentation level using the specified options.
        /// </summary>
        /// <param name="options">The options to push onto the stack.</param>
        public UnindentClass Indent(ISqlSource source, BuildOptions options)
        {
            ArgumentNullException.ThrowIfNull(source);

            StackList.Push((source, options));

            return new(Unindent);
        }

        /// <summary>
        /// Unindent Class
        /// </summary>
        /// <param name="unindent"></param>
        public class UnindentClass(Action unindent) : IDisposable
        {
            //public Action Unindent => unindent;
            void IDisposable.Dispose()
            {
                unindent();
                GC.SuppressFinalize(this);
            }
        }

        /// <summary>
        /// Decreases the indentation level by popping the last options from the stack.
        /// </summary>
        private void Unindent()
        {
            StackList.TryPop(out _);
        }

        public override string ToString()
        {
            return $"{nameof(BuildContext)} [ {nameof(Depth)}: {Depth} {nameof(TableAlias)}: {string.Join(',', value: TableAlias)} ]";
        }
    }
}
