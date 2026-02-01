using MySql.Data.MySqlClient;
using Veritabani_Odevi.DAL.Utilities;
using Veritabani_Odevi.DTOs;

namespace Veritabani_Odevi.Data
{
    public class RaporRepository
    {
        private readonly DatabaseConnection _db;
        public RaporRepository(DatabaseConnection db)
        {
            _db = db;
        }
        // 5.4.1 Aggregate Sorgu 1: Kategori Bazlı Satış İstatistikleri
        public List<KategoriIstatistik> KategoriBazliSatisRaporu()
        {
            var liste = new List<KategoriIstatistik>();
            string sql = @"
        SELECT 
            k.KategoriAdi,
            COUNT(DISTINCT sd.SiparisDetayID) AS ToplamSatisAdedi,
            SUM(sd.Adet) AS ToplamKitapAdedi,
            SUM(sd.Adet * sd.BirimFiyat) AS ToplamCiro,
            COALESCE(AVG(y.Puan), 0) AS OrtalamaPuan,
            COUNT(DISTINCT kit.KitapID) AS KategoriKitapSayisi
        FROM Kategori k
        LEFT JOIN KitapKategori kk ON k.KategoriID = kk.KategoriID
        LEFT JOIN Kitap kit ON kk.KitapID = kit.KitapID
        LEFT JOIN SiparisDetay sd ON kit.KitapID = sd.KitapID
        LEFT JOIN Yorum y ON kit.KitapID = y.KitapID AND y.OnayDurumu = TRUE
        GROUP BY k.KategoriID, k.KategoriAdi
        ORDER BY ToplamCiro DESC";

            using (var baglanti = _db.BaglantiyiAc())
            using (var komut = new MySqlCommand(sql, baglanti))
            using (var okuyucu = komut.ExecuteReader())
            {
                while (okuyucu.Read())
                {
                    liste.Add(new KategoriIstatistik
                    {
                        KategoriAdi = okuyucu.GetString("KategoriAdi"),
                        ToplamSatisAdedi = okuyucu.GetInt32("ToplamSatisAdedi"),
                        ToplamKitapAdedi = okuyucu.IsDBNull(okuyucu.GetOrdinal("ToplamKitapAdedi"))
                            ? 0 : okuyucu.GetInt32("ToplamKitapAdedi"),
                        ToplamCiro = okuyucu.IsDBNull(okuyucu.GetOrdinal("ToplamCiro"))
                            ? 0 : okuyucu.GetDecimal("ToplamCiro"),
                        OrtalamaPuan = okuyucu.GetDecimal("OrtalamaPuan"),
                        KategoriKitapSayisi = okuyucu.GetInt32("KategoriKitapSayisi")
                    });
                }
            }

            return liste;
        }
        // View
        public List<KategoriYorumIstatistik> KategoriYorumRaporu()
        {
            var liste = new List<KategoriYorumIstatistik>();
            string sql = "SELECT * FROM VW_KategoriYorumIstatistikleri";

            using (var baglanti = _db.BaglantiyiAc())
            using (var komut = new MySqlCommand(sql, baglanti))
            using (var okuyucu = komut.ExecuteReader())
            {
                while (okuyucu.Read())
                {
                    liste.Add(new KategoriYorumIstatistik
                    {
                        KategoriAdi = okuyucu.GetString("KategoriAdi"),
                        ToplamKitapSayisi = okuyucu.GetInt32("ToplamKitapSayisi"),
                        ToplamYorumSayisi = okuyucu.GetInt32("ToplamYorumSayisi"),
                        OrtalamaPuan = okuyucu.GetDecimal("OrtalamaPuan"),
                        BesYildizSayisi = okuyucu.GetInt32("BesYildizSayisi"),
                        DortArtıYildizSayisi = okuyucu.GetInt32("DortArtıYildizSayisi"),
                        FarkliYorumcuSayisi = okuyucu.GetInt32("FarkliYorumcuSayisi")
                    });
                }
            }

            return liste;
        }

        // Tüm sistemdeki benzersiz yorumcu sayısı
        public int GenelFarkliYorumcuSayisi()
        {
            string sql = "SELECT COUNT(DISTINCT KullaniciID) FROM Yorum WHERE OnayDurumu = TRUE";

            using (var baglanti = _db.BaglantiyiAc())
            using (var komut = new MySqlCommand(sql, baglanti))
            {
                var sonuc = komut.ExecuteScalar();
                return sonuc != null ? Convert.ToInt32(sonuc) : 0;
            }
        }
    }
}
