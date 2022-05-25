using MassTransit;
using Models.Reservations;
using Models.Payments;

namespace Reservation.Orchestration
{
    public class StatefulReservation : SagaStateMachineInstance
    {
        // TODO change fields if needed
        public int CurrentState { get; set; }
        public Guid CorrelationId { get; set; }
        public Guid OfferId { get; set; }
        public Guid ReservationId { get; set; }
        public Guid UserId { get; set; }
        public Guid? ReservationTimeoutEventId { get; set; }
        public string Destination { get; set; }
        public string Departure { get; set; }
        public DateTime BeginDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime DepartureTime { get; set; }
        public int NumberOfPeople { get; set; }
        public int TransportId { get; set; }
        public string? HotelName { get; set; }
        public int HotelId { get; set; }
        public bool TravelReservationSuccesful { get; set; }
        public bool HotelReservationSuccesful { get; set; }
        public bool PaymentInformationReceived { get; set; }
        public CardCredentials CardCredentials { get; set; }
        public double Price { get; set; }
        public bool PaymentSuccesful { get; set; }
        public int Adults { get; set; }
        public int ChildrenUnder3 { get; set; }
        public int ChildrenUnder10 { get; set; }
        public int ChildrenUnder18 { get; set; }
        public int SmallRooms { get; set; }
        public int BigRooms { get; set; }
        public bool HasInternet { get; set; }
        public bool HasBreakfast { get; set; }
        public bool HasOwnTransport { get; set; }
        public double HotelPrice { get; set; }
        public double TransportPrice { get; set; }
        public bool HasPromotionCode { get; set; }
    }
}
