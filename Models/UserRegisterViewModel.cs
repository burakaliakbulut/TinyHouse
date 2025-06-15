using System.ComponentModel.DataAnnotations;

public class UserRegisterViewModel
{
    [Required]
    public string Ad { get; set; }

    [Required]
    public string Soyad { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    [Required]
    public string Rol { get; set; } // Admin, EvSahibi, Kiraci
}
