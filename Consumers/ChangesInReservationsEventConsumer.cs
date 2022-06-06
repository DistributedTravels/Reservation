using MassTransit;
using Models.Reservations;
using Reservation.Services;
using Reservation.Database.Tables;

namespace Reservation.Consumers
{
    public class ChangesInReservationsEventConsumer : IConsumer<ChangesInReservationsEvent>
    {
        private readonly IReservationChangesService _reservationChangeService;
        public ChangesInReservationsEventConsumer(IReservationChangesService reservationChangesService)
        {
            _reservationChangeService = reservationChangesService;
        }
        public async Task Consume(ConsumeContext<ChangesInReservationsEvent> context)
        {
            var reservationChange = new ReservationChangeEntity()
            {
                ReservationId = context.Message.ReservationId,
                HotelId = context.Message.ChangesInHotel.HotelId,
                HotelName = context.Message.ChangesInHotel.HotelName,
                BigRoomNumberChange = context.Message.ChangesInHotel.BigRoomNumberChange,
                SmallRoomNumberChange = context.Message.ChangesInHotel.SmallRoomNumberChange,
                BreakfastAvailable = context.Message.ChangesInHotel.BreakfastAvailable,
                WifiAvailable = context.Message.ChangesInHotel.WifiAvailable,
                ChangeInHotelPrice = context.Message.ChangesInHotel.ChangeInHotelPrice,
                HotelAvailable = context.Message.ChangesInHotel.HotelAvailable,
                TransportId = context.Message.ChangesInTransport.TransportId,
                ChangeInTransportPrice = context.Message.ChangesInTransport.ChangeInTransportPrice,
                FreeSeatsChange = context.Message.ChangesInTransport.FreeSeatsChange,
                PlaneAvailable = context.Message.ChangesInTransport.PlaneAvailable,
                ChangeDate = context.Message.CreationDate,
                ReservationAvailable = context.Message.ReservationAvailable
            };
            _reservationChangeService.AddChanges(reservationChange);
        }
    }
}
