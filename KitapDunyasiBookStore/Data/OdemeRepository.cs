using MySql.Data.MySqlClient;
using Veritabani_Odevi.DAL.Utilities;
using Veritabani_Odevi.Models;

namespace Veritabani_Odevi.Data
{
    public class OdemeRepository
    {
        private readonly DatabaseConnection _db;

        public OdemeRepository(DatabaseConnection db)
        {
            _db = db;
        }

        public void OdemeEkle(Odeme odeme)
        {
            string sql = @"
            INSERT INTO Odeme
            (SiparisID, OdemeTuru, Tutar, BankaOnayKodu, Durum)
            VALUES
            (@SiparisID, @OdemeTuru, @Tutar, @OnayKodu, @Durum)";

            using var baglanti = _db.BaglantiyiAc();
            using var cmd = new MySqlCommand(sql, baglanti);

            cmd.Parameters.AddWithValue("@SiparisID", odeme.SiparisID);
            cmd.Parameters.AddWithValue("@OdemeTuru", odeme.OdemeTuru);
            cmd.Parameters.AddWithValue("@Tutar", odeme.Tutar);
            cmd.Parameters.AddWithValue("@OnayKodu", odeme.BankaOnayKodu);
            cmd.Parameters.AddWithValue("@Durum", odeme.Durum);

            cmd.ExecuteNonQuery();
        }
    }
}
