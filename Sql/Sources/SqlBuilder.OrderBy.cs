using HuskyKit.Extensions;
using HuskyKit.Sql.Columns;
using System.Text;

namespace HuskyKit.Sql.Sources
{
    public partial class SqlBuilder : ISqlSource
    {

        private static void DetermineOffset(int? skip, int? length, StringBuilder sb)
        {
            // Generar OFFSET y FETCH si es aplicable
            if (skip.HasValue && skip.Value >= 0)
            {
                sb.Append($" OFFSET {skip.Value} ROWS");

                if (length.HasValue && length.Value > 0)
                {
                    sb.Append($" FETCH NEXT {length.Value} ROWS ONLY");
                }
            }
        }

        /// <summary>
        /// Determines and constructs the OFFSET and ORDER BY clauses for the query.
        /// </summary>
        /// <param name="context">The build context for constructing the query.</param>
        /// <returns>A tuple containing the TOP SQL clause and the ORDER BY SQL clause.</returns>
        private (string TopSql, Func<StringBuilder, StringBuilder> OrderDelegate) _DetermineOffset(BuildContext context)
        {
            // Inicialización de variables
            string TopSql = string.Empty;
            Func<StringBuilder, StringBuilder> OrderDelegate = sb => sb;

            var options = context.CurrentOptions;
            var skip = context.CurrentOptions.Skip ?? Skip;
            var length = context.CurrentOptions.Length ?? Length;

            var clauses = OrderByClauses.ToArray();

            // Determinar ORDER BY y OFFSET
            OrderDelegate = DeterminePagination(context, OrderDelegate, options, skip, length, clauses);

            if (!(skip.HasValue && skip.Value >= 0) && length.HasValue && length.Value > 0)
            {
                TopSql = $"TOP({length.Value}) ";
            }

            return (TopSql, OrderDelegate);
        }

        private static void DetermineOrder(BuildContext context, BuildOptions options, int? skip, int? length, OrderByClause[] clauses, StringBuilder sb)
        {
            // Validar condiciones en subtablas
            if (context.Depth > 1 &&
                (!skip.HasValue || skip.Value < 0) &&
                (!length.HasValue || length.Value < 0) &&
                !options.ForJson.HasValue)
            {
                // Encapsular advertencia y cláusulas en /* */
                sb.Comment($"No se puede utilizar ORDER BY debido a la falta de condiciones válidas.");
                sb.Comment($"-- Cláusulas encontradas:");

                foreach (var clause in clauses)
                {
                    sb.Comment($"{context.IndentToken}   {clause.GetSqlExpression(context)}");
                }


                //sb.Append($"ORDER BY ")
                //    .AppendLine(string.Join($",{Environment.NewLine}{context.IndentToken}       ",
                //        clauses.Select(clause => $"{clause.Expression} {clause.Direction}")))
                //    .AppendLine($"{Environment.NewLine}{context.IndentToken}OFFSET 0 ROWS");
            }
            else
            {
                // Generar cláusulas ORDER BY
                sb.Append($"ORDER BY ");
                sb.AppendLine(string.Join($",{Environment.NewLine}{context.IndentToken}       ",
                    clauses.Select(clause => clause.GetSqlExpression(context))));
            }
        }

        private static Func<StringBuilder, StringBuilder> DeterminePagination(
            BuildContext context,
            Func<StringBuilder, StringBuilder> OrderDelegate,
            BuildOptions options,
            int? skip,
            int? length,
            OrderByClause[] clauses)
        {
            if (clauses.Length > 0)
            {
                OrderDelegate = sb =>
                {
                    sb.Append(context.IndentToken);
                    sb.DebugComment($"{nameof(OrderDelegate)}()");

                    DetermineOrder(context, options, skip, length, clauses, sb);

                    DetermineOffset(skip, length, sb);

                    return sb;
                };
            }

            return OrderDelegate;
        }


        /// <summary>
        /// Elimina todas las cláusulas de ordenación (`ORDER BY`) actuales de la consulta SQL.
        /// </summary>
        /// <returns>El mismo <see cref="SqlBuilder"/> para permitir el encadenamiento de métodos.</returns>
        /// <remarks>
        /// Esta función también restablece la dirección e índice de orden en todas las columnas definidas en la consulta.
        /// </remarks>
        public SqlBuilder ClearOrder()
        {
            LocalOrderByClauses.Clear();
            foreach (var (_, Column) in Columns)
            {
                Column.Order.Reset();
            }

            return this;
        }

        /// <summary>
        /// Define una cláusula `ORDER BY` para las columnas especificadas con la dirección proporcionada.
        /// </summary>
        /// <param name="direction">La dirección de orden (ASC o DESC).</param>
        /// <param name="Columns">Un conjunto de nombres de columnas sobre las cuales aplicar la ordenación.</param>
        /// <returns>El mismo <see cref="SqlBuilder"/> para permitir el encadenamiento de métodos.</returns>
        /// <exception cref="InvalidOperationException">
        /// Se lanza cuando:
        /// - Alguna columna especificada no existe en la consulta.
        /// - Hay ambigüedad entre varias columnas con el mismo nombre pero diferentes alias de tabla.
        /// </exception>
        /// <remarks>
        /// - Primero, elimina cualquier orden existente llamando a <see cref="ClearOrder"/>.
        /// - Luego, intenta asignar la dirección de orden a cada columna especificada.
        /// - Si se encuentra más de una columna con el mismo nombre (y diferentes alias de tabla), se lanza una excepción.
        /// </remarks>
        public SqlBuilder OrderBy(OrderDirection direction, params string[] Columns)
        {
            ClearOrder();

            var groupJoin = Columns.GroupJoin(this.Columns, x => x, x => x.Column.Name, (x, y) => (x, y.ToArray()));

            int i = 0;
            foreach (var (name, columnsWithAlias) in groupJoin)
            {
                switch (columnsWithAlias.Length)
                {
                    case 0:
                        throw new InvalidOperationException($"La columna {name} no existe");

                    case 1:
                        columnsWithAlias[0].Column.Order.Direction = direction;
                        columnsWithAlias[0].Column.Order.Index = i++;
                        break;

                    default:
                        throw new InvalidOperationException($"Ambigüedad entre: {string.Join(",", columnsWithAlias.Select(x => $"{x.Column.Name}" + (x.TableAlias != Alias ? $" ({x.TableAlias})" : string.Empty))).ToArray()}");
                }
            }

            return this;
        }

        /// <summary>
        /// Define una cláusula `ORDER BY` para las columnas especificadas en orden ascendente (ASC).
        /// </summary>
        /// <param name="Columns">Un conjunto de nombres de columnas sobre las cuales aplicar la ordenación.</param>
        /// <returns>El mismo <see cref="SqlBuilder"/> para permitir el encadenamiento de métodos.</returns>
        /// <exception cref="InvalidOperationException">
        /// Se lanza cuando:
        /// - Alguna columna especificada no existe en la consulta.
        /// - Hay ambigüedad entre varias columnas con el mismo nombre pero diferentes alias de tabla.
        /// </exception>
        /// <remarks>
        /// - Primero, elimina cualquier orden existente llamando a <see cref="ClearOrder"/>.
        /// - Luego, asigna la dirección de orden ASC a cada columna especificada.
        /// - Si se encuentra más de una columna con el mismo nombre (y diferentes alias de tabla), se lanza una excepción.
        /// </remarks>
        public SqlBuilder OrderBy(params string[] Columns)
        {
            ClearOrder();

            var groupJoin = Columns.GroupJoin(this.Columns, x => x, x => x.Column.Name, (x, y) => (x, y.ToArray()));

            int i = 0;
            foreach (var (name, columnsWithAlias) in groupJoin)
            {
                switch (columnsWithAlias.Length)
                {
                    case 0:
                        throw new InvalidOperationException($"La columna {name} no existe");

                    case 1:
                        columnsWithAlias[0].Column.Order.Direction = OrderDirection.ASC;
                        columnsWithAlias[0].Column.Order.Index = i++;
                        break;

                    default:
                        throw new InvalidOperationException($"Ambigüedad entre: {string.Join(",", columnsWithAlias.Select(x => $"{x.Column.Name}" + (x.TableAlias != Alias ? $" ({x.TableAlias})" : string.Empty))).ToArray()}");
                }
            }

            return this;
        }

        public SqlBuilder OrderBy(params OrderByClause[] Columns)
        {
            ClearOrder();

            foreach (var column in Columns)
                LocalOrderByClauses.Add(column);

            return this;
        }
    }
}
