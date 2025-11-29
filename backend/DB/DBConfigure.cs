using Microsoft.EntityFrameworkCore;

namespace FjDailyReport.DB;

public static class DBConfigure
{
    public static void Configure(this DbContextOptionsBuilder dbContextOptionsBuilder, IConfiguration configuration, IHostEnvironment environment)
    {
        string? dbType = configuration["DBType"];
        string? connectionString = configuration.GetConnectionString("AppDB") ?? throw new Exception("ConnectionStrings:AppDB not found");

        if (dbType == null || dbType.Equals("sqlite", StringComparison.OrdinalIgnoreCase))
        {
            // 自动创建 AppData 文件夹
            if (connectionString == "Data Source=./AppData/app.db" && !Directory.Exists("AppData"))
            {
                Console.WriteLine("Creating AppData folder...");
                Directory.CreateDirectory("AppData");
            }
            dbContextOptionsBuilder.UseSqlite(connectionString);
        }
        else if (dbType.Equals("sqlserver", StringComparison.OrdinalIgnoreCase) || dbType.Equals("mssql", StringComparison.OrdinalIgnoreCase))
        {
            dbContextOptionsBuilder.UseSqlServer(connectionString);
        }
        else
        {
            throw new Exception("Unknown DBType: " + dbType);
        }

        if (environment.IsDevelopment())
        {
            dbContextOptionsBuilder.EnableSensitiveDataLogging();
        }
    }
}
