using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
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
            MySql_PostgreSql_Sqlite
        }

        public static SqlType? SqlConnectionType { get; set; }

        public static async ValueTask<(IEnumerable<T>, int)> PageAsync<T>(this IDbConnection conn, Action<PageConfig> action)
        {
            PageConfig p = new PageConfig();
            action?.Invoke(p);

            (string templateSql, string countSql) = GetTemplateSql(conn, p);
            object param = p.Params;

            if (conn.State == ConnectionState.Closed) conn.Open();

            using var trans = conn.BeginTransaction();
            var data = await conn.QueryAsync<T>(templateSql, param, trans).ConfigureAwait(false);
            int count = await conn.ExecuteScalarAsync<int>(countSql, param, trans).ConfigureAwait(false);
            trans.Commit();
            return (data, count);
        }

        internal static ValueTuple<string, string> GetTemplateSql(in IDbConnection conn, in PageConfig p)
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
            else if (SqlConnectionType == SqlType.MySql_PostgreSql_Sqlite)
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
            else if (SqlConnectionType == SqlType.MySql_PostgreSql_Sqlite)
            {
                templateBuild.Append($@" join (select {key} from {mainTable} {where} {order} 
                           LIMIT {p.PageSize} OFFSET {startRow}) as t on {mainTable}.{key} = t.{key}");
            }

            return (templateBuild.ToString(), countBuild.ToString());
        }

        public static DataTable GetDataTable(this IDbConnection conn, string strSql, object param = null)
        {
            using var dataReader = conn.ExecuteReader(strSql, param);
            if (dataReader == null) return null;

            DataTable table = new DataTable();
            table.Load(dataReader);
            return table;
        }

        #region sql server bulk copy
        //public static async ValueTask BulkCopyAsync(this SqlConnection conn, string tableName, DataTable table)
        //{
        //    using SqlBulkCopy bulkCopy = new SqlBulkCopy(conn)
        //    {
        //        DestinationTableName = tableName,
        //        BatchSize = table.Rows.Count
        //    };

        //    foreach (DataColumn column in table.Columns)
        //    {
        //        bulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
        //    }

        //    await bulkCopy.WriteToServerAsync(table).ConfigureAwait(false);
        //}
        #endregion
    }
}