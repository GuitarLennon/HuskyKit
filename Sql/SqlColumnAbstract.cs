﻿namespace HuskyKit.Sql
{
    /// <summary>
    /// Abstract class representing a SQL column, providing methods for generating SQL expressions.
    /// </summary>
    public abstract class SqlColumnAbstract
    {
        /// <summary>
        /// Implicitly converts a string to a SqlColumnAbstract instance.
        /// </summary>
        /// <param name="column">The name of the column as a string.</param>
        /// <returns>A new SqlColumn instance.</returns>
        public static implicit operator SqlColumnAbstract(string column)
        {
            return new SqlColumn(column);
        }

        /// <summary>
        /// Implicitly converts a tuple of (string value, string alias) to a SqlColumnAbstract instance.
        /// </summary>
        /// <param name="column">A tuple containing the column name and alias.</param>
        /// <returns>A new SqlColumn instance with the specified alias.</returns>
        public static implicit operator SqlColumnAbstract((string value, string AsAlias) column)
        {
            return new SqlColumn(column.value, column.AsAlias);
        }

        /// <summary>
        /// Implicitly converts a tuple of (object value, string alias) to a SqlColumnAbstract instance.
        /// </summary>
        /// <param name="column">A tuple containing the column value and alias.</param>
        /// <returns>A new SqlColumn instance with the specified alias.</returns>
        public static implicit operator SqlColumnAbstract((object value, string AsAlias) column)
        {
            return new SqlColumn(column.value, column.AsAlias);
        }

        /// <summary>
        /// Implicitly converts a tuple of (object value, string alias, bool groupBy) to a SqlColumnAbstract instance.
        /// </summary>
        /// <param name="column">A tuple containing the column value, alias, and groupBy flag.</param>
        /// <returns>A SqlColumn instance configured with grouping.</returns>
        public static implicit operator SqlColumnAbstract((object value, string AsAlias, bool groupBy) column)
        {
            return column.value.As(column.AsAlias, column.groupBy);
        }

        /// <summary>
        /// Raw name of the column as defined in the database.
        /// </summary>
        protected string? raw_name;

        /// <summary>
        /// Name to display for the column (overrides raw_name if set).
        /// </summary>
        protected string? show_name;

        /// <summary>
        /// Indicates whether the column is part of an aggregate function.
        /// </summary>
        public bool Aggregate { get; set; }

        /// <summary>
        /// Gets or sets the display name of the column. Defaults to raw_name if not set.
        /// </summary>
        public string? Name { get => show_name ?? raw_name; set => show_name = value; }

        /// <summary>
        /// Gets or sets the column order configuration.
        /// </summary>
        public ColumnOrder Order { get; set; }

        /// <summary>
        /// Generates the SQL expression for the column, including table alias and context.
        /// </summary>
        /// <param name="TableAlias">The alias of the table containing the column.</param>
        /// <param name="context">The build context for constructing the query.</param>
        /// <returns>The SQL expression for the column.</returns>
        public abstract string GetSqlExpression(string TableAlias, BuildContext context);

        /// <summary>
        /// Generates the SQL GROUP BY expression for the column.
        /// </summary>
        /// <param name="TableAlias">The alias of the table containing the column.</param>
        /// <param name="context">The build context for constructing the query.</param>
        /// <returns>The GROUP BY SQL expression for the column.</returns>
        public abstract string GetGroupByExpression(string TableAlias, BuildContext context);

        /// <summary>
        /// Generates the SQL ORDER BY expression for the column.
        /// </summary>
        /// <param name="TableAlias">The alias of the table containing the column.</param>
        /// <param name="context">The build context for constructing the query.</param>
        /// <returns>The ORDER BY SQL expression for the column.</returns>
        public virtual string GetOrderByExpression(string TableAlias, BuildContext context)
        {
            var _format = context.IndentToken.Replace("{", "{{").Replace("}", "}}") + $"[{Name}] {Order.Direction}";
            try
            {
                return Order.Direction != OrderDirection.NONE ? string.Format(_format, TableAlias) : $"[{Name}]";

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        /// <summary>
        /// Generates the SQL SELECT expression for the column.
        /// </summary>
        /// <param name="TableAlias">The alias of the table containing the column.</param>
        /// <param name="context">The build context for constructing the query.</param>
        /// <returns>The SELECT SQL expression for the column.</returns>
        public abstract string GetSelectExpression(string TableAlias, BuildContext context);

        /// <summary>
        /// Generates the SQL WHERE expression for the column based on a predicate.
        /// </summary>
        /// <param name="TableAlias">The alias of the table containing the column.</param>
        /// <param name="predicate">The predicate to apply in the WHERE clause.</param>
        /// <param name="context">The build context for constructing the query.</param>
        /// <returns>The WHERE SQL expression for the column.</returns>
        public abstract string GetWhereExpression(string TableAlias, string predicate, BuildContext context);
    }
}
