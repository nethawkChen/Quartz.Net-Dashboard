namespace Quartz.Net.Dashboard.Model.Entities {
    public class TbSample {
        public int Id { get; set; }
        // 代號
        public string AgentId { get; set; }
        // 姓名
        public string AgentName { get; set; }
        // 性別
        public string Gender { get; set; }
        // 出生日期
        public DateTime? BirthDate { get; set; }
        // 入職日期
        public DateTime? HireDate { get; set; }
        // 離職日期
        public DateTime? ResignationDate { get; set; }
        // 是否在職
        public string IsActive { get; set; }
        // 創建時間
        public DateTime CreatedAt { get; set; }
        // 創建者
        public string CreatedBy { get; set; }
        // 更新時間
        public DateTime UpdatedAt { get; set; }
        // 更新者
        public string UpdatedBy { get; set; }

    }
}
