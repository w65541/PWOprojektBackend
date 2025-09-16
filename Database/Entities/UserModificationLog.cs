using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Entities
{
    public class UserModificationLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int UserId { get; set; }
        public virtual User User { get; set; }
        public DateTime UpdateDate { get; set; }

        public string modColumn { get; set; }
        public string oldValue { get; set; }
        public string newValue { get; set; }
    }
}
