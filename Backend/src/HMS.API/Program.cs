// ─────────────────────────────────────────────────────────────────────────────
// HMS.API — Program.cs  (Modular Monolith entry point)
// ─────────────────────────────────────────────────────────────────────────────

// ── Legacy namespace imports (kept during transition) ─────────────────────────
using HMS.API.Middleware;
using HMS.API.Middlewares;                                  // AuditMiddleware, ExceptionMiddleware
using HMS.Application.Abstractions.Caching;                // IPermissionCacheService
using HMS.Application.Abstractions.CurrentUser;            // ICurrentUser, ICurrentUserService
using HMS.Application.Abstractions.Security;               // IRefreshTokenHasher
using HMS.Application.Abstractions.Services;               // IAssignmentService, INotificationService
using HMS.Application.Features.Auth.Login;                 // LoginHandler (legacy MediatR scan root)
using HMS.Application.Features.PatientIntake.Services;     // PatientIntakeService
using HMS.Infrastructure.Caching;                          // PermissionCacheService
using HMS.Infrastructure.CurrentUser;                      // CurrentUser, CurrentUserService
using HMS.Infrastructure.DependencyInjection;              // AddInfrastructure()
using HMS.Infrastructure.Persistence;                      // ApplicationDbContext
using HMS.Infrastructure.Persistence.Seed;                 // PermissionSeeder
using HMS.Infrastructure.RealTime;                         // NotificationHub, DashboardHub, DashboardNotifier
using HMS.Infrastructure.Security;                         // RefreshTokenHasher
using HMS.Infrastructure.Services;                         // AuditLogService, AssignmentService, etc.
using HMS.Notifications.Infrastructure;                    // AddNotificationsModule()
using HMS.Persistence;                                     // AddHmsPersistence()
using HMS.SharedKernel.Application.Behaviors;              // LoggingBehavior, ValidationBehavior

// ── Framework imports ─────────────────────────────────────────────────────────
using FluentValidation;

using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using QuestPDF.Infrastructure;
using Serilog;
using System.Security.Claims;
using System.Text;

// ── Serilog bootstrap ─────────────────────────────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console(outputTemplate:
        "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File("logs/hms-.txt",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 14)
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();

    QuestPDF.Settings.License = LicenseType.Community;

    // ── Controllers ───────────────────────────────────────────────────────────
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddMemoryCache();

    // ── CORS ──────────────────────────────────────────────────────────────────
    builder.Services.AddCors(options =>
        options.AddPolicy("AllowFrontend", policy =>
            policy.WithOrigins(
                    "http://localhost:5173",  "https://localhost:5173",
                    "http://localhost:5174",  "https://localhost:5174")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials()));

    // ── Legacy Infrastructure (kept during transition) ────────────────────────
    builder.Services.AddInfrastructure(builder.Configuration);

    // ── MediatR — scans ALL assemblies (legacy + new modules) ─────────────────
    builder.Services.AddMediatR(cfg =>
    {
        // Legacy
        cfg.RegisterServicesFromAssembly(typeof(LoginHandler).Assembly);

        // SharedKernel behaviors
        cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        // New module application assemblies
        cfg.RegisterServicesFromAssembly(
            typeof(HMS.Identity.Application.Features.Auth.Login.LoginCommandHandler).Assembly);
        cfg.RegisterServicesFromAssembly(
            typeof(HMS.Visits.Application.Features.Visits.EventHandlers.IntakeSubmittedEventHandler).Assembly);
        cfg.RegisterServicesFromAssembly(
            typeof(HMS.Notifications.Application.EventHandlers.VisitCreatedNotificationHandler).Assembly);
    });

    // FluentValidation — scan new module assemblies
    builder.Services.AddValidatorsFromAssembly(
        typeof(HMS.Identity.Application.Features.Auth.Login.LoginCommandHandler).Assembly);

    // ── SignalR ───────────────────────────────────────────────────────────────
    builder.Services.AddSignalR();

    // ── New Modular Persistence (HmsDbContext + all module IDbContext mappings) ─
    builder.Services.AddHmsPersistence(builder.Configuration);

    // ── Notifications module ───────────────────────────────────────────────────
    builder.Services.AddNotificationsModule();

    // ── Legacy services ────────────────────────────────────────────────────────
    builder.Services.AddScoped<INotificationService, NotificationService>();
    builder.Services.AddScoped<IQrCodeService, QrCodeService>();
    builder.Services.AddScoped<IDashboardNotifier, DashboardNotifier>();
    builder.Services.AddScoped<IRealTimeNotifier, SignalRNotifier>();
    builder.Services.AddScoped<IPdfService, PdfService>();
    builder.Services.AddScoped<AuditLogService>();
    builder.Services.AddScoped<IPermissionCacheService, PermissionCacheService>();
    builder.Services.AddScoped<IRefreshTokenHasher, RefreshTokenHasher>();
    builder.Services.AddScoped<ICurrentUser, CurrentUser>();
    builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
    builder.Services.AddScoped<IAssignmentService, AssignmentService>();
    builder.Services.AddScoped<IRequestInfoProvider, RequestInfoProvider>();

    // ── New module: Identity infra services ───────────────────────────────────
    builder.Services.AddScoped<HMS.Identity.Application.Abstractions.IPasswordHasher,
                               HMS.Identity.Infrastructure.Authentication.PasswordHasher>();
    builder.Services.AddScoped<HMS.Identity.Application.Abstractions.IJwtService,
                               HMS.Identity.Infrastructure.Authentication.JwtService>();

    // ── JWT Authentication ─────────────────────────────────────────────────────
    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
                ValidateIssuer   = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                NameClaimType    = ClaimTypes.Name,
                RoleClaimType    = ClaimTypes.Role,
            };

            // Support JWT in SignalR query-string
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = ctx =>
                {
                    var token = ctx.Request.Query["access_token"];
                    if (!string.IsNullOrEmpty(token) &&
                        ctx.HttpContext.Request.Path.StartsWithSegments("/hubs"))
                        ctx.Token = token;
                    return Task.CompletedTask;
                }
            };
        });

    // ── Authorization policies ─────────────────────────────────────────────────
    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("ViewAuditLogs",
            p => p.RequireClaim("permission", "audit_logs.view"));

        options.AddPolicy("DashboardReceptionView",
            p => p.RequireClaim("permission", "dashboard.reception.view"));

        options.AddPolicy("ManagePatients",
            p => p.RequireClaim("permission", "patients.manage"));

        options.AddPolicy("ManageRooms",
            p => p.RequireClaim("permission", "rooms.manage"));
    });

    // ── Swagger ────────────────────────────────────────────────────────────────
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title       = "HMS — Hospital Management System API",
            Version     = "v1",
            Description = "Modular Monolith — Clean Architecture + DDD"
        });

        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name          = "Authorization",
            Type          = SecuritySchemeType.Http,
            Scheme        = "bearer",
            BearerFormat  = "JWT",
            In            = ParameterLocation.Header,
            Description   = "Enter: Bearer {token}"
        });

        options.AddSecurityDefinition("Tenant", new OpenApiSecurityScheme
        {
            Name        = "X-Tenant-Id",
            Type        = SecuritySchemeType.ApiKey,
            In          = ParameterLocation.Header,
            Description = "Tenant Id header (Super Admin override)"
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                        { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                },
                []
            },
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                        { Type = ReferenceType.SecurityScheme, Id = "Tenant" }
                },
                []
            }
        });
    });

    // ── Health checks ──────────────────────────────────────────────────────────
    builder.Services.AddHealthChecks()
        .AddDbContextCheck<ApplicationDbContext>(
            name: "database",
            tags: ["db", "sql"]);

    // ─────────────────────────────────────────────────────────────────────────
    var app = builder.Build();
    // ─────────────────────────────────────────────────────────────────────────

    // ── Global exception handler (must be FIRST in pipeline) ─────────────────
    app.UseMiddleware<GlobalExceptionMiddleware>();

    // ── Swagger ────────────────────────────────────────────────────────────────
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "HMS API v1");
            c.RoutePrefix = "swagger";
        });
    }

    // ── CORS (must be before Auth) ─────────────────────────────────────────────
    app.UseCors("AllowFrontend");

    app.UseSerilogRequestLogging();
    app.UseStaticFiles();

    // ── Auth ──────────────────────────────────────────────────────────────────
    app.UseAuthentication();
    app.UseAuthorization();

    // ── Legacy middleware ──────────────────────────────────────────────────────
    app.UseMiddleware<AuditMiddleware>();

    // ── Endpoints ─────────────────────────────────────────────────────────────
    app.MapControllers();
    app.MapHub<NotificationHub>("/hubs/notifications");
    app.MapHub<DashboardHub>("/hubs/dashboard");
    app.MapHealthChecks("/health");

    // ── Database seed ──────────────────────────────────────────────────────────
    using (var scope = app.Services.CreateScope())
    {
        var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        // 🔥 DATABASE CLEANUP & REPAIR (NUKE MODULAR SCHEMAS)
        var db = ctx.Database;
        try 
        {
            Log.Information("Cleaning up modular schemas and repairing legacy tables...");
            
            // 1. Nuke modular schemas and their tables
            await db.ExecuteSqlRawAsync(@"
                IF EXISTS (SELECT * FROM sys.schemas WHERE name = 'visits') BEGIN DROP TABLE IF EXISTS visits.Visits; DROP SCHEMA visits; END
                IF EXISTS (SELECT * FROM sys.schemas WHERE name = 'patients') BEGIN DROP TABLE IF EXISTS patients.Patients; DROP SCHEMA patients; END
                IF EXISTS (SELECT * FROM sys.schemas WHERE name = 'rooms') BEGIN DROP TABLE IF EXISTS rooms.RoomAssignments; DROP TABLE IF EXISTS rooms.Rooms; DROP SCHEMA rooms; END
                IF EXISTS (SELECT * FROM sys.schemas WHERE name = 'intake') BEGIN 
                    DROP TABLE IF EXISTS intake.IntakeFlags; 
                    DROP TABLE IF EXISTS intake.IntakeInsurance; 
                    DROP TABLE IF EXISTS intake.IntakeEmergencyContacts; 
                    DROP TABLE IF EXISTS intake.Intakes; 
                    DROP SCHEMA intake; 
                END
                IF EXISTS (SELECT * FROM sys.schemas WHERE name = 'identity') BEGIN 
                    DROP TABLE IF EXISTS identity.UserSessions; 
                    DROP TABLE IF EXISTS identity.RefreshTokens; 
                    DROP TABLE IF EXISTS identity.RolePermissions; 
                    DROP TABLE IF EXISTS identity.UserRoles; 
                    DROP TABLE IF EXISTS identity.Permissions; 
                    DROP TABLE IF EXISTS identity.Roles; 
                    DROP TABLE IF EXISTS identity.Users; 
                    DROP SCHEMA identity; 
                END
            ");

            // 2. Repair legacy tables (Add missing columns to dbo schema)
            await db.ExecuteSqlRawAsync(@"
                -- Visits table repairs
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Visits]') AND name = 'ChiefComplaint') ALTER TABLE [dbo].[Visits] ADD [ChiefComplaint] nvarchar(1000) NULL;
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Visits]') AND name = 'Notes') ALTER TABLE [dbo].[Visits] ADD [Notes] nvarchar(max) NULL;
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Visits]') AND name = 'Priority') ALTER TABLE [dbo].[Visits] ADD [Priority] int NOT NULL DEFAULT 1;
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Visits]') AND name = 'ArrivalMethod') ALTER TABLE [dbo].[Visits] ADD [ArrivalMethod] int NOT NULL DEFAULT 1;
                
                -- Intakes table repairs
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Intakes]') AND name = 'ChiefComplaint') ALTER TABLE [dbo].[Intakes] ADD [ChiefComplaint] nvarchar(1000) NULL;
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Intakes]') AND name = 'Notes') ALTER TABLE [dbo].[Intakes] ADD [Notes] nvarchar(max) NULL;
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Intakes]') AND name = 'VisitType') ALTER TABLE [dbo].[Intakes] ADD [VisitType] int NOT NULL DEFAULT 1;
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Intakes]') AND name = 'Priority') ALTER TABLE [dbo].[Intakes] ADD [Priority] int NOT NULL DEFAULT 1;
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Intakes]') AND name = 'ArrivalMethod') ALTER TABLE [dbo].[Intakes] ADD [ArrivalMethod] int NOT NULL DEFAULT 1;
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Intakes]') AND name = 'PaymentType') ALTER TABLE [dbo].[Intakes] ADD [PaymentType] int NOT NULL DEFAULT 1;
            ");

            Log.Information("Database cleanup and repair completed successfully.");
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Database cleanup/repair encountered an issue. This might be fine if the database is already clean.");
        }

        await PermissionSeeder.SeedAsync(ctx);
        await TenantSeeder.SeedAsync(ctx, CancellationToken.None);
    }

    Log.Information("HMS API starting on {Environment}", app.Environment.EnvironmentName);
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "HMS API failed to start");
}
finally
{
    Log.CloseAndFlush();
}
