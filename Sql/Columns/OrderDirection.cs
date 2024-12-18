using System;

namespace HuskyKit.Sql
{

    /// <summary>
    /// Define las posibles direcciones de ordenación para una columna en SQL.
    /// </summary>
    public enum OrderDirection
    {
        /// <summary>
        /// No se aplica ninguna ordenación.
        /// </summary>
        NONE,

        /// <summary>
        /// Orden ascendente (ASC).
        /// </summary>
        ASC,

        /// <summary>
        /// Orden descendente (DESC).
        /// </summary>
        DESC
    }
}
