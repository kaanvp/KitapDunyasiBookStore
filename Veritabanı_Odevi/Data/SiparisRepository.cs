using MySql.Data.MySqlClient;
using System.Data;
using System.Text;
using Veritabani_Odevi.DAL.Utilities;
using Veritabani_Odevi.DTOs;
using Veritabani_Odevi.Models;
using Veritabani_Odevi.Models.ViewModels;

namespace Veritabani_Odevi.Data
{
    public class SiparisRepository
    {
        private readonly DatabaseConnection _db;
        public SiparisRepository(DatabaseConnection db)
        {
            _db = db;
        }
        // 5.3.2 Dinamik Sorgu 2: Tarih Aralığına Göre Sipariş Filtreleme
        public List<SiparisViewModel> TariheGoreSiparisGetir(int kullaniciID)
        {
            var liste = new List<SiparisViewModel>();

            string sql = @"
            SELECT
                SiparisID,
                SiparisNumarası,
                SiparisTarihi,
                ToplamTutar,
                Durum
            FROM Siparis
            WHERE KullaniciID = @KullaniciID
            ORDER BY SiparisTarihi DESC
        ";

            using var baglanti = _db.BaglantiyiAc();
            using var cmd = new MySqlCommand(sql, baglanti);
            cmd.Parameters.AddWithValue("@KullaniciID", kullaniciID);

            using var dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                liste.Add(new SiparisViewModel
                {
                    SiparisID = dr.GetInt32("SiparisID"),
                    SiparisNo = dr.GetString("SiparisNumarası"),
                    Tarih = dr.GetDateTime("SiparisTarihi"),
                    ToplamTutar = dr.GetDecimal("ToplamTutar"),
                    Durum = dr.GetString("Durum")
                });
            }

            return liste;
        }
        // Complex join Query 1 Kullanıcının tüm sipariş detaylarını getir
        public List<SiparisDetayDTO> KullaniciSiparisDetaylari(int kullaniciID)
        {
            var liste = new List<SiparisDetayDTO>();
            string sql = @"
            SELECT 
                s.SiparisNumarası,
                s.SiparisTarihi,
                s.ToplamTutar,
                s.Durum AS SiparisDurumu,
                s.TeslimatAdresi,
                kit.Baslik AS KitapBaslik,
                kit.Yazar,
                sd.Adet,
                sd.BirimFiyat,
                (sd.Adet * sd.BirimFiyat) AS AltToplam,
                CONCAT(satici.Ad, ' ', satici.Soyad) AS SaticiAdSoyad,
                satici.Eposta AS SaticiEposta,
                satici.Telefon AS SaticiTelefon,
                kit.KapakResmiUrl
            FROM Siparis s
            INNER JOIN SiparisDetay sd ON s.SiparisID = sd.SiparisID
            INNER JOIN Kitap kit ON sd.KitapID = kit.KitapID
            INNER JOIN Kullanici satici ON kit.SaticiKullaniciID = satici.KullaniciID
            WHERE s.KullaniciID = @KullaniciID
            ORDER BY s.SiparisTarihi DESC";

            using (var baglanti = _db.BaglantiyiAc())
            using (var komut = new MySqlCommand(sql, baglanti))
            {
                komut.Parameters.AddWithValue("@KullaniciID", kullaniciID);

                using (var okuyucu = komut.ExecuteReader())
                {
                    while (okuyucu.Read())
                    {
                        liste.Add(new SiparisDetayDTO
                        {
                            SiparisNumarası = okuyucu.GetString("SiparisNumarası"),
                            SiparisTarihi = okuyucu.GetDateTime("SiparisTarihi"),
                            ToplamTutar = okuyucu.GetDecimal("ToplamTutar"),
                            SiparisDurumu = okuyucu.GetString("SiparisDurumu"),
                            TeslimatAdresi = okuyucu.GetString("TeslimatAdresi"),
                            KitapBaslik = okuyucu.GetString("KitapBaslik"),
                            Yazar = okuyucu.IsDBNull(okuyucu.GetOrdinal("Yazar"))
                                ? "Bilinmiyor" : okuyucu.GetString("Yazar"),
                            Adet = okuyucu.GetInt32("Adet"),
                            BirimFiyat = okuyucu.GetDecimal("BirimFiyat"),
                            AltToplam = okuyucu.GetDecimal("AltToplam"),
                            SaticiAdSoyad = okuyucu.GetString("SaticiAdSoyad"),
                            SaticiEposta = okuyucu.GetString("SaticiEposta"),
                            SaticiTelefon = okuyucu.IsDBNull(okuyucu.GetOrdinal("SaticiTelefon"))
                                ? "-" : okuyucu.GetString("SaticiTelefon"),
                            KapakResmiUrl = okuyucu.IsDBNull(okuyucu.GetOrdinal("KapakResmiUrl"))
                                ? "/images/no-cover.jpg" : okuyucu.GetString("KapakResmiUrl")
                        });
                    }
                }
            }

            return liste;
        }
        private Siparis SiparisOkuyucudanOlustur(MySqlDataReader okuyucu)
        {
            var siparis = new Siparis
            {
                SiparisID = okuyucu.GetInt32("SiparisID"),
                SiparisNumarasi = okuyucu.GetString("SiparisNumarası"),
                KullaniciID = okuyucu.GetInt32("KullaniciID"),
                SiparisTarihi = okuyucu.GetDateTime("SiparisTarihi"),
                ToplamTutar = okuyucu.GetDecimal("ToplamTutar"),
                TeslimatAdresi = okuyucu.GetString("TeslimatAdresi"),
                Durum = okuyucu.GetString("Durum"),

                // JOIN ile gelen ek alanlar
                KullaniciAdSoyad = okuyucu.IsDBNull(okuyucu.GetOrdinal("MusteriAdSoyad"))
                    ? null
                    : okuyucu.GetString("MusteriAdSoyad"),

                Detaylar = new List<SiparisDetay>() // detaylar ayrıca yüklenecekse boş liste
            };

            return siparis;
        }
        public int SiparisOlustur(
         int kullaniciID,
         string teslimatAdresi,
         List<SepetElemani> sepet)


        {
            using var baglanti = _db.BaglantiyiAc();
            using var transaction = baglanti.BeginTransaction();

            try
            {
                // 0️⃣ ÖNCELİKLE STOK KONTROLÜ YAP (Trigger stoku düşürecek ama önce yeterli mi kontrol et)
                string stokKontrolSql = @"SELECT StokAdedi FROM Kitap WHERE KitapID = @KitapID";
                
                foreach (var item in sepet)
                {
                    using var kontrolCmd = new MySqlCommand(stokKontrolSql, baglanti, transaction);
                    kontrolCmd.Parameters.AddWithValue("@KitapID", item.KitapID);
                    
                    var sonuc = kontrolCmd.ExecuteScalar();
                    int mevcutStok = sonuc != null ? Convert.ToInt32(sonuc) : 0;
                    
                    if (mevcutStok < item.Adet)
                    {
                        throw new Exception($"Stok yetersiz: '{item.kitap.Baslik}' için yeterli stok bulunmuyor. (Mevcut: {mevcutStok}, İstenen: {item.Adet})");
                    }
                }

                // 1️⃣ Sipariş tablosuna ekle
                string siparisNo = Guid.NewGuid().ToString().Substring(0, 8).ToUpper();

                string siparisSql = @"
                INSERT INTO Siparis
                (SiparisNumarası, KullaniciID, SiparisTarihi, ToplamTutar, TeslimatAdresi, Durum)
                VALUES
                (@No, @KullaniciID, NOW(), @Toplam, @Adres, 'Hazırlanıyor');
                SELECT LAST_INSERT_ID();";

                decimal toplamTutar = sepet.Sum(x => x.kitap.Fiyat * x.Adet);

                int siparisID;
                using (var cmd = new MySqlCommand(siparisSql, baglanti, transaction))
                {
                    cmd.Parameters.AddWithValue("@No", siparisNo);
                    cmd.Parameters.AddWithValue("@KullaniciID", kullaniciID);
                    cmd.Parameters.AddWithValue("@Toplam", toplamTutar);
                    cmd.Parameters.AddWithValue("@Adres", teslimatAdresi);

                    siparisID = Convert.ToInt32(cmd.ExecuteScalar());
                }

                // 2️⃣ Sipariş detayları (INSERT sonrası trigger stoku otomatik düşürecek)
                string detaySql = @"
                INSERT INTO SiparisDetay
                (SiparisID, KitapID, Adet, BirimFiyat)
                VALUES
                (@SiparisID, @KitapID, @Adet, @Fiyat)";

                foreach (var item in sepet)
                {
                    using var detayCmd = new MySqlCommand(detaySql, baglanti, transaction);
                    detayCmd.Parameters.AddWithValue("@SiparisID", siparisID);
                    detayCmd.Parameters.AddWithValue("@KitapID", item.KitapID);
                    detayCmd.Parameters.AddWithValue("@Adet", item.Adet);
                    detayCmd.Parameters.AddWithValue("@Fiyat", item.kitap.Fiyat);
                    detayCmd.ExecuteNonQuery();
                    // ⚡ NOT: TR_SiparisDetayEkle_StokAzalt trigger'ı burada stoku otomatik düşürür
                }

                transaction.Commit();
                return siparisID;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public List<SiparisDetayViewModel> SiparisDetayGetir(int siparisID)
        {
            var liste = new List<SiparisDetayViewModel>();

            string sql = @"
            SELECT
                k.Baslik,
                sd.Adet,
                sd.BirimFiyat
            FROM SiparisDetay sd
            INNER JOIN Kitap k ON sd.KitapID = k.KitapID
            WHERE sd.SiparisID = @SiparisID
        ";

            using var baglanti = _db.BaglantiyiAc();
            using var cmd = new MySqlCommand(sql, baglanti);
            cmd.Parameters.AddWithValue("@SiparisID", siparisID);

            using var dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                liste.Add(new SiparisDetayViewModel
                {
                    KitapAdi = dr.GetString("Baslik"),
                    Adet = dr.GetInt32("Adet"),
                    Fiyat = dr.GetDecimal("BirimFiyat")
                });
            }

            return liste;
        }
        public SiparisDetaySayfaViewModel SiparisDetaySayfaGetir(int siparisID)
        {
            SiparisDetaySayfaViewModel model = null;

            // 1️⃣ Sipariş Üst Bilgileri
            string siparisSql = @"
            SELECT 
            s.SiparisNumarası,
            s.SiparisTarihi,
            s.Durum,
            s.TeslimatAdresi,
            s.ToplamTutar,

            o.OdemeTuru,
            o.Durum AS OdemeDurum,
            o.IslemTarihi AS OdemeTarihi,
            o.BankaOnayKodu
            FROM Siparis s
            LEFT JOIN Odeme o ON s.SiparisID = o.SiparisID
            WHERE s.SiparisID = @SiparisID";
            using (var baglanti = _db.BaglantiyiAc())
            {
                using (var cmd = new MySqlCommand(siparisSql, baglanti))
                {
                    cmd.Parameters.AddWithValue("@SiparisID", siparisID);

                    using (var dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            model = new SiparisDetaySayfaViewModel
                            {
                                SiparisNo = dr.GetString("SiparisNumarası"),
                                SiparisTarihi = dr.GetDateTime("SiparisTarihi"),
                                Durum = dr.GetString("Durum"),
                                TeslimatAdresi = dr.GetString("TeslimatAdresi"),
                                ToplamTutar = dr.GetDecimal("ToplamTutar"),

                                // 👇 ÖDEME BİLGİLERİ
                                OdemeTuru = dr.IsDBNull("OdemeTuru") ? null : dr.GetString("OdemeTuru"),
                                OdemeDurum = dr.IsDBNull("OdemeDurum") ? null : dr.GetString("OdemeDurum"),
                                OdemeTarihi = dr.IsDBNull("OdemeTarihi") ? null : dr.GetDateTime("OdemeTarihi"),
                                BankaOnayKodu = dr.IsDBNull("BankaOnayKodu") ? null : dr.GetString("BankaOnayKodu"),

                                Urunler = new List<SiparisDetayViewModel>()
                            };
                        }

                    }
                }

                // 2️⃣ Sipariş Detayları
                string detaySql = @"
                    SELECT 
                    k.Baslik,
                    sd.Adet,
                    sd.BirimFiyat,
                    k.KapakResmiUrl
                    FROM SiparisDetay sd
                    INNER JOIN Kitap k ON sd.KitapID = k.KitapID
                    WHERE sd.SiparisID = @SiparisID
                    ";
                using (var cmd = new MySqlCommand(detaySql, baglanti))
                {
                    cmd.Parameters.AddWithValue("@SiparisID", siparisID);

                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            model.Urunler.Add(new SiparisDetayViewModel
                            {
                                KitapAdi = dr.GetString("Baslik"),
                                Adet = dr.GetInt32("Adet"),
                                Fiyat = dr.GetDecimal("BirimFiyat"),
                                KapakResmiUrl = dr.IsDBNull(dr.GetOrdinal("KapakResmiUrl"))
                                ? "/images/no-cover.jpg"
                                : dr.GetString("KapakResmiUrl")
                            });

                        }
                    }
                }
            }

            return model;
        }

        public decimal SiparisToplamTutarGetir(int siparisID)
        {
            string sql = @"SELECT ToplamTutar FROM Siparis WHERE SiparisID = @SiparisID";

            using var baglanti = _db.BaglantiyiAc();
            using var cmd = new MySqlCommand(sql, baglanti);
            cmd.Parameters.AddWithValue("@SiparisID", siparisID);

            var sonuc = cmd.ExecuteScalar();

            return sonuc != null ? Convert.ToDecimal(sonuc) : 0;
        }

    }
}
