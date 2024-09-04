/*
 *   IQuartzScheduleService 介面實作
 */
using Microsoft.AspNetCore.Http.HttpResults;
using Newtonsoft.Json;
using Quartz.Impl.Matchers;
using Quartz.Impl.Triggers;
using Quartz.Net.Dashboard.Lib;
using Quartz.Net.Dashboard.Model;
using Quartz.Net.Dashboard.Model.Dto;

namespace Quartz.Net.Dashboard.Schedule {
    public class QuartzScheduleService : IQuartzScheduleService {
        private readonly ILogger<QuartzScheduleService> _logger;
        public ISchedulerFactory _schedulerFactory;
        public IServiceProvider _serviceProvider;

        public QuartzScheduleService(ILogger<QuartzScheduleService> logger, ISchedulerFactory schedulerFactory,IServiceProvider serviceProvider) {
            _logger = logger;
            _schedulerFactory = schedulerFactory;
            _serviceProvider = serviceProvider;
        }

        #region Quartz 提供 SignalR core 查詢/暫停/重啟/刪除 Job
        /// <summary>
        /// 取得所有 Job
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<IEnumerable<JobInfo>> GetAllJobs() {
            try {
                _logger.LogInformation($"{DateTime.Now:yyyy/MM/dd HH:mm:ss}\tGetAllJob start!");

                var scheduler=await _schedulerFactory.GetScheduler(); //從 SchedulerFactory 取得 排程器 Scheduler (取得排程器是為了取得 JobDetail, 因為是持久化所以從資料庫中取回)

                List<JobKey> jobKeyList = new List<JobKey>();        //以 JobKey 存放所有的 Job, 每一個 Job 都有一個雖一的 JobKey
                List<JobInfo> jobInfoLst = new List<JobInfo>();   //存放每一筆 Job 的明細資料
                var groupNames = await scheduler.GetJobGroupNames(); //取得排程器中所有已知的 IJobDetail group

                // 以 groupname 取得 JobKey放入 jobKeyList 中
                foreach (var groupName in groupNames.OrderBy(t => t)) {
                    jobKeyList.AddRange(await scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals(groupName)));
                }

                // 以 JobKey 取得 Joh的明細 放入 jobInfoLst 中
                foreach (var jobKey in jobKeyList.OrderBy(t => t.Name)) {
                    var jobDetail=await scheduler.GetJobDetail(jobKey);  //以JobKey 取得 JobDetail
                    if (jobDetail == null) continue;

                    var triggersList = await scheduler.GetTriggersOfJob(jobKey);  //以JobKey取得所有 Trigger(一個 Job 可以有多個 Trigger)
                    var triggers=triggersList.AsEnumerable().FirstOrDefault();  //從 Trigger 清單中取得第一個 Trigger
                    if (triggers == null) continue;

                    // 檢查 trigger 的類型, 根據不同的類型給予 interval 不同的值
                    var interval = string.Empty;
                    if (triggers is SimpleTriggerImpl)
                        interval = (triggers as SimpleTriggerImpl)?.RepeatInterval.ToString();
                    else
                        interval = (triggers as CronTriggerImpl)?.CronExpressionString;

                    // 將每一筆 Job 的明細資料放入 jobInfoLst 中
                    jobInfoLst.Add(new JobInfo() {
                        JobName = jobKey.Name ?? "",
                        JobGroup = jobKey.Group ?? "",
                        TriggerState = await scheduler.GetTriggerState(triggers.Key),
                        PreviousFireTime = triggers.GetPreviousFireTimeUtc()?.LocalDateTime,
                        NextFireTime = triggers?.GetNextFireTimeUtc()?.LocalDateTime,
                        StartTime = triggers.StartTimeUtc.LocalDateTime,
                        EndTime = triggers.EndTimeUtc?.LocalDateTime,
                        JobDesc = jobDetail.Description ?? "",
                        ScheduleExpression = interval ?? "",
                        ScheduleExpressionDesc = triggers.Description ?? "",
                        JobData = JsonConvert.SerializeObject(jobDetail.JobDataMap),
                        JobType = null
                    });

                }

                return jobInfoLst;

            } catch(Exception er) {
                _logger.LogError($"{DateTime.Now:yyyy/MM/dd HH:mm:ss}\tGetAllJob error!---{er.Message}");
                throw new Exception($"GetAllJob error!---{er.Message}");
            }
        }

        #region 將任務加入排程中
        /// <summary>
        /// 新加入的任務, 加入 Quartz.Net 持久化資料庫中開始排程
        /// </summary>
        /// <returns></returns>
        public async Task<ResponseModel<string>> AddSchedule() {
            try {
                // JobDTL 是 Scoped：每個 HTTP 請求創建一次實例 或 Transient：每次請求都創建新的實例
                // QuartzScheduleService 是 Singleton：應用程式生命週期內只創建一次實例
                // 將 Scoped 服務注入到 Singleton 服務中時，會導致生命週期不匹配的問題。這是因為 Singleton 服務的生命週期比 Scoped 服務長，這可能會導致 Scoped 服務在其生命週期結束後仍被使用，從而引發錯誤
                // 所以不採用 DI注入﹐改用動態解析 IServiceProvider 
                using (var scope = _serviceProvider.CreateScope()) {
                    var _jobDTL=scope.ServiceProvider.GetRequiredService<JobDTL>();

                    var jobInfos = (await _jobDTL.GetJobInfos()).Where(x => x.JobStatus == "Y").ToList();
                    var scheduler = await _schedulerFactory.GetScheduler();
                    foreach (var item in jobInfos) {
                        // 以 JobKey 檢查 Job 是否已經存在於持久化資料庫中
                        JobKey jobKey = new JobKey(item.JobName, item.JobGroup);

                        if (await scheduler.CheckExists(jobKey)) {
                            continue;
                        } else {
                            JobInfo jobinfo = new JobInfo() {
                                Id = item.Id,
                                JobName = item.JobName,
                                JobGroup = item.JobGroup,
                                JobType = Type.GetType(item.JobTypeName),
                                JobDesc = item.JobDesc,
                                ScheduleExpression = item.ScheduleExpression,
                                ScheduleExpressionDesc = item.ScheduleExpressionDesc,
                                JobStatus = item.JobStatus
                            };
                            await scheduler.ScheduleJob(CreateJobDetail(jobinfo), CreateTrigger(jobinfo));
                        }
                    }

                    return new ResponseModel<string>();
                }
            } catch(Exception er) {
                return new ResponseModel<string>() {
                    Code = "99",
                    Message = $"QuartzScheduleService.AddSchedule error!---{er.Message}",
                    Data = null
                };
            }
        }

        /// <summary>
        /// 加入任務到排程
        /// </summary>
        /// <param name="jobInfo"></param>
        /// <returns></returns>
        public async Task<ResponseModel<string>> AddSchedule(JobInfo jobInfo) {
            try {
                JobKey jobKey = new JobKey(jobInfo.JobName, jobInfo.JobGroup);
                var scheduler = await _schedulerFactory.GetScheduler();
                if (await scheduler.CheckExists(jobKey)) {
                    return new ResponseModel<string>() {
                        Code = "97",
                        Message = $"Job 已存在於排程中",
                        Data = null
                    };
                } else {
                    await scheduler.ScheduleJob(CreateJobDetail(jobInfo), CreateTrigger(jobInfo));
                    _logger.LogInformation($"QuartzScheduleService.AddSchedule[jobName={jobInfo.JobName},jobGroup={jobInfo.JobGroup}] success!");
                    return new ResponseModel<string>();
                }
            }catch(Exception er) {
                string errMsg=$"QuartzScheduleService.AddSchedule[jobName={jobInfo.JobName},jobGroup={jobInfo.JobGroup}] error!---{er.Message}";
                _logger.LogError(errMsg);
                return new ResponseModel<string>("99", errMsg, null);
            }
        }
        #endregion

        /// <summary>
        /// 暫停 Job
        /// </summary>
        /// <param name="jobKey"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<ResponseModel<string>> SuspendJob(JobKey jobKey) {
            string jobString = $"Job Group={jobKey.Group},Name={jobKey.Name}";
            try {
                var scheduler = await _schedulerFactory.GetScheduler();  //從 SchedulerFactory 取得 排程器 Scheduler
                //var jobDetail = await scheduler.GetJobDetail(jobKey);    //以 JobKey 取得 JobDetail
                if (await scheduler.CheckExists(jobKey)) {
                    await scheduler.PauseJob(jobKey);  //對目標 job 暫停
                    
                    _logger.LogInformation($"{jobString} 暫停成功");
                    return new ResponseModel<string>();
                } else {
                    string errMsg = $"{jobString} 不存在,暫停失敗!";
                    _logger.LogError(errMsg);
                    return new ResponseModel<string>("97", errMsg, null);
                }    
            } catch (Exception er) {
                string errMsg = $"{jobString} 暫停失敗!--[{er.Message}]";
                _logger.LogError(errMsg);
                return new ResponseModel<string>("99", errMsg, null);
            }

        }

        /// <summary>
        /// 重啟 Job
        /// </summary>
        /// <param name="jobKey"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<ResponseModel<string>> ResumeJob(JobKey jobKey) {
            ResponseModel<string> responseModel = new ResponseModel<string>();
            string errMsg = string.Empty;
            string jobString = $"Job Group={jobKey.Group},Name={jobKey.Name}";
            try {
                var scheduler = await _schedulerFactory.GetScheduler();
                //檢查 Job 是否存在
                if (await scheduler.CheckExists(jobKey)) {
                    await scheduler.ResumeJob(jobKey);  //重啟目標 job
                    responseModel.Code = "200";
                    responseModel.Data = "Success";
                    _logger.LogInformation($"{jobString} 重啟成功");
                } else {
                    errMsg = $"{jobString} 不存在,無法重啟";
                    responseModel.Code = "97";
                    responseModel.Message = errMsg;
                    _logger.LogWarning(errMsg);
                }
            } catch (Exception er) {
                errMsg = $"{jobString} 重啟失敗!--[{er.Message}]";
                responseModel.Code = "99";
                responseModel.Message = errMsg;
            }

            return responseModel;
        }

        /// <summary>
        /// 立刻執行 Job
        /// </summary>
        /// <param name="jobKey"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<ResponseModel<string>> ImmediatelyExecuteJob(JobKey jobKey) {
            ResponseModel<string> responseModel = new ResponseModel<string>();
            string errMsg = string.Empty;
            string jobString = $"Job Group={jobKey.Group},Name={jobKey.Name}";
            try {
                var scheduler = await _schedulerFactory.GetScheduler();
                await scheduler.TriggerJob(jobKey);

                responseModel.Code = "200";
                responseModel.Data = "Success";
                _logger.LogInformation($"{jobString} 立刻執行成功");
            } catch (Exception er) {
                errMsg = $"{jobString} 立刻執行失敗!--[{er.Message}]";
                responseModel.Code = "99";
                responseModel.Message = errMsg;
            }
            return responseModel;
        }

        /// <summary>
        /// 刪除 Job
        /// </summary>
        /// <param name="jobKey"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<ResponseModel<string>> DeleteJob(JobKey jobKey) {
            ResponseModel<string> responseModel = new ResponseModel<string>();
            string errMsg = string.Empty;
            string jobString = $"Job Group={jobKey.Group},Name={jobKey.Name}";
            try {
                //刪除 Job 前必須先暫停 Job 執行才能刪除
                var result = await SuspendJob(jobKey);
                if (result.Code == "200") {
                    var scheduler = await _schedulerFactory.GetScheduler();
                    await scheduler.DeleteJob(jobKey);

                    responseModel.Code = "200";
                    responseModel.Data = "Success";
                    _logger.LogInformation($"{jobString} 刪除成功");
                } else {
                    errMsg = $"{jobString} 刪除失敗!--[Job暫停失敗:{result.Message}]";
                    responseModel.Code = "98";
                    responseModel.Message = errMsg;
                }

            } catch (Exception er) {
                errMsg = $"{jobString} 刪除Job失敗!--[{er.Message}]";
                responseModel.Code = "99";
                responseModel.Message = errMsg;
            }
            return responseModel;
        }
        #endregion

        #region Create Job & trigger
        /// <summary>
        /// 建立 JobDetail
        /// </summary>
        /// <param name="jobInfo"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public IJobDetail CreateJobDetail(JobInfo jobInfo) {
            try {
                var jobType = jobInfo.JobType;
                var jobDetail = JobBuilder.Create(jobType)
                    .WithIdentity(jobInfo.JobName, jobInfo.JobGroup)
                    .WithDescription(jobInfo.JobDesc)
                    .Build();
                //使用 Put 傳資料給 Job 工作, 之後再研究

                return jobDetail;
            } catch (Exception er) {
                string errMsg = $"CreateJobDetail error.--[{er.Message}]";
                _logger.LogError(errMsg);
                throw new Exception(errMsg);
            }
        }

        /// <summary>
        /// 建立 Trigger
        /// </summary>
        /// <param name="jobInfo"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public ITrigger CreateTrigger(JobInfo jobInfo) {
            try {
                var trigger = TriggerBuilder.Create()
                    .WithIdentity($"{jobInfo.JobName}.trigger", jobInfo.JobGroup)
                    .WithCronSchedule(jobInfo.ScheduleExpression)
                    .WithDescription(jobInfo.ScheduleExpressionDesc)
                    .Build();

                return trigger;
            } catch (Exception er) {
                string errMsg = $"CreateTrigger error.--[{er.Message}]";
                _logger.LogError(errMsg);
                throw new Exception(errMsg);
            }
        }
        #endregion

    }
}
