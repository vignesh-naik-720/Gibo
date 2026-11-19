using FluentValidation;
using Gibo.Api.Middleware;
using Gibo.Application.Common.Behaviors;
using Gibo.Application.Common.Interfaces;
using Gibo.Application.Features.Onboarding.Commands.RegisterOnboardingUser;
using Gibo.Application.Features.Onboarding.Services;
using Gibo.Application.Features.Planning.Services;
using Gibo.Infrastructure.Logging;
using Gibo.Infrastructure.Persistence;
using Gibo.Infrastructure.Persistence.Repositories;
using Gibo.Infrastructure.Security;
using MediatR;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Register Persistence & Planning Services
builder.Services.AddSingleton<IPlanRepository, InMemoryPlanRepository>();
builder.Services.AddScoped<ITaskRankingEngine, TaskRankingEngine>();

// Add Controllers and JSON options
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Gibo API",
        Version = "v1",
        Description = "Adaptive Healthy Habit Companion for Abu Dhabi - Onboarding & Governance API"
    });
});

// Configure MediatR with pipeline behaviors
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(RegisterOnboardingUserCommand).Assembly);
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(IdempotencyBehavior<,>));
});

// Register FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(RegisterOnboardingUserValidator).Assembly);

// Register Core Services
builder.Services.AddSingleton<IFirstPlanCandidateSelector, FirstPlanCandidateSelector>();
builder.Services.AddSingleton<IEncryptionProvider, AesEncryptionProvider>();
builder.Services.AddSingleton<IAuditEventLogger, AuditEventLogger>();

// Allow local mobile emulator connectivity
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowMobileClient", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure Middleware Pipeline
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseMiddleware<RateLimitingMiddleware>();

// Enable Swagger documentation UI
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Gibo API v1");
    c.RoutePrefix = "swagger";
});

app.UseCors("AllowMobileClient");
app.MapControllers();

app.Run();
