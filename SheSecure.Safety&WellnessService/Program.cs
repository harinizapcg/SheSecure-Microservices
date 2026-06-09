using Microsoft.EntityFrameworkCore;
using SheSecure.Safety_WellnessService.Interfaces;
using SheSecure.Safety_WellnessService.Repositories;
using SheSecure.Safety_WellnessService.Services;
using SheSecure.WellnessSafetyService.Interfaces;
using SheSecure.WellnessSafetyService.Services;
using SheSecure.Safety_WellnessService.Data;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<WellnessDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<
    IWellnessRequestRepository,
    WellnessRequestRepository>();

builder.Services.AddScoped<
    ISafeReachRepository,
    SafeReachRepository>();

builder.Services.AddScoped<
    ISafeReachService,
    SafeReachService>();
builder.Services.AddScoped<
    IWellnessRequestService,
    WellnessRequestService>();

builder.Services.AddScoped<
    IEmergencyAlertRepository,
    EmergencyAlertRepository>();

builder.Services.AddScoped<
    IEmergencyAlertService,
    EmergencyAlertService>();
builder.Services.AddScoped<IMoodLogRepository, MoodLogRepository>();
builder.Services.AddScoped<IMoodLogService, MoodLogService>();
var app = builder.Build();

app.UseSwagger();

app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();