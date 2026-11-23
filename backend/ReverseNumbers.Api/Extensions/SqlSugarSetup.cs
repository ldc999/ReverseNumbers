using System.Text.RegularExpressions;
using ReverseNumbers.Api.Models.Entities;
using SqlSugar;

namespace ReverseNumbers.Api.Extensions;

public static class SqlSugarSetup
{
    public static void AddSqlSugar(this IServiceCollection services, IConfiguration configuration, string contentRoot)
    {
        var connectionString = configuration.GetConnectionString("Default") ?? "DataSource=arnumbers.db";
        connectionString = ResolveSqlitePath(connectionString, contentRoot);

        var db = new SqlSugarScope(new ConnectionConfig
        {
            ConnectionString = connectionString,
            DbType = DbType.Sqlite,
            IsAutoCloseConnection = true,
            InitKeyType = InitKeyType.Attribute
        });

        db.Aop.OnLogExecuting = (sql, pars) =>
        {
            Console.WriteLine($"[SqlSugar] {sql}");
        };

        services.AddSingleton<ISqlSugarClient>(db);

        // SQLite 数据库文件不存在时自动创建
        db.DbMaintenance.CreateDatabase();

        // Auto-create tables on startup (DLT)
        db.CodeFirst.InitTables(
            typeof(Issue),
            typeof(PlayerNumber),
            typeof(SavedNumberHistory),
            typeof(SystemConfig),
            typeof(RetainedNumber),
            typeof(FavoriteNumber),
            typeof(Ticket)
        );

        // Auto-create tables on startup (SSQ)
        db.CodeFirst.InitTables(
            typeof(SsqIssue),
            typeof(SsqPlayerNumber),
            typeof(SsqSavedNumberHistory),
            typeof(SsqSystemConfig),
            typeof(SsqRetainedNumber),
            typeof(SsqFavoriteNumber),
            typeof(SsqTicket),
            typeof(LotteryDraw)
        );

        // Seed system_config data
        SeedConfig(db);
        SeedSsqConfig(db);
    }

    /// <summary>
    /// 将连接串中的相对数据库路径解析为绝对路径（相对于应用内容根目录），
    /// 并确保所在目录存在。
    /// </summary>
    private static string ResolveSqlitePath(string connectionString, string contentRoot)
    {
        var match = Regex.Match(connectionString, @"Data\s*Source\s*=\s*([^;]+)", RegexOptions.IgnoreCase);
        if (!match.Success) return connectionString;

        var path = match.Groups[1].Value.Trim().Trim('"');
        if (!Path.IsPathRooted(path))
            path = Path.Combine(contentRoot, path);

        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

        return $"DataSource={path}";
    }

    private static void SeedSsqConfig(ISqlSugarClient db)
    {
        var configs = new[]
        {
            new SsqSystemConfig { ConfigKey = "max_front_highlight", ConfigValue = "3", Description = "红球最大高亮数", UpdatedAt = DateTime.Now },
            new SsqSystemConfig { ConfigKey = "max_back_highlight", ConfigValue = "1", Description = "蓝球最大高亮数", UpdatedAt = DateTime.Now },
            new SsqSystemConfig { ConfigKey = "max_generation_iterations", ConfigValue = "10000", Description = "随机生成最大迭代次数", UpdatedAt = DateTime.Now }
        };
        foreach (var config in configs)
        {
            if (!db.Queryable<SsqSystemConfig>().Where(c => c.ConfigKey == config.ConfigKey).Any())
                db.Insertable(config).ExecuteCommand();
        }
    }

    private static void SeedConfig(ISqlSugarClient db)
    {
        var configs = new[]
        {
            new SystemConfig
            {
                ConfigKey = "max_front_highlight",
                ConfigValue = "3",
                Description = "前区最大高亮数",
                UpdatedAt = DateTime.Now
            },
            new SystemConfig
            {
                ConfigKey = "max_back_highlight",
                ConfigValue = "1",
                Description = "后区最大高亮数",
                UpdatedAt = DateTime.Now
            },
            new SystemConfig
            {
                ConfigKey = "max_generation_iterations",
                ConfigValue = "10000",
                Description = "随机生成最大迭代次数",
                UpdatedAt = DateTime.Now
            }
        };

        foreach (var config in configs)
        {
            var exists = db.Queryable<SystemConfig>()
                .Where(c => c.ConfigKey == config.ConfigKey)
                .Any();
            if (!exists)
            {
                db.Insertable(config).ExecuteCommand();
            }
        }
    }
}
