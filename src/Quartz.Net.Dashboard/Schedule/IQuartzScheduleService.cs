/*
 * 提供 SignalR 呼叫介面
 */
using Quartz.Net.Dashboard.Model;
using Quartz.Net.Dashboard.Model.Dto;

namespace Quartz.Net.Dashboard.Schedule {
    public interface IQuartzScheduleService {
        /// <summary>
        /// 取得所有 Job
        /// </summary>
        /// <returns></returns>
        public Task<IEnumerable<JobInfo>> GetAllJobs();

        public Task<ResponseModel<string>> AddSchedule();
        public Task<ResponseModel<string>> AddSchedule(JobInfo jobinfo);

        /// <summary>
        /// 暫停 Job
        /// </summary>
        /// <param name="jobKey"></param>
        /// <returns></returns>
        public Task<ResponseModel<string>> SuspendJob(JobKey jobKey);

        /// <summary>
        /// 恢復 Job
        /// </summary>
        /// <param name="jobKey"></param>
        /// <returns></returns>
        public Task<ResponseModel<string>> ResumeJob(JobKey jobKey);

        /// <summary>
        /// 立即執行 Job
        /// </summary>
        /// <param name="jobKey"></param>
        /// <returns></returns>
        public Task<ResponseModel<string>> ImmediatelyExecuteJob(JobKey jobKey);

        /// <summary>
        /// 刪除 Job
        /// </summary>
        /// <param name="jobKey"></param>
        /// <returns></returns>
        public Task<ResponseModel<string>> DeleteJob(JobKey jobKey);

        /// <summary>
        /// 建立 Job
        /// </summary>
        /// <param name="JobDetailList"></param>
        /// <returns></returns>
        public IJobDetail CreateJobDetail(JobInfo jobInfo);

        /// <summary>
        /// 建立 Trigger
        /// </summary>
        /// <param name="JobDetailList"></param>
        /// <returns></returns>
        public ITrigger CreateTrigger(JobInfo jobInfo);
    }
}
