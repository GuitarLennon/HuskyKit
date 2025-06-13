using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace HuskyKit.Models
{
    public class SqlResult
    {
        public SqlResult() { }

        public bool HasData => Columns.Length != 0 && Rows.Any();

        public required string[] Columns { get; init; }

        public required IReadOnlyList<object?[]> Rows { get; init; }

        public IEnumerable<T> GetColumn<T>(int position)
            => Rows.Select(x => x[position]).Cast<T>();

        public IEnumerable<T> GetColumn<T>(string column)
            => GetColumn<T>(Array.IndexOf(Columns, column));

        public IEnumerable<IDictionary<string, object?>> AsDictionaryRows()
        {
            foreach (var row in Rows)
            {
                yield return Columns.Zip(row, (col, val)
                    => new KeyValuePair<string, object?>(col, val))
                                    .ToDictionary(x => x.Key, x => x.Value);
            }
        }

        public string ToStringWithMaxColumnWidth(int maxColumnWidth)
        {
            int colCount = Columns.Length;

            // Calcular ancho máximo requerido para cada columna
            int[] columnWidths = new int[colCount];
            bool[] isNumeric = new bool[colCount];

            // Determinar si la columna es numérica
            for (int i = 0; i < colCount; i++)
            {
                isNumeric[i] = Rows.Any() && Rows.All(r => r[i] == null || IsNumber(r[i]));
                columnWidths[i] = Truncate(Columns[i], maxColumnWidth).Length;
            }

            foreach (var row in Rows)
            {
                for (int i = 0; i < colCount; i++)
                {
                    var cellStr = Truncate(row[i]?.ToString() ?? "", maxColumnWidth);
                    if (cellStr.Length > columnWidths[i])
                        columnWidths[i] = cellStr.Length;
                }
            }
            for (int i = 0; i < colCount; i++)
            {
                columnWidths[i] = Math.Min(columnWidths[i], maxColumnWidth);
            }

            var sb = new StringBuilder();

            // Encabezados
            sb.Append('|');
            for (int i = 0; i < colCount; i++)
            {
                var header = Truncate(Columns[i], maxColumnWidth);
                // Encabezados alineados a la izquierda siempre
                sb.Append(' ' + header.PadRight(columnWidths[i]));
                sb.Append(" |");
            }
            sb.AppendLine();

            // Filas
            foreach (var row in Rows)
            {
                sb.Append('|');
                for (int i = 0; i < colCount; i++)
                {
                    var cell = Truncate(row[i]?.ToString() ?? "", maxColumnWidth);
                    if (isNumeric[i])
                        sb.Append(' ' + cell.PadLeft(columnWidths[i]));
                    else
                        sb.Append(' ' + cell.PadRight(columnWidths[i]));
                    sb.Append(" |");
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }

        private static bool IsNumber(object? value)
        {
            if (value is null) 
                return false;

            return Type.GetTypeCode(value.GetType()) switch
            {
                TypeCode.Byte or TypeCode.SByte or TypeCode.UInt16 or TypeCode.UInt32 or TypeCode.UInt64 or TypeCode.Int16 or TypeCode.Int32 or TypeCode.Int64 or TypeCode.Decimal or TypeCode.Double or TypeCode.Single => true,
                _ => false,
            };
        }


        public override string ToString()
        {
            return ToStringWithMaxColumnWidth(50);
        }

        private static string Truncate(string value, int maxLength)
        {
            if (value.Length > maxLength)
                return value[..(maxLength - 3)] + "...";
            return value;
        }
    }
}