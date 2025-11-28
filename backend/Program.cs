using FjDailyReport.DB;
using FjDailyReport.Infrastructure;
using FjDailyReport.Models;
using FjDailyReport.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add database context
builder.Services.AddDbContext<AppDB>(options =>
{
    options.Configure(builder.Configuration, builder.Environment);
});

// Add HTTP client
builder.Services.AddHttpClient();

// Add HTTP context accessor
builder.Services.AddHttpContextAccessor();

// Add custom services
builder.Services.AddSingleton<JwtService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddSingleton<HostUrlService>();
builder.Services.AddScoped<KeycloakService>();
builder.Services.Configure<KeycloakConfig>(builder.Configuration.GetSection("Keycloak"));

// Configure JWT authentication
var jwtSecretKey = builder.Configuration["JwtSecretKey"] ?? throw new InvalidOperationException("JwtSecretKey is required");
var key = Encoding.UTF8.GetBytes(jwtSecretKey);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = "sso-template",
            ValidateAudience = true,
            ValidAudience = "sso-template",
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// Configure CORS
var feUrl = builder.Configuration["FE_URL"] ?? throw new InvalidOperationException("FE_URL is required");
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(feUrl)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var app = builder.Build();

// 自动创建/迁移数据库
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDB>();
    db.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// 使用前端中间件（在路由之后，这样当路由没有匹配时才会处理前端页面）
app.UseMiddleware<FrontendMiddleware>();
app.UseStaticFiles();

app.Run();