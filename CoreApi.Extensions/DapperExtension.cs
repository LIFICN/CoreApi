using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreApi.Extensions
{
    public static class DapperExtension
    {
        public class PageConfig
        {
            public string[] TableName { get; set; }
            public string[] On { get; set; }
            public int PageIndex { get; set; }
            public int PageSize { get; set; }
            public object Params { get; set; }
            public string Column { get; set; } = "*";
            public string Where { get; set; } = "1=1";
            public string Order { get; set; }
            public string KeyColumn { get; set; }
        }

        public enum SqlType
        {
            SqlServer,
            MySql_Sqlite
        }

        static DapperExtension()
        {
            DefaultTypeMap.MatchNamesWithUnderscores = true;
        }

        public static SqlType? SqlConnectionType { get; set; }

        public static async Task<(List<T>, int)> PageAsync<T>(this IDbConnection conn, Action<PageConfig> action)
        {
            PageConfig p = new PageConfig();
            action?.Invoke(p);

            object param = p.Params;
            (string templateSql, string countSql) = GetTemplateSql(p);

            if (conn.State == ConnectionState.Closed) conn.Open();

            using var trans = conn.BeginTransaction();
            var data = await conn.QueryAsync<T>(templateSql, param, trans).ConfigureAwait(false);
            int total = await conn.ExecuteScalarAsync<int>(countSql, param, trans).ConfigureAwait(false);
            trans.Commit();
            return (data.ToList(), total);
        }

        internal static ValueTuple<string, string> GetTemplateSql(PageConfig p)
        {
            if (p == null || p.TableName == null || p.TableName.Length == 0 || string.IsNullOrWhiteSpace(p.KeyColumn))
                throw new Exception("dapper page method params was error");

            if (p.TableName.Length > 1 && p.On != null && p.On.Length != p.TableName.Length - 1)
                throw new Exception("dapper page method params was error");

            if (SqlConnectionType == null) throw new Exception("DapperExtension SqlConnectionType was null ");

            string[] on = p.On;
            string key = p.KeyColumn;
            string mainTable = string.Empty;
            string[] tableName = p.TableName;
            int startRow = (p.PageIndex - 1) * p.PageSize;
            string where = !string.IsNullOrWhiteSpace(p.Where) ? $"where {p.Where}" : string.Empty;
            string order = !string.IsNullOrWhiteSpace(p.Order) ? $"order by { p.Order}" :
                           SqlConnectionType == SqlType.SqlServer ? $"order by { p.KeyColumn}" : string.Empty;

            StringBuilder templateBuilder = new StringBuilder(200);
            StringBuilder countBuilder = new StringBuilder("select count(1) from ", 100);

            if (SqlConnectionType == SqlType.SqlServer)
                templateBuilder.Append($"select {p.Column},row_number() over({order}) as rowNum from ");

            else if (SqlConnectionType == SqlType.MySql_Sqlite)
                templateBuilder.Append($"select {p.Column} from ");

            for (int i = 0; i < tableName.Length; i++)
            {
                if (i == 0)
                {
                    mainTable = tableName[i];
                    templateBuilder.Append(mainTable);
                    countBuilder.Append(mainTable);
                }
                else
                {
                    string joinSql = $" left join {tableName[i]} on {on[i - 1]} ";
                    templateBuilder.Append(joinSql);
                    countBuilder.Append(joinSql);
                }
            }

            countBuilder.Append($" {where}");

            if (SqlConnectionType == SqlType.SqlServer)
                templateBuilder.Append($" {where} rowNum > {startRow} and rowNum <= {startRow + p.PageSize}");

            else if (SqlConnectionType == SqlType.MySql_Sqlite)
                templateBuilder.Append($@" join (select {key} from {mainTable} {where} {order} 
                           LIMIT {p.PageSize} OFFSET {startRow}) as t on {mainTable}.{key} = t.{key}");

            return (templateBuilder.ToString(), countBuilder.ToString());
        }
    }
}