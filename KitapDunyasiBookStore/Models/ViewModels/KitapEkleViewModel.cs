namespace Veritabani_Odevi.Models.ViewModels
{
    public class KitapEkleViewModel
    {
        public string Baslik { get; set; }
        public string Yazar { get; set; }
        public string YayinEvi { get; set; }
        public string ISBN { get; set; }
        public decimal Fiyat { get; set; }
        public int StokAdedi { get; set; }
        public DurumEnum Durum { get; set; }
        public string Aciklama { get; set; }

        public List<int> KategoriIDler { get; set; }

        public IFormFile KapakDosyasi { get; set; } // 🔥 URL DEĞİL
    }
}
