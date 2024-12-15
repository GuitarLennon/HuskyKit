using System;
using System.Collections;
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

        public static SqlColumn Count(string alias = "Count") => "Count(*)".As(alias, true);

        public static object Round(this object expression, int precision) => $"Round({expression}, {precision})";
        public static object Convert(this object expression, string dbType) => $"Convert({dbType}, {expression})";
        public static SqlColumn StringAgg(this object expression, string separator, string alias)
            => $"String_agg({expression}, {separator})".As(alias, true);

        public static SqlColumn JsonObject(string[] columns, string alias, bool aggregate = false)
            => JsonObject(columns).As(alias, aggregate);

        public static string JsonObject(Dictionary<string, string> columns)
        {
            var r =
            columns
                .Select((x, i) => $"\n'{(i == 0 ? "" : ",")}\"{x.Key}\":' + COALESCE(CONVERT(VARCHAR(MAX), [{x.Value}]), 'null')  + ")
                .Aggregate("", (a, b) => a + " " + b);

            var r2 = $"'{{{{' + {r} + '}}}}'";

            return r2;
        }

        public static string JsonObject(string[] columns)
            => JsonObject(columns.ToDictionary(x => x, x => x));

        public static SqlColumn JsonKeyObject(string Key, object expression, string alias) 
        {    
            var r = $"String_agg('\"' + CONVERT(VARCHAR(MAX), [{Key}]) + '\": ' + {expression}, ', ')";

            var r2 = $"'{{{{' + {r} + '}}}}'";

            return r2.As(alias, true);
        }

        public static SqlColumn StringAgg(this string columnName, string separator = ",", string? alias = null)
            => $"String_agg([{columnName}], '{separator}')".As(alias ?? columnName, true);


        public static SqlColumn CaseOrderIndex(string columnName, string[] columns, string alias) =>
            ($"CASE " + string.Join(" ", columns.Select((x, i) => $" WHEN [{{0}}].[{columnName}] = '{x}' THEN {i}").ToArray()) + " END").As(alias, false, new ColumnOrder() { Index = 0 });
    }
}
