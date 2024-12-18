using HuskyKit.Sql;
using HuskyKit.Sql.Columns;
using HuskyKit.Sql.Sources;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HuskyKit.Datatables
{
    public record DataTablesRecord(SqlBuilder DTResponse, SqlBuilder FilteredData, SqlBuilder UnfilteredData, SqlQueryColumn Data)
    {
    }


    public static class Extensions
    {
        public static DataTablesRecord ApplyDTRequest(this SqlBuilder builder, DTRequest dTRequest, bool sort = true)
        {
            dTRequest.Refresh();

            List<SqlColumn> columns = [];
            List<string> filters = [];

            foreach (var column in dTRequest.Columns)
            {
                var sqlColumn = column.GetColumn(sort);

                if (column.Searchable && !string.IsNullOrWhiteSpace(column.Search.Value))
                    if (column.Search.Regex == true)
                        filters.Add($"CONVERT(VARCHAR(4000),{sqlColumn.Expression}) LIKE '%{column.Search.Value}%'");
                    else
                        filters.Add($"CONVERT(VARCHAR(4000), {sqlColumn.Expression}) = '{column.Search.Value}'");

                if (sqlColumn.IsMappedToColumn && !builder.Columns.Any(x => x.Column.Name == sqlColumn.Name))
                {
                    builder.PreQueryOptions += $"/* Warning: No se encontró la columna: {sqlColumn}*/\n";
                }
                else
                {
                    columns.Add(sqlColumn);
                }
            }

            if (!string.IsNullOrWhiteSpace(dTRequest.Search.Value))
            {
                Func<SqlColumn, string> d = dTRequest.Search.Regex == true ?
                    x => $"CONVERT(VARCHAR(4000), {x.Expression}) LIKE '%{dTRequest.Search.Value}%' )" : x => $"{x.Expression} = '{dTRequest.Search.Value}'";

                filters.Add($"({columns.Select(d).Aggregate((a, b) => a + "\n OR " + b)})");
            }

            SqlBuilder filtered = builder.AsSubquery("filtered")
                .CleanSelect([.. columns])
                .Where([.. filters]);

            var data = filtered.AsColumn("data", default, dTRequest.Start, dTRequest.Length, ForJsonOptions.PATH | ForJsonOptions.INCLUDE_NULL_VALUES);

            var subquery = SqlBuilder
                .With(builder, filtered)
                .Select(
                    dTRequest.Draw.As("draw"),
                    builder.AsColumnCount("recordsTotal"),
                    filtered.AsColumnCount("recordsFiltered"),
                    data
                );

            return new DataTablesRecord(subquery, filtered, builder, data);
        }

    }
}
