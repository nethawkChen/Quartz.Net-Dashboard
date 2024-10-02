/*
 * Description: 測試案例, 將TbSample資料同步到TbSampleSync Job
 *      Author: Kevin Chen
 * Create Date: 2024.06.01
 */
using Quartz.Net.Dashboard.Model;
using Quartz.Net_Dashboard.JobImpl;

namespace Quartz.Net.Dashboard.Jobs {
    [DisallowConcurrentExecution]
    public class TbSampleSyncJob : IJob {
        private readonly ILogger<TestJob> _logger;
        private readonly dbQuartzNetContext _dbQuartzNetContext;
        private readonly TbSampleSyncImpl _tbSampleSyncImpl;

        public TbSampleSyncJob(ILogger<TestJob> logger, dbQuartzNetContext dbQuartzNetContext, TbSampleSyncImpl tbSampleSyncImpl) {
            _logger = logger;
            _dbQuartzNetContext = dbQuartzNetContext;
            _tbSampleSyncImpl = tbSampleSyncImpl;
        }

        public async Task Execute(IJobExecutionContext context) {
            await _tbSampleSyncImpl.Run();
        }

    }
}
