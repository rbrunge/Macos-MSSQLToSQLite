namespace ClassLibrary;

public class IndexSchema
{
    public List<IndexColumn> Columns;
    public string IndexName;

    public bool IsUnique;
}

public class IndexColumn
{
    public string ColumnName;
    public bool IsAscending;
}