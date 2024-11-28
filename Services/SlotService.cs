using Microsoft.EntityFrameworkCore;
using WebApplication3.Data;
using WebApplication3.Models;

namespace WebApplication3.Services
{
    public class SlotService
    {
        private readonly ApplicationDbContext _context;

        public SlotService(ApplicationDbContext context)
        {
            _context = context;
        }

        //public async Task<bool> MakeReservation(int userId, int slotId)
        //{
        //    var slot = await _context.Rooms.FirstOrDefaultAsync(s => s.Id == slotId);
        //    if (slot == null || slot.IsAvailable <= 0)
        //    {
        //        return false;  // Slot dolmuş veya bulunamadı
        //    }

        //    // Slot kapasitesini güncelle
        //    slot.IsAvailable -= 1;
        //    _context.Slots.Update(slot);

        //    // Yeni rezervasyon kaydını ekle
        //    var reservation = new Reservation
        //    {
        //        UserId = userId,
        //        SlotId = slotId,
        //        ReservationTime = DateTime.Now,
        //        Status = ReservationStatus.Pending
        //    };
        //    await _context.Reservations.AddAsync(reservation);

        //    await _context.SaveChangesAsync();
        //    return true;  // Rezervasyon başarılı
        //}
    }

}
