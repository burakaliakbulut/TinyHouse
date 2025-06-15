using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using TinyHouse.Models;


public class HouseController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;

    public HouseController(ApplicationDbContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }


    public async Task<IActionResult> Index(double? minPuan, decimal? maxFiyat, string? konum, string? sirala)
    {
        var evler = await _context.TinyHouses
            .Include(e => e.Resimler)
            .Include(e => e.Rezervasyonlar)
                .ThenInclude(r => r.Yorum)
            .Where(e => e.Durum == "Aktif")
            .ToListAsync();

        foreach (var ev in evler)
        {
            var puanlar = ev.Rezervasyonlar
                .Where(r => r.Yorum != null)
                .Select(r => r.Yorum.Puan);

            ev.OrtalamaPuan = puanlar.Any() ? puanlar.Average() : 0;
        }
        //  FİLTRELEME

        if (minPuan.HasValue)
            evler = evler.Where(e => e.OrtalamaPuan >= minPuan.Value).ToList();

        if (maxFiyat.HasValue)
            evler = evler.Where(e => e.Fiyat <= maxFiyat.Value).ToList();

        if (!string.IsNullOrEmpty(konum))
            evler = evler.Where(e => e.Konum.ToLower().Contains(konum.ToLower())).ToList();

        evler = sirala switch
        {
            "fiyatArtan" => evler.OrderBy(e => e.Fiyat).ToList(),
            "fiyatAzalan" => evler.OrderByDescending(e => e.Fiyat).ToList(),
            "puanYuksek" => evler.OrderByDescending(e => e.OrtalamaPuan).ToList(),
            _ => evler
        };

        return View(evler);
    }


    public async Task<IActionResult> Details(int id)
    {
        var ev = await _context.TinyHouses
            .Include(e => e.Resimler)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (ev == null)
            return NotFound();

        var user = await _userManager.GetUserAsync(User);
        ViewBag.UserId = user?.Id;

        return View(ev);
    }
    [Authorize(Roles = "Kiraci,EvSahibi")]

    [HttpGet]
    public async Task<IActionResult> RezervasyonYap(int id)
    {
        

        var house = await _context.TinyHouses.FindAsync(id);
        if (house == null) return NotFound();
        var user = await _userManager.GetUserAsync(User);
        if (house.EvSahibiId == user.Id)
        {
            return Content("Kendi evinize rezervasyon yapamazsınız.");
        }
        ViewBag.House = house;
        return View();
    }

    [Authorize(Roles = "Kiraci,EvSahibi")]
    [HttpPost]
    public async Task<IActionResult> RezervasyonYap(int id, DateTime baslangic, DateTime bitis)
    {
        var house = await _context.TinyHouses.FindAsync(id);
        if (house == null) return NotFound();

        var user = await _userManager.GetUserAsync(User);

        // 🔒 Tarih çakışması kontrolü
        var tarihCakismasi = await _context.Reservations
            .AnyAsync(r => r.HouseId == id && r.Durum == "Onaylandi" &&
                ((baslangic >= r.BaslangicTarihi && baslangic < r.BitisTarihi) ||
                 (bitis > r.BaslangicTarihi && bitis <= r.BitisTarihi) ||
                 (baslangic <= r.BaslangicTarihi && bitis >= r.BitisTarihi)));

        if (tarihCakismasi)
        {
            TempData["Toast"] = "Bu tarihlerde zaten bir rezervasyon var.";
            TempData["ToastClass"] = "text-bg-danger";
            return RedirectToAction("Details", new { id });
        }

        // ✅ Rezervasyon oluşturuluyor
        var rezervasyon = new Reservation
        {
            HouseId = id,
            KiraciId = user.Id,
            BaslangicTarihi = baslangic,
            BitisTarihi = bitis,
            Durum = "Onaylandi", // artık otomatik onaylanıyor
            OlusturmaTarihi = DateTime.Now
        };

        _context.Reservations.Add(rezervasyon);
        await _context.SaveChangesAsync();
        if (user.Bakiye < house.Fiyat)
        {
            TempData["Toast"] = "Bakiyeniz yetersiz.";
            TempData["ToastClass"] = "text-bg-danger";
            return RedirectToAction("Details", new { id });
        }
        user.Bakiye -= house.Fiyat;
        await _userManager.UpdateAsync(user);
        var odeme = new Payment
        {
            ReservationId = rezervasyon.Id,
            Tutar = house.Fiyat,
            OdemeTarihi = DateTime.Now,
            OdemeDurumu = "Ödendi"
        };

        _context.Payments.Add(odeme);
        await _context.SaveChangesAsync();

        TempData["Toast"] = "Rezervasyon ve ödeme başarıyla oluşturuldu!";
        TempData["ToastClass"] = "text-bg-success";

        return RedirectToAction("Rezervasyonlarim");
    }



    [Authorize(Roles = "Kiraci,EvSahibi")]

    public async Task<IActionResult> Rezervasyonlarim()
    {
        var user = await _userManager.GetUserAsync(User);

        var rezervasyonlar = await _context.Reservations
            .Include(r => r.House)
            .Where(r => r.KiraciId == user.Id)
            .OrderByDescending(r => r.OlusturmaTarihi)
            .ToListAsync();

        return View(rezervasyonlar);
    }

    [Authorize(Roles = "Kiraci,EvSahibi,Admin")]
    [HttpPost]
    public async Task<IActionResult> IptalEt(int id)
    {
        var rezervasyon = await _context.Reservations
            .Include(r => r.House)
            .Include(r => r.Kiraci)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (rezervasyon == null || rezervasyon.Durum == "Iptal")
            return NotFound();

        // 1. Rezervasyonu iptal et
        rezervasyon.Durum = "Iptal";

        // 2. Ödeme kaydını bul
        var odeme = await _context.Payments.FirstOrDefaultAsync(p => p.ReservationId == rezervasyon.Id);
        if (odeme != null)
        {
            // 3. Bakiyeyi iade et
            rezervasyon.Kiraci.Bakiye += odeme.Tutar;

            // 4. Ödemeyi sil
            _context.Payments.Remove(odeme);
        }

        await _context.SaveChangesAsync();

        TempData["Toast"] = "Rezervasyon iptal edildi ve ödeme iade edildi.";
        TempData["ToastClass"] = "text-bg-warning";

        // Yönlendirme:
        if (User.IsInRole("Admin"))
            return RedirectToAction("Rezervasyonlar", "Admin");

        return RedirectToAction("Rezervasyonlarim", "House");
    }


    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> RezervasyonIptalEt(int id)
    {
        var rezervasyon = await _context.Reservations.FindAsync(id);
        if (rezervasyon == null || rezervasyon.Durum == "Iptal")
            return NotFound();

        rezervasyon.Durum = "Iptal";
        await _context.SaveChangesAsync();

        TempData["Toast"] = "Rezervasyon başarıyla iptal edildi.";
        TempData["ToastClass"] = "text-bg-warning";

        return RedirectToAction("Rezervasyonlar");
    }

    [Authorize(Roles = "Kiraci,EvSahibi")]

    [HttpGet]
    public async Task<IActionResult> YorumYap(int rezervasyonId)
    {
        var rezervasyon = await _context.Reservations
            .Include(r => r.House)
            .FirstOrDefaultAsync(r => r.Id == rezervasyonId && r.Durum == "Onaylandi");

        if (rezervasyon == null || rezervasyon.Yorum != null)
            return NotFound();

        ViewBag.EvAdi = rezervasyon.House.Baslik;
        return View(new Review { ReservationId = rezervasyonId });
    }

    [Authorize(Roles = "Kiraci,EvSahibi")]

    [HttpPost]
    public async Task<IActionResult> YorumYap(Review review)
    {
        if (ModelState.IsValid)
        {
            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();
            return RedirectToAction("Rezervasyonlarim");
        }

        return View(review);
    }
    [HttpGet]
    public async Task<IActionResult> RezervasyonTarihleri(int id)
    {
        var tarihler = await _context.Reservations
            .Where(r => r.HouseId == id && r.Durum == "Onaylandi")
            .Select(r => new {
                title = $"Dolu ({r.BaslangicTarihi:dd.MM})",
                start = r.BaslangicTarihi.ToString("yyyy-MM-dd"),
                end = r.BitisTarihi.AddDays(1).ToString("yyyy-MM-dd") 
            })
            .ToListAsync();

        return Json(tarihler);
    }

}