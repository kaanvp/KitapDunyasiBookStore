using MySql.Data.MySqlClient;
using Veritabani_Odevi.DAL.Utilities;
using Veritabani_Odevi.DTOs;
using Veritabani_Odevi.Models;

namespace Veritabani_Odevi.Data
{
    public class KullaniciRaporRepository
    {
        private readonly DatabaseConnection _db;
        public KullaniciRaporRepository(DatabaseConnection db)
        {
            _db = db;
        }
        // 5.4.2 Aggregate Sorgu 2: En Aktif Kullanıcılar(En Çok Yorum Yapanlar)
        public List<KullaniciAktiviteRaporu> EnAktifKullanicilar(int minYorumSayisi = 2)
        {
            var liste = new List<KullaniciAktiviteRaporu>();
            string sql = @"
            SELECT 
            CONCAT(k.Ad, ' ', k.Soyad) AS AdSoyad,
            k.Eposta,
            COUNT(y.YorumID) AS YorumSayisi,
            AVG(y.Puan) AS OrtalamaVerdigiPuan,
            k.Derecelendirme AS AlinanOrtalamaPuan,
            (SELECT COUNT(*) FROM Kitap WHERE SaticiKullaniciID = k.KullaniciID AND StokAdedi > 0) AS SatistakiKitapSayisi
            FROM Kullanici k
            INNER JOIN Yorum y ON k.KullaniciID = y.KullaniciID
            WHERE y.OnayDurumu = TRUE
            GROUP BY k.KullaniciID, k.Ad, k.Soyad, k.Eposta, k.Derecelendirme
            HAVING YorumSayisi >= @MinYorum
            ORDER BY YorumSayisi DESC
            LIMIT 20";

            using (var baglanti = _db.BaglantiyiAc())
            using (var komut = new MySqlCommand(sql, baglanti))
            {
                komut.Parameters.AddWithValue("@MinYorum", minYorumSayisi);

                using (var okuyucu = komut.ExecuteReader())
                {
                    while (okuyucu.Read())
                    {
                        liste.Add(new KullaniciAktiviteRaporu
                        {
                            AdSoyad = okuyucu.GetString("AdSoyad"),
                            Eposta = okuyucu.GetString("Eposta"),
                            YorumSayisi = okuyucu.GetInt32("YorumSayisi"),
                            OrtalamaVerdigiPuan = okuyucu.GetDecimal("OrtalamaVerdigiPuan"),
                            AlinanOrtalamaPuan = okuyucu.GetDecimal("AlinanOrtalamaPuan"),
                            SatistakiKitapSayisi = okuyucu.GetInt32("SatistakiKitapSayisi")
                        });
                    }
                }
            }
            return liste;
        }
    }
}
