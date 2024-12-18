/*
  Codigo generado por Dr. Arturo Juárez Flores
  Investigación UTC
*/


namespace HuskyKit.Datatables
{
    public class DTRequest
    {
        public DTRequest() { }

        public ColumnItem[] Columns { get; set; } = [];

        public int? Draw { get; set; } 

        public int? Length { get; set; } = -1;

        public OrderItem[] Order { get; set; } = [];

        public SearchItem Search { get; set; } = new();

        public int? Start { get; set; }

        public void Refresh()
        {
            foreach (var order in Order)
            {
                if (order.Column.HasValue)
                {

                    var other = Columns[order.Column.Value];
                    order.ColumnName = other.Name;

                    order.ColumnName = other.Name ?? other.Data;

                    other.Order = order;
                }
            }
        }

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