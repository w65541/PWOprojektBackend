using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Database;
using Database.Entities;
using Microsoft.AspNetCore.Authorization;
using Database.Dto;

using AutoMapper;
namespace WebApplication1.Controllers
{
    
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly DatabaseContext _context;
        public UserController(DatabaseContext context,IMapper mapper)
        { 
            _context = context;
            _mapper = mapper;
        }
       


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
            try
            {
                if (_context.Users.Where(x => x.Login == username && x.IsActive).Any()) return BadRequest("Username taken");
                if (_context.Users.Where(x => x.Email == username && x.IsActive).Any()) return BadRequest("Email taken");
                _context.Users.Add(new Database.Entities.User
            {
                Email = email,
                Haslo = password,
                Login = username,
                IsActive = true,
                TypeId = 2,
                CreationDate= DateTime.Now,
            }
            );
            _context.SaveChanges();
            return Ok();
            }
            catch (Exception e)
            {

                return BadRequest(e.Message);
            }
            
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
            try
            {
            User temp = _context.Users.Where(x => x.Id == id).FirstOrDefault();
            if (temp != null)
            {
                
                return new OkObjectResult(_mapper.Map<UserDto>(temp));
            }
            return BadRequest("No such user");
            }
            catch (Exception e)
            {

                return BadRequest(e.Message);
            }
            
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

            try
            {
                User arch= _context.Users.Where(x => x.Id == archId && x.IsActive).FirstOrDefault();
                User temp=_context.Users.Where(x=>x.Id == id && x.IsActive).FirstOrDefault();

                if (temp != null && arch != null) {
                    if(id==archId || arch.TypeId == 1)
                    {
                        temp.IsActive = false;
                        temp.ArchiveDate= DateTime.Now;
                        temp.ArchiverId=archId;
                        _context.SaveChanges();

                        return Ok();
                    }
                    return Unauthorized();
            }
            return BadRequest("No such user");


            }
            catch (Exception e)
            {

                return BadRequest(e.Message);
            }
            
        }

    }
}
