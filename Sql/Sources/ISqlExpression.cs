﻿namespace HuskyKit.Sql.Sources
{
    public interface ISqlExpression 
    {
        public string GetSqlExpression(BuildContext context);
    }
}