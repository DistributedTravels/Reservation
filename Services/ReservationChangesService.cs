using Reservation.Database;
using Reservation.Database.Tables;
using Models.Reservations;
using MassTransit;


namespace Reservation.Services
{
    public class ReservationChangesService : IReservationChangesService
    {
        private readonly ReservationsContext _context;
        private readonly IPublishEndpoint _publishEndpoint;
        public ReservationChangesService(ReservationsContext context, IPublishEndpoint publishEndpoint)
        {
            _context = context;
            _publishEndpoint = publishEndpoint;
        }
        public void AddChanges(ReservationChangeEntity reservationChange)
        {
            reservationChange.ChangeDate = reservationChange.ChangeDate.ToUniversalTime();
            // reservation change applies to multiple reservations
            if (reservationChange.ReservationId.Equals(Guid.Empty))
            {
                // changes from hotels
                if(reservationChange.HotelId != -1)
                {
                    var affectedReservations = _context.Reservations
                        .Where(r => r.HotelId == reservationChange.HotelId && r.BeginDate > reservationChange.ChangeDate)
                        .Select(r => r)
                        .ToList();
                    foreach(var affectedReservation in affectedReservations)
                    {
                        var singleReservationChange = new ReservationChangeEntity()
                        {
                            ReservationId = affectedReservation.ReservationId,
                            HotelId = reservationChange.HotelId,
                            HotelName = reservationChange.HotelName,
                            BigRoomNumberChange = reservationChange.BigRoomNumberChange,
                            SmallRoomNumberChange = reservationChange.SmallRoomNumberChange,
                            BreakfastAvailable = reservationChange.BreakfastAvailable,
                            WifiAvailable = reservationChange.WifiAvailable,
                            ChangeInHotelPrice = reservationChange.ChangeInHotelPrice,
                            HotelAvailable = reservationChange.HotelAvailable,
                            TransportId = reservationChange.TransportId,
                            ChangeInTransportPrice = reservationChange.ChangeInTransportPrice,
                            FreeSeatsChange = reservationChange.FreeSeatsChange,
                            PlaneAvailable = reservationChange.PlaneAvailable,
                            ChangeDate = reservationChange.ChangeDate,
                            ReservationAvailable = reservationChange.ReservationAvailable
                        };
                        var singleReservationChangeEvent = new ChangedReservationEvent()
                        {
                            ReservationId = affectedReservation.ReservationId,
                            HotelId = reservationChange.HotelId,
                            HotelName = reservationChange.HotelName,
                            BigRoomNumberChange = reservationChange.BigRoomNumberChange,
                            SmallRoomNumberChange = reservationChange.SmallRoomNumberChange,
                            BreakfastAvailable = reservationChange.BreakfastAvailable,
                            WifiAvailable = reservationChange.WifiAvailable,
                            ChangeInHotelPrice = reservationChange.ChangeInHotelPrice,
                            HotelAvailable = reservationChange.HotelAvailable,
                            TransportId = reservationChange.TransportId,
                            ChangeInTransportPrice = reservationChange.ChangeInTransportPrice,
                            FreeSeatsChange = reservationChange.FreeSeatsChange,
                            PlaneAvailable = reservationChange.PlaneAvailable,
                            ReservationAvailable = reservationChange.ReservationAvailable
                        };
                        _context.Add(singleReservationChange);
                        _publishEndpoint.Publish<ChangedReservationEvent>(singleReservationChangeEvent);
                    }
                        
                }
                // changes from transport
                else
                {
                    var affectedReservations = _context.Reservations
                        .Where(r => r.HotelId == reservationChange.TransportId && r.BeginDate > reservationChange.ChangeDate)
                        .Select(r => r)
                        .ToList();
                    foreach (var affectedReservation in affectedReservations)
                    {
                        var singleReservationChange = new ReservationChangeEntity()
                        {
                            ReservationId = affectedReservation.ReservationId,
                            HotelId = reservationChange.HotelId,
                            HotelName = reservationChange.HotelName,
                            BigRoomNumberChange = reservationChange.BigRoomNumberChange,
                            SmallRoomNumberChange = reservationChange.SmallRoomNumberChange,
                            BreakfastAvailable = reservationChange.BreakfastAvailable,
                            WifiAvailable = reservationChange.WifiAvailable,
                            ChangeInHotelPrice = reservationChange.ChangeInHotelPrice,
                            HotelAvailable = reservationChange.HotelAvailable,
                            TransportId = reservationChange.TransportId,
                            ChangeInTransportPrice = reservationChange.ChangeInTransportPrice,
                            FreeSeatsChange = reservationChange.FreeSeatsChange,
                            PlaneAvailable = reservationChange.PlaneAvailable,
                            ChangeDate = reservationChange.ChangeDate,
                            ReservationAvailable = reservationChange.ReservationAvailable
                        };
                        var singleReservationChangeEvent = new ChangedReservationEvent()
                        {
                            ReservationId = affectedReservation.ReservationId,
                            HotelId = reservationChange.HotelId,
                            HotelName = reservationChange.HotelName,
                            BigRoomNumberChange = reservationChange.BigRoomNumberChange,
                            SmallRoomNumberChange = reservationChange.SmallRoomNumberChange,
                            BreakfastAvailable = reservationChange.BreakfastAvailable,
                            WifiAvailable = reservationChange.WifiAvailable,
                            ChangeInHotelPrice = reservationChange.ChangeInHotelPrice,
                            HotelAvailable = reservationChange.HotelAvailable,
                            TransportId = reservationChange.TransportId,
                            ChangeInTransportPrice = reservationChange.ChangeInTransportPrice,
                            FreeSeatsChange = reservationChange.FreeSeatsChange,
                            PlaneAvailable = reservationChange.PlaneAvailable,
                            ReservationAvailable = reservationChange.ReservationAvailable
                        };
                        _context.Add(singleReservationChange);
                        _publishEndpoint.Publish<ChangedReservationEvent>(singleReservationChangeEvent);
                    }
                }
            }
            // reservation change applies to a single reservation
            else
            {
                var singleReservationChangeEvent = new ChangedReservationEvent()
                {
                    ReservationId = reservationChange.ReservationId,
                    HotelId = reservationChange.HotelId,
                    HotelName = reservationChange.HotelName,
                    BigRoomNumberChange = reservationChange.BigRoomNumberChange,
                    SmallRoomNumberChange = reservationChange.SmallRoomNumberChange,
                    BreakfastAvailable = reservationChange.BreakfastAvailable,
                    WifiAvailable = reservationChange.WifiAvailable,
                    ChangeInHotelPrice = reservationChange.ChangeInHotelPrice,
                    HotelAvailable = reservationChange.HotelAvailable,
                    TransportId = reservationChange.TransportId,
                    ChangeInTransportPrice = reservationChange.ChangeInTransportPrice,
                    FreeSeatsChange = reservationChange.FreeSeatsChange,
                    PlaneAvailable = reservationChange.PlaneAvailable,
                    ReservationAvailable = reservationChange.ReservationAvailable
                };
                _context.ReservationChanges.Add(reservationChange);
                _publishEndpoint.Publish<ChangedReservationEvent>(singleReservationChangeEvent);
            }
            _context.SaveChanges();
        }
        public IEnumerable<ReservationChangeEntity> GetAllChanges()
        {
            return _context.ReservationChanges.ToList();
        }
        public IEnumerable<ReservationChangeEntity> GetChanges(Guid reservationId)
        {
            return _context.ReservationChanges.Where(change => change.ReservationId.Equals(reservationId)).ToList();
        }
    }
}
