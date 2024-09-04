using Microsoft.EntityFrameworkCore;
using Quartz.Net.Dashboard.Model.Entities;

namespace Quartz.Net.Dashboard.Model {
    public class dbQuartzNetContext: DbContext {
        public dbQuartzNetContext() {

        }

        public dbQuartzNetContext(DbContextOptions<dbQuartzNetContext> options)
            : base(options) {
        }

        public virtual DbSet<TbJobList> TbJobList { get; set; } = null!;
        public virtual DbSet<TbSample> TbSample { get; set; } = null!;
        public virtual DbSet<TbSampleSync> TbSampleSync { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<TbJobList>(entity => {
                entity.ToTable("TbJobList");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.JobName).HasMaxLength(50).IsRequired();
                entity.Property(e => e.JobGroup).HasMaxLength(20).IsRequired();
                entity.Property(e => e.JobTypeName).HasMaxLength(50).IsRequired();
                entity.Property(e => e.JobDesc).HasMaxLength(200).IsRequired();
                entity.Property(e => e.JobData).HasMaxLength(100);
                entity.Property(e => e.ScheduleExpression).HasMaxLength(20).IsRequired();
                entity.Property(e => e.ScheduleExpressionDesc).HasMaxLength(200).IsRequired();
                entity.Property(e => e.JobStatus).HasMaxLength(10).IsRequired();
                entity.Property(e => e.CrAgent).HasMaxLength(25);
                entity.Property(e => e.CrDateTime)
                .HasConversion(
                    src => src.HasValue && src.Value.Kind == DateTimeKind.Utc ? src : DateTime.SpecifyKind(src.Value, DateTimeKind.Utc),
                    dst => dst.HasValue && dst.Value.Kind == DateTimeKind.Utc ? dst : DateTime.SpecifyKind(dst.Value, DateTimeKind.Utc));

            });

            modelBuilder.Entity<TbSample>(entity => {
                entity.ToTable("TbSample");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedNever();
                entity.Property(e => e.AgentId).HasMaxLength(20);
                entity.Property(e => e.AgentName).HasMaxLength(50);
                entity.Property(e => e.Gender).HasMaxLength(1);
                entity.Property(e => e.BirthDate)
                .HasConversion(
                    src => src.HasValue && src.Value.Kind == DateTimeKind.Utc ? src : DateTime.SpecifyKind(src.Value, DateTimeKind.Utc),
                    dst => dst.HasValue && dst.Value.Kind == DateTimeKind.Utc ? dst : DateTime.SpecifyKind(dst.Value, DateTimeKind.Utc));
                entity.Property(e => e.HireDate)
                .HasConversion(
                    src => src.HasValue && src.Value.Kind == DateTimeKind.Utc ? src : DateTime.SpecifyKind(src.Value, DateTimeKind.Utc),
                    dst => dst.HasValue && dst.Value.Kind == DateTimeKind.Utc ? dst : DateTime.SpecifyKind(dst.Value, DateTimeKind.Utc));
                entity.Property(e => e.ResignationDate)
                .HasConversion(
                    src => src.HasValue && src.Value.Kind == DateTimeKind.Utc ? src : DateTime.SpecifyKind(src.Value, DateTimeKind.Utc),
                    dst => dst.HasValue && dst.Value.Kind == DateTimeKind.Utc ? dst : DateTime.SpecifyKind(dst.Value, DateTimeKind.Utc));
                entity.Property(e => e.IsActive).HasMaxLength(1);
                entity.Property(e => e.CreatedAt)
                .HasConversion(
                    src => src.Kind == DateTimeKind.Utc ? src : DateTime.SpecifyKind(src, DateTimeKind.Utc),
                    dst => dst.Kind == DateTimeKind.Utc ? dst : DateTime.SpecifyKind(dst, DateTimeKind.Utc));
                entity.Property(e => e.CreatedBy).HasMaxLength(20);

                entity.Property(e => e.UpdatedAt)
                .HasConversion(
                    src => src.Kind == DateTimeKind.Utc ? src : DateTime.SpecifyKind(src, DateTimeKind.Utc),
                    dst => dst.Kind == DateTimeKind.Utc ? dst : DateTime.SpecifyKind(dst, DateTimeKind.Utc));
                entity.Property(e => e.UpdatedBy).HasMaxLength(20);

            });

            modelBuilder.Entity<TbSampleSync>(entity => {
                entity.ToTable("TbSampleSync");
                entity.Property(e => e.Id);
                entity.Property(e => e.AgentId).HasMaxLength(20);
                entity.Property(e => e.AgentName).HasMaxLength(50);
                entity.Property(e => e.Gender).HasMaxLength(1);
                entity.Property(e => e.BirthDate)
                .HasConversion(
                    src => src.HasValue && src.Value.Kind == DateTimeKind.Utc ? src : DateTime.SpecifyKind(src.Value, DateTimeKind.Utc),
                    dst => dst.HasValue && dst.Value.Kind == DateTimeKind.Utc ? dst : DateTime.SpecifyKind(dst.Value, DateTimeKind.Utc));
                entity.Property(e => e.HireDate)
                .HasConversion(
                    src => src.HasValue && src.Value.Kind == DateTimeKind.Utc ? src : DateTime.SpecifyKind(src.Value, DateTimeKind.Utc),
                    dst => dst.HasValue && dst.Value.Kind == DateTimeKind.Utc ? dst : DateTime.SpecifyKind(dst.Value, DateTimeKind.Utc));
                entity.Property(e => e.ResignationDate)
                .HasConversion(
                    src => src.HasValue && src.Value.Kind == DateTimeKind.Utc ? src : DateTime.SpecifyKind(src.Value, DateTimeKind.Utc),
                    dst => dst.HasValue && dst.Value.Kind == DateTimeKind.Utc ? dst : DateTime.SpecifyKind(dst.Value, DateTimeKind.Utc));
                entity.Property(e => e.IsActive).HasMaxLength(1);
                entity.Property(e => e.CreatedAt)
                .HasConversion(
                    src => src.Kind == DateTimeKind.Utc ? src : DateTime.SpecifyKind(src, DateTimeKind.Utc),
                    dst => dst.Kind == DateTimeKind.Utc ? dst : DateTime.SpecifyKind(dst, DateTimeKind.Utc));
                entity.Property(e => e.CreatedBy).HasMaxLength(20);

                entity.Property(e => e.UpdatedAt)
                .HasConversion(
                    src => src.Kind == DateTimeKind.Utc ? src : DateTime.SpecifyKind(src, DateTimeKind.Utc),
                    dst => dst.Kind == DateTimeKind.Utc ? dst : DateTime.SpecifyKind(dst, DateTimeKind.Utc));
                entity.Property(e => e.UpdatedBy).HasMaxLength(20);

            });


            base.OnModelCreating(modelBuilder);
        }
    }
}
