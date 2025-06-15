CREATE TABLE TinyHouses (
    Id INT PRIMARY KEY IDENTITY,
    Baslik NVARCHAR(100),
    Aciklama NVARCHAR(MAX),
    Fiyat DECIMAL(18,2),
    Konum NVARCHAR(100),
    Durum NVARCHAR(50),
    OlusturmaTarihi DATETIME,
    EvSahibiId NVARCHAR(450)
);
