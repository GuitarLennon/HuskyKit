namespace HuskyKit.Models
{
    public class SqlResult
    {
        public required IReadOnlyList<string> Columns { get; init; }
        public required IReadOnlyList<object?[]> Rows { get; init; }

        public IEnumerable<IDictionary<string, object?>> AsDictionaryRows()
        {
            foreach (var row in Rows)
            {
                yield return Columns.Zip(row, (col, val) => new KeyValuePair<string, object?>(col, val))
                                    .ToDictionary(x => x.Key, x => x.Value);
            }
        }
    }

}