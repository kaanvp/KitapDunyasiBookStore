using MySql.Data.MySqlClient;
using Veritabani_Odevi.DAL.Utilities;
using Veritabani_Odevi.Models;

namespace Veritabani_Odevi.Data
{
    public class YorumRepository
    {
        private readonly DatabaseConnection _db;
        public YorumRepository(DatabaseConnection db)
        {
            _db = db;
        }
        // CREATE - Yorum Ekleme
        public int YorumEkle(Yorum yorum)
        {
            int yeniID = 0;
            string sql = @"
            INSERT INTO Yorum (KitapID, KullaniciID, Baslik, Icerik, Puan, OnayDurumu)
            VALUES (@KitapID, @KullaniciID, @Baslik, @Icerik, @Puan, 1);
            SELECT LAST_INSERT_ID();";

            try
            {
                using (var baglanti = _db.BaglantiyiAc())
                using (var komut = new MySqlCommand(sql, baglanti))
                {
                    komut.Parameters.AddWithValue("@KitapID", yorum.KitapID);
                    komut.Parameters.AddWithValue("@KullaniciID", yorum.KullaniciID);
                    komut.Parameters.AddWithValue("@Baslik",
                        string.IsNullOrEmpty(yorum.Baslik) ? (object)DBNull.Value : yorum.Baslik);
                    komut.Parameters.AddWithValue("@Icerik", yorum.Icerik);
                    komut.Parameters.AddWithValue("@Puan", yorum.Puan);

                    yeniID = Convert.ToInt32(komut.ExecuteScalar());
                }
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1062) // Duplicate key
                    throw new Exception("Bu kitap için zaten yorum yapmışsınız!");
                throw;
            }

            return yeniID;
        }
        // READ - Kitaba ait yorumlar
        public List<Yorum> KitapYorumlariniGetir(int kitapID)
        {
            var liste = new List<Yorum>();
            string sql = @"
            SELECT y.*, 
            CONCAT(k.Ad, ' ', k.Soyad) AS YorumcuAdSoyad,
            k.Derecelendirme AS YorumcuPuan
            FROM Yorum y
            INNER JOIN Kullanici k ON y.KullaniciID = k.KullaniciID
            WHERE y.KitapID = @KitapID 
            AND y.OnayDurumu = TRUE
            ORDER BY y.YorumTarihi DESC";

            using (var baglanti = _db.BaglantiyiAc())
            using (var komut = new MySqlCommand(sql, baglanti))
            {
                komut.Parameters.AddWithValue("@KitapID", kitapID);

                using (var okuyucu = komut.ExecuteReader())
                {
                    while (okuyucu.Read())
                    {
                        liste.Add(new Yorum
                        {
                            YorumID = okuyucu.GetInt32("YorumID"),
                            KitapID = okuyucu.GetInt32("KitapID"),
                            KullaniciID = okuyucu.GetInt32("KullaniciID"),
                            Baslik = okuyucu.IsDBNull(okuyucu.GetOrdinal("Baslik"))
                                ? null : okuyucu.GetString("Baslik"),
                            Icerik = okuyucu.GetString("Icerik"),
                            Puan = okuyucu.GetByte("Puan"),
                            YorumTarihi = okuyucu.GetDateTime("YorumTarihi"),
                        });
                    }
                }
            }

            return liste;
        }
        // UPDATE - Yorum Güncelleme
        public bool YorumGuncelle(Yorum yorum)
        {
            string sql = @"
            UPDATE Yorum 
            SET Baslik = @Baslik,
                Icerik = @Icerik,
                Puan = @Puan,
                OnayDurumu = 1
            WHERE YorumID = @YorumID 
            AND KullaniciID = @KullaniciID";

            using (var baglanti = _db.BaglantiyiAc())
            using (var komut = new MySqlCommand(sql, baglanti))
            {
                komut.Parameters.AddWithValue("@YorumID", yorum.YorumID);
                komut.Parameters.AddWithValue("@Baslik",
                    string.IsNullOrEmpty(yorum.Baslik) ? (object)DBNull.Value : yorum.Baslik);
                komut.Parameters.AddWithValue("@Icerik", yorum.Icerik);
                komut.Parameters.AddWithValue("@Puan", yorum.Puan);
                komut.Parameters.AddWithValue("@KullaniciID", yorum.KullaniciID);

                return komut.ExecuteNonQuery() > 0;
            }
        }

        // DELETE - Yorum Silme
        public bool YorumSil(int yorumID, int kullaniciID)
        {
            string sql = @"
            DELETE FROM Yorum 
            WHERE YorumID = @YorumID 
            AND KullaniciID = @KullaniciID";

            using (var baglanti = _db.BaglantiyiAc())
            using (var komut = new MySqlCommand(sql, baglanti))
            {
                komut.Parameters.AddWithValue("@YorumID", yorumID);
                komut.Parameters.AddWithValue("@KullaniciID", kullaniciID);

                return komut.ExecuteNonQuery() > 0;
            }
        }
        // ADMIN - Yorum Onaylama
        public bool YorumOnayla(int yorumID)
        {
            string sql = "UPDATE Yorum SET OnayDurumu = TRUE WHERE YorumID = @YorumID";

            using (var baglanti = _db.BaglantiyiAc())
            using (var komut = new MySqlCommand(sql, baglanti))
            {
                komut.Parameters.AddWithValue("@YorumID", yorumID);
                return komut.ExecuteNonQuery() > 0;
            }
        }
        public List<Yorum> GetByKitapId(int kitapId)
        {
            var liste = new List<Yorum>();

            string sql = @"SELECT *
                   FROM Yorum
                   WHERE KitapID = @KitapID";

            using (var conn = _db.BaglantiyiAc())
            using (var cmd = new MySqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@KitapID", kitapId);

                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        liste.Add(new Yorum
                        {
                            YorumID = Convert.ToInt32(dr["YorumID"]),
                            KitapID = Convert.ToInt32(dr["KitapID"]),
                            KullaniciID = Convert.ToInt32(dr["KullaniciID"]),
                            Baslik = dr["Baslik"]?.ToString(),
                            Icerik = dr["Icerik"].ToString(),
                            Puan = Convert.ToInt32(dr["Puan"]),
                            OnayDurumu = Convert.ToBoolean(dr["OnayDurumu"]),
                            YorumTarihi = Convert.ToDateTime(dr["YorumTarihi"])
                        });
                    }
                }
            }

            return liste;
        }
        public Yorum GetById(int yorumID)
        {
            string sql = @"SELECT * FROM Yorum WHERE YorumID = @YorumID";

            using (var conn = _db.BaglantiyiAc())
            using (var cmd = new MySqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@YorumID", yorumID);

                using (var dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        return new Yorum
                        {
                            YorumID = Convert.ToInt32(dr["YorumID"]),
                            KitapID = Convert.ToInt32(dr["KitapID"]),
                            KullaniciID = Convert.ToInt32(dr["KullaniciID"]),
                            Baslik = dr["Baslik"]?.ToString(),
                            Icerik = dr["Icerik"].ToString(),
                            Puan = Convert.ToInt32(dr["Puan"]),
                            OnayDurumu = Convert.ToBoolean(dr["OnayDurumu"]),
                            YorumTarihi = Convert.ToDateTime(dr["YorumTarihi"])
                        };
                    }
                }
            }
            return null;
        }

        // Kullanıcının kendi yorumlarını getir (Kitap adıyla birlikte)
        public List<dynamic> KullaniciYorumlariniGetir(int kullaniciID)
        {
            var liste = new List<dynamic>();
            string sql = @"
            SELECT y.YorumID, y.KitapID, y.KullaniciID, y.Baslik, y.Icerik, 
                   y.Puan, y.YorumTarihi, y.OnayDurumu, k.Baslik as KitapAdi
            FROM Yorum y
            INNER JOIN Kitap k ON y.KitapID = k.KitapID
            WHERE y.KullaniciID = @KullaniciID
            ORDER BY y.YorumTarihi DESC";

            using (var baglanti = _db.BaglantiyiAc())
            using (var komut = new MySqlCommand(sql, baglanti))
            {
                komut.Parameters.AddWithValue("@KullaniciID", kullaniciID);

                using (var okuyucu = komut.ExecuteReader())
                {
                    while (okuyucu.Read())
                    {
                        liste.Add(new
                        {
                            YorumID = okuyucu.GetInt32("YorumID"),
                            KitapID = okuyucu.GetInt32("KitapID"),
                            KullaniciID = okuyucu.GetInt32("KullaniciID"),
                            Baslik = okuyucu.IsDBNull(okuyucu.GetOrdinal("Baslik"))
                                ? null : okuyucu.GetString("Baslik"),
                            Icerik = okuyucu.GetString("Icerik"),
                            Puan = okuyucu.GetByte("Puan"),
                            YorumTarihi = okuyucu.GetDateTime("YorumTarihi"),
                            OnayDurumu = okuyucu.GetBoolean("OnayDurumu"),
                            KitapAdi = okuyucu.GetString("KitapAdi")
                        });
                    }
                }
            }

            return liste;
        }

    }
}
