namespace HuskyKit.Sql
{
    public static partial class SqlBuilderExtensions
    {

        public static SqlColumn As(this object expression, string AsAlias, bool aggregate = false, ColumnOrder order = default)
            => new (expression, AsAlias, aggregate, order);

        public static SqlColumn OrderBy(this string columnName, int Index, OrderDirection order = OrderDirection.ASC)
            => new(columnName, columnName, false, new() { Index = Index, Direction = order });


        public static SqlQueryColumn AsColumn(this SqlBuilder sqlBuilder,
            string Alias,
            ColumnOrder order = default,
            bool forJson = true,
            bool aggregate = false)
        {
            return new(sqlBuilder, Alias, aggregate, order, forJson);
        }

        public static SqlColumn Aggregate(this object expression, string AsAlias, ColumnOrder order = default)
           => new (expression, AsAlias, true, order);


        public static SqlBuilder From(this SqlBuilder sqlBuilder, (string Schema, string Table, string Alias) from)
        {
            sqlBuilder.Table = $"[{from.Schema}].[{from.Table}]";
            sqlBuilder.Alias = from.Alias;

            return sqlBuilder;
        }

        public static SqlBuilder From(this SqlBuilder sqlBuilder, (string Schema, string Table) from)
            => From(sqlBuilder,(from.Schema, from.Table, from.Table));

        public static SqlBuilder From(this SqlBuilder sqlBuilder, string table, string? alias = null)
        {
            sqlBuilder.Table = table;
            sqlBuilder.Alias = alias ?? table;

            return sqlBuilder;
        }

        public static SqlBuilder Select(this SqlBuilder sqlBuilder, params SqlColumnAbstract[] columns)
        {
            sqlBuilder.TableColumns.AddRange(columns);

            return sqlBuilder;
        }

        public static SqlBuilder Where(this SqlBuilder sqlBuilder, params string[] condition)
        {
            sqlBuilder.WhereConditions.AddRange(condition);
            return sqlBuilder;
        }

        public static SqlBuilder Where(this SqlBuilder sqlBuilder, bool applyCondition, params string[] conditions)
        {
            if(applyCondition)
                sqlBuilder.WhereConditions.AddRange(conditions);
            return sqlBuilder;
        }

        public static SqlBuilder Join(this SqlBuilder sqlBuilder,
            JoinTypes joinType,
            (string Schema, string Table, string? Alias) JoinTable,
            string predicate,
            params SqlColumnAbstract[] columns
        )
        {
            var tableJoin = new TableJoin(joinType,
              rawTable: (JoinTable.Schema, JoinTable.Table),
              alias: JoinTable.Alias,
              predicate: predicate,
              columns: columns);

            sqlBuilder.Joins.Add(tableJoin);

            return sqlBuilder;
        }

        public static SqlBuilder Join(this SqlBuilder sqlBuilder,
            JoinTypes joinType,
            (string Schema, string Table) JoinTable,
            string predicate,
            params SqlColumnAbstract[] columns
        )
        => Join(sqlBuilder, joinType, (JoinTable.Schema, JoinTable.Table, null), predicate, columns);
       

        public static SqlBuilder NaturalJoin(
            this SqlBuilder sqlBuilder,
            SqlBuilder other,
            JoinTypes JoinType = JoinTypes.INNER,
            params SqlColumnAbstract[] Columns
        )
        {

            string predicate =
                sqlBuilder.TableColumns
                    .Join(other.TableColumns, x => x.Name, x => x.Name,
                        (x, y) => $"{x.GetSqlExpression(sqlBuilder.Alias, new())} = {y.GetSqlExpression(other.Alias, new())}")
                    .Aggregate((a, b) => a + " AND " + b);


            return Join(sqlBuilder, JoinType, other, predicate, Columns);
        }


        public static SqlBuilder NaturalJoin(
            this SqlBuilder sqlBuilder,
            (string Schema, string Table, string? Alias) other,
            JoinTypes JoinType = JoinTypes.INNER,
            params SqlColumn[] Columns
        )
        {
            var _other = SqlBuilder.SelectFrom(other)
                .Select(Columns);

            string predicate = string.Empty;

            if(JoinType != JoinTypes.CROSS)
              predicate =
                sqlBuilder.TableColumns
                    .Join(_other.TableColumns, x => x.Name, x => x.Name,
                        (x, y) => $"{x.GetSqlExpression(sqlBuilder.Alias, new())} = {y.GetSqlExpression(_other.Alias, new())}")
                    .Aggregate((a, b) => a + " AND " + b);


            return Join(sqlBuilder, JoinType, other, predicate, Columns);
        }

        public static SqlBuilder Join(this SqlBuilder sqlBuilder,
            JoinTypes JoinType,
            SqlBuilder SubQuery,
            string? Predicate,
            params SqlColumnAbstract[] Columns
        )
        {
            sqlBuilder.WithTables.Add(SubQuery);

            var tableJoin = new TableJoin(JoinType,
                qualifiedTarget: SubQuery.Alias,
                alias: SubQuery.Alias,
                predicate: Predicate,
                columns: Columns);

            sqlBuilder.Joins.Add(tableJoin);

            return sqlBuilder;
        }

        public static SqlBuilder OrderBy(this SqlBuilder sqlBuilder
            , params ColumnOrder[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                var c = sqlBuilder.ColumnsWithAlias.FirstOrDefault(x => x.Alias == values[i].Column).Column;
                if (c != null)
                    c.Order = (i, c.Name!, values[i].Direction);
            }

            return sqlBuilder;
        }
    }
}
