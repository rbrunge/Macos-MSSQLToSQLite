namespace ClassLibrary;

/// <summary>
///     Contains the schema of a single DB column.
/// </summary>
public class ColumnSchema
{
    public string ColumnName;

    public string ColumnType;

    public string DefaultValue;

    public bool? IsCaseSensitivite = null;

    public bool IsIdentity;

    public bool IsNullable;

    public int Length;
}