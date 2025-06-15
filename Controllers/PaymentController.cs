using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize(Roles = "Admin")]
public class PaymentsController : Controller
{
    private readonly ApplicationDbContext _context;

    public PaymentsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var payments = await _context.Payments
            .Include(p => p.Reservation)
                .ThenInclude(r => r.House)
            .Include(p => p.Reservation)
                .ThenInclude(r => r.Kiraci)
            .ToListAsync();

        ViewBag.Toplam = payments.Sum(p => p.Tutar);

        return View(payments);
    }
}
