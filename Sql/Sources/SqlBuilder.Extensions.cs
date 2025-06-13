using HuskyKit.Sql.Columns;

namespace HuskyKit.Sql.Sources
{
    public partial class SqlBuilder : ISqlSource
    {

        public static SqlBuilder From(ISqlSource sqlSource)
        {
            var ret = new SqlBuilder();

            ret.From(sqlSource);

            return ret;
        }

        public static SqlBuilder From(SqlTable sqlSource)
        {
            var ret = new SqlBuilder();

            ret.From(sqlSource);

            return ret;
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
