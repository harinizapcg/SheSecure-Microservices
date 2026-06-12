using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using SheSecure.ComplaintService.Data;
using SheSecure.ComplaintService.Interfaces;
using SheSecure.ComplaintService.Repositories;
using SheSecure.ComplaintService.Services;

var builder = WebApplication.CreateBuilder(args);

// ===================== CONTROLLERS =====================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ===================== DB CONTEXT =====================
builder.Services.AddDbContext<ComplaintDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

// ===================== HTTP CLIENT =====================
builder.Services.AddHttpClient("NotificationService", c =>
    c.BaseAddress = new Uri(
        builder.Configuration["ServiceUrls:NotificationService"]
            ?? "https://localhost:7179/"
    ))
    .ConfigurePrimaryHttpMessageHandler(() =>
        new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        }
    );

// ===================== REPOSITORIES =====================
builder.Services.AddScoped<IComplaintRepository, ComplaintRepository>();
builder.Services.AddScoped<IComplaintFileRepository, ComplaintFileRepository>();
builder.Services.AddScoped<IComplaintCommentRepository, ComplaintCommentRepository>();
builder.Services.AddScoped<IComplaintStatusHistoryRepository, ComplaintStatusHistoryRepository>();

// ===================== SERVICES =====================
builder.Services.AddScoped<IComplaintService, ComplaintService>();
builder.Services.AddScoped<IComplaintFileService, ComplaintFileService>();
builder.Services.AddScoped<IComplaintCommentService, ComplaintCommentService>();
builder.Services.AddScoped<IComplaintStatusHistoryService, ComplaintStatusHistoryService>();

// ===================== AUTHORIZATION =====================
// JWT is intentionally removed for now.
// Role and userId are passed as query parameters.
// Re-enable JWT authentication here when ready.
builder.Services.AddAuthorization();

// ===================== SWAGGER =====================
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SheSecure Complaint Service",
        Version = "v1"
    });
});

var app = builder.Build();

// ===================== PIPELINE =====================
app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();

app.MapControllers();

app.Run();