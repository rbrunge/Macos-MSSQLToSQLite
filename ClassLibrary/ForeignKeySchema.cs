namespace ClassLibrary;

public class ForeignKeySchema
{
    public bool CascadeOnDelete;

    public string ColumnName;

    public string ForeignColumnName;

    public string ForeignTableName;

    public bool IsNullable;
    public string TableName;
}