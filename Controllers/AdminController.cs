using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TinyHouse.Models;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;

    public AdminController(ApplicationDbContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Dashboard()
    {
        var toplamKullanici = await _context.Users.CountAsync();
        var toplamEv = await _context.TinyHouses.CountAsync();
        var toplamRezervasyon = await _context.Reservations.CountAsync();
        var toplamOdeme = await _context.Payments.SumAsync(p => (decimal?)p.Tutar) ?? 0;

        var model = new AdminDashboardViewModel
        {
            KullaniciSayisi = toplamKullanici,
            EvSayisi = toplamEv,
            RezervasyonSayisi = toplamRezervasyon,
            ToplamOdemeTutari = toplamOdeme
        };

        return View(model);
    }
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> KullaniciListesi()
    {
        var users = await _userManager.Users.ToListAsync();
        return View(users);
    }
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> KullaniciDurumDegistir(string id, bool aktifMi)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        user.AktifMi = aktifMi;
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
        TempData["Toast"] = $"Kullanıcı {(aktifMi ? "aktif" : "pasif")} hale getirildi.";
        return RedirectToAction("KullaniciListesi");
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> KullaniciSil(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        await _userManager.DeleteAsync(user);
        TempData["Toast"] = "Kullanici Silindi";
        return RedirectToAction("KullaniciListesi");
    }

}
