using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using NibelungLog.Api.Validators;
using NibelungLog.DAL.Data;
using NibelungLog.DAL.Repositories;
using NibelungLog.Domain.Interfaces;
using NibelungLog.Domain.Interfaces.Repositories;
using NibelungLog.Service.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<GetPlayersQueryValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<GetGuildMembersQueryValidator>();

var postgresConnectionString = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING") 
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Host=localhost;Port=5432;Database=nibelunglog;Username=postgres;Password=password";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(postgresConnectionString));

builder.Services.AddScoped<IPlayerRepository, PlayerRepository>();
builder.Services.AddScoped<ICharacterSpecRepository, CharacterSpecRepository>();
builder.Services.AddScoped<IRaidTypeRepository, RaidTypeRepository>();
builder.Services.AddScoped<IRaidRepository, RaidRepository>();
builder.Services.AddScoped<IEncounterRepository, EncounterRepository>();
builder.Services.AddScoped<IPlayerEncounterRepository, PlayerEncounterRepository>();

builder.Services.AddScoped<IPlayerQueryRepository, PlayerQueryRepository>();
builder.Services.AddScoped<IRaidQueryRepository, RaidQueryRepository>();
builder.Services.AddScoped<IEncounterQueryRepository, EncounterQueryRepository>();
builder.Services.AddScoped<IRaidTypeQueryRepository, RaidTypeQueryRepository>();
builder.Services.AddScoped<IGuildQueryRepository, GuildQueryRepository>();

builder.Services.AddScoped<IPlayerQueryService, PlayerQueryService>();
builder.Services.AddScoped<IRaidQueryService, RaidQueryService>();
builder.Services.AddScoped<IEncounterQueryService, EncounterQueryService>();
builder.Services.AddScoped<IRaidTypeQueryService, RaidTypeQueryService>();
builder.Services.AddScoped<IGuildQueryService, GuildQueryService>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseHttpsRedirection();
app.MapControllers();

app.Run();
