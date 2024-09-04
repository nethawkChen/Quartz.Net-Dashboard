/*
 * Job 列表資料, 定義 Job 類型、描述、狀態﹑執行的計劃等
*/

namespace Quartz.Net.Dashboard.Model.Entities {
    public class TbJobList {
        /// <summary>
        /// PK
        /// </summary>
        public int? Id { get; set; }
        /// <summary>
        /// Job 名稱
        /// </summary>
        public string? JobName { get; set; }
        /// <summary>
        /// Job 群組
        /// </summary>
        public string? JobGroup { get; set; }
        /// <summary>
        /// 任務類型(這裏填入 Job class 名稱)
        /// </summary>
        public string? JobTypeName { get; set; }
        /// <summary>
        /// Job 描述
        /// </summary>
        public string? JobDesc { get; set; }
        /// <summary>
        /// Job 額外的數據
        /// </summary>
        public string? JobData { get; set; }
        /// <summary>
        /// 執行計劃的 Cron 或 Simple 表達式
        /// </summary>
        public string? ScheduleExpression { get; set; }
        /// <summary>
        /// 執行計劃的說明
        /// </summary>
        public string? ScheduleExpressionDesc {  get; set; }
        /// <summary>
        /// Job 狀態(表示 Job 是否啟用或停止)
        /// </summary>
        public string? JobStatus { get; set; }
        /// <summary>
        /// Job 建立者
        /// </summary>
        public string? CrAgent { get; set; }
        /// <summary>
        /// 建立時間
        /// </summary>
        public DateTime? CrDateTime { get; set; }
        /// <summary>
        /// Job 異動者
        /// </summary>
        public string? UpAgent { get; set; }
        /// <summary>
        /// 異動時間
        /// </summary>
        public DateTime? UpDateTime { get; set; }
    }
}
