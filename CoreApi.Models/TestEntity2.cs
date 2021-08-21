using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoreApi.Models
{
    [Table("test2")]
    public class TestEntity2
    {
        [Key]
        public int ID { get; set; }

        public string Name { get; set; }
    }
}
