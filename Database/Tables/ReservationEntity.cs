using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Models.Reservations.Dto;

namespace Reservation.Database.Tables
{
    public class ReservationEntity
    {
        [Key] [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public int TransportId { get; set; }
        public string? HotelName { get; set; }
        public int HotelId { get; set; }
        public string? Destination { get; set; }
        public string? Departure { get; set; }
        public int NumberOfPeople { get; set; }
        public DateTime BeginDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime DepartureTime { get; set; }
        public string Status { get; set; }
        public int Adults { get; set; }
        public int ChildrenUnder3 { get; set; }
        public int ChildrenUnder10 { get; set; }
        public int ChildrenUnder18 { get; set; }
        public int SmallRooms { get; set; }
        public int BigRooms { get; set; }
        public bool HasInternet { get; set; }
        public bool HasBreakfast { get; set; }
        public bool HasOwnTransport { get; set; }

        public void SetFields(ReservationDto dto)
        {
            this.UserId = dto.UserId;
            this.TransportId = dto.TransportId;
            this.HotelName = dto.HotelName;
            this.HotelId = dto.HotelId;
            this.Destination = dto.Destination;
            this.Departure = dto.Departure;
            this.NumberOfPeople = dto.NumberOfPeople;
            this.BeginDate = dto.BeginDate;
            this.EndDate = dto.EndDate;
            this.DepartureTime = dto.DepartureTime;
            this.Status = dto.Status;
            this.Adults = dto.Adults;
            this.ChildrenUnder3 = dto.ChildrenUnder3;
            this.ChildrenUnder10 = dto.ChildrenUnder10;
            this.ChildrenUnder18 = dto.ChildrenUnder18;
            this.SmallRooms = dto.SmallRooms;
            this.BigRooms = dto.BigRooms;
            this.HasInternet = dto.HasInternet;
            this.HasBreakfast = dto.HasBreakfast;
            this.HasOwnTransport = dto.HasOwnTransport;
        }
        public ReservationDto ToReservationDto()
        {
            return new ReservationDto()
            {
                UserId = this.UserId,
                TransportId = this.TransportId,
                HotelName = this.HotelName,
                HotelId = this.HotelId,
                Destination = this.Destination,
                Departure = this.Departure,
                NumberOfPeople = this.NumberOfPeople,
                BeginDate = this.BeginDate,
                EndDate = this.EndDate,
                DepartureTime = this.DepartureTime,
                Status = this.Status,
                Adults = this.Adults,
                ChildrenUnder3 = this.ChildrenUnder3,
                ChildrenUnder10 = this.ChildrenUnder10,
                ChildrenUnder18 = this.ChildrenUnder18,
                SmallRooms = this.SmallRooms,
                BigRooms = this.BigRooms,
                HasInternet = this.HasInternet,
                HasBreakfast = this.HasBreakfast,
                HasOwnTransport = this.HasOwnTransport
            };
        }
    }
}
