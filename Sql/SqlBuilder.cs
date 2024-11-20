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
        /*
          public SqlBuilder(SqlBuilder source, string? Alias)
          {
              this.Alias = Alias ?? source.Alias;
              source.Indent++;
              Table = $"({source.Build()}) [{this.Alias}]";
          }
        */
        //public static SqlBuilder FromDTRequest(SqlBuilder ret, DTRequest dTRequest)
        //{
        //    ExtractColumns(ret, dTRequest);

        //    if (!string.IsNullOrWhiteSpace(dTRequest.Search.Value))
        //        ret.GeneralFilter(dTRequest.Search.Value);

        //    var ret_2 = FromColumns("ah?",
        //       new SqlColumn("draw", dTRequest.Draw),
        //       ret.AsCount("recordsFiltered")
        //    );

        //    if (dTRequest.Columns.Length > 0)
        //        ret_2.TableColumns.Add(ret.AsColumn("data", dTRequest.Start, dTRequest.Length));

        //    ret.WhereConditions.Clear();

        //    ret_2.TableColumns.Add(ret.AsCount("recordsTotal"));

        //    return ret_2;
        //}

        //private static void ExtractColumns(SqlBuilder ret, DTRequest dTRequest)
        //{
        //    for (int i = 0; i < dTRequest.Columns.Length; i++)
        //    {
        //        ColumnItem? item = dTRequest.Columns[i];
        //        if (item.Name != null)
        //        {
        //            SqlColumn Col = GetColumn(item);

        //            ret.TableColumns.Add(Col);

        //            if (item.Search.Value != null)
        //            {
        //                var sujeto = (Col.Aggregate || Operators.Any(x => Regex.IsMatch(item.Name, x, RegexOptions.IgnoreCase))) ?
        //                    Col.Expression : $"[{ret.Alias}].[{Col.Name}]";

        //                if (item.Search.Regex)
        //                    ret.WhereConditions.Add($"CONVERT(VARCHAR, {sujeto}) LIKE '%{item.Search.Value}%'");
        //                else
        //                    ret.WhereConditions.Add($"CONVERT(VARCHAR, {sujeto}) = '{item.Search.Value}'");
        //            }
        //            GetOrder(dTRequest, i, Col);
        //        }
        //    }
        //}

        //private static void GetOrder(DTRequest dTRequest, int i, SqlColumn Col)
        //{
        //    var g = dTRequest.Order
        //        .Select((x, i) => (i, x))
        //        .FirstOrDefault(x => x.x.ColumnIx.HasValue && x.x.ColumnIx.Value == i);

        //    if (g.x?.Dir != null)
        //        Col.Order = (g.i, g.x.Dir.Trim().Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? OrderDirection.DESC : OrderDirection.ASC);
        //}

        //private static SqlColumn GetColumn(ColumnItem item)
        //{
        //    SqlColumn Col;
        //    var name = item.Name ?? throw new InvalidOperationException();
        //    var columnAlias = item.Data ?? item.Name ?? throw new InvalidOperationException();
        //    var columnExpression = item.Name;

        //    if (AggregateFunctions.Any(x => Regex.IsMatch(name, x, RegexOptions.IgnoreCase | RegexOptions.Multiline)))
        //    {
        //        Col = new SqlColumn(columnAlias, name, aggregate: true);
        //    }
        //    else if (Operators.Any(x => Regex.IsMatch(name, x, RegexOptions.IgnoreCase | RegexOptions.Multiline)))
        //    {
        //        Col = new SqlColumn(columnAlias, name, aggregate: false);
        //    }
        //    else
        //    {
        //        Col = new SqlColumn(columnAlias, columnExpression);
        //    }

        //    return Col;
        //}


        //public void ApplyDTRequest(DTRequest request, bool applyLimits = false)
        //{
        //    if (applyLimits)
        //    {
        //        Skip = request.Start;
        //        Lenght = request.Length;
        //    }

        //    var múltiple = ApplyColumns(request);

        //    if (!string.IsNullOrWhiteSpace(request.Search.Value))
        //    {
        //        WhereConditions.Add("(" + múltiple.Aggregate((a, b) => a + " OR " + b) + ")");
        //    }

        //}

        //private List<string> ApplyColumns(DTRequest request)
        //{
        //    List<string> múltiple = new();

        //    foreach (var item in Columns)
        //    {
        //        for (int i = 0; i < request.Columns.Length; i++)
        //        {
        //            ColumnItem? outer = request.Columns[i];
        //            if (item.Column.Name.Equals(outer.Name, StringComparison.OrdinalIgnoreCase))
        //            {
        //                ApplyWhere(request, múltiple, item, outer);

        //                ApplyOrder(request, item, i);
        //            }
        //        }
        //    }

        //    return múltiple;
        //}

        //private void ApplyWhere(DTRequest request, List<string> múltiple, (string Alias, SqlColumn Column) item, ColumnItem outer)
        //{
        //    if (!string.IsNullOrWhiteSpace(request.Search.Value))
        //        if (request.Search.Regex)
        //            múltiple.Add(item.Column.GetWhereExpression(item.Alias, $" LIKE '%{request.Search.Value}%'"));
        //        else
        //            múltiple.Add(item.Column.GetWhereExpression(item.Alias, $" = '{request.Search.Value}'"));


        //    if (!string.IsNullOrWhiteSpace(outer.Search?.Value))
        //        if (outer.Search.Regex)
        //            WhereConditions.Add(item.Column.GetWhereExpression(item.Alias, $" LIKE '%{outer.Search.Value}%'"));
        //        else
        //            WhereConditions.Add(item.Column.GetWhereExpression(item.Alias, $" = '{outer.Search.Value}'"));
        //}

        //private static void ApplyOrder(DTRequest request, (string Alias, SqlColumn Column) item, int i)
        //{
        //    var order = request.Order
        //        .Select((x, ix) => new { Order = x, Index = ix })
        //        .FirstOrDefault(x => x.Order.ColumnIx == i);

        //    if (order != null && !string.IsNullOrWhiteSpace(order.Order.Dir))
        //        item.Column.Order = (order.Index, "asc".Equals(order.Order.Dir,
        //            StringComparison.CurrentCultureIgnoreCase) ? OrderDirection.ASC : OrderDirection.DESC);
        //}

        //public SqlBuilder AsDTResponse(int draw, int? skip = null, int? length = null)
        //{
        //    var ret = FromColumns("DTResponse",
        //        new SqlColumn(nameof(DTResponse.Draw), draw),
        //        AsColumn(nameof(DTResponse.Data), skip, length),
        //        AsColumn(nameof(DTResponse.RecordsTotal)),
        //        AsColumn(nameof(DTResponse.RecordsFiltered))
        //    );

        //    return ret;
        //}

        //private static string[] AggregateFunctions { get; }
        //  = [
        //      @"APPROX_COUNT_DISTINCT\(.*",
        //        @"AVG\(.*",
        //        @"CHECKSUM_AGG\(.*",
        //        @"COUNT\(.*",
        //        @"COUNT_BIG\(.*",
        //        @"GROUPING\(.*",
        //        @"GROUPING_ID\(.*",
        //        @"MAX\(.*",
        //        @"MIN\(.*",
        //        @"STDEV\(.*",
        //        @"STDEVP\(.*",
        //        @"STRING_AGG\(.*",
        //        @"SUM\(.*",
        //        @"VAR\(.*",
        //        @"VARP\(.*"
        //  ];

        //private static string[] Operators { get; }
        //    = [
        //        @".*\+.*",
        //        @".*\-.*",
        //        @".*\*.*",
        //        @".*\/.*",
        //        @".*\%.*",
        //        @".*\&.*",
        //        @".*\|.*",
        //        @".*\^.*",
        //        @".*\=.*",
        //        @".*\>.*",
        //        @".*\<.*",
        //        @".*\>=.*",
        //        @".*\<=.*",
        //        @".*\<\>.*",
        //        @"CASE.*WHEN.*THEN.*ELSE.*END",
        //        @"ISNULL\(.*\)",
        //        @"\[.*\]\.\[.*\]",
        //        @".*\.\[.*\]",
        //        @"\[.*\]\..*",
        //        @".*\..*",
        //        @"[a-zA-Z\(.*\)]",
        //    ];


    }

    public partial class SqlBuilder
    {
        public override string ToString()
        {
            return Build();
        }

        public static SqlBuilder With(params SqlBuilder[] Subqueries)
        {
            var r = new SqlBuilder(string.Empty);

            foreach (var query in Subqueries)
                r.WithTables.Add(query);

            return r;
        }

 


        public static SqlBuilder SelectFrom(
                (string schema, string table, string? asAlias) FromTable)
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
 
        public SqlBuilder AsSubquery(params TableJoin[] joins)
        {
            var ret = new SqlBuilder(this, "sq");

            if (joins != null && joins.Length != 0)
                ret.Joins.AddRange(joins);

            foreach (var item in ColumnsWithAlias)
                ret.TableColumns.Add(new SqlColumn(item.Column.Name ?? throw new ArgumentNullException(nameof(joins), "Columna sin nombre")));

            return ret;
        }

    }

    public partial class SqlBuilder
    {
        
        public static implicit operator SqlBuilder(SqlColumn[] columns )
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
            this.Alias = Alias;

            Table = $"[{WithSource.Alias}]";

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
            this.Alias = Alias ?? RawTable.Name;
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

        public List<SqlColumnAbstract> TableColumns { get; protected set; } = [];

        public List<string> WhereConditions { get; } = [];

        internal IEnumerable<(string Alias, SqlColumnAbstract Column)> ColumnsWithAlias =>
            Joins.SelectMany(x => x.Columns.Select(y => (x.Alias, Column: y)))
            .Union(TableColumns.Select(x => (Alias, Column: x)));

        internal ICollection<SqlBuilder> WithTables { get; } = [];


        public class BuildOptions
        {
            public int? Skip { get; set; }
            
            public int? Length { get; set; }
            
            public bool ForJson { get; set; }
            
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
                bool? ForJson = null,
                int? Skip = null,
                int? Length = null
            )
            {
                var g = (BuildOptions) MemberwiseClone();
                g.Indentation = Indentation;
                g.ForJson = ForJson ?? g.ForJson;
                g.Skip = Skip ?? g.Skip;
                g.Length = Length ?? g.Length;
                return g;
            }
        }

        public string Build(BuildOptions? options = default)
        {
            if (!ColumnsWithAlias.Any())
                throw new InvalidOperationException("No se especificaron columnas que seleccionar");

            options ??= new BuildOptions();

            options.Indent();
            
            var skip = options.Skip ?? Skip;
            var length = options.Length ?? Length;
            var _indent = options.IndentToken;


            
            (string top_sql, string order_sql) = 
                DetermineOffset(options.Clone(Skip: skip, Length: length));

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
                sb.Append(_indent);
            sb.Append(order_sql);

            if (options.ForJson)
                sb.Append($"{_indent} FOR JSON PATH ");

            if (!string.IsNullOrWhiteSpace(QueryOptions))
            {
                sb.Append(_indent);
                sb.Append(QueryOptions);
            }

            options.Unindent();
            
            var returning = sb.ToString();

            if(options.Depth == 0)
                returning = returning.Replace(options.IndentationPlacement, options.Indentation);

            return returning;
        }

        public string BuildAsJson(BuildOptions? options = default, string? ForJsonOptions = null)
        {
            var builder = 
                With([.. WithTables])
                .Select(
                    this.AsColumn("TEXT", forJson:true)
                );

            options ??= new BuildOptions();
            return builder.Build(options);
        }

        //public string Count(string Alias, string? ColumnName = "*", bool Indent = true)
        //{
        //    var _indent = "\n" + new string('\t', Indent ? this.Indent + 1 : this.Indent);

        //    var select_sql = $"Count({ColumnName}) AS [{Alias}]";

        //    var where = WhereConditions.Count != 0 ? $"{_indent}WHERE " + WhereConditions
        //        .Aggregate((a, b) => a + $" AND {_indent}\t" + b) : string.Empty;

        //    var joins_sql = Joins.Count != 0 ? Joins.Select(x => x.GetSqlExpression(this.Alias))
        //        .Aggregate(_indent, (a, b) => a + $"{_indent}\t" + b) : string.Empty;

        //    var from_sql = string.IsNullOrEmpty(Table) ?
        //        Joins.Count != 0 ? throw new InvalidOperationException("No hay from") : string.Empty :
        //        string.IsNullOrWhiteSpace(this.Alias) ? $"{_indent}FROM {Table}" : $"{_indent}FROM {Table} [{this.Alias}]";

        //    if (Columns.Any(x => x.Column.Aggregate))
        //    {
        //        var groupBySql = $"{_indent}\t  GROUP BY " +
        //        Columns.Where(x => !x.Column.Aggregate)
        //            .Select(x => x.Column.GetGroupByExpression(x.Alias))
        //            .Aggregate((a, b) => a + $"{_indent}\t\t, " + b);


        //        var subselect = Columns.Where(x => x.Column.Aggregate)
        //            .Select(x => _indent + "\n" + x.Column.GetSelectExpression(x.Alias))
        //            .Aggregate((a, b) => a + "," + b);

        //        return $"SELECT {select_sql} FROM (SELECT {subselect} {from_sql} {joins_sql} {where} {groupBySql}) X";
        //    }

        //    return $"SELECT {select_sql} {from_sql} {joins_sql} {where}";
        //}

        public void GeneralFilter(string filter)
        {
            var g = ColumnsWithAlias.Select(x => $"[{x.Alias}].[{x.Column.Name}] LIKE '%{filter}%'")
                .Aggregate((a, b) => a + " OR " + b);

            WhereConditions.Add($"({g})");
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
            if (!ColumnsWithAlias.Any(x => x.Column.Aggregate))
                return;


            sb.AppendLine();
            sb.Append(options.IndentToken);
            sb.Append("  GROUP BY ");

            var first = true;

            options.Indent(2);
            foreach (var columnInfo in ColumnsWithAlias.Where(x => !x.Column.Aggregate))
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
                        order_sql += $"{_indent}\tOFFSET {options.Skip.Value} ROWS FETCH NEXT {options.Length.Value} ROWS ONLY";
                    else
                        order_sql += $"{_indent}\t\tOFFSET {options.Skip.Value} ROWS ";
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
                order_sql = $"ORDER BY 1 OFFSET {options.Skip.Value} ROWS";

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
                first = false;
            }

            foreach (var col in ColumnsWithAlias)
            {

                if (!first)
                {
                    sb.Append(_indent);
                    sb.Append(options.IndentationPlacement);
                    sb.Append(options.IndentationPlacement);
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

            foreach(var join in Joins)
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
                    //sb.Append("  WHERE ");
                    sb.Append("   ");
                    sb.Append(" AND ");
                }
                sb.AppendLine(condition);

                first = false;
            }
        }

        private void DetermineWith(StringBuilder sb, BuildOptions options)
        {
            var withTables = GetWithTableBuilders().ToHashSet();

            if (withTables.Count == 0)
                return;

            withTables.Remove(this);

            sb.Append("\n\n;WITH ");

            bool first = true;
            foreach (var table in withTables)
            {
                if (!first)
                    sb.AppendLine(",");

                sb.AppendLine($"[{table.Alias}] AS (");

                sb.Append(options.IndentToken);

                sb.AppendLine(table.Build(options.Clone(false)));

                sb.AppendLine(")");
                /*
                if (!first)
                    sb.AppendLine();
                */
                first = false;
            }
        }
    }
}
