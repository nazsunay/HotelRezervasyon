using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication3.Data;
using WebApplication3.Models;

namespace WebApplication3.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AdminController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

     
        public IActionResult Index()
        {
            if (!IsUserAuthenticated())
            {
                return RedirectToAction("Index", "Login"); 
            }

            var rooms = _context.Rooms.Include(r => r.Reservations).ToList();
            return View(rooms);
        }

        public IActionResult Create()
        {
            if (!IsUserAuthenticated())
            {
                return RedirectToAction("Index", "Login");
            }

            return View();
        }

       
        [HttpPost]
        public async Task<IActionResult> Create(Room room)
        {
            if (!IsUserAuthenticated())
            {
                return RedirectToAction("Index", "Login");
            }

            if (ModelState.IsValid)
            {
                
                if (room.ImgUrl != null)
                {
                    string fileName = Path.GetFileNameWithoutExtension(room.ImgUrl.FileName);
                    string extension = Path.GetExtension(room.ImgUrl.FileName);
                    string webRootPath = _webHostEnvironment.WebRootPath;
                    string directoryPath = Path.Combine(webRootPath, "images", "room");

                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }

                    fileName = fileName + DateTime.Now.ToString("yymmssfff") + extension;
                    string filePath = Path.Combine(directoryPath, fileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await room.ImgUrl.CopyToAsync(fileStream);
                    }

                    room.ImageUrl = "/images/room/" + fileName;
                }

                room.IsAvailable = true;

                _context.Add(room);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(room);
        }

        

        private bool IsUserAuthenticated()
        {
            return HttpContext.Session.GetString("IsAuthenticated") == "true";
        }

        
        public async Task<IActionResult> Edit(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null)
            {
                return NotFound();
            }
            return View(room);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Room room)
        {
            if (id != room.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                try
                {
                    if (room.ImgUrl != null)
                    {
                        string fileName = Path.GetFileNameWithoutExtension(room.ImgUrl.FileName);
                        string extension = Path.GetExtension(room.ImgUrl.FileName);
                        string webRootPath = _webHostEnvironment.WebRootPath;
                        string directoryPath = Path.Combine(webRootPath, "images", "rooms");

                        if (!Directory.Exists(directoryPath))
                        {
                            Directory.CreateDirectory(directoryPath);
                        }

                        fileName = fileName + DateTime.Now.ToString("yymmssfff") + extension;
                        string filePath = Path.Combine(directoryPath, fileName);
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await room.ImgUrl.CopyToAsync(fileStream);
                        }

                        room.ImageUrl = "/images/rooms/" + fileName;
                    }

                    _context.Update(room);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RoomExists(room.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(room);
        }

     
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

       
        private bool RoomExists(int id)
        {
            return _context.Rooms.Any(e => e.Id == id);
        }

       
        public IActionResult Reservations()
        {
            if (!IsUserAuthenticated())
            {
                return RedirectToAction("Index", "Login");
            }

            var reservations = _context.Reservations.Include(r => r.Room).ToList();
            return View(reservations);
        }

        
        [HttpPost]
        public async Task<IActionResult> UpdateReservationStatus(int reservationId, ReservationStatus status)
        {
            var reservation = await _context.Reservations.FindAsync(reservationId);

            if (reservation != null)
            {
                reservation.Status = status;
                _context.Update(reservation);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Rezervasyon durumu başarıyla güncellendi!";
            }
            else
            {
                TempData["ErrorMessage"] = "Rezervasyon bulunamadı!";
            }

            return RedirectToAction("Reservations");
        }

        
        [HttpPost, ActionName("DeleteReservation")]
       
        public async Task<IActionResult> DeleteReservation(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);

            if (reservation != null)
            {
                _context.Reservations.Remove(reservation);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Rezervasyon başarıyla silindi!";
            }
            else
            {
                TempData["ErrorMessage"] = "Rezervasyon bulunamadı!";
            }

            return RedirectToAction("Reservations");
        }
    }
}
