using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        public int NumberOfPeople { get; set; }
        public DateTime BeginDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; }
    }
}
