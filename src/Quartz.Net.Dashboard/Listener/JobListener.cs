
using Quartz.Net.Dashboard.Hubs;

namespace Quartz.Net.Dashboard.Listener {
    public class JobListener : IJobListener {
        private readonly ILogger<JobListener> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public JobListener(ILogger<JobListener> logger, IServiceScopeFactory serviceScopeFactory) {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public string Name => "Job Listener";

        /// <summary>
        /// 工作執行完畢 (目前 Job 狀態尚未移出 Executing 清單)
        /// Scheduler 在JobDetail 被执行之后调用这个方法
        /// </summary>
        /// <param name="context"></param>
        /// <param name="jobException"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task JobWasExecuted(IJobExecutionContext context, JobExecutionException? jobException, CancellationToken cancellationToken = default) {
            try {               
                // 工作執行完畢 (目前 Job 狀態尚未移出 Executing 清單)
                var jobName = context.JobDetail.Key.Name;
                _logger.LogInformation($"{DateTime.Now:yyyy/MM/dd HH:mm:ss} - JobListener job {jobName} - Job 執行結束!");

                using (var scope = _serviceScopeFactory.CreateScope()) {
                    var scheduleHub = scope.ServiceProvider.GetRequiredService<ScheduleHub>();
                    await scheduleHub.NotifyJobStatusChange();
                }
            } catch (Exception er) {
                _logger.LogError($"JobListener JobWasExecuted error:{er.Message}");
            }
        }

        /// <summary>
        /// 工作將被執行(目前 Job 尚未進入 Executing 清單)
        /// Scheduler 在JobDetail 將要被執行時調用這個方法
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task JobToBeExecuted(IJobExecutionContext context, CancellationToken cancellationToken = default) {
            try {
                // 工作將被執行(目前 Job 尚未進入 Executing 清單)
                var jobName = context.JobDetail.Key.Name;
                _logger.LogInformation($"{DateTime.Now:yyyy/MM/dd HH:mm:ss} - JobListener job {jobName} - Job 即將執行!");

                using (var scope = _serviceScopeFactory.CreateScope()) {
                    var scheduleHub = scope.ServiceProvider.GetRequiredService<ScheduleHub>();
                    await scheduleHub.NotifyJobStatusChange();
                }
            } catch (Exception er) {
                _logger.LogError($"JobListener JobToBeExecuted error:{er.Message}");
            }
        }

        /// <summary>
        /// Scheduler 在JobDetail 即将被执行，但又被TriggerListener 否决了时调用这个方法
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Task JobExecutionVetoed(IJobExecutionContext context, CancellationToken cancellationToken = default) {
            throw new NotImplementedException();
        }
    }
}
