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
            From = sqlSource;
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
        internal SqlBuilder(string alias) => Alias = alias;

        /// <summary>
        /// Initializes a new instance of the SqlBuilder class with an alias.
        /// </summary>
        internal SqlBuilder()
        {
            //Alias = $"Alias-{count++}";
        }

        /// <summary>
        /// Initializes a new instance of the SqlBuilder class with a source, alias, and optional columns.
        /// </summary>
        /// <param name="source">The SQL source object used to build the query.</param>
        /// <param name="alias">The alias for the SQL source in the query.</param>
        /// <param name="columns">Optional array of SqlColumn objects to include in the query.</param>
        internal SqlBuilder(ISqlSource source, string? alias, params SqlColumn[] columns)
        {
            From = source;
            Alias = alias!; //Can accept null

            if (source is SqlBuilder builder)
            {
                LocalWithTables.Add(builder);
                TableColumns.AddRange(
                    builder.Columns.Select(c =>
                        new SqlColumn(
                            c.Column.Name ?? throw new InvalidOperationException(),
                            c.Column.Name,
                            false,
                            c.Column.Order
                        )
                    )
                );
            }

            TableColumns.AddRange(columns);
        }

        public static SqlBuilder Select(params ISqlColumn[] columns)
        {
            var ret = new SqlBuilder();

            ret.Select(columns);

            return ret;
        }

        public static SqlBuilder SelectAll(bool selectAll = true)
        {
            return new()
            {
                TableColumns = { new SqlWildCardColumn(selectAll) }
            };
        }

        /// <summary>
        /// Creates a new SqlBuilder instance with the specified subqueries added to the WITH clause.
        /// </summary>
        /// <param name="subqueries">An array of subqueries to include in the WITH clause.</param>
        /// <returns>A new SqlBuilder instance containing the specified subqueries.</returns>
        public static SqlBuilder With(params SqlBuilder[] subqueries)
        {
            //if (subqueries == null || subqueries.Length == 0)
            //throw new ArgumentException("At least one subquery must be provided.", nameof(subqueries));

            var result = new SqlBuilder();

            result.LocalWithTables.AddRange(subqueries);

            return result;
        }
    }

}
