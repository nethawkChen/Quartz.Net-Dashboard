/*
 * Description: 對指定的 Job 進行 debug
 *      Author: Kevin Chen
 * Create Date: 2024.06.01
 */
using Microsoft.AspNetCore.Mvc;
using Quartz.Net_Dashboard.JobImpl;

namespace Quartz.Net_Dashboard.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class DebugController : ControllerBase {
        private readonly ILogger<DebugController> _logger;
        private readonly TbSampleSyncImpl _tbSampleSyncImpl;

        public DebugController(ILogger<DebugController> logger, TbSampleSyncImpl tbSampleSyncImpl) {
            _logger = logger;
            _tbSampleSyncImpl = tbSampleSyncImpl;
        }


        [HttpGet, Route("TbSampleSyncJob")]
        public async Task<IActionResult> debugTbSampleSyncJob() {
            await _tbSampleSyncImpl.Run();

            return Ok();
        }
    }
}
