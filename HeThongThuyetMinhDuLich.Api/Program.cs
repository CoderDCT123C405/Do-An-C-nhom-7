using HeThongThuyetMinhDuLich.Api.Data;
using HeThongThuyetMinhDuLich.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json.Serialization;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<JwtTokenService>();

var dbProvider = builder.Configuration["Database:Provider"] ?? "SqlServer";
builder.Services.AddDbContext<DuLichDbContext>(options =>
{
    if (string.Equals(dbProvider, "Sqlite", StringComparison.OrdinalIgnoreCase))
    {
        var sqliteConn = builder.Configuration.GetConnectionString("SqliteConnection")
            ?? "Data Source=HeThongThuyetMinhDuLich.offline.db";
        options.UseSqlite(sqliteConn);
    }
    else
    {
        var sqlServerConn = builder.Configuration.GetConnectionString("SqlServerConnection")
            ?? builder.Configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Missing SQL Server connection string.");
        options.UseSqlServer(sqlServerConn);
    }
});

var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSection["Key"] ?? throw new InvalidOperationException("Missing Jwt:Key");
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidAudience = jwtSection["Audience"],
            IssuerSigningKey = signingKey
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

await AdminSeedService.EnsureAdminAsync(app.Services, builder.Configuration, dbProvider);
app.UseSwagger();
app.UseSwaggerUI();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseStaticFiles();
app.MapControllers();

app.Run();
