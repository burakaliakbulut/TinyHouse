namespace TinyHouse.Models
{
    public class HouseImage
    {
        public int Id { get; set; }
        public string ResimUrl { get; set; }

        public int HouseId { get; set; }
        public House House { get; set; }
    }
}
