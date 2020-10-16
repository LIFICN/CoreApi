using Dapper.Contrib.Extensions;

namespace CoreApi.Models
{
    [Table("users")]
    public class User
    {
       // [ExplicitKey]   非自增Id主键使用
        [Key]
        public int ID { get; set; }

        public string name { get; set; }

        public string password { get; set; }
    }
}
