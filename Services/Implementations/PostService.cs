using ASP_MessageBoard.Common.Exceptions;
using ASP_MessageBoard.Models.Entities;
using ASP_MessageBoard.Repositories.Interfaces;
using ASP_MessageBoard.Repositories.Models;
using ASP_MessageBoard.Services.DTOs;
using ASP_MessageBoard.Services.Interfaces;
using Microsoft.Data.SqlClient;

namespace ASP_MessageBoard.Services.Implementations
{
    public class PostService : IPostService
    {
        private readonly IPostRepository _postRepository;
        private readonly IImageStorageService _imageStorageService;
        private readonly ILogger<PostService> _logger;

        public PostService(
            IPostRepository postRepository,
            IImageStorageService imageStorageService,
            ILogger<PostService> logger
        )
        {
            _postRepository = postRepository;
            _imageStorageService = imageStorageService;
            _logger = logger;
        }

        public async Task<IReadOnlyList<PostDetails>> GetAllAsync(
            CancellationToken cancellationToken = default
        )
        {
            var records = await _postRepository.GetAllAsync(cancellationToken);

            return records.Select(MapPostDetails).ToList();
        }

        public async Task<PostDetails?> GetByIdAsync(
            int postId,
            CancellationToken cancellationToken = default
        )
        {
            var record = await _postRepository.GetByIdAsync(postId, cancellationToken);

            return record is null ? null : MapPostDetails(record);
        }

        public async Task<PostDetails> CreateAsync(
            CreatePostRequest request,
            CancellationToken cancellationToken = default
        )
        {
            var content = request.Content.Trim();
            string? imagePath = null;

            if (request.Image is not null)
            {
                imagePath = await _imageStorageService.SavePostImageAsync(
                    request.Image,
                    cancellationToken
                );
            }

            var post = new Post
            {
                UserId = request.UserId,
                Content = content,
                ImagePath = imagePath,
            };

            try
            {
                var record = await _postRepository.CreateAsync(post, cancellationToken);

                return MapPostDetails(record);
            }
            catch
            {
                // 圖片已寫入，但資料庫新增失敗時進行補償刪除。
                if (imagePath is not null)
                {
                    await TryDeleteImageAsync(imagePath);
                }

                throw;
            }
        }

        public async Task<PostDetails> UpdateAsync(
            UpdatePostRequest request,
            CancellationToken cancellationToken = default
        )
        {
            var existingPost = await _postRepository.GetByIdAsync(
                request.PostId,
                cancellationToken
            );

            if (existingPost is null)
            {
                throw new PostNotFoundException();
            }

            if (existingPost.UserId != request.UserId)
            {
                throw new PostAccessDeniedException();
            }

            var content = request.Content.Trim();
            var oldImagePath = existingPost.ImagePath;
            string? newImagePath = null;

            if (request.NewImage is not null)
            {
                newImagePath = await _imageStorageService.SavePostImageAsync(
                    request.NewImage,
                    cancellationToken
                );
            }

            var targetImagePath = newImagePath ?? (request.RemoveImage ? null : oldImagePath);

            var post = new Post
            {
                PostId = request.PostId,
                UserId = request.UserId,
                Content = content,
                ImagePath = targetImagePath,
            };

            PostRecord updatedRecord;

            try
            {
                updatedRecord = await _postRepository.UpdateAsync(post, cancellationToken);
            }
            catch (SqlException exception) when (exception.Number == 50010)
            {
                if (newImagePath is not null)
                {
                    await TryDeleteImageAsync(newImagePath);
                }

                throw new PostAccessDeniedException(exception);
            }
            catch
            {
                // 新圖片已儲存，但資料庫更新失敗。
                if (newImagePath is not null)
                {
                    await TryDeleteImageAsync(newImagePath);
                }

                throw;
            }

            if (oldImagePath is not null && oldImagePath != targetImagePath)
            {
                await TryDeleteImageAsync(oldImagePath);
            }

            return MapPostDetails(updatedRecord);
        }

        public async Task DeleteAsync(
            int postId,
            int userId,
            CancellationToken cancellationToken = default
        )
        {
            var existingPost = await _postRepository.GetByIdAsync(postId, cancellationToken);

            if (existingPost is null)
            {
                throw new PostNotFoundException();
            }

            if (existingPost.UserId != userId)
            {
                throw new PostAccessDeniedException();
            }

            try
            {
                await _postRepository.DeleteAsync(postId, userId, cancellationToken);
            }
            catch (SqlException exception) when (exception.Number == 50011)
            {
                throw new PostAccessDeniedException(exception);
            }

            // 先確認資料庫刪除成功，再刪除圖片。
            if (existingPost.ImagePath is not null)
            {
                await TryDeleteImageAsync(existingPost.ImagePath);
            }
        }

        private async Task TryDeleteImageAsync(string imagePath)
        {
            try
            {
                await _imageStorageService.DeleteAsync(imagePath);
            }
            catch (Exception exception)
            {
                // 資料庫可能已成功異動，不應因清理舊圖片失敗，
                // 讓使用者誤以為資料庫操作也失敗。
                _logger.LogWarning(exception, "無法刪除文章圖片：{ImagePath}", imagePath);
            }
        }

        private static PostDetails MapPostDetails(PostRecord record)
        {
            return new PostDetails
            {
                PostId = record.PostId,
                UserId = record.UserId,
                UserName = record.UserName,
                Content = record.Content,
                ImagePath = record.ImagePath,
                CreatedAt = record.CreatedAt,
                UpdatedAt = record.UpdatedAt,
            };
        }
    }
}
