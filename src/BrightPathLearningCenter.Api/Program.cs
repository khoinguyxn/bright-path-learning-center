using BrightPathLearningCenter.Api.Contracts;
using BrightPathLearningCenter.Api.Endpoints;
using BrightPathLearningCenter.Api.Infrastructure;
using BrightPathLearningCenter.Api.Infrastructure.Persistence;
using BrightPathLearningCenter.Api.Infrastructure.Seed;
using BrightPathLearningCenter.Api.Validators;

using FluentValidation;

using Microsoft.EntityFrameworkCore;

using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

string connectionString = builder.Configuration.GetConnectionString("Scheduling")
                          ?? "Data Source=brightpath.db";
builder.Services.AddInfrastructure(connectionString);
builder.Services.AddScoped<IValidator<CreateLessonRequest>, CreateLessonRequestValidator>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();

    await using AsyncServiceScope scope = app.Services.CreateAsyncScope();
    SchedulingDbContext dbContext = scope.ServiceProvider.GetRequiredService<SchedulingDbContext>();
    await dbContext.Database.MigrateAsync();

    LessonSeeder seeder = scope.ServiceProvider.GetRequiredService<LessonSeeder>();
    await seeder.SeedAsync();
}

app.UseHttpsRedirection();

app.MapLessonEndpoints();

app.Run();
