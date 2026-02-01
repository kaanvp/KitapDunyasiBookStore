using MySql.Data.MySqlClient;
using Veritabani_Odevi.DAL.Utilities;
using Veritabani_Odevi.Models;

namespace Veritabani_Odevi.Data
{
    public class SepetRepository
    {
        private readonly DatabaseConnection _db;
        public SepetRepository(DatabaseConnection db)
        {
            _db = db;
        }
        // CREATE - Sepete Ekle (Varsa adet artırır)
        public int SepeteEkle(SepetElemani sepetElemani)
        {
            string sql = @"
                INSERT INTO Sepet (KullaniciID, KitapID, Adet, EklenmeTarihi)
                VALUES (@KullaniciID, @KitapID, @Adet, NOW())
                ON DUPLICATE KEY UPDATE
                Adet = Adet + @Adet;
                ";

            using (var baglanti = _db.BaglantiyiAc())
            using (var komut = new MySqlCommand(sql, baglanti))
            {
                komut.Parameters.AddWithValue("@KullaniciID", sepetElemani.KullaniciID);
                komut.Parameters.AddWithValue("@KitapID", sepetElemani.KitapID);
                komut.Parameters.AddWithValue("@Adet", sepetElemani.Adet);

                return komut.ExecuteNonQuery();
            }
        }

        // READ - Kullanıcının sepetini getir
        public List<SepetElemani> SepetiGetir(int kullaniciID)
        {
            var liste = new List<SepetElemani>();

            string sql = @"
                SELECT 
                    s.SepetID,
                    s.KitapID,
                    s.Adet,
                    k.Baslik,
                    k.Yazar,
                    k.Fiyat,
                    k.KapakResmiUrl,
                    i.IndirimOrani,
                    CASE 
                        WHEN i.IndirimOrani IS NOT NULL AND i.Aktif = TRUE 
                             AND i.BaslangicTarihi <= NOW() AND i.BitisTarihi >= NOW()
                        THEN ROUND(k.Fiyat * (1 - i.IndirimOrani / 100), 2)
                        ELSE k.Fiyat
                    END AS IndirimliFiyat
                FROM Sepet s
                INNER JOIN Kitap k ON s.KitapID = k.KitapID
                LEFT JOIN KitapIndirim ki ON k.KitapID = ki.KitapID
                LEFT JOIN Indirim i ON ki.IndirimID = i.IndirimID 
                    AND i.Aktif = TRUE 
                    AND i.BaslangicTarihi <= NOW() 
                    AND i.BitisTarihi >= NOW()
                WHERE s.KullaniciID = @KullaniciID;
                ";

            using (var baglanti = _db.BaglantiyiAc())
            using (var komut = new MySqlCommand(sql, baglanti))
            {
                komut.Parameters.AddWithValue("@KullaniciID", kullaniciID);

                using (var okuyucu = komut.ExecuteReader())
                {
                    while (okuyucu.Read())
                    {
                        var orijinalFiyat = okuyucu.GetDecimal("Fiyat");
                        var indirimliFiyat = okuyucu.GetDecimal("IndirimliFiyat");
                        decimal? indirimOrani = okuyucu.IsDBNull(okuyucu.GetOrdinal("IndirimOrani"))
                            ? null
                            : okuyucu.GetDecimal("IndirimOrani");

                        liste.Add(new SepetElemani
                        {
                            SepetID = okuyucu.GetInt32("SepetID"),
                            KitapID = okuyucu.GetInt32("KitapID"),
                            Adet = okuyucu.GetInt32("Adet"),
                            OrijinalFiyat = orijinalFiyat,
                            IndirimliFiyat = indirimliFiyat,
                            IndirimOrani = indirimOrani,
                            kitap = new Kitap
                            {
                                KitapID = okuyucu.GetInt32("KitapID"),
                                Baslik = okuyucu.GetString("Baslik"),
                                Yazar = okuyucu.GetString("Yazar"),
                                Fiyat = orijinalFiyat,
                                KapakResmiUrl = okuyucu.IsDBNull(okuyucu.GetOrdinal("KapakResmiUrl"))
                                    ? "/books/no-cover.jpg"
                                    : okuyucu.GetString("KapakResmiUrl")
                            }
                        });
                    }
                }
            }

            return liste;
        }


        // DELETE - Sepetten ürün sil
        public bool SepettenSil(int kullaniciID, int kitapID)
        {
            string sql = @"
        DELETE FROM Sepet
        WHERE KullaniciID = @KullaniciID AND KitapID = @KitapID;
        ";

            using (var baglanti = _db.BaglantiyiAc())
            using (var komut = new MySqlCommand(sql, baglanti))
            {
                komut.Parameters.AddWithValue("@KullaniciID", kullaniciID);
                komut.Parameters.AddWithValue("@KitapID", kitapID);

                return komut.ExecuteNonQuery() > 0;
            }
        }

        // DELETE - Sepeti tamamen temizle
        public bool SepetiTemizle(int kullaniciID)
        {
            string sql = "DELETE FROM Sepet WHERE KullaniciID = @KullaniciID";

            using (var baglanti = _db.BaglantiyiAc())
            using (var komut = new MySqlCommand(sql, baglanti))
            {
                komut.Parameters.AddWithValue("@KullaniciID", kullaniciID);
                return komut.ExecuteNonQuery() > 0;
            }
        }

        // UPDATE - Adet güncelle
        public bool AdetGuncelle(int kullaniciID, int kitapID, int yeniAdet)
        {
            if (yeniAdet <= 0)
            {
                return SepettenSil(kullaniciID, kitapID);
            }

            string sql = @"
                UPDATE Sepet 
                SET Adet = @Adet 
                WHERE KullaniciID = @KullaniciID AND KitapID = @KitapID";

            using (var baglanti = _db.BaglantiyiAc())
            using (var komut = new MySqlCommand(sql, baglanti))
            {
                komut.Parameters.AddWithValue("@Adet", yeniAdet);
                komut.Parameters.AddWithValue("@KullaniciID", kullaniciID);
                komut.Parameters.AddWithValue("@KitapID", kitapID);

                return komut.ExecuteNonQuery() > 0;
            }
        }
    }
}
