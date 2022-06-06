using Reservation.Database.Tables;

namespace Reservation.Services
{
    public interface IReservationChangesService
    {
        public void AddChanges(ReservationChangeEntity reservationChange);
        public IEnumerable<ReservationChangeEntity> GetAllChanges();
        public IEnumerable<ReservationChangeEntity> GetChanges(Guid reservationId);
    }
}
