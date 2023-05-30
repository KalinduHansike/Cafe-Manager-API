using Microsoft.Extensions.Configuration;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Cafe_Manager_API.Repository.Db
{
    public class DbConnection
    {
        private static Dictionary<int, string> connections = new Dictionary<int, string>();
        public const int DB_EMPCAFE = 0;

        static DbConnection()
        {
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            connections.Add(DB_EMPCAFE, config["ConnectionStrings:EMPCAFE"]);
        }

        public static MySqlConnection GetOpenedConnection(int database)
        {
            MySqlConnection connection = null;
            try
            {
                return connection = new MySqlConnection(connections[database]);
            }
            finally
            {
                if (connection != null) { connection.Open(); }
            }
        }
        public static string GetOpenedConnectionString(int database)
        {
            MySqlConnection connection = null;
            try
            {
                connection = new MySqlConnection(connections[database]);
                return connection.ConnectionString;
            }
            finally
            {
                if (connection != null) { connection.Open(); }
            }
        }
    }
}
