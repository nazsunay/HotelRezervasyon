using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication3.Models
{
    public enum ReservationStatus
    {
        Pending ,  // Beklemede
        Confirmed ,  // Onaylı
        Canceled ,  // İptal Edildi
        Completed  // Tamamlandı
    }

	public class Reservation
	{
		public int Id { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string Email { get; set; }
		public string Phone {  get; set; }
		
		public int RoomId { get; set; } // Slot (oda) bilgisi
		public Room Room { get; set; }
		public DateTime CheckInDate { get; set; } // Giriş tarihi
		public DateTime CheckOutDate { get; set; } // Çıkış tarihi
		public DateTime CreatedDate { get; set; } // Rezervasyon oluşturulma tarihi
		public ReservationStatus Status { get; set; } // Rezervasyon durumu
		public string? SpecialRequests { get; set; } // Özel talepler
	}

	

}
