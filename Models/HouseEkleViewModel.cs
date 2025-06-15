using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

public class HouseEkleViewModel
{
    public string Baslik { get; set; }
    public string Aciklama { get; set; }
    public decimal Fiyat { get; set; }
    public string Konum { get; set; }

    public List<IFormFile> Resimler { get; set; }
}
