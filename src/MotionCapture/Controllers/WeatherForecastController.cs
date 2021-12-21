using Microsoft.AspNetCore.Mvc;
using MotionCapture.Application.Device;

namespace MotionCapture.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IDeviceManager _deviceManager;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IDeviceManager deviceManager)
        {
            _logger = logger;
            _deviceManager = deviceManager;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {

            _logger.LogInformation("Executing Get action method.");

            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [HttpGet]
        public IActionResult Reset()
        {
            _deviceManager.Reset();

            return Ok();
        }
    }
}