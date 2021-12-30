using Microsoft.AspNetCore.Mvc;

namespace MotionCaptureV2.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class DeviceController : ControllerBase
    {
        private readonly ILogger<DeviceController> _logger;
        //private readonly IDeviceManager _deviceManager;

        public DeviceController(ILogger<DeviceController> logger)//, IDeviceManager deviceManager)
        {
            _logger = logger;
            //_deviceManager = deviceManager;
        }

        // Not exactly awesome to use HttpGet for an action that results in state change, but makes it easy to test via 
        // my phone's browser.  Should probably change this to HttpPost later when I hook this up to Hubitat Elevation &
        // add authentication.

        [HttpGet]
        public IActionResult Reset()
        {
            //_deviceManager.Reset();

            return Ok();
        }
    }
}