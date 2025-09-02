using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Database;
using Database.Entities;
using Microsoft.AspNetCore.Authorization;
namespace WebApplication1.Controllers
{
    
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly DatabaseContext _context;
        public UserController(DatabaseContext context) { _context = context; }


        
        
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
                Type = "regular",
                CreationDate= DateTime.Now,
                
            }
            );
            _context.SaveChanges();
            return Ok();
        }


        /// <summary>
        /// Archive user
        /// </summary>
        [HttpPost]
        [Route("archive")]
        public ActionResult Archive(int id) {
            User temp=_context.Users.Where(x=>x.Id == id).FirstOrDefault();
            if (temp != null) {
                temp.IsActive = false;
                _context.SaveChanges();
                return Ok();
            }
            return BadRequest("No such user");
        }

    }
}
