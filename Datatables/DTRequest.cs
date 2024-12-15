/*
  Codigo generado por Dr. Arturo Juárez Flores
  Investigación UTC
*/

//using Microsoft.AspNetCore.Mvc;

namespace HuskyKit.Datatables
{
    public class DTRequest
    {
        //[FromForm()]
        //[FromQuery]
        public ColumnItem[] Columns { get; set; } = [];

        //[FromForm()]
        //[FromQuery]
        public int Draw { get; set; }

        //[FromForm()]
        //[FromQuery]
        public int? Length { get; set; } = -1;

        //[FromForm()]
        //[FromQuery]
        public OrderItem[] Order { get; set; } = [];

        //[FromForm()]
        //[FromQuery]
        public SearchItem Search { get; set; } = new();

        //[FromForm()]
        //[FromQuery]
        public int? Start { get; set; }


        public override int GetHashCode()
        {
            var hash = new HashCode();

            foreach (var item in Columns)
                hash.Add(item);

            foreach (var item in Order)
                hash.Add(item);

            hash.Add(Search);

            return hash.ToHashCode();
        }
    }
}