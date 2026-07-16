using System.Data;
using System.Data.Common;
using ASP_MessageBoard.Models.Entities;
using ASP_MessageBoard.Repositories.Interfaces;
using ASP_MessageBoard.Repositories.Models;

namespace ASP_MessageBoard.Repositories.Implementations
{
    public class PostRepository : IPostRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public PostRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IReadOnlyList<PostRecord>> GetAllAsync(
            CancellationToken cancellationToken = default
        )
        {
            await using var connection = _connectionFactory.CreateConnection();

            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = "dbo.usp_Post_GetAll";
            command.CommandType = CommandType.StoredProcedure;

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            var posts = new List<PostRecord>();

            while (await reader.ReadAsync(cancellationToken))
            {
                posts.Add(MapPostRecord(reader));
            }

            return posts;
        }

        public async Task<PostRecord?> GetByIdAsync(
            int postId,
            CancellationToken cancellationToken = default
        )
        {
            await using var connection = _connectionFactory.CreateConnection();

            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = "dbo.usp_Post_GetById";
            command.CommandType = CommandType.StoredProcedure;

            AddParameter(command, "@PostId", DbType.Int32, postId);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            return await reader.ReadAsync(cancellationToken) ? MapPostRecord(reader) : null;
        }

        public async Task<PostRecord> CreateAsync(
            Post post,
            CancellationToken cancellationToken = default
        )
        {
            await using var connection = _connectionFactory.CreateConnection();

            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = "dbo.usp_Post_Create";
            command.CommandType = CommandType.StoredProcedure;

            AddParameter(command, "@UserId", DbType.Int32, post.UserId);

            AddParameter(command, "@Content", DbType.String, post.Content, 2000);

            AddParameter(command, "@ImagePath", DbType.String, post.ImagePath, 500);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            if (!await reader.ReadAsync(cancellationToken))
            {
                throw new InvalidOperationException(
                    "建立文章後，Stored Procedure 未回傳文章資料。"
                );
            }

            return MapPostRecord(reader);
        }

        public async Task<PostRecord> UpdateAsync(
            Post post,
            CancellationToken cancellationToken = default
        )
        {
            await using var connection = _connectionFactory.CreateConnection();

            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = "dbo.usp_Post_Update";
            command.CommandType = CommandType.StoredProcedure;

            AddParameter(command, "@PostId", DbType.Int32, post.PostId);

            AddParameter(command, "@UserId", DbType.Int32, post.UserId);

            AddParameter(command, "@Content", DbType.String, post.Content, 2000);

            AddParameter(command, "@ImagePath", DbType.String, post.ImagePath, 500);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            if (!await reader.ReadAsync(cancellationToken))
            {
                throw new InvalidOperationException(
                    "更新文章後，Stored Procedure 未回傳文章資料。"
                );
            }

            return MapPostRecord(reader);
        }

        public async Task DeleteAsync(
            int postId,
            int userId,
            CancellationToken cancellationToken = default
        )
        {
            await using var connection = _connectionFactory.CreateConnection();

            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = "dbo.usp_Post_Delete";
            command.CommandType = CommandType.StoredProcedure;

            AddParameter(command, "@PostId", DbType.Int32, postId);

            AddParameter(command, "@UserId", DbType.Int32, userId);

            var result = await command.ExecuteScalarAsync(cancellationToken);

            if (result is null || result == DBNull.Value || !Convert.ToBoolean(result))
            {
                throw new InvalidOperationException("Stored Procedure 未確認文章已刪除。");
            }
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

        private static PostRecord MapPostRecord(DbDataReader reader)
        {
            return new PostRecord
            {
                PostId = reader.GetInt32(reader.GetOrdinal("PostId")),

                UserId = reader.GetInt32(reader.GetOrdinal("UserId")),

                UserName = reader.GetString(reader.GetOrdinal("UserName")),

                Content = reader.GetString(reader.GetOrdinal("Content")),

                ImagePath = GetNullableString(reader, "ImagePath"),

                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),

                UpdatedAt = GetNullableDateTime(reader, "UpdatedAt"),

                UserCoverImagePath = GetNullableString(reader, "UserCoverImagePath"),

                UserBiography = GetNullableString(reader, "UserBiography"),
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
