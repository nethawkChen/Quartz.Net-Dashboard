/*
 * Description: 測試案例, 將TbSample資料同步到TbSampleSync Job 實作
 *      Author: Kevin Chen
 * Create Date: 2024.06.01
 */
using Microsoft.EntityFrameworkCore;
using Quartz.Net.Dashboard.Model;
using Quartz.Net.Dashboard.Model.Entities;

namespace Quartz.Net_Dashboard.JobImpl {
    public class TbSampleSyncImpl {
        private readonly ILogger<TbSampleSyncImpl> _logger;
        private readonly dbQuartzNetContext _dbQuartzNetContext;

        public TbSampleSyncImpl(ILogger<TbSampleSyncImpl> logger, dbQuartzNetContext dbQuartzNetContext) {
            _logger = logger;
            _dbQuartzNetContext = dbQuartzNetContext;
        }

        public async Task Run() {
            string msg = "";
            msg = $"{DateTime.Now:yyyy/MM/dd HH:mm:ss}\tTbSampleSyncJob 開始執行!";
            _logger.LogInformation(msg);
            Console.WriteLine(msg);

            try {
                //TbSample UpdatedAt 是日期型態取前一天的資料
                //var sample = await _dbQuartzNetContext.TbSample
                //    .Where(x => x.CreatedAt < DateTime.Now && >= DateTime.Now.AddDays(-1))
                //    .ToListAsync();

                var data = _dbQuartzNetContext.TbSample
                    .Where(x => x.CreatedAt < DateTime.Now && x.CreatedAt >= DateTime.Now.AddMinutes(-5))
                    .AsAsyncEnumerable();

                int count = 0;

                await foreach (var item in data) {
                    var tbSampleSyncData = new TbSampleSync {
                        Id = item.Id,
                        AgentId = item.AgentId,
                        AgentName = item.AgentName,
                        Gender = item.Gender,
                        BirthDate = item.BirthDate,
                        HireDate = item.HireDate,
                        ResignationDate = item.ResignationDate,
                        IsActive = item.IsActive,
                        CreatedAt = item.CreatedAt,
                        CreatedBy = item.CreatedBy,
                        UpdatedAt = item.UpdatedAt,
                        UpdatedBy = item.UpdatedBy
                    };
                    await _dbQuartzNetContext.TbSampleSync.AddAsync(tbSampleSyncData);
                    await _dbQuartzNetContext.SaveChangesAsync();

                    count++;
                }

                msg = $"{DateTime.Now:yyyy/MM/dd HH:mm:ss}\tTbSampleSyncJob 執行完畢. 一共新增 {count} 筆資料。";
                _logger.LogInformation(msg);
                Console.WriteLine(msg);

            } catch (Exception er) {
                msg = $"{DateTime.Now:yyyy/MM/dd HH:mm:ss}\tTbSampleSyncJob error.\t{er.Message}";
                _logger.LogError(msg);
                Console.WriteLine(msg);
            }
        }
    }
}
