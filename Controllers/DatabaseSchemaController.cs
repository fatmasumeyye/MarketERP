using MarketERP.Helpers;
using MarketERP.Models;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;

namespace MarketERP.Controllers
{
    [PermissionAuthorize("database.view")]
    public class DatabaseSchemaController : Controller
    {
        private readonly IConfiguration _configuration;

        public DatabaseSchemaController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            var model = new DatabaseSchemaViewModel();

            using var connection = CreateConnection();
            connection.Open();

            const string query = """
                SELECT
                    c.TABLE_NAME,
                    c.COLUMN_NAME,
                    c.COLUMN_TYPE,
                    c.IS_NULLABLE,
                    c.COLUMN_KEY,
                    k.REFERENCED_TABLE_NAME,
                    k.REFERENCED_COLUMN_NAME
                FROM INFORMATION_SCHEMA.COLUMNS c
                LEFT JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE k
                    ON c.TABLE_SCHEMA = k.TABLE_SCHEMA
                    AND c.TABLE_NAME = k.TABLE_NAME
                    AND c.COLUMN_NAME = k.COLUMN_NAME
                    AND k.REFERENCED_TABLE_NAME IS NOT NULL
                WHERE c.TABLE_SCHEMA = @schemaName
                ORDER BY c.TABLE_NAME, c.ORDINAL_POSITION
                """;

            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@schemaName", connection.Database);

            using var reader = command.ExecuteReader();

            DatabaseTableSchema? currentTable = null;

            while (reader.Read())
            {
                var tableName = reader.GetString("TABLE_NAME");

                if (currentTable == null || currentTable.Name != tableName)
                {
                    currentTable = new DatabaseTableSchema { Name = tableName };
                    model.Tables.Add(currentTable);
                }

                var referencedTable = reader["REFERENCED_TABLE_NAME"] as string;
                var referencedColumn = reader["REFERENCED_COLUMN_NAME"] as string;

                var column = new DatabaseColumnSchema
                {
                    Name = reader.GetString("COLUMN_NAME"),
                    DataType = reader.GetString("COLUMN_TYPE"),
                    IsNullable = reader.GetString("IS_NULLABLE") == "YES",
                    IsPrimaryKey = reader.GetString("COLUMN_KEY") == "PRI",
                    IsForeignKey = !string.IsNullOrWhiteSpace(referencedTable),
                    ReferencedTable = referencedTable,
                    ReferencedColumn = referencedColumn
                };

                currentTable.Columns.Add(column);

                if (column.IsForeignKey)
                {
                    model.Relations.Add(new DatabaseRelationSchema
                    {
                        SourceTable = tableName,
                        SourceColumn = column.Name,
                        TargetTable = referencedTable!,
                        TargetColumn = referencedColumn ?? "id"
                    });
                }
            }

            AddKnownRelations(model);
            model.Relations = model.Relations
                .DistinctBy(r => new
                {
                    r.SourceTable,
                    r.SourceColumn,
                    r.TargetTable,
                    r.TargetColumn
                })
                .OrderBy(r => r.TargetTable)
                .ThenBy(r => r.SourceTable)
                .ThenBy(r => r.SourceColumn)
                .ToList();

            return View(model);
        }

        public IActionResult Details(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                return BadRequest();
            }

            using var connection = CreateConnection();
            connection.Open();

            const string tableCheckQuery = """
                SELECT COUNT(*)
                FROM INFORMATION_SCHEMA.TABLES
                WHERE TABLE_SCHEMA = @schemaName
                  AND TABLE_NAME = @tableName
                """;

            using (var tableCheckCommand = new MySqlCommand(tableCheckQuery, connection))
            {
                tableCheckCommand.Parameters.AddWithValue("@schemaName", connection.Database);
                tableCheckCommand.Parameters.AddWithValue("@tableName", tableName);

                if (Convert.ToInt32(tableCheckCommand.ExecuteScalar()) == 0)
                {
                    return NotFound();
                }
            }

            var rows = new List<Dictionary<string, object>>();
            var escapedTableName = tableName.Replace("`", "``");
            var query = $"SELECT * FROM `{escapedTableName}` LIMIT 50";

            using var command = new MySqlCommand(query, connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                var row = new Dictionary<string, object>();

                for (var i = 0; i < reader.FieldCount; i++)
                {
                    row[reader.GetName(i)] = reader.GetValue(i);
                }

                rows.Add(row);
            }

            ViewBag.TableName = tableName;
            return View(rows);
        }

        private MySqlConnection CreateConnection()
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("DefaultConnection bağlantı bilgisi bulunamadı.");
            }

            return new MySqlConnection(connectionString);
        }

        private static void AddKnownRelations(DatabaseSchemaViewModel model)
        {
            var knownRelations = new[]
            {
                ("sales", "customer_id", "customers", "id"),
                ("sales", "employee_id", "employees", "id"),
                ("sale_details", "sale_id", "sales", "id"),
                ("sale_details", "product_id", "products", "id"),
                ("products", "category_id", "categories", "id"),
                ("products", "supplier_id", "suppliers", "id"),
                ("employee_leaves", "employee_id", "employees", "id"),
                ("employee_shifts", "employee_id", "employees", "id"),
                ("role_permissions", "role_id", "roles", "id"),
                ("role_permissions", "permission_id", "permissions", "id"),
                ("user_roles", "employee_id", "employees", "id"),
                ("user_roles", "role_id", "roles", "id"),
                ("purchase_orders", "supplier_id", "suppliers", "id"),
                ("purchase_order_items", "purchase_order_id", "purchase_orders", "id"),
                ("purchase_order_items", "product_id", "products", "id"),
                ("stock_movements", "product_id", "products", "id")
            };

            foreach (var relation in knownRelations)
            {
                var sourceTable = model.Tables.FirstOrDefault(t => t.Name == relation.Item1);
                var targetTable = model.Tables.FirstOrDefault(t => t.Name == relation.Item3);
                var sourceColumn = sourceTable?.Columns.FirstOrDefault(c => c.Name == relation.Item2);
                var targetColumnExists = targetTable?.Columns.Any(c => c.Name == relation.Item4) == true;

                if (sourceColumn == null || !targetColumnExists)
                {
                    continue;
                }

                sourceColumn.IsForeignKey = true;
                sourceColumn.ReferencedTable ??= relation.Item3;
                sourceColumn.ReferencedColumn ??= relation.Item4;

                model.Relations.Add(new DatabaseRelationSchema
                {
                    SourceTable = relation.Item1,
                    SourceColumn = relation.Item2,
                    TargetTable = relation.Item3,
                    TargetColumn = relation.Item4
                });
            }
        }
    }
}
