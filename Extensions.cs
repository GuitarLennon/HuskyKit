using HuskyKit.Sql;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HuskyKit.Datatables
{
    public static class Extensions
    {

        public static SqlBuilder ApplyDTRequest(this SqlBuilder builder, DTRequest dTRequest, bool sort = true)
        {
            List<SqlColumn> columns = [];
            List<string> filters = [];

            for (int i = 0; i < dTRequest.Columns.Length; i++)
            {
                var column = dTRequest.Columns[i];

                var order = dTRequest.Order.FirstOrDefault(x => x.ColumnIx == i);

                var sqlColumn = column.GetColumn(sort ? order : default);

                columns.Add(sqlColumn);

                if (column.Searchable && !string.IsNullOrWhiteSpace(column.Search.Value))
                    if (column.Search.Regex)
                        filters.Add($"CONVERT(VARCHAR(4000),{sqlColumn.Expression}) LIKE '%{column.Search.Value}%'");
                    else
                        filters.Add($"CONVERT(VARCHAR(4000), {sqlColumn.Expression}) = '{column.Search.Value}'");

            }

            if (!string.IsNullOrWhiteSpace(dTRequest.Search.Value))
            {
                Func<SqlColumn, string> d = dTRequest.Search.Regex ?
                    x => $"CONVERT(VARCHAR(4000), {x.Expression}) LIKE '%{dTRequest.Search.Value}%' )" : x => $"{x.Expression} = '{dTRequest.Search.Value}'";

                filters.Add($"({columns.Select(d).Aggregate((a, b) => a + "\n OR " + b)})");
            }

            SqlBuilder unfiltered = SqlBuilder.SelectFrom(builder, "unf")
                .CleanSelect([.. columns]);


            SqlBuilder filtered =
                filters.Any() ?
                SqlBuilder.SelectFrom(unfiltered, "fil")
                .Where([.. filters]) : unfiltered;

            var subquery = SqlBuilder
                .With(builder, filtered)
                .Select(
                    dTRequest.Draw.As("draw"),
                    unfiltered.AsColumnCount("recordsTotal"),
                    filtered.AsColumnCount("recordsFiltered"),
                    filtered.AsColumn("data", default, dTRequest.Start, dTRequest.Length, ForJsonOptions.PATH | ForJsonOptions.INCLUDE_NULL_VALUES)
                );

            return subquery;
        }

    }
}
