using MassTransit;
using Models.Reservations;
using Models.Reservations.Dto;
using Reservation.Services;

namespace Reservation.Consumers
{
    public class GetReservationsFromDatabaseEventConsumer : IConsumer<GetReservationsFromDatabaseEvent>
    {
        private readonly IReservationService _reservationService;
        private readonly IReservationChangesService _reservationChangesService;

        public GetReservationsFromDatabaseEventConsumer(IReservationService reservationService, IReservationChangesService reservationChangesService)
        {
            _reservationService = reservationService;
            _reservationChangesService = reservationChangesService;
        }
        public async Task Consume(ConsumeContext<GetReservationsFromDatabaseEvent> context)
        {
            var userId = context.Message.UserId;
            var reservations = _reservationService.GetReservations(userId).ToArray();
            for(int i = 0; i < reservations.Count(); i++)
            {
                var reservationChanges = _reservationChangesService.GetChanges(reservations[i].ReservationId);
                foreach(var change in reservationChanges)
                {
                    reservations[i].ApplyChanges(change);
                }
            }
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
