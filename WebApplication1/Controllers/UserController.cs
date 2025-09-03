using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Database;
using Database.Entities;
using Microsoft.AspNetCore.Authorization;
using Database.Dto;
using Database.Extensions;
namespace WebApplication1.Controllers
{
    
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly DatabaseContext _context;
        public UserController(DatabaseContext context) { _context = context; }
        private UserExtension userExtension= new UserExtension();


        /// <summary>
        /// Create new user
        /// </summary>
        /// <param name="username">Login</param>
        /// <param name="password">Password</param>
        /// <param name="email">Email</param>
        /// <returns>200</returns>
        [HttpPost]
        [Route("add")]
        [AllowAnonymous]
      
        public ActionResult AddUser(string username, string password, string email)
        {
            _context.Users.Add(new Database.Entities.User
            {
                Email = email,
                Haslo = password,
                Login = username,
                IsActive = true,
                Type = _context.UserTypes.Where(x=>x.Id==2).FirstOrDefault(),
                CreationDate= DateTime.Now,
                
            }
            );
            _context.SaveChanges();
            return Ok();
        }


        /// <summary>
        /// Get user information
        /// </summary>
        /// <param name="id"></param>
        /// <returns>UserDto if found, otherwise 400</returns>
        [HttpGet]
        [Route("get")]
        public ActionResult<UserDto> GetUser(int id)
        {
            User temp = _context.Users.Where(x => x.Id == id).FirstOrDefault();
            if (temp != null)
            {
                
                return new OkObjectResult(userExtension.toDto(temp));
            }
            return BadRequest("No such user");
        }


        /// <summary>
        /// Archive User
        /// </summary>
        /// <param name="id">Id of archivee</param>
        /// <param name="archId">Id of archiver</param>
        /// <returns>200 if found, otherwise 400</returns>
        [HttpPost]
        [Route("archive")]
        public ActionResult Archive(int id, int archId) {
            User temp=_context.Users.Where(x=>x.Id == id).FirstOrDefault();
            if (temp != null) {
                temp.IsActive = false;
                temp.ArchiveDate= DateTime.Now;
                temp.ArchiverId=archId;
                _context.SaveChanges();

                return Ok();
            }
            return BadRequest("No such user");
        }

    }
}
