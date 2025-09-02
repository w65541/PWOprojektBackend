using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Dto
{
    public class UserDto
    {
        
        public int Id { get; set; }
        public string Login { get; set; }
        public string Haslo { get; set; }
        public string Email { get; set; }
        public string Type { get; set; }
        public bool IsActive { get; set; }

        public DateTime CreationDate { get; set; }
        public DateTime LastLoginDate { get; set; }
        public DateTime ArchiveDate { get; set; }
        public int ArchiverId { get; set; }
    }
}
