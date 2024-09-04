using Quartz.Net.Dashboard.Model.Entities;

namespace Quartz.Net.Dashboard.Model {
    public class DbInitializer {
        public static void Initialize(dbQuartzNetContext context) {
            context.Database.EnsureCreated();  //EF 自動建立資料庫。看到沒有資料庫﹐就會建立一個資料庫

            //先檢查資料庫中是否有資料
            if (context.TbSample.Any()) {
                return; // DB中的 TB_Manager 已有資料﹐不建資料離開
            }

            //建立 TbSample 測試資料
            var tbSamples = new TbSample[] {
                new TbSample{AgentId="Tom",AgentName="湯姆大叔",Gender="M",BirthDate=new DateTime(1997,03,09),HireDate=new DateTime(2021,5,8),ResignationDate=null,IsActive="Y",CreatedAt=DateTime.Now,CreatedBy="admin",UpdatedAt=DateTime.Now,UpdatedBy="admin"},
            };
            foreach(TbSample s in tbSamples) {
                context.TbSample.Add(s);
            }
            context.SaveChanges();
        }
    }
}
