using HuskyKit.Contracts;
using HuskyKit.Models;
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
    }
}
