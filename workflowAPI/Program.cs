using Microsoft.EntityFrameworkCore;
using Serilog;
using workflowAPI.Data;
using workflowAPI.Data.Repositories;
using workflowAPI.Data.UnitOfWork;
using workflowAPI.Extensions;
using workflowAPI.Middleware;
using workflowAPI.Services;
using workflowAPI.Services.Callback;
using workflowAPI.Services.Callback.ActionHandler;
using workflowAPI.Services.Callback.ConditionHandler;
using workflowAPI.Services.Callback.WorkflowHandler;
using WorkflowApi.Services.WorkflowHandlers;

var builder = WebApplication.CreateBuilder(args);


// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/workflow-api-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();


// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Workflow API",
        Version = "v1",
        Description = "API for integrating with OptimaJet Workflow Server"
    });
});


// Add Database Context
builder.Services.AddDbContext<WorkflowDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
            sqlOptions.CommandTimeout(60);
        });

    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// Register Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ILeaveRepository, LeaveRepository>();
builder.Services.AddScoped<ILeaveBalanceRepository, LeaveBalanceRepository>();

// Register Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Register Database Seeder
builder.Services.AddScoped<DbSeeder>();


// Add Workflow services
builder.Services.AddWorkflowServices(builder.Configuration);

// Add Callback services - SOLID implementation
builder.Services.AddScoped<ICallbackHandler, CallbackHandler>();
builder.Services.AddScoped<INotificationService, NotificationService>();

// Add Identity service (now using database)
builder.Services.AddScoped<IIdentityService, IdentityService>();

// Add Leave services
builder.Services.AddScoped<ILeaveService, LeaveService>();
builder.Services.AddScoped<ILeaveBalanceService, LeaveBalanceService>();
builder.Services.AddScoped<ILeaveTypeService, LeaveTypeService>();

// Register all workflow handlers (Strategy Pattern)
builder.Services.AddScoped<IWorkflowHandler, LeaveApprovalHandler>();
builder.Services.AddScoped<IWorkflowHandler, PurchaseOrderHandler>();

// Register handler factory (Factory Pattern)
builder.Services.AddScoped<IWorkflowHandlerFactory, WorkflowHandlerFactory>();

// Register all action handlers (Strategy Pattern)
builder.Services.AddScoped<IActionHandler, LeaveApprovalActionHandler>();

// Register action handler factory (Factory Pattern)
builder.Services.AddScoped<IActionHandlerFactory, ActionHandlerFactory>();

// Register all condition handlers (Strategy Pattern)

builder.Services.AddScoped<IConditionHandler, LeaveApprovalConditionHandler>();

// Register condition handler factory (Factory Pattern)

builder.Services.AddScoped<IConditionHandlerFactory, ConditionHandlerFactory>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();


// Configure middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseCors("AllowAll");
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseMiddleware<LoggingMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();


// Initialize Database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<WorkflowDbContext>();

        Log.Information("Applying database migrations...");

        if (app.Environment.IsDevelopment())
        {
            // Auto-migrate in development
            await context.Database.MigrateAsync();
            Log.Information("Database migrations applied successfully");
        }
        else
        {
            // In production, check if database exists and migrations are needed
            var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
            if (pendingMigrations.Any())
            {
                Log.Warning("Pending migrations detected in production. Please run migrations manually.");
            }
        }

        // Seed initial data
        var seeder = services.GetRequiredService<DbSeeder>();
        await seeder.SeedAsync();

        Log.Information("Database initialization completed successfully");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "An error occurred while initializing the database");
        throw;
    }
}


Log.Information("Workflow API starting...");
app.Run();
Log.Information("Workflow API stopped");