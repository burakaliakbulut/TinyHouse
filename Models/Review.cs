namespace TinyHouse.Models
{
    public class Review
    {
        public int Id { get; set; }
        public int Puan { get; set; }
        public string Yorum { get; set; }
        public DateTime Tarih { get; set; }

        public int ReservationId { get; set; }
        public Reservation Reservation { get; set; }
    }
}
