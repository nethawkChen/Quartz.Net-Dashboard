using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.SignalR;
using Quartz.Net.Dashboard.Lib;
using Quartz.Net.Dashboard.Model;
using Quartz.Net.Dashboard.Model.Dto;
using Quartz.Net.Dashboard.Schedule;

namespace Quartz.Net.Dashboard.Hubs {
    public class ScheduleHub:Hub {
        private ILogger<ScheduleHub> _logger;
        private ISchedulerFactory _scheduleFactory;
        private IQuartzScheduleService _quartzService;
        private IServiceProvider _serviceProvider;

        public ScheduleHub(ILogger<ScheduleHub> logger, ISchedulerFactory scheduleFactory, IQuartzScheduleService quartzService, IServiceProvider serviceProvider) {
            _logger = logger;
            _scheduleFactory = scheduleFactory;
            _quartzService = quartzService;
            _serviceProvider = serviceProvider;
        }

        #region SignalR 生命週期
        /// <summary>
        /// OnConnected 用戶端連線
        /// </summary>
        /// <returns></returns>
        public override async Task OnConnectedAsync() {
            _logger.LogInformation("ScheduleHub OnConnectedAsync");
            try {
                await Groups.AddToGroupAsync(Context.ConnectionId, "SignalR Users");
                //await NotifyJobSTatusChange();
                await base.OnConnectedAsync();

                if (Clients != null) {
                    _logger.LogDebug($"OnConnectedAsync Clients is not null");
                }

                _logger.LogInformation($"ScheduleHub OnConnectedAsync Success:{Context.ConnectionId}");
            }catch(Exception er) {
                _logger.LogError($"ScheduleHub OnConnectedAsync Error: {er.Message}");
            }
        }

        /// <summary>
        /// OnDisconnected 用戶端斷線
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public override async Task OnDisconnectedAsync(Exception exception) {
            _logger.LogInformation("ScheduleHub OnDisconnectedAsync");
            try {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, "SignalR Users");
                await base.OnDisconnectedAsync(exception);
                _logger.LogInformation($"ScheduleHub OnDisconnectedAsync Success:{Context.ConnectionId}");
             }catch(Exception er) {
                _logger.LogError($"ScheduleHub OnDisconnectedAsync Error: {er.Message}");
            }
        }
        #endregion

        #region Hub service method
        /// <summary>
        /// 通知 Job 狀態改變
        /// </summary>
        /// <returns></returns>
        public async Task NotifyJobStatusChange() {
            try {
                if (Clients != null) {
                    _logger.LogInformation("監聽器呼叫改變狀態");
                    await Clients.All.SendAsync("JobStatusChange");   //呼叫 前端提供的方法 JobStgatusChange
                }
            } catch (Exception er) {
                _logger.LogError($"Hub NotifyJobStatusChange error: {er.Message}");
            }
        }

        /// <summary>
        /// 取得所有 Job
        /// </summary>
        /// <returns></returns>
        public async Task<ResponseModel<List<JobInfo>>> GetAllJobs() {
            ResponseModel<List<JobInfo>> responseModel = new ResponseModel<List<JobInfo>>();
            try {
                List<JobInfo> jobDetailList = (await _quartzService.GetAllJobs()).ToList();

                responseModel.Code = "200";
                responseModel.Data = jobDetailList;

            } catch (Exception er) {
                _logger.LogError($"Hub GetAllJobs error: {er.Message}");

                responseModel.Code = "99";
                responseModel.Data = null;
                responseModel.Message = er.Message;
            }
            return responseModel;
        }

        /// <summary>
        /// 暫停 Job
        /// </summary>
        /// <param name="jobKey"></param>
        /// <returns></returns>
        public async Task<ResponseModel<string>> SuspendJob(string jobName, string groupName) {
            JobKey jobKey = new JobKey(jobName, groupName);
            return await _quartzService.SuspendJob(jobKey);
        }

        /// <summary>
        /// 重啟 Job
        /// </summary>
        /// <param name="jobKey"></param>
        /// <returns></returns>
        public async Task<ResponseModel<string>> ResumeJob(string jobName, string groupName) {
            JobKey jobKey = new JobKey(jobName, groupName);
            return await _quartzService.ResumeJob(jobKey);
        }

        /// <summary>
        /// 立即執行 job
        /// </summary>
        /// <param name="jobName"></param>
        /// <param name="groupName"></param>
        /// <returns></returns>
        public async Task<ResponseModel<string>> ImmediatelyExecuteJob(string jobName, string groupName) {
            JobKey jobKey = new JobKey(jobName, groupName);
            return await _quartzService.ImmediatelyExecuteJob(jobKey);
        }

        /// <summary>
        /// 刪除 Job
        /// </summary>
        /// <param name="jobName"></param>
        /// <param name="groupName"></param>
        /// <returns></returns>
        public async Task<ResponseModel<string>> DeleteJob(string jobName, string groupName) {
            string msg = "";
            JobKey jobKey = new JobKey(jobName, groupName);

            var result = await _quartzService.DeleteJob(jobKey);
            if (result.Code == "200") {
                _logger.LogInformation($"Delete Job Success: {jobName}:{groupName}");
                
                try {
                    using (var scope = _serviceProvider.CreateScope()) {
                        var _jobDTL = scope.ServiceProvider.GetRequiredService<JobDTL>();
                        await _jobDTL.Update(jobName, groupName, "N");
                        _logger.LogInformation($"Update Job 任務 N Success: {jobName}:{groupName}");

                        return result;
                    }
                } catch(Exception er) {
                    result.Code = "89";
                    result.Message = $"刪除 Job 排程成功, 但更新 Job 任務狀態失敗: {jobName}:{groupName}--[{er.Message}]";
                    _logger.LogError(result.Message);

                    return result;
                }
            } else {
                msg = $"Delete Job Error: {jobName}:{groupName}--[{result.Message}]";
                _logger.LogError(msg);
                return result;
            }
            //return await _quartzService.DeleteJob(jobKey);
        }
        #endregion
    }
}
