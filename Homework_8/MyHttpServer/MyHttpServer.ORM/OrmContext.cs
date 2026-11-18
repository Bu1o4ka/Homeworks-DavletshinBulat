using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;

namespace MyHttpServer.ORM;

public sealed class OrmContext : IOrmContext
{
    private readonly string _connectionString;

    public OrmContext(string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        _connectionString = connectionString;
    }

    public async Task<IReadOnlyList<T>> GetAllAsync<T>(string tableName, CancellationToken cancellationToken = default) where T : class, new()
    {
        string safeTableName = EscapeIdentifier(tableName);
        string sql = $"SELECT * FROM {safeTableName}";
        var result = new List<T>();

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        PropertyInfo[] properties = typeof(T)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(property => property.CanWrite && property.SetMethod?.IsPublic == true)
            .ToArray();

        var columns = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (int index = 0; index < reader.FieldCount; index++)
            columns[reader.GetName(index)] = index;

        while (await reader.ReadAsync(cancellationToken))
        {
            T entity = new();
            foreach (PropertyInfo property in properties)
            {
                if (!columns.TryGetValue(property.Name, out int ordinal)) continue;
                if (await reader.IsDBNullAsync(ordinal, cancellationToken))
                {
                    if (!property.PropertyType.IsValueType || Nullable.GetUnderlyingType(property.PropertyType) != null)
                        property.SetValue(entity, null);
                    continue;
                }

                object value = reader.GetValue(ordinal);
                Type targetType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                object convertedValue = targetType.IsInstanceOfType(value) ? value
                    : targetType.IsEnum ? Enum.ToObject(targetType, value)
                    : Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
                property.SetValue(entity, convertedValue);
            }
            result.Add(entity);
        }
        return result;
    }

    public async Task<int> InsertAsync<T>(string tableName, T entity, CancellationToken cancellationToken = default) where T : class
    {
        ArgumentNullException.ThrowIfNull(entity);
        string safeTable = EscapeIdentifier(tableName);

        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite && !p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        string columns = string.Join(", ", properties.Select(p => $"[{p.Name}]"));
        string values = string.Join(", ", properties.Select((_, i) => $"@p{i}"));
        
        string sql = $"INSERT INTO {safeTable} ({columns}) VALUES ({values}); SELECT CAST(SCOPE_IDENTITY() AS INT);";

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand(sql, connection);

        for (int i = 0; i < properties.Length; i++)
            command.Parameters.AddWithValue($"@p{i}", properties[i].GetValue(entity) ?? DBNull.Value);

        var newId = await command.ExecuteScalarAsync(cancellationToken);

        if (newId != null && newId != DBNull.Value)
        {
            var idProp = typeof(T).GetProperty("Id");
            if (idProp != null)
                idProp.SetValue(entity, Convert.ChangeType(newId, idProp.PropertyType));
        }

        return 1;
    }

    public async Task<int> UpdateAsync<T>(string tableName, int id, T entity, CancellationToken cancellationToken = default) where T : class
    {
        ArgumentNullException.ThrowIfNull(entity);
        string safeTable = EscapeIdentifier(tableName);

        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite && !p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        string setClause = string.Join(", ", properties.Select((p, i) => $"[{p.Name}] = @p{i}"));
        string sql = $"UPDATE {safeTable} SET {setClause} WHERE [Id] = @id;";

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand(sql, connection);

        for (int i = 0; i < properties.Length; i++)
            command.Parameters.AddWithValue($"@p{i}", properties[i].GetValue(entity) ?? DBNull.Value);
        
        command.Parameters.AddWithValue("@id", id);

        return await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<int> DeleteAsync(string tableName, int id, CancellationToken cancellationToken = default)
    {
        string safeTable = EscapeIdentifier(tableName);
        string sql = $"DELETE FROM {safeTable} WHERE [Id] = @id;";

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand(sql, connection);
        
        command.Parameters.AddWithValue("@id", id);

        return await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static string EscapeIdentifier(string identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier) || !Regex.IsMatch(identifier, @"^[A-Za-z_][A-Za-z0-9_]*$"))
            throw new ArgumentException("Недопустимое имя SQL-таблицы.", nameof(identifier));
        
        return $"[{identifier}]";
    }
}