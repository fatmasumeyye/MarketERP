namespace MarketERP.Models
{
    public class DatabaseSchemaViewModel
    {
        public List<DatabaseTableSchema> Tables { get; set; } = new();

        public List<DatabaseRelationSchema> Relations { get; set; } = new();
    }

    public class DatabaseTableSchema
    {
        public string Name { get; set; } = string.Empty;

        public List<DatabaseColumnSchema> Columns { get; set; } = new();
    }

    public class DatabaseColumnSchema
    {
        public string Name { get; set; } = string.Empty;

        public string DataType { get; set; } = string.Empty;

        public bool IsPrimaryKey { get; set; }

        public bool IsForeignKey { get; set; }

        public bool IsNullable { get; set; }

        public string? ReferencedTable { get; set; }

        public string? ReferencedColumn { get; set; }
    }

    public class DatabaseRelationSchema
    {
        public string SourceTable { get; set; } = string.Empty;

        public string SourceColumn { get; set; } = string.Empty;

        public string TargetTable { get; set; } = string.Empty;

        public string TargetColumn { get; set; } = string.Empty;
    }
}
