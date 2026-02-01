using MySql.Data.MySqlClient;
using Veritabani_Odevi.DAL.Utilities;
using Veritabani_Odevi.Models;
using Veritabani_Odevi.Utilities;

namespace Veritabani_Odevi.Data
{
    public class KullaniciRepository
    {
        private readonly DatabaseConnection _db;
        public KullaniciRepository(DatabaseConnection db)
        {
            _db = db;
        }
        public int KullaniciEkle(Kullanici kullanici)
        {
            int yeniID = 0;
            // KayitTarihi ve Derecelendirme eklendi
            string sql = @"
            INSERT INTO Kullanici (Ad, Soyad, Eposta, SifreHash, Telefon, Adres, Rol, KayitTarihi, Derecelendirme)
            VALUES (@Ad, @Soyad, @Eposta, @SifreHash, @Telefon, @Adres, @Rol, @KayitTarihi, @Derecelendirme);
            SELECT LAST_INSERT_ID();";

            try
            {
                using (var baglanti = _db.BaglantiyiAc())
                using (var komut = new MySqlCommand(sql, baglanti))
                {
                    komut.Parameters.AddWithValue("@Ad", kullanici.Ad);
                    komut.Parameters.AddWithValue("@Soyad", kullanici.Soyad);
                    komut.Parameters.AddWithValue("@Eposta", kullanici.Eposta);
                    komut.Parameters.AddWithValue("@SifreHash", kullanici.SifreHash); // Zaten hashlenmiş olmalı
                    komut.Parameters.AddWithValue("@Telefon", string.IsNullOrEmpty(kullanici.Telefon) ? (object)DBNull.Value : kullanici.Telefon);
                    komut.Parameters.AddWithValue("@Adres", string.IsNullOrEmpty(kullanici.Adres) ? (object)DBNull.Value : kullanici.Adres);
                    komut.Parameters.AddWithValue("@Rol", kullanici.Rol);
                    komut.Parameters.AddWithValue("@KayitTarihi", kullanici.KayitTarihi);
                    komut.Parameters.AddWithValue("@Derecelendirme", kullanici.Derecelendirme);

                    yeniID = Convert.ToInt32(komut.ExecuteScalar());
                }
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1062) // Duplicate entry
                    HataYoneticisi.MySQLHataMesajınıCevir(ex);
                throw;
            }

            return yeniID;
        }
        // UPDATE - Kullanıcı Güncelleme
        public bool KullaniciGuncelle(Kullanici kullanici)
        {
            string sql = @"
            UPDATE Kullanici 
            SET Ad = @Ad, 
                Soyad = @Soyad, 
                Telefon = @Telefon, 
                Adres = @Adres
            WHERE KullaniciID = @KullaniciID";

            try
            {
                using (var baglanti = _db.BaglantiyiAc())
                using (var komut = new MySqlCommand(sql, baglanti))
                {
                    komut.Parameters.AddWithValue("@KullaniciID", kullanici.KullaniciID);
                    komut.Parameters.AddWithValue("@Ad", kullanici.Ad);
                    komut.Parameters.AddWithValue("@Soyad", kullanici.Soyad);
                    komut.Parameters.AddWithValue("@Telefon",
                        string.IsNullOrEmpty(kullanici.Telefon) ? (object)DBNull.Value : kullanici.Telefon);
                    komut.Parameters.AddWithValue("@Adres",
                        string.IsNullOrEmpty(kullanici.Adres) ? (object)DBNull.Value : kullanici.Adres);

                    return komut.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        // DELETE - Kullanıcı Silme (Cascade ile ilgili veriler de silinir)
        public bool KullaniciSil(int kullaniciID)
        {
            string sql = "DELETE FROM Kullanici WHERE KullaniciID = @KullaniciID";

            try
            {
                using (var baglanti = _db.BaglantiyiAc())
                using (var komut = new MySqlCommand(sql, baglanti))
                {
                    komut.Parameters.AddWithValue("@KullaniciID", kullaniciID);
                    return komut.ExecuteNonQuery() > 0;
                }
            }
            catch (MySqlException ex)
            {
                // Foreign key constraint hatası
                if (ex.Number == 1451)
                    throw new Exception("Bu kullanıcı silinemez. İlişkili kayıtlar mevcut.");
                throw;
            }
        }
        // GET - Tüm kullanıcıları getir
        public List<Kullanici> TumKullanicilariGetir()
        {
            var kullaniciListesi = new List<Kullanici>();
            string sql = "SELECT * FROM Kullanici ORDER BY KayitTarihi DESC"; // İsteğe göre sıralama yapılabilir

            using (var baglanti = _db.BaglantiyiAc())
            using (var komut = new MySqlCommand(sql, baglanti))
            using (var okuyucu = komut.ExecuteReader())
            {
                while (okuyucu.Read())
                {
                    var kullanici = new Kullanici
                    {
                        KullaniciID = Convert.ToInt32(okuyucu["KullaniciID"]),
                        Ad = okuyucu["Ad"].ToString(),
                        Soyad = okuyucu["Soyad"].ToString(),
                        Eposta = okuyucu["Eposta"].ToString(),
                        SifreHash = okuyucu["SifreHash"].ToString(), // Dikkat: Gerçek bir uygulamada bu bilgi genelde client'a gönderilmez
                        Telefon = okuyucu["Telefon"] != DBNull.Value ? okuyucu["Telefon"].ToString() : null,
                        Adres = okuyucu["Adres"] != DBNull.Value ? okuyucu["Adres"].ToString() : null,
                        KayitTarihi = Convert.ToDateTime(okuyucu["KayitTarihi"]),
                        Derecelendirme = Convert.ToDecimal(okuyucu["Derecelendirme"]),
                        Rol = okuyucu["Rol"].ToString()
                    };
                    kullaniciListesi.Add(kullanici);
                }
            }
            return kullaniciListesi;
        }
        // GET - ID'ye göre kullanıcı getir
        public Kullanici KullaniciyiGetir(int kullaniciID)
        {
            Kullanici kullanici = null;
            string sql = "SELECT * FROM Kullanici WHERE KullaniciID = @KullaniciID";
            using (var baglanti = _db.BaglantiyiAc())
            using (var komut = new MySqlCommand(sql, baglanti))
            {
                komut.Parameters.AddWithValue("@KullaniciID", kullaniciID);
                using (var okuyucu = komut.ExecuteReader())
                {
                    if (okuyucu.Read())
                    {
                        kullanici = new Kullanici
                        {
                            KullaniciID = Convert.ToInt32(okuyucu["KullaniciID"]),
                            Ad = okuyucu["Ad"].ToString(),
                            Soyad = okuyucu["Soyad"].ToString(),
                            Eposta = okuyucu["Eposta"].ToString(),
                            SifreHash = okuyucu["SifreHash"].ToString(),
                            Telefon = okuyucu["Telefon"] != DBNull.Value ? okuyucu["Telefon"].ToString() : null,
                            Adres = okuyucu["Adres"] != DBNull.Value ? okuyucu["Adres"].ToString() : null,
                            KayitTarihi = Convert.ToDateTime(okuyucu["KayitTarihi"]),
                            Derecelendirme = Convert.ToDecimal(okuyucu["Derecelendirme"]),
                            Rol = okuyucu["Rol"].ToString()
                        };
                    }
                }
            }
            return kullanici;
        }
        // GET - E-postaya göre kullanıcı getir
        public Kullanici KullaniciyiGetirByEmail(string email)
        {
            Kullanici kullanici = null;
            string sql = "SELECT * FROM Kullanici WHERE Eposta = @Email";
            using (var baglanti = _db.BaglantiyiAc())
            using (var komut = new MySqlCommand(sql, baglanti))
            {
                komut.Parameters.AddWithValue("@Email", email);
                using (var okuyucu = komut.ExecuteReader())
                {
                    if (okuyucu.Read())
                    {
                        kullanici = new Kullanici
                        {
                            KullaniciID = Convert.ToInt32(okuyucu["KullaniciID"]),
                            Ad = okuyucu["Ad"].ToString(),
                            Soyad = okuyucu["Soyad"].ToString(),
                            Eposta = okuyucu["Eposta"].ToString(),
                            SifreHash = okuyucu["SifreHash"].ToString(),
                            Telefon = okuyucu["Telefon"] != DBNull.Value ? okuyucu["Telefon"].ToString() : null,
                            Adres = okuyucu["Adres"] != DBNull.Value ? okuyucu["Adres"].ToString() : null,
                            KayitTarihi = Convert.ToDateTime(okuyucu["KayitTarihi"]),
                            Derecelendirme = Convert.ToDecimal(okuyucu["Derecelendirme"]),
                            Rol = okuyucu["Rol"].ToString()
                        };
                    }
                }
            }
            return kullanici;
        }
        // Şifre hash fonksiyonu (basit örnek - gerçek projede BCrypt kullanın)
        private string HashSifre(string sifre)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(sifre));
                return Convert.ToBase64String(bytes);
            }
        }
    }
}
