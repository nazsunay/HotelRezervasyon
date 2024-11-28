namespace WebApplication3.Models
{
    public class ReservationViewModel
    {
        public List<Reservation>?Reservations { get; set; }
        public List<Room>? Rooms { get; set; }
        public Reservation Reservation { get; set; }
        public Room Room { get; set; }
        
    }
}
