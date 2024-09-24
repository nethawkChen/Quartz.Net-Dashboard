using Microsoft.EntityFrameworkCore;
using Quartz.Net.Dashboard.Controllers;
using Quartz.Net.Dashboard.Model;
using Quartz.Net.Dashboard.Model.Entities;

namespace Quartz.Net.Dashboard.Lib {
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

        #region Add
        /// <summary>
        /// 新增 TbJobList
        /// </summary>
        /// <param name="jobList"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<string> Create(TbJobList jobList) {
            string errMsg = "";
            try {
                //以 jobList 的 JobName 和 JobGroup 查詢 _dbQuartzNetContext.TbJobList 中是否有相同的資料, 如果有則回傳錯誤
                if (await _dbQuartzNetContext.TbJobList.Where(x => x.JobName == jobList.JobName && x.JobGroup == jobList.JobGroup).AnyAsync()) {
                    errMsg = $"Create error.\t{jobList.JobName}:{jobList.JobGroup} is exist.";
                    return errMsg;
                }

                _dbQuartzNetContext.TbJobList.Add(jobList);
                await _dbQuartzNetContext.SaveChangesAsync();

                return errMsg;
            } catch(Exception er) {
                throw new Exception($"{DateTime.Now:yyyy/MM/dd HH:mm:ss}\tJobDTL.Create 失敗!--{er.Message}");
            }
        }
        #endregion

        #region Update
        /// <summary>
        /// 異動 TbJobList
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task Update(updData data) {
            try {
                _dbQuartzNetContext.TbJobList.Update(data.newData);
                await _dbQuartzNetContext.SaveChangesAsync();
            } catch(Exception er) {
                throw new Exception($"{DateTime.Now:yyyy/MM/dd HH:mm:ss}\tJobDTL.Update 失敗!--{er.Message}");
            }
        }

        /// <summary>
        /// 異動 TbJobList
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
        #endregion

        #region delete
        public async Task Delete(TbJobList jobList) {
            try {
                _dbQuartzNetContext.TbJobList.Remove(await _dbQuartzNetContext.TbJobList.Where(x => x.Id == jobList.Id).FirstOrDefaultAsync());
                await _dbQuartzNetContext.SaveChangesAsync();
            } catch(Exception er) {
                throw new Exception($"{DateTime.Now:yyyy/MM/dd HH:mm:ss}\tJobDTL.Delete 失敗!--{er.Message}");
            }
        }
        #endregion
    }
}
