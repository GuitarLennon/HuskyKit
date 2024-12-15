/*
  Codigo generado por Dr. Arturo Juárez Flores
  Investigación UTC
*/

//using Microsoft.AspNetCore.Mvc;

namespace HuskyKit.Datatables
{
    public class SearchItem
    {
        //[FromForm()]
        //[FromQuery]
        public bool Regex { get; set; }

        //[FromForm()]
        //[FromQuery]
        public string? Value { get; set; }

        public override string ToString()
        {
            return $"{Value} (Regex: {Regex})";
        }

        public override int GetHashCode()
        {
            var hash = new HashCode();

            hash.Add(Regex);
            hash.Add(Value);

            return hash.ToHashCode();
        }
    }
}