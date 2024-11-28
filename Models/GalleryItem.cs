namespace WebApplication3.Models
{
    public class GalleryItem
    {
        public int Id { get; set; } // Görsel ID'si
        public string ImageUrl { get; set; } // Görsel URL'si
        public string Type { get; set; } // 'Hotel' veya 'Room' tipi olabilir
        public int? RoomId { get; set; } // Odaya aitse RoomId'yi tutacak
        public Room Room { get; set; }
       
    }
}
