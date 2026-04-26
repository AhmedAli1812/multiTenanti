using HMS.API.Middlewares;
using HMS.Application.Abstractions.Caching;
using HMS.Application.Abstractions.CurrentUser;
using HMS.Application.Abstractions.Security;
using HMS.Application.Abstractions.Services;
using HMS.Application.Features.Auth.Login;
using HMS.Infrastructure.Caching;
using HMS.Infrastructure.DependencyInjection;
using HMS.Infrastructure.Persistence;
using HMS.Infrastructure.Persistence.Seed;
using HMS.Infrastructure.RealTime;
using HMS.Infrastructure.Security;
using HMS.Infrastructure.Services;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using QuestPDF.Infrastructure;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

QuestPDF.Settings.License = LicenseType.Community;

// =====================
// 🔥 Services
// =====================

builder.Services.AddControllers();

// =====================
// 🌐 CORS
// =====================

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Infrastructure (DB + Services)
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddScoped<INotificationService, NotificationService>();

// 🔥 MediatR
builder.Services.AddMediatR(cfg =>
cfg.RegisterServicesFromAssembly(typeof(LoginHandler).Assembly));

builder.Services.AddSignalR();
builder.Services.AddScoped<IQrCodeService, QrCodeService>();
builder.Services.AddScoped<IDashboardNotifier, DashboardNotifier>();
builder.Services.AddScoped<IRealTimeNotifier, SignalRNotifier>();
builder.Services.AddScoped<IPdfService, PdfService>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// =====================
// 🔐 JWT Authentication
// =====================

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        NameClaimType = ClaimTypes.Name,
        RoleClaimType = ClaimTypes.Role,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
    Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };
});

// =====================
// 🔐 Authorization (Permissions)
// =====================

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ViewAuditLogs", policy =>
    {
        policy.RequireClaim("permission", "audit_logs.view");
    });
});
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("DashboardReceptionView", policy =>
    {
        policy.RequireClaim("permission", "dashboard.reception.view");
    });

});

// =====================
// 🧾 Swagger
// =====================

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Elhaya Hospital API",
        Version = "v1",
        Description = "Internal Hospital Management System API"
    });

    // 🔐 JWT
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter: Bearer {token}"
    });

    // 🏢 Tenant Header (🔥 GLOBAL)
    options.AddSecurityDefinition("Tenant", new OpenApiSecurityScheme
    {
        Name = "X-Tenant-Id",
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Description = "Tenant Id for switching (Super Admin)"
    });

    // 🔥 Apply JWT + Tenant globally
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        },
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Tenant"
                }
            },
            Array.Empty<string>()
        }
    });
});

// =====================
// 🔧 DI
// =====================

builder.Services.AddScoped<AuditLogService>();
builder.Services.AddMemoryCache();
builder.Services.AddScoped<IPermissionCacheService, PermissionCacheService>();
builder.Services.AddScoped<IRefreshTokenHasher, RefreshTokenHasher>();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddScoped<IAssignmentService, AssignmentService>();
builder.Services.AddScoped<IRequestInfoProvider, RequestInfoProvider>();

// =====================
// 🚀 Build
// =====================

var app = builder.Build();

// =====================
// 🔥 Middleware
// =====================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ✅ CORS أول حاجة — قبل كل حاجة
app.UseCors("AllowFrontend");

app.MapHub<NotificationHub>("/hubs/notifications");
app.UseStaticFiles();

// 🔐 Auth
app.UseAuthentication();
app.UseAuthorization();

// 🔥 Audit Logging
app.UseMiddleware<AuditMiddleware>();
app.UseMiddleware<ExceptionMiddleware>();

app.MapControllers();

// =====================
// 💣 Seed Data
// =====================

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await PermissionSeeder.SeedAsync(context);
}

var hash = BCrypt.Net.BCrypt.HashPassword("Zxx020508@");
Console.WriteLine(hash);

app.MapHub<DashboardHub>("/hubs/dashboard");

app.Run();
