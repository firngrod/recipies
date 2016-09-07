using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using ExtensionMethods;

namespace RecipieManager
{
    public sealed class Database
    {
        private static volatile Database instance;
        private static Object syncRoot = new Object();
        private static Object lockObject = new Object();

        private Database() { }

        public static Database Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        instance = new Database();
                    }
                }
                return instance;
            }
        }

        private SQLiteConnection _dbConnection;
        public SQLiteConnection dbConnection
        {
            get
            {
                lock (lockObject)
                {

                    String Appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    String Directory = System.IO.Path.Combine(Appdata, "RecipieManager");
                    String DatabasePath = System.IO.Path.Combine(Directory, "Database.sqlite");
                    if (!System.IO.File.Exists(DatabasePath))
                    {
                        if (!System.IO.Directory.Exists(Directory))
                        {
                            System.IO.Directory.CreateDirectory(Directory);
                        }
                        SQLiteConnection.CreateFile(DatabasePath);
                    }
                    if (_dbConnection == null)
                    {
                        _dbConnection = new SQLiteConnection("Data Source=" + DatabasePath + ";Version=3");
                        _dbConnection.Open();
                    }
                    return _dbConnection;
                }
            }
        }

        public void VerifyTables()
        {
            var tableSetup = new Dictionary<String, List<KeyValuePair<String, String>>>()
            {
                {
                    "ingredients",
                    new List<KeyValuePair<String, String>>()
                    {
                        new KeyValuePair<String, String>("ing_id", "integer"),
                        new KeyValuePair<String, String>("name", "text"),
                    }
                },
                {
                    "recipies",
                    new List<KeyValuePair<String, String>>()
                    {
                        new KeyValuePair<String, String>("rec_id", "integer"),
                        new KeyValuePair<String, String>("name", "text"),
                        new KeyValuePair<String, String>("timesmade", "integer"),
                        new KeyValuePair<String, String>("lastmade", "integer"),
                        new KeyValuePair<String, String>("recipie", "text"),
                    }
                },
                {
                    "ingredientlink",
                    new List<KeyValuePair<String, String>>()
                    {
                        new KeyValuePair<String, String>("rec_id", "integer"),
                        new KeyValuePair<String, String>("ing_id", "string"),
                    }
                }
            };
            String sql = "SELECT name FROM sqlite_master WHERE type='table'";
            // sql = "PRAGMA table_info(ingredients)";
            List<String> tableList = new List<String>();
            SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                tableList.Add((String)reader["name"]);
            }
            foreach (var table in tableSetup)
            {
                if (!tableList.Contains(table.Key))
                {
                    String tableSql = "CREATE TABLE " + table.Key + " (";
                    foreach (var column in table.Value)
                    {
                        tableSql += column.Key + " " + column.Value + ", ";
                    }
                    tableSql = tableSql.ReplaceLastOf(", ", "");
                    tableSql += ")";
                    SQLiteCommand tableCmd = new SQLiteCommand(tableSql, dbConnection);
                    tableCmd.ExecuteNonQuery();
                }
            }
        }
    }
}
