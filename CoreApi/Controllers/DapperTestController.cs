using CoreApi.Extensions;
using CoreApi.Models;
using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreApi.Controllers
{
    /// <summary>
    /// Dapper测试
    /// </summary>
    [Route("api/[controller]/[action]")]
    [AllowAnonymous]
    [ApiVersion("1.0")]
    public class DapperTestController : BaseController
    {
        private readonly string strConnection = $"Data Source={AppContext.BaseDirectory}test.db";

        /// <summary>
        /// 单条添加
        /// </summary>
        [HttpGet]
        public async Task<bool> Add()
        {
            using SqliteConnection conn = new SqliteConnection(strConnection);
            User user = new User()
            {
                name = "Taro奥特曼",
                password = "m78"
            };
            // int ok = await conn.ExecuteAsync("insert into User(name,password) values(@name,@password)", user);
            int ok = await conn.InsertAsync<User>(user);
            return ok > 0 ? true : false;
        }

        /// <summary>
        /// 多条添加
        /// </summary>
        [HttpGet]
        public bool AddList()
        {
            using SqliteConnection conn = new SqliteConnection(strConnection);
            List<User> users = new List<User>()
            {
                new User(){name="ff" ,password="34324"},
                new User(){ name="432234234",password="jghjghhj"}
            };
            //var ok = conn.Execute(" insert into User(name,password) values(@name,@password) ", users);
            var ok = conn.Insert<List<User>>(users);
            return ok > 0 ? true : false;
        }

        /// <summary>
        /// 多条删除
        /// </summary>
        [HttpGet]
        public async Task<bool> Delete()
        {
            using SqliteConnection conn = new SqliteConnection(strConnection);
            // int ok = await conn.Delete<Model.User>(user);
            //切记 dapper sql 语句 where in的时候别这样写 where in(@Ids)
            int ok = await conn.ExecuteAsync("delete User where ID in @Ids", new { Ids = new int[] { 2, 3, 4, 5 } });
            return ok > 0 ? true : false;
        }

        /// <summary>
        /// 单条修改
        /// </summary>
        [HttpGet]
        public async Task<bool> Update()
        {
            using SqliteConnection conn = new SqliteConnection(strConnection);
            User users = new User()
            {
                ID = 6,
                name = "new2",
                password = "1234567"
            };
            // var ok = await conn.UpdateAsync<Model.User>(users);
            var ok = await conn.ExecuteAsync(" update User set name=@name,password=@password where ID=@ID ", users);
            return ok > 0 ? true : false;
        }

        /// <summary>
        /// 多条修改
        /// </summary>
        [HttpGet]
        public bool UpdateList()
        {
            using SqliteConnection conn = new SqliteConnection(strConnection);
            List<User> users = new List<User>()
            {
                new User(){ID=1,name="帝霸" ,password="赛文"},
                new User(){ID=6,name="大主宰",password="迪迦"}
            };
            //var ok = conn.Execute(" update User set name=@name,password=@password where ID=@ID ", users);
            var ok = conn.Update<List<User>>(users);
            return ok;
        }

        /// <summary>
        /// 查询单个结果
        /// </summary>
        [HttpGet]
        public async Task<int> GetOne()
        {
            using SqliteConnection conn = new SqliteConnection(strConnection);
            var data = await conn.ExecuteScalarAsync<int>(" select count(ID) from User where ID>0 ");
            return data;
        }

        /// <summary>
        /// 获取单条数据
        /// </summary>
        [HttpGet]
        public User GetUserByID()
        {
            using SqliteConnection conn = new SqliteConnection(strConnection);
            string strSql = "select * from User where ID=1";
            //User users = conn.Query<User>(strSql).FirstOrDefault();
            User users = conn.QueryFirstOrDefault<User>(strSql);
            return users;
        }

        /// <summary>
        /// 查询多条
        /// </summary>
        [HttpGet]
        public async Task<List<User>> Select()
        {
            using SqliteConnection conn = new SqliteConnection(strConnection);
            //var data = await conn.QueryAsync<User>(" select * from User ");
            // var data = await conn.GetAsync<User>(6);
            var data = await conn.GetAllAsync<User>();
            return data.ToList();
        }

        /// <summary>
        /// Dapper分页
        /// </summary>
        [HttpGet]
        public async Task<dynamic> Page()
        {
            using SqliteConnection conn = new SqliteConnection(strConnection);

            (IEnumerable<dynamic> data, int totalCount) = await conn.PageAsync<dynamic>((p) =>
            {
                p.TableName = new string[] { "user", "area" };
                p.On = new string[] { "user.area_id=area.id" };
                p.KeyColumn = "user.id";
                p.PageIndex = 1;
                p.PageSize = 10;
            });

            return data;
        }

        /// <summary>
        /// 跨多表查询
        /// </summary>
        [HttpGet]
        public dynamic SelectJoin()
        {
            using SqliteConnection conn = new SqliteConnection(strConnection);
            var strSQL = @" select u.ID,u.name as Name,i.Number,i.Note from User u inner join Info i on i.UserID=u.ID ";
            var data = conn.Query<dynamic>(strSQL);
            return data;
        }

        /// <summary>
        /// 执行多条查询语句返回多个结果集
        /// </summary>
        [HttpGet]
        public dynamic SelectTables()
        {
            using SqliteConnection conn = new SqliteConnection(strConnection);
            using var data = conn.QueryMultiple(" select * from User ; select * from Info ");
            var users = data.Read();
            var info = data.Read();
            //return info.ToList()[0].Note;
            return info;
        }
    }
}