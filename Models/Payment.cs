namespace TinyHouse.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public decimal Tutar { get; set; }
        public DateTime OdemeTarihi { get; set; }
        public string OdemeDurumu { get; set; }

        public int ReservationId { get; set; }
        public Reservation Reservation { get; set; }
    }
}
