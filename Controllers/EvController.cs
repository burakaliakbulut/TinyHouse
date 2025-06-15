using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TinyHouse.Models;

[Authorize(Roles = "EvSahibi")]
public class EvController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;

    public EvController(ApplicationDbContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Dashboard()
    {
        var user = await _userManager.GetUserAsync(User);
        var evler = _context.TinyHouses.Where(x => x.EvSahibiId == user.Id).ToList();

        ViewBag.EvSayisi = evler.Count;
        return View(evler);
    }
    // GET: Ev/Ekle
    [HttpGet]
    public async Task<IActionResult> Ekle()
    {
        var user = await _userManager.GetUserAsync(User);
        if (!user.AktifMi)
        {
            return RedirectToAction("Dashboard", new { mesaj = "Hesabınız henüz admin tarafından onaylanmadı." });
        }

        return View();
    }


    // POST: Ev/Ekle
    [HttpPost]
    public async Task<IActionResult> Ekle(HouseEkleViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (!user.AktifMi)
        {
            return RedirectToAction("Dashboard", new { mesaj = "Hesabınız henüz admin tarafından onaylanmadı." });
        }
        if (ModelState.IsValid)
        {
            
            var house = new House
            {
                Baslik = model.Baslik,
                Aciklama = model.Aciklama,
                Fiyat = model.Fiyat,
                Konum = model.Konum,
                Durum = "Aktif",
                OlusturmaTarihi = DateTime.Now,
                EvSahibiId = user.Id
            };

            _context.TinyHouses.Add(house);
            await _context.SaveChangesAsync();

            if(model.Resimler != null && model.Resimler.Any())
        {
                foreach (var resim in model.Resimler)
                {
                    if (resim.Length > 0)
                    {
                        var dosyaAdi = Guid.NewGuid() + Path.GetExtension(resim.FileName);
                        var yol = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", dosyaAdi);

                        using (var stream = new FileStream(yol, FileMode.Create))
                        {
                            await resim.CopyToAsync(stream);
                        }

                        _context.HouseImages.Add(new HouseImage
                        {
                            HouseId = house.Id,
                            ResimUrl = "/images/" + dosyaAdi
                        });
                    }
                }
                await _context.SaveChangesAsync();
            }
            TempData["Toast"] = "İlan Başarıyla Eklendi";
            TempData["ToastClass"] = "text-bg-success";
            return RedirectToAction("Dashboard");
        }

        return View(model);
    }

    [Authorize(Roles = "EvSahibi")]
    [HttpGet]
    public async Task<IActionResult> Duzenle(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        var ev = await _context.TinyHouses.FirstOrDefaultAsync(e => e.Id == id && e.EvSahibiId == user.Id);
        if (ev == null) return NotFound();

        var model = new HouseEkleViewModel
        {
            Baslik = ev.Baslik,
            Aciklama = ev.Aciklama,
            Fiyat = ev.Fiyat,
            Konum = ev.Konum
        };

        ViewBag.EvId = id;
        return View(model);
    }
    

    [Authorize(Roles = "EvSahibi")]
    [HttpPost]
    public async Task<IActionResult> Duzenle(int id, HouseEkleViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        var ev = await _context.TinyHouses.FirstOrDefaultAsync(e => e.Id == id && e.EvSahibiId == user.Id);
        if (ev == null) return NotFound();

        ev.Baslik = model.Baslik;
        ev.Aciklama = model.Aciklama;
        ev.Konum = model.Konum;
        ev.Fiyat = model.Fiyat;

        await _context.SaveChangesAsync();

        TempData["Toast"] = "İlan başarıyla güncellendi.";
        TempData["ToastClass"] = "text-bg-success";
        return RedirectToAction("Dashboard");
    }

    
    [Authorize(Roles = "EvSahibi")]
    [HttpPost]
    public async Task<IActionResult> Sil(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        var ev = await _context.TinyHouses
            .Include(e => e.Resimler)
            .FirstOrDefaultAsync(e => e.Id == id && e.EvSahibiId == user.Id);

        if (ev == null) return NotFound();

        // Resimleri de sil
        foreach (var resim in ev.Resimler)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", resim.ResimUrl.TrimStart('/'));
            if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
            _context.HouseImages.Remove(resim);
        }

        _context.TinyHouses.Remove(ev);
        await _context.SaveChangesAsync();

        TempData["Toast"] = "İlan başarıyla silindi.";
        TempData["ToastClass"] = "text-bg-danger";
        return RedirectToAction("Dashboard");
    }
}
