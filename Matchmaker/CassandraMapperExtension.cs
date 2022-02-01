using Cassandra.Mapping;
public static class CassandraMapperExtension
{
    public static void CreateIndex(this IMapper mapper, string tableName, string columnName)
    {
        mapper.Execute($"CREATE INDEX ON {tableName} ({columnName})");
    }

    public static void DropTableIfExists(this IMapper mapper, string tableName)
    {
        mapper.Execute($"DROP TABLE IF EXISTS {tableName}");
    }
}