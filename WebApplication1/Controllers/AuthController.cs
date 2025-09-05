using Database;
using Database.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace WebApplication1.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly RsaImp rsa = new RsaImp();
        private readonly DatabaseContext _context;
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        public AuthController(DatabaseContext context) { _context = context; }


        [HttpGet]
        [Route("pubkey")]
        [AllowAnonymous]
        public ActionResult<string> GetPub()
        {
            return rsa.GetPublicKey();
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("authEncr")]
        public ActionResult<string> AuthenticateEncr(string login, string pwd)
        {
            return Authenticate(login, rsa.Decode(pwd));
        }



        /// <summary>
        /// Login to get session key
        /// </summary>
        /// <param name="login">Login</param>
        /// <param name="pwd">Password</param>
        /// <returns> Jwt Security Token if login credentials are correct, otherwise 401.</returns>
        [HttpPost]
        [AllowAnonymous]
        public ActionResult<string> Authenticate(string login, string pwd)
        {
            try
            {
                User user = _context.Users.Where(x => x.Login == login && x.Haslo == pwd && x.IsActive).FirstOrDefault();

                if (user == null)
                {
                    Logger.Debug("Unsucesfull login attempt to User= " + login);
                    return Unauthorized();
                }

                user.LastLoginDate = DateTime.Now;
                _context.SaveChanges();

                var key = "Cp5wQhqCpttzoJG53ausxUwlTH38jd24ChC0tA8SGaEkJBHqWpHVwGhevEcXCVE"u8.ToArray();

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                        {
                        new Claim(ClaimTypes.Name, user.Id.ToString()),
                        new Claim(ClaimTypes.GivenName, user.Login),
                        new Claim(ClaimTypes.Surname, user.Haslo),
                        new Claim(ClaimTypes.Email, user.Email),
                        new Claim(ClaimTypes.Actor, user.Type.Id.ToString())
                    }),
                    Expires = DateTime.UtcNow.AddDays(1),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var Token = tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
                Logger.Debug("Login of User=" + login);
                return new OkObjectResult(Token);
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return BadRequest();

            }

        }


    }
}
