using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Quartz.Net.Dashboard.Lib;
using Quartz.Net.Dashboard.Model;
using Quartz.Net.Dashboard.Model.Dto;
using Quartz.Net.Dashboard.Model.Entities;
using Quartz.Net.Dashboard.Schedule;

namespace Quartz.Net.Dashboard.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class JobsMangerController : ControllerBase {
        private readonly ILogger<JobsMangerController> _logger;
        private readonly ISchedulerFactory _schedulerFactory;
        private readonly IQuartzScheduleService _quartzScheduleService;
        private readonly dbQuartzNetContext _dbQuartzNetContext;
        private readonly JobDTL _jobDTL;

        public JobsMangerController(ILogger<JobsMangerController> logger, ISchedulerFactory schedulerFactory, IQuartzScheduleService quartzScheduleService, dbQuartzNetContext dbQuartzNetContext, JobDTL jobDTL) {
            _logger = logger;
            _schedulerFactory = schedulerFactory;
            _quartzScheduleService = quartzScheduleService;
            _dbQuartzNetContext = dbQuartzNetContext;
            _jobDTL = jobDTL;
        }

        //public JobsMangerController(ILogger<JobsMangerController> logger, ISchedulerFactory schedulerFactory, IQuartzScheduleService quartzScheduleService, dbQuartzNetContext dbQuartzNetContext) {
        //    _logger = logger;
        //    _schedulerFactory = schedulerFactory;
        //    _quartzScheduleService = quartzScheduleService;
        //    _dbQuartzNetContext = dbQuartzNetContext;
        //}

        #region Quartz Schedule 相關作業
        /// <summary>
        /// 啟動 Quartz Scheduler
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("Start")]
        public async Task<IActionResult> Start() {
            var scheduler = await _schedulerFactory.GetScheduler();
            await scheduler.Start();
            return Ok();
        }

        /// <summary>
        /// 停止 Quartz Scheduler
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("Shutdown")]
        public async Task<IActionResult> Shutdown() {
            var scheduler = await _schedulerFactory.GetScheduler();
            await scheduler.Shutdown();

            return Ok();
        }

        ///// <summary>
        ///// 加入排程作業
        ///// </summary>
        ///// <returns></returns>
        //[HttpPost, Route("AddSchedule")]
        //public async Task<IActionResult> AddSchedule() {
        //    List<JobInfo> _allJobInfo = new List<JobInfo>();
        //    var jobInfos = (await _jobDTL.GetJobInfos()).Where(x => x.JobStatus == "Y").ToList();
        //    foreach (var item in jobInfos) {
        //        JobInfo jobinfo = new JobInfo();
        //        jobinfo.Id = item.Id;
        //        jobinfo.JobName = item.JobName;
        //        jobinfo.JobGroup = item.JobGroup;
        //        jobinfo.JobType = Type.GetType(item.JobTypeName);
        //        jobinfo.JobDesc = item.JobDesc;
        //        jobinfo.ScheduleExpression = item.ScheduleExpression;
        //        jobinfo.ScheduleExpressionDesc = item.ScheduleExpressionDesc;
        //        jobinfo.JobStatus = item.JobStatus;

        //        _allJobInfo.Add(jobinfo);
        //    }

        //    var scheduler = await _schedulerFactory.GetScheduler();

        //    foreach (var myJob in _allJobInfo) {
        //        JobKey jobKey = new JobKey(myJob.JobName, myJob.JobGroup);
        //        //先檢查 job 是否已經存在 Quartz.Net 持久化資料庫中
        //        if (await scheduler.CheckExists(jobKey)) {
        //            continue;
        //        } else {
        //            await scheduler.ScheduleJob(_quartzScheduleService.CreateJobDetail(myJob), _quartzScheduleService.CreateTrigger(myJob));
        //        }
        //    }

        //    return Ok();
        //}
        #endregion

        #region JobScheduleView.vue UI
        /// <summary>
        /// 取得所有 Job 任務資料
        /// </summary>
        /// <returns></returns>
        [HttpPost, Route("GetJobSchedules")]
        public async Task<IActionResult> GetJobSchedules() {
            try {
                var jobLst = await _dbQuartzNetContext.TbJobList.ToListAsync();
                return Ok(new ResponseModel<TbJobList>(jobLst));
            } catch (Exception er) {
                string errMsg = $"GetJobSchedules error.\t{er.Message}";
                return BadRequest(new ResponseModel<TbJobList>("99", errMsg, null));
            }
        }

        /// <summary>
        /// 以 Id 取得 Job 任務
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost, Route("GetJobById")]
        public async Task<IActionResult> GetJobById(int id) {
            try {
                var jobLst = await _dbQuartzNetContext.TbJobList.Where(x => x.Id == id).FirstOrDefaultAsync();
                return Ok(new ResponseModel<TbJobList>(jobLst));
            } catch (Exception er) {
                string errMsg = $"GetJobById error.\t{er.Message}";
                return BadRequest(new ResponseModel<TbJobList>("99", errMsg, null));
            }
        }

        /// <summary>
        /// 新增 Job 任務
        /// </summary>
        /// <param name="jobList"></param>
        /// <returns></returns>
        [HttpPost, Route("Create")]
        public async Task<IActionResult> Create(TbJobList jobList) {
            try {
                //以 jobList 的 JobName 和 JobGroup 查詢 _dbQuartzNetContext.TbJobList 中是否有相同的資料, 如果有則回傳錯誤
                if (await _dbQuartzNetContext.TbJobList.Where(x => x.JobName == jobList.JobName && x.JobGroup == jobList.JobGroup).AnyAsync()) {
                    string errMsg = $"Create error.\t{jobList.JobName}:{jobList.JobGroup} is exist.";
                    return Ok(new ResponseModel<string>("90", errMsg, null));
                }

                _dbQuartzNetContext.TbJobList.Add(jobList);
                await _dbQuartzNetContext.SaveChangesAsync();

                if (jobList.JobStatus == "Y") {
                    await StartScheduler(jobList);
                }

                return Ok(new ResponseModel<string>());
            } catch (Exception er) {
                string errMsg = $"Create error.\t{er.Message}";
                return Ok(new ResponseModel<string>("99", errMsg, null));
            }
        }

        /// <summary>
        /// 更新 Job 任務
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost, Route("Update")]
        public async Task<IActionResult> Update(updData data) {
            try {
                _dbQuartzNetContext.TbJobList.Update(data.newData);
                await _dbQuartzNetContext.SaveChangesAsync();

                var jobKey = new JobKey(data.oldData.JobName, data.oldData.JobGroup);
                if (data.oldData.JobStatus == "Y" && data.newData.JobStatus == "N") {
                    await _quartzScheduleService.DeleteJob(jobKey);
                } else if (data.oldData.JobStatus == "N" && data.newData.JobStatus == "Y") {
                    await StartScheduler(data.newData);
                }

                return Ok(new ResponseModel<string>());
            }catch(Exception er) {
                string errMsg = $"Update error.\t{er.Message}";
                return BadRequest(new ResponseModel<string>("99", errMsg, null));
            }
        }

        //public async Task<IActionResult> Update(string jobName, string jobGroup, string jobStatus) {
        //    try {
        //        var tbJobList= await _dbQuartzNetContext.TbJobList.Where(x => x.JobName == jobName && x.JobGroup == jobGroup).FirstOrDefaultAsync();
        //        tbJobList.JobStatus = jobStatus;

        //        _dbQuartzNetContext.TbJobList.Update(tbJobList);
        //        await _dbQuartzNetContext.SaveChangesAsync();

        //    }catch(Exception er) {

        //    }
        //}

        /// <summary>
        /// 刪除 Job 任務
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost, Route("Delete")]
        public async Task<IActionResult> Delete(TbJobList jobList) {
            try {
                _dbQuartzNetContext.TbJobList.Remove(await _dbQuartzNetContext.TbJobList.Where(x => x.Id == jobList.Id).FirstOrDefaultAsync());
                await _dbQuartzNetContext.SaveChangesAsync();
                return Ok(new ResponseModel<string>());
            } catch (Exception er) {
                string errMsg = $"Delete error.\t{er.Message}";
                return BadRequest(new ResponseModel<string>("99", errMsg, null));
            }
        }
        #endregion

        #region 啟動和刪除 Quartz 排程
        /// <summary>
        /// 啟動排程
        /// </summary>
        /// <param name="jobList"></param>
        /// <returns></returns>
        private async Task StartScheduler(TbJobList jobList) {
            try {
                JobInfo jobinfo = new JobInfo();
                jobinfo.Id = jobList.Id;
                jobinfo.JobName = jobList.JobName;
                jobinfo.JobGroup = jobList.JobGroup;

                string className = $"Quartz.Net.Dashboard.Jobs.{jobList.JobTypeName}"; ;  //type 的名稱要加上命名空間
                Type jobType = Type.GetType(className);
                jobinfo.JobType = jobType;

                jobinfo.JobDesc = jobList.JobDesc;
                jobinfo.ScheduleExpression = jobList.ScheduleExpression;
                jobinfo.ScheduleExpressionDesc = jobList.ScheduleExpressionDesc;
                jobinfo.JobStatus = jobList.JobStatus;

                var result = await _quartzScheduleService.AddSchedule(jobinfo);

                if (result.Code != "200") {
                    throw new Exception($"StartScheduler error!--[{result.Message}]");
                }
            } catch (Exception er) {
                throw new Exception($"StartScheduler exception.--[{er.Message}]");
            }
        }

        /// <summary>
        /// 刪除排程
        /// </summary>
        /// <param name="jobName"></param>
        /// <param name="jobGroup"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private async Task StopScheduler(string jobName, string jobGroup) {

            try {
                var jobKey = new JobKey(jobName, jobGroup);
                var result = await _quartzScheduleService.DeleteJob(jobKey);
                if (result.Code != "200") {
                    throw new Exception($"StopScheduler error!--[{result.Message}]");
                }
            } catch (Exception er) {
                throw new Exception($"StopScheduler exception.--[{er.Message}]");
            }
        }
        #endregion
    }


    public class updData {
        public TbJobList newData { get; set; }
        public TbJobList oldData { get; set; }
    }
}
