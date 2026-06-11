using Database.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Dto
{
    public class NotificationDto
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public DateTime Created { get; set; }

        public bool Read { get; set; }

        public string Message { get; set; }
    }
}
