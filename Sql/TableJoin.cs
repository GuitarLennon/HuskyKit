namespace HuskyKit.Sql
{
    public class TableJoin
    {
        public TableJoin(JoinTypes joinTypes, string qualifiedTarget, string? alias = null, string? predicate = null, IEnumerable<SqlColumnAbstract>? columns = null)
        {
            JoinType = joinTypes;
            Target = qualifiedTarget;
            Predicate = predicate;
            Alias = alias ?? GetHashCode().ToString();
            if (columns != null)
                Columns.AddRange(columns);
        }

        public TableJoin(JoinTypes joinTypes, (string Schema, string Name) rawTable, string? alias = null, string? predicate = null, IEnumerable<SqlColumnAbstract>? columns = null)
            : this(joinTypes, $"[{rawTable.Schema}].[{rawTable.Name}]", alias ?? rawTable.Name, predicate, columns)
        {

        }

        public string Alias { get; set; }

        public List<SqlColumnAbstract> Columns { get; protected set; } = new();

        public string GetSqlExpression(string fromAlias, string? indent = null) =>
            $"{indent}  {JoinType.ToString().Replace('_', ' ')} JOIN " + 
            (Target == Alias ? $"[{Alias}]" : $"{Target} AS [{Alias}]") +
            (!string.IsNullOrWhiteSpace(Predicate) ? $"\n{indent}    ON {Predicate}" : string.Empty);

        public JoinTypes JoinType { get; set; }

        public string? Predicate { get; set; }

        public string Target { get; set; }

        public override string ToString() => GetSqlExpression("{{From Table}}");

    }
}
