using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Quartz.Net.Dashboard.Model;
using Quartz.Net.Dashboard.Model.Entities;

namespace Quartz.Net.Dashboard.Lib {
    //public class JobDTL {
    //    private readonly ILogger<JobDTL> _logger;
    //    private readonly IServiceProvider _serviceProvider;

    //    public JobDTL(ILogger<JobDTL> logger, IServiceProvider serviceProvider) {
    //        _logger = logger;
    //        _serviceProvider = serviceProvider;
    //    }

    //    public async Task<List<TbJobList>> GetJobInfos() {
    //        try {
    //            using (var scope = _serviceProvider.CreateScope()) {
    //                var dbContext = scope.ServiceProvider.GetRequiredService<dbQuartzNetContext>();
    //                return await dbContext.TbJobList.ToListAsync();
    //            }
    //        } catch (Exception er) {
    //            _logger.LogError($"{DateTime.Now:yyyy/MM/dd HH:mm:ss}\tJobDTL.GetJobInfos 失敗!--{er.Message}");
    //            return null;
    //        }
    //    }
    //}

    public class JobDTL {
        private readonly ILogger<JobDTL> _logger;
        private readonly dbQuartzNetContext _dbQuartzNetContext;

        public JobDTL(ILogger<JobDTL> logger, dbQuartzNetContext dbQuartzNetContext) {
            _logger = logger;
            _dbQuartzNetContext = dbQuartzNetContext;
        }

        /// <summary>
        /// Get TbJobList Data
        /// </summary>
        /// <returns></returns>
        public async Task<List<TbJobList>> GetJobInfos() {
            try {
                return await _dbQuartzNetContext.TbJobList.ToListAsync();
            } catch (Exception er) {
                _logger.LogError($"{DateTime.Now:yyyy/MM/dd HH:mm:ss}\tJobDTL.GetJobInfos 失敗!--{er.Message}");
                return null;
            }
        }

        /// <summary>
        /// 異動 Job 任務狀態
        /// </summary>
        /// <param name="jobName">Job 名稱</param>
        /// <param name="jobGroup">Group 名稱</param>
        /// <param name="jobStatus">狀態(Y/P/N)</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task Update(string jobName, string jobGroup, string jobStatus) {
            try {
                var tbJobList = await _dbQuartzNetContext.TbJobList.Where(x => x.JobName == jobName && x.JobGroup == jobGroup).FirstOrDefaultAsync();
                tbJobList.JobStatus = jobStatus;

                _dbQuartzNetContext.TbJobList.Update(tbJobList);
                await _dbQuartzNetContext.SaveChangesAsync();

            } catch (Exception er) {
                string errMsg = $"{DateTime.Now:yyyy/MM/dd HH:mm:ss}\tJobDTL.Update 失敗!--{er.Message}";
                throw new Exception(errMsg);
            }
        }
    }
}
