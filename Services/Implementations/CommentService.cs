using ASP_MessageBoard.Common.Exceptions;
using ASP_MessageBoard.Models.Entities;
using ASP_MessageBoard.Repositories.Interfaces;
using ASP_MessageBoard.Repositories.Models;
using ASP_MessageBoard.Services.DTOs;
using ASP_MessageBoard.Services.Interfaces;
using Microsoft.Data.SqlClient;

namespace ASP_MessageBoard.Services.Implementations
{
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _commentRepository;

        public CommentService(ICommentRepository commentRepository)
        {
            _commentRepository = commentRepository;
        }

        public async Task<IReadOnlyList<CommentDetails>> GetByPostIdAsync(
            int postId,
            CancellationToken cancellationToken = default
        )
        {
            var records = await _commentRepository.GetByPostIdAsync(postId, cancellationToken);

            return records.Select(MapCommentDetails).ToList();
        }

        public async Task<CommentDetails> CreateAsync(
            CreateCommentRequest request,
            CancellationToken cancellationToken = default
        )
        {
            var comment = new Comment
            {
                UserId = request.UserId,
                PostId = request.PostId,
                Content = request.Content.Trim(),
            };

            try
            {
                var record = await _commentRepository.CreateAsync(comment, cancellationToken);

                return MapCommentDetails(record);
            }
            catch (SqlException exception) when (exception.Number == 50020)
            {
                throw new PostNotFoundException();
            }
        }

        private static CommentDetails MapCommentDetails(CommentRecord record)
        {
            return new CommentDetails
            {
                CommentId = record.CommentId,
                UserId = record.UserId,
                UserName = record.UserName,
                PostId = record.PostId,
                Content = record.Content,
                CreatedAt = record.CreatedAt,
            };
        }
    }
}
