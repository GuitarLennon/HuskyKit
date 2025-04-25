using HuskyKit.Extensions;
using HuskyKit.Sql;
using HuskyKit.Sql.Columns;
using System.Collections;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Linq.Expressions;
using System.Net.Http.Headers;
using System.Text;

/// <summary>
/// Core class for building SQL queries dynamically.
/// </summary>
namespace HuskyKit.Sql.Sources
{
    // Propiedades
    public partial class SqlBuilder : ISqlSource
    {

        private static int count = 0;

        private readonly int ID = count++;

        private int? length;

        private string? m_custom_alias;

        bool ISqlSource.HasAlias => !string.IsNullOrWhiteSpace(m_custom_alias) || (From_Source?.HasAlias ?? false);

        /// <summary>
        /// Gets or sets the alias for the SQL source.
        /// </summary>
        public string Alias
        {
            get
            {
                if (string.IsNullOrWhiteSpace(m_custom_alias))
                {
                    if (From_Source is null)
                        return $"No-alias-{ID}";

                    if (From_Source is SqlTable table)
                        return table.Alias;

                    return From_Source.Alias;
                }
                return m_custom_alias;

            }
            set
            {
                m_custom_alias = value;
            }
        }

        /// <summary>
        /// Gets the columns with their associated aliases.
        /// </summary>
        public IEnumerable<(string TableAlias, ISqlColumn Column)> Columns
        {
            get
            {
                foreach (var column in TableColumns)
                {
                    yield return (From_Source?.Alias ?? Alias, column);
                }

                foreach (var join in Joins)
                {
                    foreach (var column in join.Columns)
                    {
                        yield return (join.Alias, column);
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the SQL source object used to build the query.
        /// </summary>
        public ISqlSource? From_Source { get; set; }

        /// <summary>
        /// Gets or sets the SQL builder that represents the set operator
        /// </summary>
        public List<SetOperation> SetOperations { get; set; } = [];


        /// <summary>
        /// Gets the list of table joins included in the query.
        /// </summary>
        public List<TableJoin> Joins { get; } = [];

        /// <summary>
        /// Gets or sets the number of rows to return (used for pagination).
        /// </summary>
        public int? Length { get => length; set => length = value < 0 ? null : value; }

        /// <summary>
        /// Gets or sets the local parameters on SQL Builder
        /// </summary>
        public IDictionary<string, object?> LocalParameters { get; set; } = new Dictionary<string, object?>();

        /// <summary>
        /// Gets the list of WHERE conditions applied to the query.
        /// </summary>
        public List<Func<BuildContext, string>> LocalWhereConditions { get; } = [];

        /// <summary>
        /// Gets the current <see cref="OrderByClause"/>  clauses of this query
        /// </summary>
        public IEnumerable<OrderByClause> OrderByClauses =>
            LocalOrderByClauses.Union(
                Columns
                    .Where(x => x.Column.Order.Direction != OrderDirection.NONE)
                    .OrderBy(x => x.Column.Order.Index)
                    .Select(x => new OrderByClause(x.Column))
            );

        /// <summary>
        /// Gets or sets parameters
        /// </summary>
        public IReadOnlyDictionary<string, object?> Parameters =>
                    new ReadOnlyDictionary<string, object?>(
                        GetNestedBuilders(true)
                        .Distinct()
                        .SelectMany(x => x.LocalParameters)
                        .ToDictionary(x => x.Key, x => x.Value)
                    );

        /// <summary>
        /// Gets or sets the pre-query options (e.g., additional SQL clauses).
        /// </summary>
        public ICollection<string> PreQueryOptions { get; } = [];

        /// <summary>
        /// Gets or sets the post-query options (e.g., additional SQL clauses).
        /// </summary>
        public string? QueryOptions { get; set; }

        /// <summary>
        /// Gets or sets the number of rows to skip (used for pagination).
        /// </summary>
        public int? Skip { get; set; }

        /// <summary>
        /// Gets the list of columns included in the query.
        /// </summary>
        public List<ISqlColumn> TableColumns { get; internal set; } = [];

        /// <summary>
        /// Gets the list of WHERE conditions applied to the query.
        /// </summary>
        public IEnumerable<Func<BuildContext, string>> WhereConditions => [.. LocalWhereConditions];

        public IDictionary<string, SqlBuilder> WithTables =>
                    GetNestedBuilders(false)
                    .Distinct()
                    .ToDictionary(x => x.Alias, x => x);

        /// <summary>
        /// Gets the list of orders used in the query.
        /// </summary>
        internal HashSet<OrderByClause> LocalOrderByClauses { get; } = [];

        /// <summary>
        /// Gets the list of subqueries used in the query.
        /// </summary>
        internal List<SqlBuilder> LocalWithTables { get; } = [];

        public virtual ISqlColumn this[string name]
        {
            get
            {
                var a = Columns.FirstOrDefault(x => x.Column.Name.Equals(name)).Column;

                if (a != null) return a;

                if (From_Source is SqlBuilder b)
                    // Column comes from subquery
                    return b.Columns.First(x => x.Column.Name.Equals(name)).Column;
                else
                    //Column is physical table or view (might be unspecified)
                    return new SqlColumn(name);

                throw new InvalidOperationException();
            }

        }

        /// <summary>
        /// Returns the SQL query string representation of the SqlBuilder object.
        /// </summary>
        /// <returns>The SQL query string.</returns>
        public override string ToString() => Build();
    }
}
