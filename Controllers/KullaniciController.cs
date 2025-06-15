using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TinyHouse.Models;

[Authorize]
public class KullaniciController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly ApplicationDbContext _context;

    public KullaniciController(UserManager<User> userManager, ApplicationDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> BakiyeYukle()
    {
        var user = await _userManager.GetUserAsync(User);
        ViewBag.Bakiye = user.Bakiye;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> BakiyeYukle(decimal miktar)
    {
        if (miktar <= 0)
        {
            TempData["Toast"] = "Geçersiz bakiye miktarı.";
            TempData["ToastClass"] = "text-bg-danger";
            return RedirectToAction("BakiyeYukle");
        }

        var user = await _userManager.GetUserAsync(User);
        user.Bakiye += miktar;
        await _userManager.UpdateAsync(user);

        TempData["Toast"] = $"₺{miktar} bakiyenize eklendi.";
        TempData["ToastClass"] = "text-bg-success";
        return RedirectToAction("BakiyeYukle");
    }
}
