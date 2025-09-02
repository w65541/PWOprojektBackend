using Database;
using Database.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {

        private readonly DatabaseContext _context;
        public AuthController(DatabaseContext context) { _context = context; }


        [HttpPost]
        [AllowAnonymous]
        public ActionResult<string> Authenticate(string login, string pwd)
        {
            User user=_context.Users.Where(x=> x.Login == login && x.Haslo==pwd).FirstOrDefault();
            
            if (user == null) return Unauthorized();
            
            user.LastLoginDate= DateTime.Now;
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
                        new Claim(ClaimTypes.Actor, user.Type)
                    }),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var Token = tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
            
            return new OkObjectResult(Token);
        }
    }
}
