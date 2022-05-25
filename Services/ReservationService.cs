using Reservation.Database;
using Reservation.Database.Tables;

namespace Reservation.Services
{
    public class ReservationService : IReservationService
    {
        private readonly ReservationsContext _context;
        public ReservationService(ReservationsContext context)
        {
            _context = context;
        }
        public IEnumerable<ReservationEntity> GetReservations(Guid userId)
        {
            return _context.Reservations
                .Where(r => r.UserId == userId)
                .Select(r => r)
                .ToList();
        }

        public void SaveReservation(ReservationEntity entity)
        {
            entity.BeginDate = entity.BeginDate.ToUniversalTime();
            entity.EndDate = entity.EndDate.ToUniversalTime();
            entity.DepartureTime = entity.DepartureTime.ToUniversalTime();
            _context.Reservations.Add(entity);
            _context.SaveChanges();
        }
    }
}
