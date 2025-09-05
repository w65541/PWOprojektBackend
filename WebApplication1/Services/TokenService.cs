using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace WebApplication1.Services
{
    public class TokenService
    {
        public bool validateToken(string token, NLog.Logger Logger)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();

                TokenValidationParameters valParam = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
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
    }
}
