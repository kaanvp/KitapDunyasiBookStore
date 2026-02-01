using MySql.Data.MySqlClient;
using System.Text;
using Veritabani_Odevi.DAL.Utilities;
using Veritabani_Odevi.DTOs;
using Veritabani_Odevi.Models;
using Veritabani_Odevi.Models.ViewModels;

namespace Veritabani_Odevi.Data
{
    public class KitapRepository
    {
        private readonly DatabaseConnection _db;
        public KitapRepository(DatabaseConnection db)
        {
            _db = db;
        }

        // CREATE - Kitap Ekleme
        public int KitapEkle(Kitap kitap, List<int> kategoriIDler)
        {
            int yeniKitapID = 0;

            using (var baglanti = _db.BaglantiyiAc())
            using (var transaction = baglanti.BeginTransaction())
            {
                try
                {
                    // 1. Kitap ekleme
                    string sqlKitap = @"
                    INSERT INTO Kitap (Baslik, Aciklama, Yazar, YayinEvi, ISBN, 
                                       Fiyat, StokAdedi, Durum, KapakResmiUrl, SaticiKullaniciID)
                    VALUES (@Baslik, @Aciklama, @Yazar, @YayinEvi, @ISBN, 
                            @Fiyat, @StokAdedi, @Durum, @KapakResmiUrl, @SaticiID);
                    SELECT LAST_INSERT_ID();";

                    using (var komut = new MySqlCommand(sqlKitap, baglanti, transaction))
                    {
                        komut.Parameters.AddWithValue("@Baslik", kitap.Baslik);
                        komut.Parameters.AddWithValue("@Aciklama",
                            string.IsNullOrEmpty(kitap.Aciklama) ? (object)DBNull.Value : kitap.Aciklama);
                        komut.Parameters.AddWithValue("@Yazar",
                            string.IsNullOrEmpty(kitap.Yazar) ? (object)DBNull.Value : kitap.Yazar);
                        komut.Parameters.AddWithValue("@YayinEvi",
                            string.IsNullOrEmpty(kitap.YayinEvi) ? (object)DBNull.Value : kitap.YayinEvi);
                        komut.Parameters.AddWithValue("@ISBN",
                            string.IsNullOrEmpty(kitap.ISBN) ? (object)DBNull.Value : kitap.ISBN);
                        komut.Parameters.AddWithValue("@Fiyat", kitap.Fiyat);
                        komut.Parameters.AddWithValue("@StokAdedi", kitap.StokAdedi);
                        komut.Parameters.AddWithValue("@Durum", kitap.Durum);
                        komut.Parameters.AddWithValue("@KapakResmiUrl",
                            string.IsNullOrEmpty(kitap.KapakResmiUrl) ? (object)DBNull.Value : kitap.KapakResmiUrl);
                        komut.Parameters.AddWithValue("@SaticiID", kitap.SaticiKullaniciID);

                        yeniKitapID = Convert.ToInt32(komut.ExecuteScalar());
                    }

                    // 2. Kategorileri ilişkilendirme
                    if (kategoriIDler != null && kategoriIDler.Any())
                    {
                        string sqlKategori = @"
                        INSERT INTO KitapKategori (KitapID, KategoriID) 
                        VALUES (@KitapID, @KategoriID)";

                        foreach (var kategoriID in kategoriIDler)
                        {
                            using (var komut = new MySqlCommand(sqlKategori, baglanti, transaction))
                            {
                                komut.Parameters.AddWithValue("@KitapID", yeniKitapID);
                                komut.Parameters.AddWithValue("@KategoriID", kategoriID);
                                komut.ExecuteNonQuery();
                            }
                        }
                    }

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }

            return yeniKitapID;
        }
        // READ - Tüm Kitapları Getir (JOIN ile) - Admin panel için stok kontrolü olmadan
        public List<Kitap> TumKitaplariGetir()
        {
            var liste = new List<Kitap>();
            string sql = @"
            SELECT k.*, 
                   CONCAT(u.Ad, ' ', u.Soyad) AS SaticiAdSoyad,
                   u.Derecelendirme AS SaticiPuan
            FROM Kitap k
            INNER JOIN Kullanici u ON k.SaticiKullaniciID = u.KullaniciID
            ORDER BY k.EklenmeTarihi DESC";

            using (var baglanti = _db.BaglantiyiAc())
            using (var komut = new MySqlCommand(sql, baglanti))
            using (var okuyucu = komut.ExecuteReader())
            {
                while (okuyucu.Read())
                {
                    string kapakUrl;

                    if (okuyucu.IsDBNull(okuyucu.GetOrdinal("KapakResmiUrl")))
                    {
                        kapakUrl = "/images/no-cover.jpg";
                    }
                    else
                    {
                        kapakUrl = okuyucu.GetString("KapakResmiUrl");

                        // 🔒 Güvenlik: eski kayıtlarda books yoksa ekle
                        if (!kapakUrl.StartsWith("/books/"))
                        {
                            kapakUrl = "/books/" + kapakUrl.TrimStart('/');
                        }
                    }
                    var kitap = new Kitap
                    {
                        KitapID = okuyucu.GetInt32("KitapID"),
                        Baslik = okuyucu.GetString("Baslik"),
                        Aciklama = okuyucu.IsDBNull(okuyucu.GetOrdinal("Aciklama"))
                        ? null : okuyucu.GetString("Aciklama"),
                                        Yazar = okuyucu.IsDBNull(okuyucu.GetOrdinal("Yazar"))
                        ? null : okuyucu.GetString("Yazar"),
                        Fiyat = okuyucu.GetDecimal("Fiyat"),
                        StokAdedi = okuyucu.GetInt32("StokAdedi"),
                        KapakResmiUrl = kapakUrl,
                        SaticiKullaniciID = okuyucu.GetInt32("SaticiKullaniciID")
                    };

                    // Durum alanını string → enum dönüştür
                    string durumStr = okuyucu.GetString("Durum");
                    if (Enum.TryParse<DurumEnum>(durumStr, out var durumValue))
                        kitap.Durum = durumValue;
                    else
                        kitap.Durum = DurumEnum.Iyi; // default değer

                    liste.Add(kitap);
                }
            }
            return liste;
        }
        // ID'ye göre kitap getir
        public Kitap KitapGetir(int kitapID)
        {
            string sql = @"
            SELECT * FROM Kitap 
            WHERE KitapID = @KitapID";
            using (var baglanti = _db.BaglantiyiAc())
            using (var komut = new MySqlCommand(sql, baglanti))
            {
                komut.Parameters.AddWithValue("@KitapID", kitapID);
                using (var okuyucu = komut.ExecuteReader())
                {
                    if (okuyucu.Read())
                    {
                        return KitapOkuyucudanOlustur(okuyucu);
                    }
                }
            }
            return null;
        }
        // UPDATE - Kitap Güncelleme
        public bool KitapGuncelle(Kitap kitap)
        {
            string sql = @"
            UPDATE Kitap 
            SET Baslik = @Baslik,
                Aciklama = @Aciklama,
                Yazar = @Yazar,
                Fiyat = @Fiyat,
                StokAdedi = @StokAdedi,
                Durum = @Durum,
                KapakResmiUrl = @KapakResmiUrl
            WHERE KitapID = @KitapID 
            AND SaticiKullaniciID = @SaticiID"; // Güvenlik kontrolü

            using (var baglanti = _db.BaglantiyiAc())
            using (var komut = new MySqlCommand(sql, baglanti))
            {
                komut.Parameters.AddWithValue("@KitapID", kitap.KitapID);
                komut.Parameters.AddWithValue("@Baslik", kitap.Baslik);
                komut.Parameters.AddWithValue("@Aciklama",
                    string.IsNullOrEmpty(kitap.Aciklama) ? (object)DBNull.Value : kitap.Aciklama);
                komut.Parameters.AddWithValue("@Yazar",
                    string.IsNullOrEmpty(kitap.Yazar) ? (object)DBNull.Value : kitap.Yazar);
                komut.Parameters.AddWithValue("@Fiyat", kitap.Fiyat);
                komut.Parameters.AddWithValue("@StokAdedi", kitap.StokAdedi);
                komut.Parameters.AddWithValue("@Durum", kitap.Durum);
                komut.Parameters.AddWithValue("@KapakResmiUrl",
                    string.IsNullOrEmpty(kitap.KapakResmiUrl) ? (object)DBNull.Value : kitap.KapakResmiUrl);
                komut.Parameters.AddWithValue("@SaticiID", kitap.SaticiKullaniciID);

                return komut.ExecuteNonQuery() > 0;
            }
        }
        // DELETE - Soft Delete (Stok 0 yapma)
        public bool KitapSil(int kitapID, int saticiID)
        {
            string sql = @"
            UPDATE Kitap 
            SET StokAdedi = 0 
            WHERE KitapID = @KitapID 
            AND SaticiKullaniciID = @SaticiID";

            using (var baglanti = _db.BaglantiyiAc())
            using (var komut = new MySqlCommand(sql, baglanti))
            {
                komut.Parameters.AddWithValue("@KitapID", kitapID);
                komut.Parameters.AddWithValue("@SaticiID", saticiID);

                return komut.ExecuteNonQuery() > 0;
            }
        }
        // 5.3.1 Dinamik Sorgu 1: Gelişmiş Kitap Arama
        public List<Kitap> GelismisArama(
            string aramaKelimesi = null,
            List<int> kategoriIDler = null,
            decimal? minFiyat = null,
            decimal? maxFiyat = null,
            string durum = null,
            string siralamaKriteri = "YenidenEskiye")
        {
            var kitaplar = new List<Kitap>();
            var sqlBuilder = new StringBuilder(@"
                SELECT DISTINCT k.*, 
                       CONCAT(u.Ad, ' ', u.Soyad) AS SaticiAdSoyad
                FROM Kitap k
                INNER JOIN Kullanici u ON k.SaticiKullaniciID = u.KullaniciID
                LEFT JOIN KitapKategori kk ON k.KitapID = kk.KitapID
                WHERE k.StokAdedi > 0");

            var parametreler = new List<MySqlParameter>();

            // Dinamik koşul ekleme
            if (!string.IsNullOrWhiteSpace(aramaKelimesi))
            {
                sqlBuilder.Append(@" AND (k.Baslik LIKE @Arama 
                             OR k.Yazar LIKE @Arama 
                             OR k.Aciklama LIKE @Arama)");
                parametreler.Add(new MySqlParameter("@Arama", $"%{aramaKelimesi}%"));
            }

            if (minFiyat.HasValue)
            {
                sqlBuilder.Append(" AND k.Fiyat >= @MinFiyat");
                parametreler.Add(new MySqlParameter("@MinFiyat", minFiyat.Value));
            }

            if (maxFiyat.HasValue)
            {
                sqlBuilder.Append(" AND k.Fiyat <= @MaxFiyat");
                parametreler.Add(new MySqlParameter("@MaxFiyat", maxFiyat.Value));
            }

            if (!string.IsNullOrWhiteSpace(durum))
            {
                sqlBuilder.Append(" AND k.Durum = @Durum");
                parametreler.Add(new MySqlParameter("@Durum", durum));
            }

            if (kategoriIDler != null && kategoriIDler.Any())
            {
                var kategoriPlaceholders = string.Join(",",
                    kategoriIDler.Select((_, i) => $"@Kat{i}"));
                sqlBuilder.Append($" AND kk.KategoriID IN ({kategoriPlaceholders})");

                for (int i = 0; i < kategoriIDler.Count; i++)
                {
                    parametreler.Add(new MySqlParameter($"@Kat{i}", kategoriIDler[i]));
                }
            }

            // Dinamik sıralama
            sqlBuilder.Append(siralamaKriteri switch
            {
                "FiyatArtan" => " ORDER BY k.Fiyat ASC",
                "FiyatAzalan" => " ORDER BY k.Fiyat DESC",
                "EskidenYeniye" => " ORDER BY k.EklenmeTarihi ASC",
                _ => " ORDER BY k.EklenmeTarihi DESC"
            });

            using (var baglanti = _db.BaglantiyiAc())
            using (var komut = new MySqlCommand(sqlBuilder.ToString(), baglanti))
            {
                komut.Parameters.AddRange(parametreler.ToArray());

                using (var okuyucu = komut.ExecuteReader())
                {
                    while (okuyucu.Read())
                    {
                        kitaplar.Add(KitapOkuyucudanOlustur(okuyucu));
                    }
                }
            }
            return kitaplar;
        }

        // Gelişmiş arama - indirimli fiyatlarla
        public List<KitapListeDTO> GelismisAramaIndirimli(
            string aramaKelimesi = null,
            List<int> kategoriIDler = null,
            decimal? minFiyat = null,
            decimal? maxFiyat = null,
            string durum = null,
            string siralamaKriteri = "YenidenEskiye")
        {
            var kitaplar = new List<KitapListeDTO>();
            var sqlBuilder = new StringBuilder(@"
                SELECT 
                    k.KitapID,
                    k.Baslik,
                    k.Yazar,
                    k.Fiyat AS OrijinalFiyat,
                    k.StokAdedi,
                    k.KapakResmiUrl,
                    k.Durum,
                    k.SaticiKullaniciID,
                    k.EklenmeTarihi,
                    (SELECT MAX(i2.IndirimOrani) 
                     FROM KitapIndirim ki2 
                     INNER JOIN Indirim i2 ON ki2.IndirimID = i2.IndirimID 
                     WHERE ki2.KitapID = k.KitapID 
                       AND i2.Aktif = TRUE 
                       AND i2.BaslangicTarihi <= NOW() 
                       AND i2.BitisTarihi >= NOW()
                    ) AS IndirimOrani,
                    CASE 
                        WHEN (SELECT MAX(i2.IndirimOrani) 
                              FROM KitapIndirim ki2 
                              INNER JOIN Indirim i2 ON ki2.IndirimID = i2.IndirimID 
                              WHERE ki2.KitapID = k.KitapID 
                                AND i2.Aktif = TRUE 
                                AND i2.BaslangicTarihi <= NOW() 
                                AND i2.BitisTarihi >= NOW()) IS NOT NULL
                        THEN ROUND(k.Fiyat * (1 - (SELECT MAX(i2.IndirimOrani) 
                              FROM KitapIndirim ki2 
                              INNER JOIN Indirim i2 ON ki2.IndirimID = i2.IndirimID 
                              WHERE ki2.KitapID = k.KitapID 
                                AND i2.Aktif = TRUE 
                                AND i2.BaslangicTarihi <= NOW() 
                                AND i2.BitisTarihi >= NOW()) / 100), 2)
                        ELSE k.Fiyat
                    END AS IndirimliFiyat
                FROM Kitap k
                INNER JOIN Kullanici u ON k.SaticiKullaniciID = u.KullaniciID
                LEFT JOIN KitapKategori kk ON k.KitapID = kk.KitapID
                WHERE k.StokAdedi > 0");

            var parametreler = new List<MySqlParameter>();

            if (!string.IsNullOrWhiteSpace(aramaKelimesi))
            {
                sqlBuilder.Append(@" AND (k.Baslik LIKE @Arama 
                             OR k.Yazar LIKE @Arama 
                             OR k.Aciklama LIKE @Arama)");
                parametreler.Add(new MySqlParameter("@Arama", $"%{aramaKelimesi}%"));
            }

            if (minFiyat.HasValue)
            {
                sqlBuilder.Append(" AND k.Fiyat >= @MinFiyat");
                parametreler.Add(new MySqlParameter("@MinFiyat", minFiyat.Value));
            }

            if (maxFiyat.HasValue)
            {
                sqlBuilder.Append(" AND k.Fiyat <= @MaxFiyat");
                parametreler.Add(new MySqlParameter("@MaxFiyat", maxFiyat.Value));
            }

            if (!string.IsNullOrWhiteSpace(durum))
            {
                sqlBuilder.Append(" AND k.Durum = @Durum");
                parametreler.Add(new MySqlParameter("@Durum", durum));
            }

            if (kategoriIDler != null && kategoriIDler.Any())
            {
                var kategoriPlaceholders = string.Join(",",
                    kategoriIDler.Select((_, i) => $"@Kat{i}"));
                sqlBuilder.Append($" AND kk.KategoriID IN ({kategoriPlaceholders})");

                for (int i = 0; i < kategoriIDler.Count; i++)
                {
                    parametreler.Add(new MySqlParameter($"@Kat{i}", kategoriIDler[i]));
                }
            }

            // GROUP BY ekle - KitapKategori join'i duplicate yapabilir
            sqlBuilder.Append(" GROUP BY k.KitapID");

            sqlBuilder.Append(siralamaKriteri switch
            {
                "FiyatArtan" => " ORDER BY IndirimliFiyat ASC",
                "FiyatAzalan" => " ORDER BY IndirimliFiyat DESC",
                "EskidenYeniye" => " ORDER BY k.EklenmeTarihi ASC",
                _ => " ORDER BY k.EklenmeTarihi DESC"
            });

            using (var baglanti = _db.BaglantiyiAc())
            using (var komut = new MySqlCommand(sqlBuilder.ToString(), baglanti))
            {
                komut.Parameters.AddRange(parametreler.ToArray());

                using (var okuyucu = komut.ExecuteReader())
                {
                    while (okuyucu.Read())
                    {
                        string kapakUrl = okuyucu.IsDBNull(okuyucu.GetOrdinal("KapakResmiUrl"))
                            ? "/images/no-cover.jpg"
                            : okuyucu.GetString("KapakResmiUrl");

                        if (!string.IsNullOrEmpty(kapakUrl) && !kapakUrl.StartsWith("/books/") && !kapakUrl.StartsWith("/images/"))
                        {
                            kapakUrl = "/books/" + kapakUrl.TrimStart('/');
                        }

                        // Durum alanı boş olabilir, varsayılan değer ata
                        string durumDeger = "Iyi";
                        if (!okuyucu.IsDBNull(okuyucu.GetOrdinal("Durum")))
                        {
                            string durumVal = okuyucu.GetString("Durum");
                            if (!string.IsNullOrEmpty(durumVal))
                            {
                                durumDeger = durumVal;
                            }
                        }

                        kitaplar.Add(new KitapListeDTO
                        {
                            KitapID = okuyucu.GetInt32("KitapID"),
                            Baslik = okuyucu.GetString("Baslik"),
                            Yazar = okuyucu.IsDBNull(okuyucu.GetOrdinal("Yazar"))
                                ? "Bilinmiyor" : okuyucu.GetString("Yazar"),
                            OrijinalFiyat = okuyucu.GetDecimal("OrijinalFiyat"),
                            IndirimOrani = okuyucu.IsDBNull(okuyucu.GetOrdinal("IndirimOrani"))
                                ? null : okuyucu.GetDecimal("IndirimOrani"),
                            IndirimliFiyat = okuyucu.GetDecimal("IndirimliFiyat"),
                            StokAdedi = okuyucu.GetInt32("StokAdedi"),
                            KapakResmiUrl = kapakUrl,
                            Durum = durumDeger,
                            SaticiKullaniciID = okuyucu.GetInt32("SaticiKullaniciID")
                        });
                    }
                }
            }
            return kitaplar;
        }

        // Complex join 2
        public List<IndirimdekiKitapDTO> AktifIndirimdekiKitaplar()
        {
            var liste = new List<IndirimdekiKitapDTO>();
            string sql = @"
            SELECT 
                k.KitapID,
                k.Baslik,
                k.Yazar,
                k.Fiyat AS OrijinalFiyat,
                i.IndirimOrani,
                ROUND(k.Fiyat * (1 - i.IndirimOrani / 100), 2) AS IndirimliFiyat,
                ROUND(k.Fiyat * (i.IndirimOrani / 100), 2) AS IndirilenTutar,
                i.IndirimAdi,
                i.BitisTarihi,
                GROUP_CONCAT(kat.KategoriAdi SEPARATOR ', ') AS Kategoriler,
                CONCAT(u.Ad, ' ', u.Soyad) AS SaticiAdSoyad,
                k.StokAdedi,
                k.KapakResmiUrl
            FROM Kitap k
            INNER JOIN KitapIndirim ki ON k.KitapID = ki.KitapID
            INNER JOIN Indirim i ON ki.IndirimID = i.IndirimID
            INNER JOIN Kullanici u ON k.SaticiKullaniciID = u.KullaniciID
            LEFT JOIN KitapKategori kk ON k.KitapID = kk.KitapID
            LEFT JOIN Kategori kat ON kk.KategoriID = kat.KategoriID
            WHERE i.Aktif = TRUE
                AND i.BaslangicTarihi <= NOW()
                AND i.BitisTarihi >= NOW()
                AND k.StokAdedi > 0
            GROUP BY k.KitapID
            ORDER BY i.IndirimOrani DESC";

            using (var baglanti = _db.BaglantiyiAc())
            using (var komut = new MySqlCommand(sql, baglanti))
            using (var okuyucu = komut.ExecuteReader())
            {
                while (okuyucu.Read())
                {
                    liste.Add(new IndirimdekiKitapDTO
                    {
                        KitapID = okuyucu.GetInt32("KitapID"),
                        Baslik = okuyucu.GetString("Baslik"),
                        Yazar = okuyucu.IsDBNull(okuyucu.GetOrdinal("Yazar"))
                            ? "Bilinmiyor" : okuyucu.GetString("Yazar"),
                        OrijinalFiyat = okuyucu.GetDecimal("OrijinalFiyat"),
                        IndirimOrani = okuyucu.GetDecimal("IndirimOrani"),
                        IndirimliFiyat = okuyucu.GetDecimal("IndirimliFiyat"),
                        IndirilenTutar = okuyucu.GetDecimal("IndirilenTutar"),
                        IndirimAdi = okuyucu.GetString("IndirimAdi"),
                        BitisTarihi = okuyucu.GetDateTime("BitisTarihi"),
                        Kategoriler = okuyucu.IsDBNull(okuyucu.GetOrdinal("Kategoriler"))
                            ? "Kategorisiz" : okuyucu.GetString("Kategoriler"),
                        SaticiAdSoyad = okuyucu.GetString("SaticiAdSoyad"),
                        StokAdedi = okuyucu.GetInt32("StokAdedi"),
                        KapakResmiUrl = okuyucu.IsDBNull(okuyucu.GetOrdinal("KapakResmiUrl"))
                            ? "/images/no-cover.jpg" : okuyucu.GetString("KapakResmiUrl")
                    });
                }
            }

            return liste;
        }
        // View
        public List<KitapDetayViewModel> AktifKitaplariGetir()
        {
            var liste = new List<KitapDetayViewModel>();
            string sql = "SELECT * FROM VW_AktifKitaplarDetayli ORDER BY EklenmeTarihi DESC";

            using (var baglanti = _db.BaglantiyiAc())
            using (var komut = new MySqlCommand(sql, baglanti))
            using (var okuyucu = komut.ExecuteReader())
            {
                while (okuyucu.Read())
                {
                    liste.Add(new KitapDetayViewModel
                    {
                        KitapID = okuyucu.GetInt32("KitapID"),
                        Baslik = okuyucu.GetString("Baslik"),
                        Yazar = okuyucu.IsDBNull(okuyucu.GetOrdinal("Yazar"))
                            ? null : okuyucu.GetString("Yazar"),
                        Fiyat = okuyucu.GetDecimal("Fiyat"),
                        GecerliFiyat = okuyucu.GetDecimal("GecerliFiyat"),
                        SaticiAdSoyad = okuyucu.GetString("SaticiAdSoyad"),
                        SaticiPuan = okuyucu.GetDecimal("SaticiPuan"),
                        Kategoriler = okuyucu.GetString("Kategoriler"),
                        OrtalamaYorumPuan = okuyucu.GetDecimal("OrtalamaYorumPuan"),
                        YorumSayisi = okuyucu.GetInt32("YorumSayisi"),
                        IndirimAdi = okuyucu.IsDBNull(okuyucu.GetOrdinal("IndirimAdi"))
                            ? null : okuyucu.GetString("IndirimAdi"),
                        IndirimOrani = okuyucu.IsDBNull(okuyucu.GetOrdinal("IndirimOrani"))
                            ? (decimal?)null : okuyucu.GetDecimal("IndirimOrani")
                    });
                }
            }

            return liste;
        }
        private Kitap KitapOkuyucudanOlustur(MySqlDataReader okuyucu)
        {
            int kapakIndex = okuyucu.GetOrdinal("KapakResmiUrl");
            
            var kitap = new Kitap
            {
                KitapID = okuyucu.GetInt32("KitapID"),
                Baslik = okuyucu.GetString("Baslik"),
                Aciklama = okuyucu.IsDBNull(okuyucu.GetOrdinal("Aciklama"))
                    ? null : okuyucu.GetString("Aciklama"),
                Yazar = okuyucu.IsDBNull(okuyucu.GetOrdinal("Yazar"))
                    ? null : okuyucu.GetString("Yazar"),
                YayinEvi = okuyucu.IsDBNull(okuyucu.GetOrdinal("YayinEvi"))
                    ? null : okuyucu.GetString("YayinEvi"),
                ISBN = okuyucu.IsDBNull(okuyucu.GetOrdinal("ISBN"))
                    ? null : okuyucu.GetString("ISBN"),
                Fiyat = okuyucu.GetDecimal("Fiyat"),
                StokAdedi = okuyucu.GetInt32("StokAdedi"),
                KapakResmiUrl = okuyucu.IsDBNull(kapakIndex)
                    ? "/images/no-cover.jpg" : okuyucu.GetString(kapakIndex),
                SaticiKullaniciID = okuyucu.GetInt32("SaticiKullaniciID")
            };
            // Durum alanını string → enum dönüştür
            string durumStr = okuyucu.GetString("Durum");
            if (Enum.TryParse<DurumEnum>(durumStr, out var durumValue))
                kitap.Durum = durumValue;
            else
                kitap.Durum = DurumEnum.Iyi; // default değer
            return kitap;
        }

        // Tüm kitapları indirim bilgisiyle birlikte getir
        public List<KitapListeDTO> TumKitaplariIndirimliGetir()
        {
            var liste = new List<KitapListeDTO>();
            string sql = @"
            SELECT 
                k.KitapID,
                k.Baslik,
                k.Yazar,
                k.Fiyat AS OrijinalFiyat,
                k.StokAdedi,
                k.KapakResmiUrl,
                k.Durum,
                k.SaticiKullaniciID,
                MAX(i.IndirimOrani) AS IndirimOrani,
                CASE 
                    WHEN MAX(i.IndirimOrani) IS NOT NULL 
                    THEN ROUND(k.Fiyat * (1 - MAX(i.IndirimOrani) / 100), 2)
                    ELSE k.Fiyat
                END AS IndirimliFiyat
            FROM Kitap k
            LEFT JOIN KitapIndirim ki ON k.KitapID = ki.KitapID
            LEFT JOIN Indirim i ON ki.IndirimID = i.IndirimID 
                AND i.Aktif = TRUE 
                AND i.BaslangicTarihi <= NOW() 
                AND i.BitisTarihi >= NOW()
            WHERE k.StokAdedi > 0
            GROUP BY k.KitapID, k.Baslik, k.Yazar, k.Fiyat, k.StokAdedi, 
                     k.KapakResmiUrl, k.Durum, k.SaticiKullaniciID, k.EklenmeTarihi
            ORDER BY k.EklenmeTarihi DESC";

            using (var baglanti = _db.BaglantiyiAc())
            using (var komut = new MySqlCommand(sql, baglanti))
            using (var okuyucu = komut.ExecuteReader())
            {
                while (okuyucu.Read())
                {
                    string kapakUrl = okuyucu.IsDBNull(okuyucu.GetOrdinal("KapakResmiUrl"))
                        ? "/images/no-cover.jpg"
                        : okuyucu.GetString("KapakResmiUrl");
                    
                    if (!string.IsNullOrEmpty(kapakUrl) && !kapakUrl.StartsWith("/books/") && !kapakUrl.StartsWith("/images/"))
                    {
                        kapakUrl = "/books/" + kapakUrl.TrimStart('/');
                    }

                    // Durum alanı boş olabilir, varsayılan değer ata
                    string durum = "Iyi";
                    if (!okuyucu.IsDBNull(okuyucu.GetOrdinal("Durum")))
                    {
                        string durumVal = okuyucu.GetString("Durum");
                        if (!string.IsNullOrEmpty(durumVal))
                        {
                            durum = durumVal;
                        }
                    }

                    liste.Add(new KitapListeDTO
                    {
                        KitapID = okuyucu.GetInt32("KitapID"),
                        Baslik = okuyucu.GetString("Baslik"),
                        Yazar = okuyucu.IsDBNull(okuyucu.GetOrdinal("Yazar"))
                            ? "Bilinmiyor" : okuyucu.GetString("Yazar"),
                        OrijinalFiyat = okuyucu.GetDecimal("OrijinalFiyat"),
                        IndirimOrani = okuyucu.IsDBNull(okuyucu.GetOrdinal("IndirimOrani"))
                            ? null : okuyucu.GetDecimal("IndirimOrani"),
                        IndirimliFiyat = okuyucu.GetDecimal("IndirimliFiyat"),
                        StokAdedi = okuyucu.GetInt32("StokAdedi"),
                        KapakResmiUrl = kapakUrl,
                        Durum = durum,
                        SaticiKullaniciID = okuyucu.GetInt32("SaticiKullaniciID")
                    });
                }
            }

            return liste;
        }
    }
}