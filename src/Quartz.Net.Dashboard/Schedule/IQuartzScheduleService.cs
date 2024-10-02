/*
 * 提供 SignalR 呼叫介面
 */
/*
 * Description: Quartz Service, 提供排程的 新增﹑暫停﹑恢復﹑刪除等方法, 提供給 SignalR 調用
 *      Author: Kevin Chen
 * Create Date: 2024.06.01
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

        /// <summary>
        /// 加入新任務到 Quartz.Net 持久化資料庫中開始排程
        /// </summary>
        /// <param name="jobinfo"></param>
        /// <returns></returns>
        public Task<ResponseModel<string>> AddSchedule(JobInfo jobinfo);

        /// <summary>
        /// 暫停 Job 執行
        /// </summary>
        /// <param name="jobKey"></param>
        /// <returns></returns>
        public Task<ResponseModel<string>> SuspendJob(JobKey jobKey);

        /// <summary>
        /// 恢復 Job 執行
        /// </summary>
        /// <param name="jobKey"></param>
        /// <returns></returns>
        public Task<ResponseModel<string>> ResumeJob(JobKey jobKey);

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
