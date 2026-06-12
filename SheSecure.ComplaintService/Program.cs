using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SheSecure.ComplaintService.Data;
using SheSecure.ComplaintService.Interfaces;
using SheSecure.ComplaintService.Repositories;
using SheSecure.ComplaintService.Services;
using System.Text;

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

// ===================== AUTHENTICATION =====================
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
            )
        };

        // ===================== JWT DEBUG LOGGING =====================
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine("❌ AUTH FAILED: " + context.Exception.GetType().Name);
                Console.WriteLine("❌ MESSAGE: " + context.Exception.Message);
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine("✅ TOKEN VALID for: " + context.Principal.Identity.Name);
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                Console.WriteLine("⚠️ CHALLENGE: " + context.Error);
                Console.WriteLine("⚠️ DESCRIPTION: " + context.ErrorDescription);
                return Task.CompletedTask;
            },
            OnMessageReceived = context =>
            {
                Console.WriteLine("📨 TOKEN RECEIVED: " + (context.Token ?? "NULL"));
                return Task.CompletedTask;
            }
        };
        // ===================== END DEBUG LOGGING =====================
    });

// ===================== AUTHORIZATION =====================
builder.Services.AddAuthorization();

// ===================== SWAGGER =====================
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter: Bearer {your JWT token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
        }
    });
});

Console.WriteLine("=================================");
Console.WriteLine("JWT KEY (ComplaintService): " + builder.Configuration["Jwt:Key"]);
Console.WriteLine("ISSUER: " + builder.Configuration["Jwt:Issuer"]);
Console.WriteLine("AUDIENCE: " + builder.Configuration["Jwt:Audience"]);
Console.WriteLine("=================================");

var app = builder.Build();

// ===================== PIPELINE =====================
app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();   // MUST come before Authorization
app.UseAuthorization();

app.MapControllers();

app.Run();