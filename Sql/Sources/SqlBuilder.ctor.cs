using HuskyKit.Sql.Columns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HuskyKit.Sql.Sources
{
    public partial class SqlBuilder : ISqlSource
    {
        /// <summary>
        /// Initializes a new instance of the SqlBuilder class with an SQL source.
        /// </summary>
        /// <param name="sqlSource">The SQL source object.</param>
        internal SqlBuilder(ISqlSource sqlSource)
        {
            Alias = $"sourceless{count++}";
            From_Source = sqlSource;
        }

        /// <summary>
        /// Initializes a new instance of the SqlBuilder class with a raw SQL table.
        /// </summary>
        /// <param name="rawTable">The raw table object to include in the query.</param>
        internal SqlBuilder(SqlTable rawTable) : this((ISqlSource)rawTable)
        {
        }

        /// <summary>
        /// Initializes a new instance of the SqlBuilder class with an alias.
        /// </summary>
        /// <param name="alias">The alias to use for the SQL source.</param>
        // internal SqlBuilder(string alias) => Alias = alias;

        /// <summary>
        /// Initializes a new instance of the SqlBuilder class with an alias.
        /// </summary>
        internal SqlBuilder()
        {
        }



        /// <summary>
        /// Initializes a new instance of the SqlBuilder class with a source, alias, and optional columns.
        /// </summary>
        /// <param name="source">The SQL source object used to build the query.</param>
        /// <param name="alias">The alias for the SQL source in the query.</param>
        /// <param name="columns">Optional array of SqlColumn objects to include in the query.</param>
        internal SqlBuilder(ISqlSource source, string? alias,
                            Predicate<(string Table, ISqlColumn SqlColumn)>? predicate = null,
                            Func<SqlColumn, SqlColumn>? Selector = null
            )
        {
            predicate ??= x => true;

            Selector ??= x => x;

            From_Source = source;
            Alias = alias!; //Can accept null

            if (source is SqlBuilder builder)
            {
                var g = builder.Columns
                    .Where(x => x.Column.IsSelectable && predicate(x))
                    .Select(c => new SqlColumn(
                                c.Column.Name ?? throw new InvalidOperationException(),
                                c.Column.Name,
                                false,
                                c.Column.Order
                            ))
                    .Select(Selector);

                LocalWithTables.Add(builder);
                TableColumns.AddRange([.. g]);
            }
        }
          
    }

}
