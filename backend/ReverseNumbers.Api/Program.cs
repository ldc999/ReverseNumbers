using ReverseNumbers.Api.Extensions;
using ReverseNumbers.Api.Services;
using ReverseNumbers.Api.Services.Ssq;
using ReverseNumbers.Api.Common;

var builder = WebApplication.CreateBuilder(args);

// Add controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// SqlSugar
builder.Services.AddSqlSugar(builder.Configuration, builder.Environment.ContentRootPath);

// Register services
builder.Services.AddScoped<IIssueService, IssueService>();
builder.Services.AddScoped<IPlayerNumberService, PlayerNumberService>();
builder.Services.AddScoped<IGenerateService, GenerateService>();
builder.Services.AddScoped<IHistoryService, HistoryService>();
builder.Services.AddScoped<IConfigService, ConfigService>();
builder.Services.AddScoped<IRetainedNumberService, RetainedNumberService>();
builder.Services.AddScoped<IFavoriteNumberService, FavoriteNumberService>();

// SSQ Services
builder.Services.AddScoped<ISsqIssueService, SsqIssueService>();
builder.Services.AddScoped<ISsqPlayerNumberService, SsqPlayerNumberService>();
builder.Services.AddScoped<ISsqGenerateService, SsqGenerateService>();
builder.Services.AddScoped<ISsqHistoryService, SsqHistoryService>();
builder.Services.AddScoped<ISsqConfigService, SsqConfigService>();
builder.Services.AddScoped<ISsqRetainedNumberService, SsqRetainedNumberService>();
builder.Services.AddScoped<ISsqFavoriteNumberService, SsqFavoriteNumberService>();
builder.Services.AddScoped<ITicketService, TicketService>();
builder.Services.AddScoped<ISsqTicketService, SsqTicketService>();
builder.Services.AddScoped<ILotteryTrendService, LotteryTrendService>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Global exception middleware
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (AppException ex)
    {
        context.Response.StatusCode = 200;
        context.Response.ContentType = "application/json";
        var result = ApiResult.Fail(ex.Message, ex.Code);
        await context.Response.WriteAsJsonAsync(result);
    }
    catch (Exception ex)
    {
        context.Response.StatusCode = 200;
        context.Response.ContentType = "application/json";
        var result = ApiResult.Fail($"服务器内部错误: {ex.Message}", 500);
        await context.Response.WriteAsJsonAsync(result);
    }
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");
app.MapControllers();

app.Run();
