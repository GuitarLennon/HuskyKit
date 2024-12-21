using HuskyKit.Sql.Columns;

namespace HuskyKit.Sql.Sources
{
    internal class SqlBuilderResolver(string Alias) : SqlBuilder(Alias)
    {
        public override ISqlColumn this[string name]
        {
            get
            {
                return new SqlColumn(name);
                //return new SqlColumn($"[{{1}}].[{name}]", name);
            }
        }
    }
}
