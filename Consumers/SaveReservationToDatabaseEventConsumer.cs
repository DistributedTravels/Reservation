using Models.Reservations;
using Reservation.Services;
using Reservation.Database.Tables;
using MassTransit;

namespace Reservation.Consumers
{
    public class SaveReservationToDatabaseEventConsumer : IConsumer<SaveReservationToDatabaseEvent>
    {
        private readonly IReservationService _reservationService;
        public SaveReservationToDatabaseEventConsumer(IReservationService reservationService)
        {
            _reservationService = reservationService;
        }
        public async Task Consume(ConsumeContext<SaveReservationToDatabaseEvent> context)
        {
            var reservationDto = context.Message.Reservation;
            var reservation = new ReservationEntity();
            reservation.SetFields(reservationDto);
            _reservationService.SaveReservation(reservation);
        }
    }
}
