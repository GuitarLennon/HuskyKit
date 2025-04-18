/*
  Codigo generado por Dr. Arturo Juárez Flores
  Investigación UTC
*/

//using Microsoft.AspNetCore.Mvc;

using HuskyKit.Sql.Columns;

namespace HuskyKit.Datatables
{
    public class ColumnItem : IEquatable<ColumnItem>
    {
        public SqlColumn GetColumn(bool useOrder = true)
        {
            ColumnOrder? columnOrder = null;

            if (useOrder && Order != null && Order.Column.HasValue)
            {
                columnOrder = new(
                    Order.Column.Value,
                    Order.Direction 
                );
            }

            var ex = new InvalidOperationException(nameof(Name) + " and " + nameof(Data) + " has not a value");

            var rawName = string.IsNullOrWhiteSpace(Name) ?
                            string.IsNullOrWhiteSpace(Data) ?
                                throw ex : Data : Name;

            var rawData = string.IsNullOrWhiteSpace(Data) ?
                            string.IsNullOrWhiteSpace(Name) ?
                                throw ex : Name : Data;

            var ret = new SqlColumn(
                rawData,
                rawName,
                false,
                columnOrder 
            );
             

            return ret;
        }

        public string? Data { get; set; }

        public string? Name { get; set; }

        public SearchItem Search { get; set; } = new();

        public bool Searchable { get; set; }

        /// <summary>
        /// Order item for this column
        /// </summary>
        public OrderItem? Order { get; set; }

        public override string ToString()
        {
            return $"{Name} ({Data}) Search:{Search}";
        }

        public override int GetHashCode()
        {
            var hash = new HashCode();

            hash.Add(Data);
            hash.Add(Name);
            hash.Add(Search);
            hash.Add(Searchable);

            return hash.ToHashCode();
        }

        public bool Equals(ColumnItem? other)
            => GetHashCode().Equals(other?.GetHashCode());
    }
}