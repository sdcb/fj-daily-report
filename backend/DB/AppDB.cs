using Microsoft.EntityFrameworkCore;

namespace FjDailyReport.DB;

public class AppDB : DbContext
{
    public AppDB(DbContextOptions<AppDB> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<DailyReport> DailyReports => Set<DailyReport>();
    public DbSet<ProjectGroup> ProjectGroups => Set<ProjectGroup>();
    public DbSet<ProjectGroupMember> ProjectGroupMembers => Set<ProjectGroupMember>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User 配置
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.DisplayName).HasMaxLength(128);
        });

        // DailyReport 配置
        modelBuilder.Entity<DailyReport>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            // 日期索引，用于快速查询某天所有成员的日报
            entity.HasIndex(e => e.Date);
            
            // 用户+日期唯一索引，每个用户每天只能有一条日报
            entity.HasIndex(e => new { e.UserId, e.Date }).IsUnique();

            entity.HasOne(e => e.User)
                .WithMany(u => u.DailyReports)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ProjectGroup 配置
        modelBuilder.Entity<ProjectGroup>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(128);
        });

        // ProjectGroupMember 配置（多对多关联表）
        modelBuilder.Entity<ProjectGroupMember>(entity =>
        {
            entity.HasKey(e => new { e.ProjectGroupId, e.UserId });

            entity.HasOne(e => e.ProjectGroup)
                .WithMany(g => g.Members)
                .HasForeignKey(e => e.ProjectGroupId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                .WithMany(u => u.ProjectGroupMembers)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
