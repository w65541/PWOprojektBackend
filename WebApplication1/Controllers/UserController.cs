using AutoMapper;
using Database;
using Database.Dto;
using Database.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Collections;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using WebApplication1.Services;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Model;
namespace WebApplication1.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly DatabaseContext _context;
        private readonly RsaImp rsa = new RsaImp();
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private static TokenService _tokenService = new TokenService();


        public UserController(DatabaseContext context, IMapper mapper)
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
                if (_context.Users.Where(x => x.Login == username && x.IsActive).Any())
                    return BadRequest("Username taken");

                if (_context.Users.Where(x => x.Email == email && x.IsActive).Any())
                    return BadRequest("Email taken");

                if (username != null && password != null && email != null)
                {
                    _context.Users.Add(new Database.Entities.User
                    {
                        Email = email,
                        Haslo = password,
                        Login = username,
                        IsActive = true,
                        TypeId = 2,
                        CreationDate = DateTime.Now,
                    }
                    );

                    _context.SaveChanges();
                    Logger.Debug("Added new user");
                    return Ok();
                }
                return BadRequest("Something is missing");
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
        if (e.InnerException != null)
                {
                    Logger.Error(e.InnerException.Message);
          return BadRequest(e.Message+"\n"+ e.InnerException.Message);
        }
        return BadRequest(e.Message);
            }

        }

    [HttpPut]
    [Route("update")]
    public ActionResult UpdateUser(int id, [FromBody] UpdateUserDto dto)
    {
      try
      {
        var user = _context.Users.Where(x => x.Id == id && x.IsActive).FirstOrDefault();
        if (user == null)
          return BadRequest("User not found");

        if (!string.IsNullOrWhiteSpace(dto.Login) && dto.Login != user.Login)
        {
          if (_context.Users.Any(x => x.Login == dto.Login && x.IsActive && x.Id != id))
            return BadRequest("Login already taken");
          
          _context.UserModificationLogs.Add(new UserModificationLog
          {
            UserId = id,
            UpdateDate = DateTime.Now,
            modColumn = "Login",
            newValue = dto.Login,
            oldValue = user.Login

          });
          user.Login = dto.Login;
        }

        if (!string.IsNullOrWhiteSpace(dto.Email) && dto.Email != user.Email)
        {
          if (_context.Users.Any(x => x.Email == dto.Email && x.IsActive && x.Id != id))
            return BadRequest("Email already taken");
          
          _context.UserModificationLogs.Add(new UserModificationLog
          {
            UserId = id,
            UpdateDate = DateTime.Now,
            modColumn = "Email",
            newValue = dto.Email,
            oldValue = user.Email

          });
          user.Email = dto.Email;
        }

        if (!string.IsNullOrWhiteSpace(dto.Password)) {
          _context.UserModificationLogs.Add(new UserModificationLog
          {
            UserId = id,
            UpdateDate = DateTime.Now,
            modColumn = "Haslo",
            newValue = dto.Password,
            oldValue = user.Haslo

          });
          user.Haslo = dto.Password;}

        if (dto.TypeId.HasValue && dto.TypeId.Value > 0)
        {
          if (!_context.UserTypes.Any(t => t.Id == dto.TypeId.Value))
            return BadRequest("Invalid TypeId");
          _context.UserModificationLogs.Add(new UserModificationLog
          {
            UserId = id,
            UpdateDate = DateTime.Now,
            modColumn = "TypeId",
            newValue = dto.TypeId+"",
            oldValue = user.TypeId+""

          });
          user.TypeId = dto.TypeId.Value;
        }

        _context.SaveChanges();
        Logger.Debug("Updated user Id=" + id);
        return Ok(_mapper.Map<UserDto>(user));
      }
      catch (Exception e)
      {
        Logger.Error(e);
        return BadRequest(e.Message);
      }
    }

    [HttpPost]
        [Route("recover")]
        [AllowAnonymous]

        public ActionResult recover(string email)
        {
            try
            {
                var user = _context.Users.Where(x => x.Email.ToLower().Equals(email.ToLower())).FirstOrDefault();
                if (user != null)
                {
                    //SendEmail
                    Logger.Debug("Send recovery email to " + email);
                    return Ok();
                }
                return BadRequest("No such email found");
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return BadRequest("No such email found");
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
                    return Ok(_mapper.Map<UserDto>(temp));
                }

                Logger.Debug("Not found requested user Id=" + id);
                return BadRequest("No such user");
            }
            catch (Exception e)
            {

                Logger.Error(e);
                return BadRequest(e);
            }

        }

        /// <summary>
        /// Get all users
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("getAll")]
        public ActionResult GetUsers()
        {
            try
            {
                var Users = new List<UserDto>();
                foreach (var item in _context.Users.ToArray())
                {
                    Users.Add(_mapper.Map<UserDto>(item));
                }
                return Ok(Users);
            }
            catch (Exception e)
            {

                Logger.Error(e);
                return BadRequest(e);
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
        public ActionResult Archive(int id, int archId)
        {

            try
            {
                User arch = _context.Users.Where(x => x.Id == archId && x.IsActive).FirstOrDefault();
                User temp = _context.Users.Where(x => x.Id == id && x.IsActive).FirstOrDefault();

                if (temp != null && arch != null)
                {
                    if (id == archId || arch.TypeId == 1)
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
                return BadRequest(e);
            }

        }



        /// <summary>
        /// Archive user using token for validation
        /// </summary>
        /// <param name="id">Archivee</param>
        /// <param name="token">Token of archiver</param>
        /// <returns>200</returns>
        [HttpPost]
        [Route("archiveToken")]
        public ActionResult ArchiveToken(int id, string token)
        {

            try
            {

                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadToken(token);
                var tokenS = jsonToken as JwtSecurityToken;
                int archId, archType;

                if (tokenS.Claims.ToList().Find(match: x => x.Type == "unique_name") != null
                    && tokenS.Claims.ToList().Find(match: x => x.Type == "actort") != null)
                {
                    archId = int.Parse(tokenS.Claims.ToList().Find(match: x => x.Type == "unique_name").Value);
                    archType = int.Parse(tokenS.Claims.ToList().Find(match: x => x.Type == "actort").Value);
                }
                else
                {
                    return BadRequest();
                }

                User temp = _context.Users.Where(x => x.Id == id && x.IsActive).FirstOrDefault();

                if (temp != null && _tokenService.validateToken(token, Logger))
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
                return BadRequest(e);
            }

        }
    }
}
