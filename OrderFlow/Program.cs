using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Application.Services;
using OrderFlow.Infrastructure.Data;
using OrderFlow.Middleware;

var builder = WebApplication.CreateBuilder(args);

// ----- Dependency Injection -----

// Controllers; enums are sent/received as text ("Confirmed") instead of numbers.
builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// EF Core with SQL Server. The DbContext is registered as Scoped (one instance per HTTP request).
builder.Services.AddDbContext<OrderFlowDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("OrderFlowDb")));

// Application services (Scoped, because they depend on the scoped DbContext).
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IReportService, ReportService>();

// Global error handling -> ProblemDetails responses.
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Swagger / OpenAPI.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ----- HTTP pipeline -----

app.UseExceptionHandler();

app.UseSwagger();
app.UseSwaggerUI();   // available at /swagger

app.MapControllers();

app.Run();
