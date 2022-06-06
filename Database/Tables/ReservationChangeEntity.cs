using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Reservation.Database.Tables
{
    public class ReservationChangeEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public Guid ReservationId { get; set; }
        public int HotelId { get; set; }
        public string HotelName { get; set; }
        public double ChangeInHotelPrice { get; set; }
        public bool WifiAvailable { get; set; }
        public bool BreakfastAvailable { get; set; }
        public bool HotelAvailable { get; set; }
        public int SmallRoomNumberChange { get; set; }
        public int BigRoomNumberChange { get; set; }
        public int TransportId { get; set; }
        public double ChangeInTransportPrice { get; set; }
        public bool PlaneAvailable { get; set; }
        public int FreeSeatsChange { get; set; }
        public bool ReservationAvailable { get; set; }
        public DateTime ChangeDate { get; set; }
    }
}
