using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HuskyKit.Extensions
{
    public static class Extensions
    {
#if DEBUG
        public static bool Debug { get; set; } = true;
#else
        public static bool Debug { get; set; } = false;
#endif
        public static StringBuilder DebugComment(this StringBuilder sb, string comment)
        {
            if (Debug)
                return Comment(sb, comment);
            return sb;
        }

        public static StringBuilder Append(this StringBuilder sb, bool condition, string @string)
        {
            if (condition)
                return sb.Append(@string);
            return sb;
        }

        public static StringBuilder AppendLine(this StringBuilder sb, bool condition, string @string)
        {
            if (condition)
                return sb.AppendLine(@string);
            return sb;
        }

        public static StringBuilder Comment(this StringBuilder sb, string comment)
        {
          

            // Obtener la indentación actual sin llamar a ToString()
            string indent = GetCurrentIndentation(sb);

            // Agregar el comentario en una nueva línea con la indentación actual
            if (sb.Length > 0 && !"\n\r".Contains(sb[^1]))
                sb.AppendLine();

            sb.Append(indent)
              .Append("/* ").Append(comment).AppendLine(" */")
              .Append(indent);

            return sb;
        }
        private static string GetCurrentIndentation(StringBuilder sb)
        {
            // Si no hay contenido, no hay indentación
            if (sb.Length == 0)
                return string.Empty;

            int index = sb.Length - 1;

            // Ir hacia atrás hasta encontrar el inicio de la última línea
            while (index >= 0 && sb[index] != '\n')
                index--;

            // Avanzar para saltar el salto de línea
            index++;

            // Construir la indentación solo con espacios y tabulaciones
            var indentBuilder = new StringBuilder();
            while (index < sb.Length && " \t{}".Contains(sb[index]))
            {
                indentBuilder.Append(sb[index]);
                index++;
            }

            return indentBuilder.ToString();
        }

    }
}
