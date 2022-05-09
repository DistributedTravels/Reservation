using Microsoft.EntityFrameworkCore;
using Reservation.Database;
using MassTransit;
using Reservation.Orchestration;

var builder = WebApplication.CreateBuilder(args);

// DB connection creation
// User, Password and Database are configured in Docker/init/db/initdb.sql file
// MariaDB -> var connectionString = "server=mariadb;user=Transport;password=transport;database=Transport";
//var connectionString = @"Host=psql;Username=Transport;Password=transport;Database=Transport";
// setting up DB as app service, some logging should be disabled for production
builder.Services.AddDbContext<ReservationsContext>(
    dbContextOptions => dbContextOptions
        .UseNpgsql(builder.Configuration.GetConnectionString("PsqlConnection"))
        // The following three options help with debugging, but should
        // be changed or removed for production.
        .LogTo(Console.WriteLine, LogLevel.Information)
        .EnableSensitiveDataLogging()
        .EnableDetailedErrors()
);
builder.Services.AddMassTransit(cfg =>
{
    cfg.AddSagaStateMachine<ReservationStateMachine, StatefulReservation>().InMemoryRepository();
    cfg.AddDelayedMessageScheduler();
    cfg.UsingRabbitMq((context, rabbitCfg) =>
    {
        rabbitCfg.Host("rabbitmq", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });
        rabbitCfg.UseDelayedMessageScheduler();
        rabbitCfg.ConfigureEndpoints(context);
    });
});
var app = builder.Build();
app.Run();