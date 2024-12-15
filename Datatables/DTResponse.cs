/*
  Codigo generado por Dr. Arturo Juárez Flores
  Investigación UTC
*/

//using Newtonsoft.Json;

namespace HuskyKit.Datatables
{
    public class DTResponse
    {
        //[JsonProperty(PropertyName = "data")]
        public dynamic? Data { get; set; }

        //[JsonProperty(PropertyName = "draw")]
        public int Draw { get; set; }

        //[JsonProperty(PropertyName = "recordsFiltered")]
        public int RecordsFiltered { get; set; }

        //[JsonProperty(PropertyName = "recordsTotal")]
        public int RecordsTotal { get; set; }
    }
}