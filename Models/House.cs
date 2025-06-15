using System.ComponentModel.DataAnnotations.Schema;

namespace TinyHouse.Models
{
    public class House
    {
        public int Id { get; set; }
        public string Baslik { get; set; }
        public string Aciklama { get; set; }
        public decimal Fiyat { get; set; }
        public string Konum { get; set; }
        public string Durum { get; set; }
        public DateTime OlusturmaTarihi { get; set; }

        public string EvSahibiId { get; set; }
        public User EvSahibi { get; set; }

        public ICollection<HouseImage> Resimler { get; set; }
        public ICollection<Reservation> Rezervasyonlar { get; set; }
        [NotMapped]
        public double OrtalamaPuan { get; set; }

    }
}
