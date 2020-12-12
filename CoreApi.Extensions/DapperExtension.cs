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

        public static SqlType? SqlConnectionType { get; set; }

        public static async ValueTask<(List<T>, int)> PageAsync<T>(this IDbConnection conn, Action<PageConfig> action)
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

            StringBuilder templateBuild = new StringBuilder();
            StringBuilder countBuild = new StringBuilder();
            countBuild.Append($"select count(1) from ");

            if (SqlConnectionType == SqlType.SqlServer)
            {
                templateBuild.Append($"select {p.Column},row_number() over({order}) as rowNum from ");
            }
            else if (SqlConnectionType == SqlType.MySql_Sqlite)
            {
                templateBuild.Append($"select {p.Column} from ");
            }

            for (int i = 0; i < tableName.Length; i++)
            {
                if (i == 0)
                {
                    mainTable = tableName[i];
                    templateBuild.Append(mainTable);
                    countBuild.Append(mainTable);
                }
                else
                {
                    string joinSql = $" left join {tableName[i]} on {on[i - 1]} ";
                    templateBuild.Append(joinSql);
                    countBuild.Append(joinSql);
                }
            }

            countBuild.Append($" {where}");

            if (SqlConnectionType == SqlType.SqlServer)
            {
                templateBuild.Append($" {where} rowNum > {startRow} and rowNum <= {startRow + p.PageSize}");
            }
            else if (SqlConnectionType == SqlType.MySql_Sqlite)
            {
                templateBuild.Append($@" join (select {key} from {mainTable} {where} {order} 
                           LIMIT {p.PageSize} OFFSET {startRow}) as t on {mainTable}.{key} = t.{key}");
            }

            return (templateBuild.ToString(), countBuild.ToString());
        }
    }
}