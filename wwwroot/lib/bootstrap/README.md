# TinyHouse Rezervasyon Sistemi

Bu proje, Tiny House konseptine sahip evlerin kiralanabileceği bir web uygulamasıdır. Kullanıcılar evleri görüntüleyebilir, rezervasyon yapabilir ve ödeme gerçekleştirebilir.

## Özellikler

- 🔑 Rol bazlı giriş: Admin, Ev Sahibi, Kiracı
- 🏠 İlan Ekleme, Görüntüleme, Filtreleme
- 📅 Takvim üzerinden dolu tarihler
- 💸 Online ödeme ve bakiye sistemi
- 🔁 Geri ödeme & rezervasyon iptali
- ⭐ Puanlama ve yorum
- ⚙️ SQL Trigger, Function, SP kullanımı

## Kullanılan Teknolojiler

- ASP.NET Core MVC
- Entity Framework Core
- MSSQL + LocalDB
- Bootstrap 5 (Dark tema)
- FullCalendar.js

## Veritabanı Özeti

- 6 ana tablo: Users, TinyHouses, Reservations, Payments, Reviews, HouseImages
- 2 Trigger, 2 Function, 2 Stored Procedure
- ER Diyagram eklidir (png)

## Kurulum

1. Projeyi klonla:
    ```bash
    git clone https://github.com/burakaliakbulut/TinyHouse.git
    ```
2. `appsettings.json` içinde connection string’i kendi SQL Server'ına göre ayarla  
3. Migration varsa çalıştır:
    ```bash
    Update-Database
    ```

## Geliştirici

> Proje: Burak Ali tarafından yapılmıştır 👊  
> Okul: Celal Bayar Üniversitesi  
> Ders: Veri Tabanı Yönetim Sistemleri
