using Serilog;
using workflowAPI.Extensions;
using workflowAPI.Middleware;
using workflowAPI.Services;
using workflowAPI.Services.Callback;
using workflowAPI.Services.Callback.ActionHandler;
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


// Add Workflow services
builder.Services.AddWorkflowServices(builder.Configuration);

// Add Callback services - SOLID implementation
builder.Services.AddScoped<ICallbackHandler, CallbackHandler>();
builder.Services.AddScoped<INotificationService, NotificationService>();

// Add Identity service for dummy user/role data
builder.Services.AddScoped<IIdentityService, IdentityService>();

// Register all workflow handlers (Strategy Pattern)
builder.Services.AddScoped<IWorkflowHandler, LeaveApprovalHandler>();
builder.Services.AddScoped<IWorkflowHandler, PurchaseOrderHandler>();

// Register handler factory (Factory Pattern)
builder.Services.AddScoped<IWorkflowHandlerFactory, WorkflowHandlerFactory>();

// Register all action handlers (Strategy Pattern)
builder.Services.AddScoped<IActionHandler, LeaveApprovalActionHandler>();

// Register action handler factory (Factory Pattern)
builder.Services.AddScoped<IActionHandlerFactory, ActionHandlerFactory>();


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

Log.Information("Workflow API starting...");
app.Run();
Log.Information("Workflow API stopped");