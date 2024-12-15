/*
  Codigo generado por Dr. Arturo Juárez Flores
  Investigación UTC
*/

//using Microsoft.AspNetCore.Mvc;

using HuskyKit.Sql;

namespace HuskyKit.Datatables
{
    public class ColumnItem
    {
        public SqlColumn GetColumn(OrderItem? order)
        {

            ColumnOrder? columnOrder = null;

            if (order != null && order.ColumnIx.HasValue)
            {
                columnOrder = new()
                {
                    Index = order.ColumnIx.Value,
                    Direction = order.Dir is null ? OrderDirection.NONE : order.Dir.Equals("desc") ? OrderDirection.DESC : OrderDirection.ASC
                };
            }
    
            var ret = new SqlColumn(
                Name ?? Data ?? throw new ArgumentNullException(nameof(Name)),
                Name ?? Data ?? throw new ArgumentNullException(nameof(Name)),
                false,
                columnOrder ?? new()
            );
             

            return ret;
        }

        //[FromForm()]
        //[FromQuery]
        public string? Data { get; set; }

        //[FromForm()]
        //[FromQuery]
        public string? Name { get; set; }

        //[FromForm()]
        //[FromQuery]
        public SearchItem Search { get; set; } = new();

        //[FromForm()]
        //[FromQuery]
        public bool Searchable { get; set; }

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
    }
}