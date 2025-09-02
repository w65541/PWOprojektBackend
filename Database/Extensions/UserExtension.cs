using Database.Dto;
using Database.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Extensions
{
    public class UserExtension
    {
        public UserDto toDto(User x)
        {
            return new UserDto
            {
                Id = x.Id,
                ArchiveDate = x.ArchiveDate,
                Email = x.Email,
                ArchiverId = x.ArchiverId,
                CreationDate = x.CreationDate,
                Haslo = x.Haslo,
                LastLoginDate = x.LastLoginDate,
                IsActive = x.IsActive,
                Login = x.Login,
                Type = x.Type
            };
        }

        public User toEntity(UserDto x)
        {
            return new User
            {
                Id = x.Id,
                ArchiveDate = x.ArchiveDate,
                Email = x.Email,
                ArchiverId = x.ArchiverId,
                CreationDate = x.CreationDate,
                Haslo = x.Haslo,
                LastLoginDate = x.LastLoginDate,
                IsActive = x.IsActive,
                Login = x.Login,
                Type = x.Type
            };
        }

    }
}
