using Reservation.Database.Tables;

namespace Reservation.Services
{
    public interface IReservationService
    {
        public IEnumerable<ReservationEntity> GetReservations(Guid userId);
        public void SaveReservation(ReservationEntity entity);
    }
}
