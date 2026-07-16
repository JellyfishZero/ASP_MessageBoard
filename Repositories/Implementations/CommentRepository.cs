using System.Data;
using System.Data.Common;
using ASP_MessageBoard.Models.Entities;
using ASP_MessageBoard.Repositories.Interfaces;
using ASP_MessageBoard.Repositories.Models;

namespace ASP_MessageBoard.Repositories.Implementations
{
    public sealed class CommentRepository : ICommentRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public CommentRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IReadOnlyList<CommentRecord>> GetByPostIdAsync(
            int postId,
            CancellationToken cancellationToken = default
        )
        {
            await using var connection = _connectionFactory.CreateConnection();

            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = "dbo.usp_Comment_GetByPostId";
            command.CommandType = CommandType.StoredProcedure;

            AddParameter(command, "@PostId", DbType.Int32, postId);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            var comments = new List<CommentRecord>();

            while (await reader.ReadAsync(cancellationToken))
            {
                comments.Add(MapCommentRecord(reader));
            }

            return comments;
        }

        public async Task<CommentRecord> CreateAsync(
            Comment comment,
            CancellationToken cancellationToken = default
        )
        {
            await using var connection = _connectionFactory.CreateConnection();

            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = "dbo.usp_Comment_Create";
            command.CommandType = CommandType.StoredProcedure;

            AddParameter(command, "@UserId", DbType.Int32, comment.UserId);

            AddParameter(command, "@PostId", DbType.Int32, comment.PostId);

            AddParameter(command, "@Content", DbType.String, comment.Content, 1000);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            if (!await reader.ReadAsync(cancellationToken))
            {
                throw new InvalidOperationException(
                    "建立留言後，Stored Procedure 未回傳留言資料。"
                );
            }

            return MapCommentRecord(reader);
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

        private static CommentRecord MapCommentRecord(DbDataReader reader)
        {
            return new CommentRecord
            {
                CommentId = reader.GetInt32(reader.GetOrdinal("CommentId")),

                UserId = reader.GetInt32(reader.GetOrdinal("UserId")),

                UserName = reader.GetString(reader.GetOrdinal("UserName")),

                PostId = reader.GetInt32(reader.GetOrdinal("PostId")),

                Content = reader.GetString(reader.GetOrdinal("Content")),

                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),

                UserCoverImagePath = GetNullableString(reader, "UserCoverImagePath"),
            };
        }

        private static string? GetNullableString(DbDataReader reader, string columnName)
        {
            var ordinal = reader.GetOrdinal(columnName);

            return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
        }
    }
}
