using Microsoft.AspNetCore.Mvc;
using MySqlConnector;

namespace MarketERP.Controllers
{
    public class DatabaseSchemaController : Controller
    {
        private readonly IConfiguration _configuration;

        public DatabaseSchemaController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            var tables = new Dictionary<string, List<string>>();
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using var connection = new MySqlConnection(connectionString);
            connection.Open();

            string query = @"
    SELECT 
        c.TABLE_NAME,
        c.COLUMN_NAME,
        k.REFERENCED_TABLE_NAME
    FROM INFORMATION_SCHEMA.COLUMNS c
    LEFT JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE k
        ON c.TABLE_SCHEMA = k.TABLE_SCHEMA
        AND c.TABLE_NAME = k.TABLE_NAME
        AND c.COLUMN_NAME = k.COLUMN_NAME
    WHERE c.TABLE_SCHEMA = 'market_erp'
    ORDER BY c.TABLE_NAME, c.ORDINAL_POSITION";

            using var command = new MySqlCommand(query, connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                string tableName = reader.GetString("TABLE_NAME");
                string columnName = reader.GetString("COLUMN_NAME");
                string referencedTable = reader["REFERENCED_TABLE_NAME"]?.ToString();
                if (!tables.ContainsKey(tableName))
                {
                    tables[tableName] = new List<string>();
                }

                if (!string.IsNullOrEmpty(referencedTable))
                {
                    tables[tableName].Add($"{columnName} → {referencedTable}");
                }
                else
                {
                    tables[tableName].Add(columnName);
                }
            }

            return View(tables);
        }

        public IActionResult Details(string tableName)
        {
            var rows = new List<Dictionary<string, object>>();
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using var connection = new MySqlConnection(connectionString);
            connection.Open();

            string query = $"SELECT * FROM `{tableName}` LIMIT 50";

            using var command = new MySqlCommand(query, connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                var row = new Dictionary<string, object>();

                for (int i = 0; i < reader.FieldCount; i++)
                {
                    row[reader.GetName(i)] = reader.GetValue(i);
                }

                rows.Add(row);
            }

            ViewBag.TableName = tableName;
            return View(rows);
        }
    }
}