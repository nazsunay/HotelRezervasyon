using IdentityOrnek.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using WebApplication3.Data;
using WebApplication3.Models;

namespace WebApplication3.Controllers
{

    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }
       
        public IActionResult Index()
        {
            
            var rooms = _context.Rooms.Where(r => r.IsAvailable)  
                                       .OrderBy(r => r.Price)
                                       .ToList();

            var reservationViewModel = new ReservationViewModel
            {
                Rooms = rooms 
            };

            return View(reservationViewModel); 
        }

        
        public IActionResult HotelGallery()
        {
            return View();
        }

        

        public IActionResult Contact()
        {
            return View();
        }

      
        public IActionResult RoomList()
        {
            
            var rooms = _context.Rooms
                .Where(r => r.IsAvailable) 
                .ToList();

           
            foreach (var room in rooms)
            {
                var hasActiveReservation = _context.Reservations.Any(r =>
                    r.RoomId == room.Id &&
                    (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Confirmed) &&
                    r.CheckOutDate > DateTime.Now); 

                room.IsAvailable = !hasActiveReservation;  
            }

            return View(rooms);
        }

       
        public IActionResult RoomDetail(int id)
        {
            
            var room = _context.Rooms.FirstOrDefault(r => r.Id == id);

            
            if (room == null)
            {
                return NotFound("Oda bulunamadý.");
            }

            
            var hasActiveReservation = _context.Reservations.Any(r =>
                r.RoomId == room.Id &&
                (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Confirmed) &&
                r.CheckOutDate > DateTime.Now); 

            room.IsAvailable = !hasActiveReservation;  

            
            var availableRooms = _context.Rooms
                .Where(r => r.IsAvailable && r.Id != room.Id) 
                .ToList();

            
            var userReservations = _context.Reservations
                .Where(r => r.RoomId == room.Id && (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Confirmed))
                .ToList();

            
            var model = new ReservationViewModel
            {
                Room = room, 
                Rooms = availableRooms, 
                Reservations = userReservations, 
                Reservation = new Reservation() 
            };

           
            return View(model);
        }

        public IActionResult About()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CheckAvailability(DateTime checkInDate, DateTime checkOutDate)
        {
            // Müsait odalarý filtreleme
            var availableRooms = _context.Rooms
                .Where(r => r.IsAvailable && !r.Reservations.Any(res =>
                    (res.CheckInDate < checkOutDate && res.CheckOutDate > checkInDate)))
                .ToList();

            // Eðer odalar bulunamazsa TempData ile mesaj gönder
            if (!availableRooms.Any())
            {
                TempData["NoAvailableRooms"] = "Müsait oda bulunamadý.";
            }

            // View'e uygun odalarý gönder
            return View(availableRooms);
        }

        public IActionResult Blog()
        {
            return View();
        }

       
        public IActionResult MakeRoom(int roomId)
        {
            var room = _context.Rooms.FirstOrDefault(r => r.Id == roomId);

            if (room == null)
            {
                return NotFound("Seçilen oda bulunamadý.");
            }

            var hasActiveReservation = _context.Reservations.Any(r =>
                r.RoomId == roomId &&
                (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Confirmed) &&
                r.CheckOutDate > DateTime.Now);

            ViewBag.IsRoomReserved = hasActiveReservation;  

            var reservation = new Reservation
            {
                RoomId = room.Id,
                Room = room,
                CheckInDate = DateTime.Now,
                CheckOutDate = DateTime.Now.AddDays(1)
            };

            return View(reservation);
        }

        [HttpPost]
        public async Task<IActionResult> MakeRoom(Reservation reservation)
        {
           
            reservation.Room = _context.Rooms.FirstOrDefault(r => r.Id == reservation.RoomId);

            if (reservation.Room == null)
            {
                return NotFound("Seçilen oda bulunamadý.");
            }

            
            var hasActiveReservation = _context.Reservations.Any(r =>
                r.RoomId == reservation.RoomId &&
                
                ((r.CheckInDate >= reservation.CheckInDate && r.CheckInDate < reservation.CheckOutDate) ||
                 (r.CheckOutDate > reservation.CheckInDate && r.CheckOutDate <= reservation.CheckOutDate) ||
                 (reservation.CheckInDate >= r.CheckInDate && reservation.CheckOutDate <= r.CheckOutDate)) &&
                (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Confirmed));

            if (hasActiveReservation)
            {
                
                TempData["ReservationMessage"] = "Seçilen oda bu tarihlerde zaten rezerve edilmiþ.";
                return RedirectToAction("MakeRoom", new { roomId = reservation.RoomId }); 
            }

            
            reservation.CreatedDate = DateTime.Now;
            reservation.Status = ReservationStatus.Pending;

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

           
            var emailSender = new EmailSender();
            var subject = "Oda Rezervasyonu Onayý";
            var message = $"Merhaba {reservation.FirstName} {reservation.LastName},<br><br>" +
                          $"Rezervasyonunuz alýnmýþtýr.<br>" +
                          $"Oda: {reservation.Room.RoomName}<br>" +
                          $"Giriþ Tarihi: {reservation.CheckInDate.ToString("dd/MM/yyyy")}<br>" +
                          $"Çýkýþ Tarihi: {reservation.CheckOutDate.ToString("dd/MM/yyyy")}<br>" +
                          $"Özel Talepler: {reservation.SpecialRequests ?? "Yok"}<br><br>" +
                          $"Ýletiþime geçilecektir. Rezervasyonunuz iþleme alýnacaktýr.<br><br>" +
                          $"Teþekkürler,<br>Otel Yönetimi";

            
            await emailSender.SendEmailAsync(reservation.Email, subject, message);

            
            TempData["ReservationSuccessMessage"] = "Odanýz baþarýyla rezerve edilmiþtir. Bilgilendirme mail aracýlýðýyla yapýlacaktýr.";

          
            return RedirectToAction("MakeRoom", new { roomId = reservation.RoomId });
        }
    }
}

