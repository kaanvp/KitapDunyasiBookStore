using MySql.Data.MySqlClient;
using Veritabani_Odevi.DAL.Utilities;
using Veritabani_Odevi.Models;

namespace Veritabani_Odevi.Data
{
    public class KategoriRepository
    {
        private readonly DatabaseConnection _db;
        public KategoriRepository(DatabaseConnection db)
        {
            _db = db;
        }
        // CREATE - Kategori Ekle
        public int KategoriEkle(Kategori kategori)
        {
            string sql = @"
                INSERT INTO Kategori (KategoriAdi, UstKategoriID)
                VALUES (@KategoriAdi, @UstKategoriID);
                SELECT LAST_INSERT_ID();";

            using (var baglanti = _db.BaglantiyiAc())
            using (var komut = new MySqlCommand(sql, baglanti))
            {
                komut.Parameters.AddWithValue("@KategoriAdi", kategori.KategoriAdi);
                komut.Parameters.AddWithValue("@UstKategoriID",
                    kategori.UstKategoriID.HasValue ? kategori.UstKategoriID : (object)DBNull.Value);

                return Convert.ToInt32(komut.ExecuteScalar());
            }
        }

        // READ - Tüm kategorileri getir
        public List<Kategori> TumKategorileriGetir()
        {
            var liste = new List<Kategori>();
            string sql = "SELECT * FROM Kategori ORDER BY KategoriAdi ASC";

            using (var baglanti = _db.BaglantiyiAc())
            using (var komut = new MySqlCommand(sql, baglanti))
            using (var okuyucu = komut.ExecuteReader())
            {
                while (okuyucu.Read())
                {
                    liste.Add(new Kategori
                    {
                        KategoriID = okuyucu.GetInt32("KategoriID"),
                        KategoriAdi = okuyucu.GetString("KategoriAdi"),
                        UstKategoriID = okuyucu.IsDBNull(okuyucu.GetOrdinal("UstKategoriID"))
                            ? (int?)null
                            : okuyucu.GetInt32("UstKategoriID")
                    });
                }
            }

            return liste;
        }

        // READ - ID ile kategori getir
        public Kategori KategoriGetir(int kategoriID)
        {
            string sql = "SELECT * FROM Kategori WHERE KategoriID = @KategoriID";

            using (var baglanti = _db.BaglantiyiAc())
            using (var komut = new MySqlCommand(sql, baglanti))
            {
                komut.Parameters.AddWithValue("@KategoriID", kategoriID);

                using (var okuyucu = komut.ExecuteReader())
                {
                    if (okuyucu.Read())
                    {
                        return new Kategori
                        {
                            KategoriID = okuyucu.GetInt32("KategoriID"),
                            KategoriAdi = okuyucu.GetString("KategoriAdi"),
                            UstKategoriID = okuyucu.IsDBNull(okuyucu.GetOrdinal("UstKategoriID"))
                                ? (int?)null
                                : okuyucu.GetInt32("UstKategoriID")
                        };
                    }
                }
            }

            return null;
        }

        // UPDATE - Kategori Güncelle
        public bool KategoriGuncelle(Kategori kategori)
        {
            string sql = @"
                UPDATE Kategori 
                SET KategoriAdi = @KategoriAdi,
                    UstKategoriID = @UstKategoriID
                WHERE KategoriID = @KategoriID";

            using (var baglanti = _db.BaglantiyiAc())
            using (var komut = new MySqlCommand(sql, baglanti))
            {
                komut.Parameters.AddWithValue("@KategoriID", kategori.KategoriID);
                komut.Parameters.AddWithValue("@KategoriAdi", kategori.KategoriAdi);
                komut.Parameters.AddWithValue("@UstKategoriID",
                    kategori.UstKategoriID.HasValue ? kategori.UstKategoriID : (object)DBNull.Value);

                return komut.ExecuteNonQuery() > 0;
            }
        }

        // DELETE - Kategori Sil (gerçek silme)
        public bool KategoriSil(int kategoriID)
        {
            string sql = "DELETE FROM Kategori WHERE KategoriID = @KategoriID";

            using (var baglanti = _db.BaglantiyiAc())
            using (var komut = new MySqlCommand(sql, baglanti))
            {
                komut.Parameters.AddWithValue("@KategoriID", kategoriID);
                return komut.ExecuteNonQuery() > 0;
            }
        }

        // READ - Belirli bir kategorinin alt kategorilerini getir
        public List<Kategori> AltKategorileriGetir(int ustKategoriID)
        {
            var liste = new List<Kategori>();
            string sql = "SELECT * FROM Kategori WHERE UstKategoriID = @UstKategoriID";

            using (var baglanti = _db.BaglantiyiAc())
            using (var komut = new MySqlCommand(sql, baglanti))
            {
                komut.Parameters.AddWithValue("@UstKategoriID", ustKategoriID);

                using (var okuyucu = komut.ExecuteReader())
                {
                    while (okuyucu.Read())
                    {
                        liste.Add(new Kategori
                        {
                            KategoriID = okuyucu.GetInt32("KategoriID"),
                            KategoriAdi = okuyucu.GetString("KategoriAdi"),
                            UstKategoriID = ustKategoriID
                        });
                    }
                }
            }

            return liste;
        }
    }
}
