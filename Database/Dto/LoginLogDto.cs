using Database.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Dto
{
    public class LoginLogDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime LoginDate { get; set; }
    }
}
