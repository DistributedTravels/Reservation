using Microsoft.EntityFrameworkCore;
using Reservation.Database;
using MassTransit;
using Reservation.Orchestration;
using Reservation.Consumers;
using Reservation.Services;

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
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<IReservationChangesService, ReservationChangesService>();
builder.Services.AddMassTransit(cfg =>
{
    cfg.AddConsumer<GetReservationsFromDatabaseEventConsumer>(context =>
    {
        context.UseMessageRetry(r => r.Interval(3, 1000));
        context.UseInMemoryOutbox();
    });
    cfg.AddConsumer<SaveReservationToDatabaseEventConsumer>(context =>
    {
        context.UseMessageRetry(r => r.Interval(3, 1000));
        context.UseInMemoryOutbox();
    });
    cfg.AddConsumer<ChangesInReservationsEventConsumer>(context =>
    {
        context.UseMessageRetry(r => r.Interval(3, 1000));
        context.UseInMemoryOutbox();
    });
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
using (var contScope = app.Services.CreateScope())
using (var context = contScope.ServiceProvider.GetRequiredService<ReservationsContext>())
{
    // Ensure Deleted possible to use for testing
    context.Database.EnsureCreated();
    context.SaveChanges(); // save to DB
    Console.WriteLine("Done clearing database");
}
app.Run();