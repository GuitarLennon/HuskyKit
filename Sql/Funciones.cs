using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HuskyKit.Sql
{
    public static class Funciones
    {
        public static object GetDate() => "GetDate()";

        public static object Count() => "Count(*)";
        public static object Round(this object expression, int precision) => $"Round({expression}, {precision})";
        public static object Convert(this object expression, string dbType) => $"Convert({dbType}, {expression})";

    }
}
