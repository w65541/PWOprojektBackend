using AutoMapper;
using Database;
using Database.Dto;
using Database.Entities;
using Database.Migrations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("[controller]")]
    public class NotificationSSEController : Controller
    {
        private readonly SSEService _sse;
        private readonly IMapper _mapper;
        private readonly DatabaseContext _context;

        public NotificationSSEController(DatabaseContext context, IMapper mapper,SSEService sse)
        {
            _context = context;
            _mapper = mapper;
            _sse = (SSEService?)sse;
        }
        [AllowAnonymous]
        [HttpGet("sse")]
        public async Task Get(string token, CancellationToken cancellationToken)
        {
            Response.Headers.Add("Cache-Control", "no-cache");
            Response.Headers.Add("Content-Type", "text/event-stream");

            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(token) as JwtSecurityToken;


            var userId = int.Parse(jsonToken.Claims.ToList().Find(match: x => x.Type == "unique_name").Value);


            Debug.WriteLine("UserId for notifs " + userId);
            _sse.AddClient(userId, Response);
            Debug.WriteLine("UserId for notifs " + userId);
            await _sse.getUnread(userId, _context, _mapper, cancellationToken);

            try
            {
                await Task.Delay(Timeout.Infinite, cancellationToken);
            }
            finally
            {
                _sse.RemoveClient(userId);
            }
        }

        [AllowAnonymous]
        [HttpGet("unread")]
        public async Task<IActionResult> GetUnread(string token, CancellationToken cancellationToken)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadToken(token) as JwtSecurityToken;
                var userId = int.Parse(jsonToken.Claims.ToList().Find(match: x => x.Type == "unique_name").Value);

                await _sse.getUnread(userId, _context, _mapper, cancellationToken);

                return Ok();
            }
            catch (Exception e)
            {

                return BadRequest(e.Message);
            }


        }


        [AllowAnonymous]
        [HttpPost("read")]
        public async Task<IActionResult> Read(int notId) {

            var temp=_context.Notifications.Where(x => x.Id == notId).FirstOrDefault();
            if (temp != null)
            {
                temp.Read = true;
                _context.SaveChanges();
                return Ok();

            }
            return BadRequest("Notification not found");
            
        }



        [AllowAnonymous]
        [HttpPost("send")]
        public async Task<IActionResult> Send(int userId, string message, CancellationToken token)
        {
            try
            {
                var not = new Notification
                {
                    UserId = userId,
                    Message = message,
                    Created = DateTime.Now,
                    Read = false,
                };
                _context.Notifications.Add(not);
                Thread.Sleep(5000);
                _context.SaveChanges();
                not = _context.Notifications.Where(x => x.UserId == not.UserId && x.Message == not.Message && x.Created == not.Created && !x.Read).FirstOrDefault();

                await _sse.SendToUserAsync(_mapper.Map<NotificationDto>(not), token);
                return Ok();
            }
            catch (Exception e)
            {

                return BadRequest(e.Message);
            }

        }
    }
}
