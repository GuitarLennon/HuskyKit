/*
  Codigo generado por Dr. Arturo Juárez Flores
  Investigación UTC
*/

//using Microsoft.AspNetCore.Mvc;

namespace HuskyKit.Datatables
{
    public class OrderItem
    {
        //[FromForm()]
        //[FromQuery]
        public string? Column { get; set; }

        public int? ColumnIx => int.TryParse(Column, out int ix) ? ix : null;

        //[FromForm()]
        //[FromQuery]
        public string? Dir { get; set; }

        public override int GetHashCode()
        {
            var hash = new HashCode();

            hash.Add(Column);
            hash.Add(Dir);

            return hash.ToHashCode();
        }
    }
}