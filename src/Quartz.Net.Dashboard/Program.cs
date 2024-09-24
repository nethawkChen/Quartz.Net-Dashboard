using Microsoft.EntityFrameworkCore;
using Quartz.Net.Dashboard.Model;
using Serilog.Events;
using Serilog;
using Quartz;
using Quartz.Net.Dashboard.Hubs;
using Quartz.Net.Dashboard.Listener;
using Quartz.Net.Dashboard.Schedule;
using Quartz.Net.Dashboard.Lib;
using Quartz.Net_Dashboard.JobImpl;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try {
    var builder = WebApplication.CreateBuilder(args);

    #region Serilog
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
    );
    #endregion

    // Add services to the container.

    builder.Services.AddControllers();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    #region 配置資料庫
    builder.Services.AddDbContext<dbQuartzNetContext>(options => {
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
        .UseLoggerFactory(LoggerFactory.Create(b => {
            b.AddConsole().AddDebug();
        }));
    });
    #endregion

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

    builder.Services.AddSingleton<IJobListener, JobListener>();
    builder.Services.AddSingleton<IQuartzScheduleService, QuartzScheduleService>();

    #region SignalR
    builder.Services.AddSingleton<ScheduleHub>();
    builder.Services.AddSignalR();
    #endregion

    builder.Services.AddScoped<JobDTL>();
    builder.Services.AddScoped<TbSampleSyncImpl>();

    var app = builder.Build();

    app.UseSerilogRequestLogging();  // Serilog

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment()) {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    #region 整合 Vue-cli
    app.UseDefaultFiles();
    app.UseStaticFiles();

    // SPA (可以將這一段抽離做為 Middleware
    app.Use(async (context, next) => {
        await next();

        //判斷是否要存取網頁﹐而不是發送API需求
        if (context.Response.StatusCode == 404 &&                         // 資源不存在
          !System.IO.Path.HasExtension(context.Request.Path.Value) &&     // 網址最後沒有帶副檔名
          !context.Request.Path.Value.StartsWith("/api"))                 // 網址不是 /api 開始 (這是因為用的是 Web API 專案﹐預設路徑是 /api
        {
            context.Request.Path = "/index.html";                         // 將網址導向 /index.html (這是 Vue 的起始網頁)
            context.Response.StatusCode = 200;                            // 將 HTTP 狀態改為 200 成功
            await next();
        }
    });
    #endregion

    //app.UseCors();

    #region global cors policy 允許所有皆可以呼叫,測試用
    app.UseCors(x => x
    .SetIsOriginAllowed(origin => true)
    .AllowAnyMethod()
    .AllowAnyHeader()
    .WithExposedHeaders("Content-Disposition", "X-File-Name")   //不加這一行﹐前端無法取得資料﹐這有點無言﹐因為前一行是 AllowAnyHeader竟然還不行
    .AllowCredentials());
    #endregion

    app.UseHttpsRedirection();

    app.UseAuthorization();

    app.MapControllers();

    #region 初始化資料
    using (var scope = app.Services.CreateScope()) {
        var services = scope.ServiceProvider;
        try {
            var context = services.GetRequiredService<dbQuartzNetContext>();
            DbInitializer.Initialize(context);
        } catch (Exception ex) {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "建立資料庫時發生錯誤");
        }
    }
    #endregion

    app.MapHub<ScheduleHub>("/scheduleHub");

    app.Run();
} catch(Exception er) {
    Log.Fatal(er, "Application terminated unexpectedly");
} finally {
    Log.CloseAndFlush();
}
