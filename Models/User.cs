using Microsoft.AspNetCore.Identity;

namespace TinyHouse.Models
{
    public class User : IdentityUser
    {
        public string Ad { get; set; }
        public string Soyad { get; set; }
        public string Rol { get; set; }
        public bool AktifMi { get; set; } = true;
        public DateTime KayitTarihi { get; set; } = DateTime.Now;

        public ICollection<House> Evler { get; set; }
        public ICollection<Reservation> Rezervasyonlar { get; set; }
        public decimal Bakiye { get; set; } = 0;

    }
}