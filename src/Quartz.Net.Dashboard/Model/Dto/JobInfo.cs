using Quartz.Net.Dashboard.Model.Entities;

namespace Quartz.Net.Dashboard.Model.Dto {
    public class JobInfo:TbJobList {
        /// <summary>
        /// 開始執行時間
        /// </summary>
        public DateTime? StartTime { get; set; }
        /// <summary>
        /// 結束執行時間
        /// </summary>
        public DateTime? EndTime { get; set; }
        /// <summary>
        /// 上次執行時間
        /// </summary>
        public DateTime? PreviousFireTime { get; set; }
        /// <summary>
        /// 下次執行時間
        /// </summary>
        public DateTime? NextFireTime { get; set; }
        /// <summary>
        /// 任務狀態 觸發器狀態
        /// </summary>
        public TriggerState TriggerState { get; set; }
        /// <summary>
        /// Job 型別(要執行的 Job 類別)
        /// </summary>
        public Type JobType { get; set; }
        /// <summary>
        /// 顯示任務狀態
        /// </summary>
        public string DisplayState {
            get {
                var state = string.Empty;
                switch (TriggerState) {
                    case TriggerState.Normal:
                        state = "正常";
                        break;
                    case TriggerState.Paused:
                        state = "暫停";
                        break;
                    case TriggerState.Complete:
                        state = "完成";
                        break;
                    case TriggerState.Error:
                        state = "錯誤";
                        break;
                    case TriggerState.Blocked:
                        state = "阻塞";
                        break;
                    case TriggerState.None:
                        state = "不存在";
                        break;
                    default:
                        state = "未知";
                        break;
                }
                return state;
            }
        }

        public JobInfo() { }

        /// <summary>
        /// 建構
        /// </summary>
        /// <param name="jobType">作業類型</param>
        /// <param name="jobGroup">作業群組</param>
        /// <param name="jobName">作業名稱</param>
        /// <param name="jobDesc">作業描述</param>
        /// <param name="scheduleExp">行程</param>
        /// <param name="scheduleExpDesc">行程描述</param>
        public JobInfo(Type jobType,string jobGroup,string jobName,string jobDesc,string scheduleExp,string scheduleExpDesc) {
            JobType = jobType;
            JobGroup = jobGroup;
            JobName = jobName;
            JobDesc = jobDesc;
            ScheduleExpression = scheduleExp;
            ScheduleExpressionDesc = scheduleExpDesc;
        }
    }
}
