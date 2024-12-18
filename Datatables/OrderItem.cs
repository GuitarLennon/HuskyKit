/*
  Codigo generado por Dr. Arturo Juárez Flores
  Investigación UTC
*/

//using Microsoft.AspNetCore.Mvc;

using HuskyKit.Sql;

namespace HuskyKit.Datatables
{
    public class OrderItem
    {
        public int? Column { get; set; }

        public string? Dir { get; set; }


        public string? ColumnName { get; set; }

        public OrderDirection Direction =>
            Dir switch
            {
                "asc" => OrderDirection.ASC,
                "desc" => OrderDirection.DESC,
                _ => OrderDirection.NONE
            };

        public override int GetHashCode()
        {
            var hash = new HashCode();

            hash.Add(Column);
            hash.Add(Dir);

            return hash.ToHashCode();
        }
    }
}