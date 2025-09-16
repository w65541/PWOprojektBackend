using AutoMapper;
using Database;
using Database.Dto;
using Database.Entities;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LogController : Controller
    {
        private readonly DatabaseContext _context;
        private readonly IMapper _mapper;
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        public LogController(DatabaseContext context, IMapper mapper) 
        { 
            _context = context;
            _mapper=mapper;
        }

        /// <summary>
        /// Get all login logs
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("getAll")]
        public ActionResult GetLogs()
        {
            try
            {
                var Logs = new List<LoginLogDto>();
                foreach (var item in _context.LoginLogs.ToArray())
                {
                    Logs.Add(_mapper.Map<LoginLogDto>(item));
                }
                return Ok(Logs);
            }
            catch (Exception e)
            {

                Logger.Error(e);
                return BadRequest(e);
            }

        }


        [HttpGet]
        [Route("getUserLog")]
        public ActionResult GetSpecificUserLogs(int id)
        {
            try
            {
                var Logs = new List<LoginLogDto>();
                User u = _context.Users.Where(x => x.Id == id && x.LoginLog!=null).FirstOrDefault();
                if (u != null) {
                    foreach (var item in u.LoginLog)
                    {
                        Logs.Add(_mapper.Map<LoginLogDto>(item));
                    }
                    Logger.Debug("Got user id:" + id + " login logs");
                    return Ok(Logs);
                }
                else {
                    Logger.Debug("Not found requested user Id=" + id);
                    return BadRequest("No such user");
                }
            }
            catch (Exception e)
            {

                Logger.Error(e);
                return BadRequest(e);
            }

        }

        [HttpGet]
        [Route("getDateRangeLog")]
        public ActionResult GetSpecificDateRangeLogs(string start,string end)
        {
            try
            {
                var st = DateTime.Parse(start);
                var et= DateTime.Parse(end);
                var Logs = new List<LoginLogDto>();
                var s = _context.LoginLogs.Where(x => x.LoginDate.Date == st.Date).FirstOrDefault();
                var e = _context.LoginLogs.Where(x => x.LoginDate.Date == et.Date).FirstOrDefault();
                if (s != null)
                {
                    if (e != null) { 
                           foreach (var item in _context.LoginLogs.Where(x=>x.LoginDate>=s.LoginDate && x.LoginDate<=e.LoginDate).ToArray())
                        {
                            Logs.Add(_mapper.Map<LoginLogDto>(item));
                        }
                    Logger.Debug("Got user  login logs");
                    return Ok(Logs);  
                    }else {
                        foreach (var item in _context.LoginLogs.Where(x => x.Id >= s.Id).ToArray())
                        {
                            Logs.Add(_mapper.Map<LoginLogDto>(item));
                        }
                        Logger.Debug("Got user  login logs");
                        return Ok(Logs);
                    }
                       
                }
                else
                {
                    Logger.Debug("Not found logs for that date range");
                    return BadRequest("No such user");
                }
            }
            catch (Exception e)
            {

                Logger.Error(e);
                return BadRequest(e);
            }

        }


        [HttpGet]
        [Route("getDateLog")]
        public ActionResult GetSpecificDateLogs(string start)
        {
            try
            {
                var st = DateTime.Parse(start);
                var Logs = new List<LoginLogDto>();
                var s = _context.LoginLogs.Where(x => x.LoginDate.Date == st.Date).FirstOrDefault();
                if (s != null)
                {
                    
                        foreach (var item in _context.LoginLogs.Where(x => x.LoginDate.Date == s.LoginDate.Date).ToArray())
                        {
                            Logs.Add(_mapper.Map<LoginLogDto>(item));
                        }
                        Logger.Debug("Got user  login logs");
                        return Ok(Logs);
                    

                }
                else
                {
                    Logger.Debug("Not found logs for that date");
                    return BadRequest("No such user");
                }
            }
            catch (Exception e)
            {

                Logger.Error(e);
                return BadRequest(e);
            }

        }
    }
}
