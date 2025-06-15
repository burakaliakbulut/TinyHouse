using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TinyHouse.Models
{
    public class Reservation
    {
        public int Id { get; set; }
        public DateTime BaslangicTarihi { get; set; }
        public DateTime BitisTarihi { get; set; }
        public string Durum { get; set; }
        public DateTime OlusturmaTarihi { get; set; }

        public string KiraciId { get; set; }
        public User Kiraci { get; set; }

        public int HouseId { get; set; }
        public House House { get; set; }

        public Review Yorum { get; set; }
        public Payment Odeme { get; set; }
    }
}
