using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication3.Models
{
	public class Room
	{
		public int Id { get; set; }
		public string RoomName { get; set; } // Oda ismi veya numarası
		public int Capacity { get; set; } // Maksimum kapasite (ör. kişi sayısı)
		public bool IsAvailable { get; set; } // Mevcut durumda uygun mu?
		public ICollection<Reservation> Reservations { get; set; } // İlgili rezervasyonlar
		public string ImageUrl { get; set; }
		[NotMapped]
		public IFormFile ImgUrl { get; set; }
		public decimal Price { get; set; }
	}
}
