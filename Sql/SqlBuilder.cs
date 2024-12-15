using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Design;
using System.Runtime.CompilerServices;
using System;
using System.Drawing;

namespace HuskyKit.Sql
{
    public partial class SqlBuilder
    {

        public static SqlBuilder With(params SqlBuilder[] Subqueries)
        {
            var r = new SqlBuilder(string.Empty);

            foreach (var query in Subqueries)
                r.WithTables.Add(query);

            return r;
        }

        public static SqlBuilder SelectFrom(
                (string schema, string table, string Alias) FromTable)
        {
            return new SqlBuilder(FromTable);
        }

        public static SqlBuilder SelectFrom(
         (string schema, string table) FromTable)
        {
            return new SqlBuilder((FromTable.schema, FromTable.table, null));
        }

        public static SqlBuilder SelectFrom(
                SqlBuilder FromTable,
                string? asAlias = null)
        {
            return new SqlBuilder(FromTable, asAlias ?? FromTable.Alias);
        }

        public static SqlBuilder Select(params SqlColumnAbstract[] columns)
        {
            return new SqlBuilder(string.Empty)
            {
                TableColumns = [.. columns]
            };
        }

        public static SqlBuilder FromColumns(string Alias, params SqlColumnAbstract[] columns)
        {
            var ret = new SqlBuilder(Alias) { };

            ret.TableColumns.AddRange(columns);

            return ret;
        }

      
     
        public override string ToString()
        {
            return Build();
        }

        public SqlBuilder Top(int? top)
        {
            Length = top;
            return this;
        }

         

        public static implicit operator SqlBuilder(SqlColumn[] columns)
        {
            return Select(columns);
        }

        public static implicit operator SqlBuilder(string[] columns)
        {
            return Select(columns.Select(x => (SqlColumn)x).ToArray());
        }

        private string? m_alias;

        internal SqlBuilder(SqlBuilder WithSource, string Alias, params SqlColumn[] columns)
        {

            Table = $"[{WithSource.Alias}]";

            this.Alias = Alias;

            WithTables.Add(WithSource);

            if (columns.Length != 0)
                this.TableColumns.AddRange(columns);
            else
                foreach (var item in WithSource.ColumnsWithAlias)
                {
                    this.TableColumns.Add(new SqlColumn(item.Column.Name ?? throw new ArgumentNullException(nameof(columns), "Columna sin nombre")));
                }
        }

        internal SqlBuilder((string Schema, string Name, string? Alias) RawTable)
        {
            this.Alias = RawTable.Alias ?? RawTable.Name;
            Table = $"[{RawTable.Schema}].[{RawTable.Name}]";
        }

        internal SqlBuilder(string Alias)
        {
            this.Alias = Alias;
        }


        public string Alias { get => m_alias ?? GetHashCode().ToString(); set => m_alias = value; }

        public List<TableJoin> Joins { get; } = [];

        public int? Length { get; set; }

        public string? PreQueryOptions { get; set; }

        public string? QueryOptions { get; set; }

        public int? Skip { get; set; }


        public string? Table { get; set; }

        public List<SqlColumnAbstract> TableColumns { get; internal set; } = [];

        public List<string> WhereConditions { get; } = [];

        internal IEnumerable<(string Alias, SqlColumnAbstract Column)> ColumnsWithAlias =>
            Joins.SelectMany(x => x.Columns.Select(y => (x.Alias, Column: y)))
            .Union(TableColumns.Select(x => (Alias, Column: x)));

        internal ICollection<SqlBuilder> WithTables { get; } = [];



        public class BuildOptions
        {


            public int? Skip { get; set; }

            public int? Length { get; set; }

            public ForJsonOptions? ForJson { get; set; }

            public string Indentation { get; set; } = "  ";

            internal string IndentationPlacement { get; set; } = "{indent}";

            internal int Depth { get; set; }

            internal string IndentToken => string.Concat(Enumerable.Repeat(IndentationPlacement, Depth));

            internal string? CurrentTableAlias { get; private set; }

            public BuildOptions SetTable(string currentTableAlias)
            {
                CurrentTableAlias = currentTableAlias;
                return this;
            }

            public BuildOptions Indent(int depth = 1)
            {
                Depth += depth;
                return this;
            }

            public BuildOptions Unindent(int depth = 1)
            {
                Depth -= depth;
                return this;
            }

            public BuildOptions Clone(
                ForJsonOptions? ForJson,
                int? Skip = null,
                int? Length = null
            )
            {
                var g = (BuildOptions)MemberwiseClone();
                g.Indentation = Indentation;
                g.ForJson = ForJson;
                g.Skip = Skip ?? g.Skip;
                g.Length = Length ?? g.Length;
                return g;
            }
        }

        public string Build(BuildOptions? options = default)
        {
            if (!ColumnsWithAlias.Any())
                throw new InvalidOperationException($"No se especificaron columnas que seleccionar en [{Table}] [{Alias}]");

            options ??= new BuildOptions();

            options.Indent();

            var skip = options.Skip ?? Skip;
            var length = options.Length ?? Length;
            var _indent = options.IndentToken;



            (string top_sql, string order_sql) =
                DetermineOffset(options.Clone(options.ForJson, Skip: skip, Length: length));

            StringBuilder sb = new();

            if (!string.IsNullOrWhiteSpace(PreQueryOptions))
                sb.AppendLine(PreQueryOptions);

            if (options.Depth == 1)
                DetermineWith(sb, options);

            DetermineSelect(sb, top_sql, options);

            DetermineFrom(sb, options);

            DetermineWhere(sb, options);

            DetermineGroupBy(sb, options);

            if (!string.IsNullOrWhiteSpace(order_sql))
            {
                sb.Append(order_sql);
            }

            if (options.ForJson.HasValue)
            {
                sb.Append(_indent)
                  .Append(options.ForJson.Value.HasFlag(ForJsonOptions.PATH) ? "FOR JSON PATH" : "FOR JSON AUTO");

                var suboptions = new List<string?>
                {
                    options.ForJson.Value.HasFlag(ForJsonOptions.INCLUDE_NULL_VALUES) ? "INCLUDE_NULL_VALUES" : null,
                    options.ForJson.Value.HasFlag(ForJsonOptions.WITHOUT_ARRAY_WRAPPER) ? "WITHOUT_ARRAY_WRAPPER" : null
                }.Where(x => x != null);

                if (suboptions.Any())
                {
                    sb.Append(", ").Append(string.Join(" ", suboptions));
                }
            }


            if (!string.IsNullOrWhiteSpace(QueryOptions))
            {
                sb.Append(_indent).Append(QueryOptions);
            }

            options.Unindent();

            var returning = sb.ToString();

            if (options.Depth == 0)
                returning = returning.Replace(options.IndentationPlacement, options.Indentation);

            return returning;
        }

        public string BuildAsJson()
            => BuildAsJson(new BuildOptions());


        public string BuildAsJson(ForJsonOptions options)
            => BuildAsJson(new BuildOptions(), options);

        public string BuildAsJson(BuildOptions? options, ForJsonOptions innerJsonOptions = ForJsonOptions.AUTO)
        {
            var builder =
                With([.. WithTables])
                .Select(
                    this.AsColumn("TEXT", forJson: innerJsonOptions)
                );

            options ??= new BuildOptions();
            return builder.Build(options);
        }


        protected IEnumerable<SqlBuilder> GetWithTableBuilders()
            => WithTables
                .SelectMany(x => x.GetWithTableBuilders())
                //.Union(TableColumns.Where(x => x is SqlQueryColumn).SelectMany(x => ((SqlQueryColumn) x).SqlBuilder.GetWithTableBuilders()))
                .Append(this);

        private void DetermineGroupBy(
            StringBuilder sb,
            BuildOptions options
        )
        {
            //Verifica si existe alguna función de agregación
            if (!(ColumnsWithAlias.Any(x => x.Column.Aggregate) && ColumnsWithAlias.Any(x => !x.Column.Aggregate)))
                return;

            sb.AppendLine();
            sb.Append(options.IndentToken);
            sb.Append("  GROUP BY ");

            var first = true;

            var list = ColumnsWithAlias.Where(x => x.Column is SqlColumn && !x.Column.Aggregate).ToList();

            foreach (var column in ColumnsWithAlias
                    .Where(x => x.Column is SqlQueryColumn q && q.SqlBuilder.Table is null)
                    .SelectMany(x => ((SqlQueryColumn)x.Column).SqlBuilder.TableColumns.Select(y => (x.Alias, y)))
            )
            {
                list.Add(column);
            }

            options.Indent(2);
            foreach (var columnInfo in list)
            {
                if (!first)
                {
                    sb.Append(options.IndentToken);
                    sb.Append(',');
                }
                sb.AppendLine(columnInfo.Column.GetGroupByExpression(columnInfo.Alias, options));
                first = false;
            }
            options.Unindent(2);
        }

        private (string top_sql, string order_sql) DetermineOffset(
            BuildOptions options
        )
        {
            string top_sql = string.Empty;
            string order_sql = string.Empty;
            string _indent = options.IndentToken;

            if (ColumnsWithAlias.Any(x => x.Column.Order.Direction != OrderDirection.NONE))
            {
                order_sql = $"{_indent}  ORDER BY " +
                    ColumnsWithAlias.Where(x => x.Column.Order.Direction != OrderDirection.NONE)
                       .OrderBy(x => x.Column.Order.Index)
                       .Select(x => x.Column.GetOrderByExpression(x.Alias, options))
                       .Aggregate((a, b) => a + ", " + b);

                //pagination
                if (options.Skip.HasValue)
                    if (options.Length.HasValue)
                        order_sql += $"\n{_indent}    OFFSET {options.Skip.Value} ROWS FETCH NEXT {options.Length.Value} ROWS ONLY\n";
                    else
                        order_sql += $"\n{_indent}    OFFSET {options.Skip.Value} ROWS\n";
                else if (options.Length.HasValue)
                    top_sql = $"TOP({options.Length.Value})";
                else
                    top_sql = $"TOP(1000000)";

            }
            else if (options.Length.HasValue)
                if (options.Skip.HasValue)
                    order_sql = $"ORDER BY (SELECT NULL) OFFSET {options.Skip.Value} ROWS FETCH NEXT {options.Length.Value} ROWS ONLY";
                else
                    top_sql = $"TOP({options.Length})";

            else if (options.Skip.HasValue)
                order_sql = $"ORDER BY 1 OFFSET {options.Skip.Value} ROWS\n";

            return (top_sql, order_sql);
        }

        private void DetermineSelect(
            StringBuilder sb,
            string top_sql,
            BuildOptions options
        )
        {
            if (!ColumnsWithAlias.Any())
                throw new InvalidOperationException("No se especificó columna");

            var _indent = options.IndentToken;

            sb.Append(_indent);
            sb.Append("SELECT ");

            var first = true;

            if (!string.IsNullOrWhiteSpace(top_sql))
            {
                sb.AppendLine(top_sql);
                //first = false;
            }

            foreach (var col in ColumnsWithAlias)
            {

                if (!first)
                {
                    sb.Append(_indent);
                    sb.Append(options.IndentationPlacement);
                    sb.Append(options.IndentationPlacement);
                    //sb.Append(options.IndentationPlacement);
                    sb.Append(',');
                }

                sb.AppendLine(col.Column.GetSelectExpression(col.Alias, options));
                first = false;
            }
        }

        private void DetermineFrom(StringBuilder sb, BuildOptions options)
        {
            if (string.IsNullOrEmpty(Table))
            {
                if (Joins.Count != 0)
                    throw new InvalidOperationException("No hay from");
            }
            else
            {
                sb.Append(options.IndentToken);



                sb.Append($"  FROM {Table.Replace("\n", "\n" + new string('\t', options.Depth))} ");

                if (!string.IsNullOrWhiteSpace(this.Alias))
                    sb.Append($"[{this.Alias}]");

                sb.AppendLine();
            }

            foreach (var join in Joins)
            {
                sb.AppendLine(join.GetSqlExpression(this.Alias, options.IndentToken + "  "));
            }
        }

        private void DetermineWhere(StringBuilder sb, BuildOptions options)
        {
            if (WhereConditions.Count == 0)
                return;

            var _indent = options.IndentToken;

            sb.Append(_indent);
            sb.Append("  WHERE ");

            var first = true;

            foreach (var condition in WhereConditions)
            {
                if (!first)
                {
                    sb.Append(_indent);
                    sb.Append("   ");
                    sb.Append(" AND ");
                }
                sb.AppendLine(string.Format(condition, Alias));

                first = false;
            }
        }

        private void DetermineWith(StringBuilder sb, BuildOptions options)
        {
            var withTables = GetWithTableBuilders().ToHashSet();

            withTables.Remove(this);

            if (withTables.Count == 0)
                return;

            sb.Append("\n\n;WITH ");

            bool first = true;
            foreach (var table in withTables)
            {
                if (!first)
                    sb.AppendLine(",");

                sb.AppendLine($"[{table.Alias}] AS (");

                sb.Append(options.IndentToken);

                sb.AppendLine(table.Build(options.Clone(null)));

                sb.Append(")");
                /*
                if (!first)
                    sb.AppendLine();
                */
                first = false;
            }

            sb.AppendLine();

        }
    }
}
