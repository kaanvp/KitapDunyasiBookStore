using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Veritabani_Odevi.DAL.Utilities
{
    public class DatabaseConnection
    {
        private readonly IConfiguration _configuration;
        private string _connectionString;

        public DatabaseConnection(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("KitapSatisDB");
        }

        public MySqlConnection BaglantiyiAc()
        {
            var baglanti = new MySqlConnection(_connectionString);
            if (baglanti.State != ConnectionState.Open)
                baglanti.Open();
            return baglanti;
        }
    }
}
