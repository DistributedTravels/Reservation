using MassTransit;
using Models.Reservations;
using Models.Reservations.Dto;
using Reservation.Services;

namespace Reservation.Consumers
{
    public class GetReservationsFromDatabaseEventConsumer : IConsumer<GetReservationsFromDatabaseEvent>
    {
        private readonly IReservationService _reservationService;

        public GetReservationsFromDatabaseEventConsumer(IReservationService reservationService)
        {
            _reservationService = reservationService;
        }
        public async Task Consume(ConsumeContext<GetReservationsFromDatabaseEvent> context)
        {
            var userId = context.Message.UserId;
            var reservations = _reservationService.GetReservations(userId);
            var reservationsDto = new List<ReservationDto>();
            foreach(var reservation in reservations)
            {
                var reservationDto = reservation.ToReservationDto();
                reservationsDto.Add(reservationDto);
            }
            context.Respond<GetReservationsFromDatabaseReplyEvent>(new GetReservationsFromDatabaseReplyEvent() { CorrelationId = context.Message.CorrelationId, Reservations = reservationsDto });
        }
    }
}
