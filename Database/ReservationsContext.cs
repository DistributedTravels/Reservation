using Microsoft.EntityFrameworkCore;
using Reservation.Database.Tables;

namespace Reservation.Database
{
    public class ReservationsContext : DbContext
    {
        public ReservationsContext(DbContextOptions<ReservationsContext> options) : base(options) { }
        public ReservationsContext() { }
        public DbSet<ReservationEntity> Reservations { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ReservationEntity>().ToTable("Reservation"); // table name overwrite (removing the plural "s")
        }
    }
}
