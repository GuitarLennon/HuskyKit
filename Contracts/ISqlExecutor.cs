using HuskyKit.Models;

namespace HuskyKit.Contracts
{
    public interface ISqlExecutor
    {
        /// <summary>
        /// Ejecuta la consulta SQL y devuelve los resultados como un arreglo de diccionarios por fila.
        /// </summary>
        Task<SqlResult> ExecuteAsync(string sql, CancellationToken cancellationToken = default);
    }
}


 