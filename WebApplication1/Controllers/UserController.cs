using AutoMapper;
using Database;
using Database.Dto;
using Database.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Model;
namespace WebApplication1.Controllers
{
    
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly DatabaseContext _context;
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
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
                Logger.Debug("Added new user");
                return Ok();
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return BadRequest();
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
                Logger.Debug("Requested user Id=" + id);
                return new OkObjectResult(_mapper.Map<UserDto>(temp));
            }
                Logger.Debug("Not found requested user Id=" + id);
                return BadRequest("No such user");
            }
            catch (Exception e)
            {

                Logger.Error(e);
                return BadRequest();
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
                        Logger.Debug("User Id=" + id + " archived by aId=" + archId);
                        return Ok();
                    }
                    Logger.Debug("Unauthorized archive atempt of User Id=" + id + " by aId=" + archId);
                    return Unauthorized();
            }
                Logger.Debug("Unnsecsefull archive atempt of not found User Id=" + id + " by aId=" + archId);
            return BadRequest("No such user");
            }
            catch (Exception e)
            {

                Logger.Error(e);
                return BadRequest();
            }
            
        }

        private bool validateToken(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                
                TokenValidationParameters valParam= new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                 {
                     ValidateIssuerSigningKey = true,
                     IssuerSigningKey = new SymmetricSecurityKey("Cp5wQhqCpttzoJG53ausxUwlTH38jd24ChC0tA8SGaEkJBHqWpHVwGhevEcXCVE"u8.ToArray()),
                     ValidateIssuer = false,
                     ValidateAudience = false
                 };
                var claims = handler.ValidateToken(token, valParam, out var tokenSecure);
                return true; 
            }
            catch (Exception)
            {
                Logger.Debug("Usage of invalid token"); 
               return false;
            }
            
        }


        [HttpPost]
        [Route("archiveToken")]
        public ActionResult ArchiveToken(int id, string token)
        {

            try
            {

                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadToken(token);
                var tokenS = jsonToken as JwtSecurityToken;
                var archId=int.Parse(tokenS.Claims.ToList().Find(match: x =>x.Type== "unique_name").Value);
                var archType= int.Parse(tokenS.Claims.ToList().Find(match: x => x.Type == "actort").Value);
                User temp = _context.Users.Where(x => x.Id == id && x.IsActive).FirstOrDefault();

                if (temp != null && validateToken(token))
                {
                    if (id == archId || archType == 1)
                    {
                        temp.IsActive = false;
                        temp.ArchiveDate = DateTime.Now;
                        temp.ArchiverId = archId;
                        _context.SaveChanges();
                        Logger.Debug("User Id=" + id + " archived by aId=" + archId);
                        return Ok();
                    }
                    Logger.Debug("Unauthorized archive atempt of User Id=" + id + " by aId=" + archId);
                    return Unauthorized();
                }
                Logger.Debug("Unnsecsefull archive atempt of not found User Id=" + id + " by aId=" + archId);
                return BadRequest("No such user");
            }
            catch (Exception e)
            {

                Logger.Error(e);
                return BadRequest();
            }

        }
    }
}
