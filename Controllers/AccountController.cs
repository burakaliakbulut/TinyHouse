using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TinyHouse.Models;

public class AccountController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AccountController(UserManager<User> userManager, SignInManager<User> signInManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
    }

    // GET: /Account/Register
    public IActionResult Register() => View();

    [HttpPost]
    public async Task<IActionResult> Register(UserRegisterViewModel model)
    {
        if (ModelState.IsValid)
        {   
            var user = new User
            {
                UserName = model.Email,
                Email = model.Email,
                Ad = model.Ad,
                Soyad = model.Soyad,
                Rol = model.Rol,
                AktifMi = model.Rol== "EvSahibi" ? false : true,
                KayitTarihi = DateTime.Now
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                if (!await _roleManager.RoleExistsAsync(model.Rol))
                    await _roleManager.CreateAsync(new IdentityRole(model.Rol));

                await _userManager.AddToRoleAsync(user, model.Rol);
                await _signInManager.SignInAsync(user, isPersistent: false);
                TempData["Toast"] = "Kayıt Başarılı";
                return RedirectToAction("RedirectToDashboard");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(model);
    }

    // GET: /Account/Login
    public IActionResult Login() => View();

    [HttpPost]
    public async Task<IActionResult> Login(UserLoginViewModel model)
    {
        if (ModelState.IsValid)
        {
            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, false, false);
            if (result.Succeeded)
                TempData["Toast"] = "Giriş Başarılı";
            return RedirectToAction("RedirectToDashboard");

            ModelState.AddModelError(string.Empty, "Giriş başarısız.");
        }

        return View(model);
    }

    public async Task<IActionResult> RedirectToDashboard()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            // Kullanıcı yoksa giriş ekranına at
            return RedirectToAction("Login", "Account");
        }

        if (await _userManager.IsInRoleAsync(user, "Admin"))
            return RedirectToAction("Dashboard", "Admin");

        if (await _userManager.IsInRoleAsync(user, "EvSahibi"))
            return RedirectToAction("Dashboard", "Ev");

        if (await _userManager.IsInRoleAsync(user, "Kiraci"))
            return RedirectToAction("Rezervasyonlarim", "House");

        // Rol yoksa varsayılan
        return RedirectToAction("Index", "Home");
    }



    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Login");
    }
}
