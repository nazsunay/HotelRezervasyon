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
                return NotFound("Oda bulunamad�.");
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
            // M�sait odalar� filtreleme
            var availableRooms = _context.Rooms
                .Where(r => r.IsAvailable && !r.Reservations.Any(res =>
                    (res.CheckInDate < checkOutDate && res.CheckOutDate > checkInDate)))
                .ToList();

            // E�er odalar bulunamazsa TempData ile mesaj g�nder
            if (!availableRooms.Any())
            {
                TempData["NoAvailableRooms"] = "M�sait oda bulunamad�.";
            }

            // View'e uygun odalar� g�nder
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
                return NotFound("Se�ilen oda bulunamad�.");
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
                return NotFound("Se�ilen oda bulunamad�.");
            }

            
            var hasActiveReservation = _context.Reservations.Any(r =>
                r.RoomId == reservation.RoomId &&
                
                ((r.CheckInDate >= reservation.CheckInDate && r.CheckInDate < reservation.CheckOutDate) ||
                 (r.CheckOutDate > reservation.CheckInDate && r.CheckOutDate <= reservation.CheckOutDate) ||
                 (reservation.CheckInDate >= r.CheckInDate && reservation.CheckOutDate <= r.CheckOutDate)) &&
                (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Confirmed));

            if (hasActiveReservation)
            {
                
                TempData["ReservationMessage"] = "Se�ilen oda bu tarihlerde zaten rezerve edilmi�.";
                return RedirectToAction("MakeRoom", new { roomId = reservation.RoomId }); 
            }

            
            reservation.CreatedDate = DateTime.Now;
            reservation.Status = ReservationStatus.Pending;

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

           
            var emailSender = new EmailSender();
            var subject = "Oda Rezervasyonu Onay�";
            var message = $"Merhaba {reservation.FirstName} {reservation.LastName},<br><br>" +
                          $"Rezervasyonunuz al�nm��t�r.<br>" +
                          $"Oda: {reservation.Room.RoomName}<br>" +
                          $"Giri� Tarihi: {reservation.CheckInDate.ToString("dd/MM/yyyy")}<br>" +
                          $"��k�� Tarihi: {reservation.CheckOutDate.ToString("dd/MM/yyyy")}<br>" +
                          $"�zel Talepler: {reservation.SpecialRequests ?? "Yok"}<br><br>" +
                          $"�leti�ime ge�ilecektir. Rezervasyonunuz i�leme al�nacakt�r.<br><br>" +
                          $"Te�ekk�rler,<br>Otel Y�netimi";

            
            await emailSender.SendEmailAsync(reservation.Email, subject, message);

            
            TempData["ReservationSuccessMessage"] = "Odan�z ba�ar�yla rezerve edilmi�tir. Bilgilendirme mail arac�l���yla yap�lacakt�r.";

          
            return RedirectToAction("MakeRoom", new { roomId = reservation.RoomId });
        }
    }
}

