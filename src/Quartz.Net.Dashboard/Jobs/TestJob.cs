
namespace Quartz.Net.Dashboard.Jobs {
    public class TestJob : IJob {
        private readonly ILogger<TestJob> _logger;

        public TestJob(ILogger<TestJob> logger) {
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context) {
            try {
                _logger.LogInformation($"{DateTime.Now:yyyy/MM/dd HH:mm:ss}\tTestJob start.");
                Console.WriteLine($"{DateTime.Now:yyyy/MM/dd HH:mm:ss}\t這裏執行 TestJob 成功!");
            } catch (Exception er) {
                _logger.LogError($"{DateTime.Now:yyyy/MM/dd HH:mm:ss}\tTestJob error.\t{er.Message}");
                Console.WriteLine($"{DateTime.Now:yyyy/MM/dd HH:mm:ss}\t這裏執行 TestJob 失敗!{er.Message}");
            }
        }
    }
}
