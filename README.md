# Quartz.Net-Dashboard
Quartz.Net 持久化及Dashboard的作法

### 環境：
資料庫：MSSQLLocal
.Net 版本：8
安裝套件
  Microsoft.EntityFramworkCore
  Microsoft.EntityFramworkCore.Design
  Microsoft.EntityFramworkCore.SqlServer
  Microsoft.EntityFramworkCore.Tools
  Microsoft.Data.SqlClient
  Quartz
  Quartz.AspNetCore
  Quartz.Serialization.Json
  Serilog.AspNetCore
  Dapper

### 專案目的
建立 Quartz.Net Dashboard, 主要是能提供介面對 Job 任務進行管理﹐如新增Job﹑修改Job行程﹑暫停行程(暫停時間範圍)﹑刪除行程,由 Job 任務管理介面進行 Job 排程的啟動或停止, 而實際執行的 Job 排程可經由 Dashboard 介面來觀察與監控, Dashboard 還可檢視目前 Job 的執行狀況。 Dashboard 是採用 Microsoft SignalR Core 以廣插方式做為即時通訊來更新資料的技術.

### 第一﹑基礎設置
建立一個 dbQuartzNet 資料庫﹐執行 Quartz 官網提供的 MS SQL schema, 加入Quartz 持久化相關的 table.
TbJobList 是提供給人員新增/異動/刪除 要執行的 Job
TbSample 是測試資料表
TbSampleSync 是測試將 TbSample 資料同步至 TbSampleSync
appsetting.json 中加入 Quartz 與資料庫連線設定
```c#{.line-numbers}
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=dbQuartzNet;Trusted_Connection=True;MultipleActiveResultSets=true"
  },
  "Quartz": {
    "quartz.scheduler.instanceName": "Quartz.Net.Exp",
  }
}
```
依官網所述可以將相閞的設定描述於 appsetting.json , 如下
```c#{.line-numbers}
  "Quartz": {
    // schedule name
    "quartz.scheduler.instanceName": "SRCExp3"
    // 線池個數
    "quartz.threadPool.threadCount": "20",
    // 類型為 JobStoreTX
    "quartz.jobStore.type": "Quartz.Impl.AdoJobStore.JobStoreTX, Quartz",
    // JobDataMap 中的數據都是字串
    //"quartz.jobStore.useProperties" : "true",
    // 資料來源名稱
    "quartz.jobStore.dataSource": "myDS",
    // 資料表名稱前綴
    "quartz.jobStore.tablePrefix": "QRTZ_",
    // 使用 Sql Server 的 ADO 操作代理類
    "quartz.jobStore.driverDelegateType": "Quartz.Impl.AdoJobStore.SqlServerDelegate, Quartz",
    // 資料庫連接字串
    "quartz.dataSource.myDS.connectionString": "Server=kevin-asus;Database=dbQuartzSample;Uid=apQuartzSample;Pwd=a12345;TrustServerCertificate=True;",
    // 資料庫類型
    // 官網有提示 SqlServer provider﹐從 Quartz 3.2 開始 .Net Core 預設為 Microsoft.Data.SqlClient 所以要安裝 Microsoft.Data.SqlClient套件
    "quartz.dataSource.myDS.provider": "SqlServer",
    // 序列化類型
    // 官網有註明 JSON 序列化來自單獨的 Quartz.Serialization.Json 套件
    "quartz.serializer.type": "json", // or binary
    // 自動生成 schedule 實例 ID, 主要為了保證集群中的實例具有唯一值
    "quartz.scheduler.instanceId": "AUTO",
    // 是否配置集群
    "quartz.jobStore.clustered": "true"
  }
```
這樣的設定如果是非持久化﹐在 WinForm 或 Console 下試過沒有問題﹐但在 .Net Core web api 注入的方式還沒試成功﹐所以本例在 appsetting.json 中僅留下 quartz.scheduler.instanceName 的設定﹐其它則在 Program.cs 中, 如下﹐在Program.cs 中注入了 Quartz 相關的設定
```c#{.line-numbers}
    #region Quartz
    // 讀取 appsetting.json 中對 Quartz 的配置
    var quartzSection = builder.Configuration.GetSection("Quartz");
    builder.Services.Configure<QuartzOptions>(quartzSection);

    // 使用持久化註冊 Quartz 服務
    builder.Services.AddQuartz(q => {
        // 使用持久化儲存, 連線資料庫為appseggint.json 中設定的 QuartzDB 連線字串
        q.UsePersistentStore(s => {
            s.UseProperties = false;
            s.RetryInterval = TimeSpan.FromSeconds(30);
            s.UseSqlServer(mssql => {
                mssql.ConnectionString = builder.Configuration.GetConnectionString("DefaultConnection").ToString();
                mssql.TablePrefix = "QRTZ_";
            });
            s.UseNewtonsoftJsonSerializer();
            s.UseClustering(c => {
                c.CheckinMisfireThreshold = TimeSpan.FromSeconds(20);
                c.CheckinInterval = TimeSpan.FromSeconds(10);
            });
        });

        q.UseMicrosoftDependencyInjectionJobFactory();
        q.UseDedicatedThreadPool(tp => {
            tp.MaxConcurrency = 10;
        });
    });

    builder.Services.AddQuartzHostedService(options => {
        options.WaitForJobsToComplete = true;
    });
    #endregion
```

### 第二﹐建置提供 SignalR Core 呼叫的 service
Dashboard 的資料更新採用 SignalR Core 做即時的推播更新, 後續會用到 Quartz 提供的 Listener, 所以採 SignalR 可以快速的提供訊息

首先建置了 IQuartzScheduleService 的 interface, 這當中的方法包含了對 Quartz Job Schedule 的查詣﹑暫停﹑恢復﹑停用﹑刪除的管理方法
```c#{.line-numbers}
    public interface IQuartzScheduleService {
        /// <summary>
        /// 取得所有 Job
        /// </summary>
        /// <returns></returns>
        public Task<IEnumerable<JobDetailList>> GetAllJob();

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
        public IJobDetail CreateJobDetail(JobDetailList jobDetailList);

        /// <summary>
        /// 建立 Trigger
        /// </summary>
        /// <param name="JobDetailList"></param>
        /// <returns></returns>
        public ITrigger CreateTrigger(JobDetailList jobDetailList);
    }
```

### 第三﹑建置 SignalR Hub
Dashboard 是利用 Microsfot SignalR Core 進行通訊, 接著建立 ScheduleHub.cs 
Hub 中提供的方法是查詢所有的 Job﹑暫停﹑重啟﹑立即執行和刪除 指定的 Job, 主要是呼叫 IQuartzScheduleService 提供的方法
在 Program.cs 中
```c#{.line-numbers}
...略
#region SignalR
builder.Services.AddSingleton<ScheduleHub>();
builder.Services.AddSignalR();
#endregion

var app = builder.Build();
...略
app.MapHub<ScheduleHub>("/scheduleHub");
app.Run();
```

### 第四﹑監聽器
Quartz 提供了多種的 Listener , 這裏先建立 JobListener, 透過 JobListener 監聽 Job 的變化呼叫 SignalR Hub 方法來及時更新訊息

### 第五﹑製作前端介面
前端介面採用 Vue3 cli 開發
這裏用到了 SignlR core ,所以要安裝套件
```npm
npm install @microsoft/signalr
```
