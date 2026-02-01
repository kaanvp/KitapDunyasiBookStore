using MySql.Data.MySqlClient;

namespace Veritabani_Odevi.Utilities
{
    public class HataYoneticisi
    {
        public static string MySQLHataMesajınıCevir(MySqlException ex)
        {
            return ex.Number switch
            {
                1062 => "Bu kayıt zaten mevcut! (Tekrar eden değer)",
                1048 => "Zorunlu alan boş bırakılamaz!",
                1451 => "Bu kayıt silinemez! İlişkili kayıtlar mevcut.",
                1452 => "Geçersiz ilişki! Bağlı kayıt bulunamadı.",
                3819 => "CHECK kısıtlaması ihlali! Geçersiz değer girildi.",
                _ => $"Veritabanı hatası: {ex.Message}"
            };
        }
    }
}
