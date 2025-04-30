using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using System.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors();

var connectionString = "Host=localhost;Port=5432;Username=ВАШ_ПОЛЬЗОВАТЕЛЬ_БД;Password=ВАШ_ПАРОЛЬ;Database=ИМЯ_ВАШЕЙ_БД";

// Endpoint: /measurements?startDateTime=2025-04-20T10:00:00&endDateTime=2025-04-27T18:30:00&tank=TANK001
//example request: YOUR_IP:PORT/measurements/?startDateTime=2025-03-20T10:00:00&endDateTime=2025-05-30T18:30:00&tank=TANK001
app.MapGet("/measurements", async (HttpContext context) =>
{
    var startDateTime = context.Request.Query["startDateTime"].ToString();
    var endDateTime = context.Request.Query["endDateTime"].ToString();
    var tank = context.Request.Query["tank"].ToString();

    if (string.IsNullOrEmpty(startDateTime) || string.IsNullOrEmpty(endDateTime) || string.IsNullOrEmpty(tank))
    {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsync("Missing parameters");
        return;
    }

    var measurements = new List<object>();

    await using var conn = new NpgsqlConnection(connectionString);
    await conn.OpenAsync();

    var query = @"
        SELECT measurement_time, density_kg_m3
        FROM oil_measurements
        WHERE measurement_time BETWEEN @startDateTime AND @endDateTime
          AND tank_number = @tank
        ORDER BY measurement_time
    ";

    await using var cmd = new NpgsqlCommand(query, conn);
    cmd.Parameters.AddWithValue("startDateTime", DateTime.Parse(startDateTime));
    cmd.Parameters.AddWithValue("endDateTime", DateTime.Parse(endDateTime));
    cmd.Parameters.AddWithValue("tank", tank);

    await using var reader = await cmd.ExecuteReaderAsync();
    while (await reader.ReadAsync())
    {
        measurements.Add(new
        {
            time = reader.GetDateTime(0),
            density = reader.GetDouble(1)
        });
    }

    await context.Response.WriteAsJsonAsync(measurements);
});

app.Run("http://0.0.0.0:5000");
