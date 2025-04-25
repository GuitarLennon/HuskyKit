using HuskyKit.Sql.Columns;
using HuskyKit.Sql.Sources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HuskyKit.Sql
{
    public static partial class SqlBuilderExtensions 
    {


        /// <summary>
        /// Wraps the builder as a subquery with a given alias.
        /// </summary>
        public static SqlBuilder AsSubquery(
            this SqlBuilder builder,
            string? alias = null
        )
        {
            if (!builder.Columns.Any())
                throw new InvalidOperationException("No se declararon columnas");

            var _builder = new SqlBuilder(builder, alias);

            return _builder;
        }


        /// <summary>
        /// Wraps the builder as a subquery with a given alias.
        /// </summary>
        public static SqlBuilder AsSubquery(
            this SqlBuilder builder,
            Predicate<(string table, ISqlColumn column)> WhereClause,
            string? alias = null
        )
        {
            if (!builder.Columns.Any())
                throw new InvalidOperationException("No se declararon columnas");

            var returning = new SqlBuilder(builder, alias, WhereClause);

            return returning;
        }

        /// <summary>
        /// Wraps the builder as a subquery with a given alias.
        /// </summary>
        public static SqlBuilder AsSubquery(
            this SqlBuilder builder,
            Predicate<(string table, ISqlColumn column)> WhereClause,
            Func<SqlColumn, SqlColumn> SelectClause,
            string? alias = null
        )
        {
            if (!builder.Columns.Any())
                throw new InvalidOperationException("No se declararon columnas");


            var returning = new SqlBuilder(builder, alias, WhereClause, SelectClause);

            return returning;
        }

         

    }
}
