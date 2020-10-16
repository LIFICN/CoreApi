using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoreApi.Models
{
    [Table("test1")]
    public class TestEntity1
    {
        [Key]
        public int ID { get; set; }

        public string Name { get; set; }
    }
}
