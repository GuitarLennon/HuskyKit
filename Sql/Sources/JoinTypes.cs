namespace HuskyKit.Sql.Sources
{
    /// <summary>
    /// Representa los diferentes tipos de JOIN que se pueden utilizar en consultas SQL.
    /// </summary>
    public enum JoinTypes
    {
        /// <summary>
        /// Realiza un INNER JOIN, devolviendo solo las filas que tienen coincidencias en ambas tablas involucradas.
        /// </summary>
        INNER,

        /// <summary>
        /// Realiza un CROSS JOIN, devolviendo el producto cartesiano de las filas de las tablas involucradas.
        /// </summary>
        CROSS,

        /// <summary>
        /// Realiza un LEFT JOIN, devolviendo todas las filas de la tabla izquierda y las filas coincidentes de la tabla derecha. 
        /// Las filas sin coincidencias en la tabla derecha contendrán valores NULL.
        /// </summary>
        LEFT,

        /// <summary>
        /// Realiza un RIGHT JOIN, devolviendo todas las filas de la tabla derecha y las filas coincidentes de la tabla izquierda. 
        /// Las filas sin coincidencias en la tabla izquierda contendrán valores NULL.
        /// </summary>
        RIGHT,

        /// <summary>
        /// Realiza un FULL OUTER JOIN, devolviendo todas las filas de ambas tablas involucradas. 
        /// Las filas sin coincidencias en cualquiera de las tablas contendrán valores NULL para las columnas de la otra tabla.
        /// </summary>
        FULL_OUTER
    }
}
