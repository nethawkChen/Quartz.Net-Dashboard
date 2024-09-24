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

    #region �t�m��Ʈw
    builder.Services.AddDbContext<dbQuartzNetContext>(options => {
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
        .UseLoggerFactory(LoggerFactory.Create(b => {
            b.AddConsole().AddDebug();
        }));
    });
    #endregion

    #region Quartz
    // Ū�� appsetting.json ���� Quartz ���t�m
    var quartzSection = builder.Configuration.GetSection("Quartz");
    builder.Services.Configure<QuartzOptions>(quartzSection);

    // �ϥΫ��[�Ƶ��U Quartz �A��
    builder.Services.AddQuartz(q => {
        // �ϥΫ��[���x�s, �s�u��Ʈw��appseggint.json ���]�w�� QuartzDB �s�u�r��
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

    #region ��X Vue-cli
    app.UseDefaultFiles();
    app.UseStaticFiles();

    // SPA (�i�H�N�o�@�q�������� Middleware
    app.Use(async (context, next) => {
        await next();

        //�P�_�O�_�n�s�������M�Ӥ��O�o�eAPI�ݨD
        if (context.Response.StatusCode == 404 &&                         // �귽���s�b
          !System.IO.Path.HasExtension(context.Request.Path.Value) &&     // ���}�̫�S���a���ɦW
          !context.Request.Path.Value.StartsWith("/api"))                 // ���}���O /api �}�l (�o�O�]���Ϊ��O Web API �M�סM�w�]���|�O /api
        {
            context.Request.Path = "/index.html";                         // �N���}�ɦV /index.html (�o�O Vue ���_�l����)
            context.Response.StatusCode = 200;                            // �N HTTP ���A�אּ 200 ���\
            await next();
        }
    });
    #endregion

    //app.UseCors();

    #region global cors policy ���\�Ҧ��ҥi�H�I�s,���ե�
    app.UseCors(x => x
    .SetIsOriginAllowed(origin => true)
    .AllowAnyMethod()
    .AllowAnyHeader()
    .WithExposedHeaders("Content-Disposition", "X-File-Name")   //���[�o�@��M�e�ݵL�k���o��ơM�o���I�L���M�]���e�@��O AllowAnyHeader���M�٤���
    .AllowCredentials());
    #endregion

    app.UseHttpsRedirection();

    app.UseAuthorization();

    app.MapControllers();

    #region ��l�Ƹ��
    using (var scope = app.Services.CreateScope()) {
        var services = scope.ServiceProvider;
        try {
            var context = services.GetRequiredService<dbQuartzNetContext>();
            DbInitializer.Initialize(context);
        } catch (Exception ex) {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "�إ߸�Ʈw�ɵo�Ϳ��~");
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
