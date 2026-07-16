using System.Data;
using System.Data.Common;
using ASP_MessageBoard.Models.Entities;
using ASP_MessageBoard.Repositories.Interfaces;

namespace ASP_MessageBoard.Repositories.Implementations
{
    /// <summary>
    /// User帳號相關操作
    /// </summary>
    public class UserRepository : IUserRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public UserRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<User> CreateAsync(User user, CancellationToken cancellationToken = default)
        {
            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = "dbo.usp_User_Create";
            command.CommandType = CommandType.StoredProcedure;

            AddParameter(command, "@UserName", DbType.String, user.UserName, 50);
            AddParameter(command, "@PhoneNumber", DbType.AnsiString, user.PhoneNumber, 20);
            AddParameter(command, "@PasswordHash", DbType.String, user.PasswordHash, 512);
            AddParameter(command, "@Email", DbType.String, user.Email, 254);
            AddParameter(command, "@CoverImagePath", DbType.String, user.CoverImagePath, 500);
            AddParameter(command, "@Biography", DbType.String, user.Biography, 500);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            if (!await reader.ReadAsync(cancellationToken))
            {
                throw new InvalidOperationException(
                    "建立使用者後，Stored Procedure 未回傳使用者資料。"
                );
            }

            return MapUser(reader);
        }

        public async Task<User?> GetByIdAsync(int userId, CancellationToken cancellationToken = default)
        {
            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = "dbo.usp_User_GetById";
            command.CommandType = CommandType.StoredProcedure;

            AddParameter(command, "@UserId", DbType.Int32, userId);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            return await reader.ReadAsync(cancellationToken) ? MapUser(reader) : null;
        }

        public async Task<User?> GetByPhoneNumberAsync(
            string phoneNumber,
            CancellationToken cancellationToken = default
        )
        {
            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = "dbo.usp_User_GetByPhoneNumber";
            command.CommandType = CommandType.StoredProcedure;

            AddParameter(command, "@PhoneNumber", DbType.AnsiString, phoneNumber, size: 20);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            return await reader.ReadAsync(cancellationToken) ? MapUser(reader) : null;
        }

        private static void AddParameter(
            DbCommand command,
            string name,
            DbType type,
            object? value,
            int? size = null
        )
        {
            var parameter = command.CreateParameter();

            parameter.ParameterName = name;
            parameter.DbType = type;
            parameter.Value = value ?? DBNull.Value;

            if (size.HasValue)
            {
                parameter.Size = size.Value;
            }

            command.Parameters.Add(parameter);
        }

        private static User MapUser(DbDataReader reader)
        {
            return new User
            {
                UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                UserName = reader.GetString(reader.GetOrdinal("UserName")),
                PhoneNumber = reader.GetString(reader.GetOrdinal("PhoneNumber")),
                Email = reader.GetString(reader.GetOrdinal("Email")),
                PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
                CoverImagePath = GetNullableString(reader, "CoverImagePath"),
                Biography = GetNullableString(reader, "Biography"),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                UpdatedAt = GetNullableDateTime(reader, "UpdatedAt"),
            };
        }

        private static string? GetNullableString(DbDataReader reader, string columnName)
        {
            var ordinal = reader.GetOrdinal(columnName);

            return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
        }

        private static DateTime? GetNullableDateTime(DbDataReader reader, string columnName)
        {
            var ordinal = reader.GetOrdinal(columnName);

            return reader.IsDBNull(ordinal) ? null : reader.GetDateTime(ordinal);
        }
    }
}
