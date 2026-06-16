using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DataExportController : ControllerBase
    {
        private readonly BackgroundNotificationService _backgroundService;
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public DataExportController(BackgroundNotificationService backgroundService)
        {
            _backgroundService = backgroundService;
        }

        /// <summary>
        /// Triggers a background data export job. 
        /// User receives a notification with download link when data is ready (~30s).
        /// Returns immediately without blocking.
        /// </summary>
        /// <param name="userId">ID of the user requesting the export</param>
        /// <param name="dataType">Type of data to export (e.g. "users", "logs")</param>
        [HttpPost("request-export")]
        [AllowAnonymous]
        public IActionResult RequestExport(int userId, string dataType = "users")
        {
            try
            {
                var fileName = $"{dataType}_export_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                var downloadUrl = $"https://localhost:7184/DataExport/download/{fileName}";

                _backgroundService.EnqueueJob(userId, fileName, downloadUrl);

                Logger.Info($"Export job enqueued for user {userId}, type: {dataType}");

                return Accepted(new
                {
                    Message = "Export started. You will receive a notification when your data is ready.",
                    EstimatedWaitSeconds = 5,
                    FileName = fileName
                });
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Simulated download endpoint — in real app this would serve the generated file.
        /// </summary>
        [HttpGet("download/{fileName}")]
        [AllowAnonymous]
        public IActionResult Download(string fileName)
        {
            var csv = "Id,Login,Email,Type,CreationDate\n1,admin,admin@example.com,Admin,2024-01-01";
            var bytes = System.Text.Encoding.UTF8.GetBytes(csv);
            return File(bytes, "text/csv", fileName);
        }
    }
}
