using HuskyKit.Contracts;
using HuskyKit.Models;
using HuskyKit.Sql.Columns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HuskyKit.Sql.Sources
{
    public partial class SqlBuilder
    {
        public Task<SqlResult> ExecuteAsync(ISqlExecutor sqlExecutor, CancellationToken cancellationToken)
        {
            var sql = Build();
            var result = sqlExecutor.ExecuteAsync(sql, cancellationToken);
            return result;
        }


        public async Task<SqlBuilder> CheckColumnsAsync(
            ISqlExecutor executor,
            CancellationToken cancellationToken = default)
        {
            if (From_Source is not SqlTable table)
                throw new InvalidOperationException("CheckColumnsAsync solo funciona con SqlTable.");

            string previewSql = From(("INFORMATION_SCHEMA", "COLUMNS", "C"))
                .Select("COLUMN_NAME")
                .IfWhere(!string.IsNullOrWhiteSpace(table.Schema), x => x["TABLE_SCHEMA"], table.Schema)
                .IfWhere(!string.IsNullOrWhiteSpace(table.Table), x => x["TABLE_NAME"], table.Table)
                .Build();

            var result = await executor.ExecuteAsync(previewSql, cancellationToken);

            if (!result.HasData)
                throw new InvalidOperationException("No se pudieron obtener columnas de la tabla.");

            return this.CleanSelect(
                [.. result.GetColumn<string>(0).Select(x => new SqlColumn(x))]);
        }
    }
}