using MySql.Data.MySqlClient;
using Veritabani_Odevi.DAL.Utilities;
using Veritabani_Odevi.Models;

namespace Veritabani_Odevi.Data
{
    public class IndirimRepository
    {
        private readonly DatabaseConnection _db;
        public IndirimRepository(DatabaseConnection db)
        {
            _db = db;
        }
        public int IndirimEkle(Indirim indirim)
        {
            string sql = @"
                INSERT INTO Indirim (IndirimAdi, IndirimOrani, BaslangicTarihi, BitisTarihi)
                VALUES (@IndirimAdi, @IndirimOrani, @BaslangicTarihi, @BitisTarihi);
                SELECT LAST_INSERT_ID();";
            using (var baglanti = _db.BaglantiyiAc())
            using (var komut = new MySqlCommand(sql, baglanti))
            {
                komut.Parameters.AddWithValue("@IndirimAdi", indirim.IndirimAdi);
                komut.Parameters.AddWithValue("@IndirimOrani", indirim.IndirimOrani);
                komut.Parameters.AddWithValue("@BaslangicTarihi", indirim.BaslangicTarihi);
                komut.Parameters.AddWithValue("@BitisTarihi", indirim.BitisTarihi);
                return Convert.ToInt32(komut.ExecuteScalar());
            }
        }
        public Indirim IndirimGetir(int indirimID)
        {
            Indirim indirim = null;
            string sql = "SELECT * FROM Indirim WHERE IndirimID = @IndirimID";
            using (var baglanti = _db.BaglantiyiAc())
            using (var komut = new MySqlCommand(sql, baglanti))
            {
                komut.Parameters.AddWithValue("@IndirimID", indirimID);
                using (var okuyucu = komut.ExecuteReader())
                {
                    if (okuyucu.Read())
                    {
                        indirim = new Indirim
                        {
                            IndirimID = okuyucu.GetInt32("IndirimID"),
                            IndirimAdi = okuyucu.GetString("IndirimAdi"),
                            IndirimOrani = okuyucu.GetDecimal("IndirimOrani"),
                            BaslangicTarihi = okuyucu.GetDateTime("BaslangicTarihi"),
                            BitisTarihi = okuyucu.GetDateTime("BitisTarihi")
                        };
                    }
                }
            }
            return indirim;
        }
        public List<Indirim> TumIndirimleriGetir()
        {
            var liste = new List<Indirim>();
            string sql = "SELECT * FROM Indirim ORDER BY IndirimAdi ASC";
            using (var baglanti = _db.BaglantiyiAc())
            using (var komut = new MySqlCommand(sql, baglanti))
            using (var okuyucu = komut.ExecuteReader())
            {
                while (okuyucu.Read())
                {
                    liste.Add(new Indirim
                    {
                        IndirimID = okuyucu.GetInt32("IndirimID"),
                        IndirimAdi = okuyucu.GetString("IndirimAdi"),
                        IndirimOrani = okuyucu.GetDecimal("IndirimOrani"),
                        BaslangicTarihi = okuyucu.GetDateTime("BaslangicTarihi"),
                        BitisTarihi = okuyucu.GetDateTime("BitisTarihi")
                    });
                }
            }
            return liste;
        }

        public List<Indirim> AktifIndirimleriGetir()
        {
            var liste = new List<Indirim>();
            string sql = "SELECT * FROM Indirim WHERE BaslangicTarihi <= NOW() AND BitisTarihi >= NOW() ORDER BY IndirimAdi ASC";
            using (var baglanti = _db.BaglantiyiAc())
            using (var komut = new MySqlCommand(sql, baglanti))
            using (var okuyucu = komut.ExecuteReader())
            {
                while (okuyucu.Read())
                {
                    liste.Add(new Indirim
                    {
                        IndirimID = okuyucu.GetInt32("IndirimID"),
                        IndirimAdi = okuyucu.GetString("IndirimAdi"),
                        IndirimOrani = okuyucu.GetDecimal("IndirimOrani"),
                        BaslangicTarihi = okuyucu.GetDateTime("BaslangicTarihi"),
                        BitisTarihi = okuyucu.GetDateTime("BitisTarihi")
                    });
                }
            }
            return liste;
        }

        public bool IndirimGuncelle(Indirim indirim)
        {
            string sql = @"
                UPDATE Indirim
                SET IndirimAdi = @IndirimAdi,
                    IndirimOrani = @IndirimOrani,
                    BaslangicTarihi = @BaslangicTarihi,
                    BitisTarihi = @BitisTarihi
                WHERE IndirimID = @IndirimID";
            using (var baglanti = _db.BaglantiyiAc())
            using (var komut = new MySqlCommand(sql, baglanti))
            {
                komut.Parameters.AddWithValue("@IndirimAdi", indirim.IndirimAdi);
                komut.Parameters.AddWithValue("@IndirimOrani", indirim.IndirimOrani);
                komut.Parameters.AddWithValue("@BaslangicTarihi", indirim.BaslangicTarihi);
                komut.Parameters.AddWithValue("@BitisTarihi", indirim.BitisTarihi);
                komut.Parameters.AddWithValue("@IndirimID", indirim.IndirimID);
                int etkilenenSatirlar = komut.ExecuteNonQuery();
                return etkilenenSatirlar > 0;
            }
        }
        public bool IndirimSil(int indirimID)
        {
            string sql = "DELETE FROM Indirim WHERE IndirimID = @IndirimID";
            using (var baglanti = _db.BaglantiyiAc())
            using (var komut = new MySqlCommand(sql, baglanti))
            {
                komut.Parameters.AddWithValue("@IndirimID", indirimID);
                int etkilenenSatirlar = komut.ExecuteNonQuery();
                return etkilenenSatirlar > 0;
            }
        }
        public void IndirimUygulaKitaba(int indirimID, int kitapID)
        {
            string sql = @"
                INSERT INTO KitapIndirim (KitapID, IndirimID)
                VALUES (@KitapID, @IndirimID)
                ON DUPLICATE KEY UPDATE IndirimID = @IndirimID";
            using (var baglanti = _db.BaglantiyiAc())
            using (var komut = new MySqlCommand(sql, baglanti))
            {
                komut.Parameters.AddWithValue("@KitapID", kitapID);
                komut.Parameters.AddWithValue("@IndirimID", indirimID);
                komut.ExecuteNonQuery();
            }
        }

    }
}
