using System.Security.Claims;
using ASP_MessageBoard.Common.Exceptions;
using ASP_MessageBoard.Services.DTOs;
using ASP_MessageBoard.Services.Interfaces;
using ASP_MessageBoard.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASP_MessageBoard.Controllers
{
    public sealed class PostsController : Controller
    {
        private readonly IPostService _postService;
        private readonly ICommentService _commentService;

        public PostsController(IPostService postService, ICommentService commentService)
        {
            _postService = postService;
            _commentService = commentService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var posts = await _postService.GetAllAsync(cancellationToken);

            return View(posts);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
        {
            var post = await _postService.GetByIdAsync(id, cancellationToken);

            if (post is null)
            {
                return NotFound();
            }

            var comments = await _commentService.GetByPostIdAsync(id, cancellationToken);

            var model = new PostDetailsViewModel
            {
                Post = post,
                Comments = comments,
                NewComment = new CreateCommentViewModel { PostId = post.PostId },
            };

            return View(model);
        }

        [Authorize]
        [HttpGet]
        public IActionResult Create()
        {
            return View(new CreatePostViewModel());
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            CreatePostViewModel model,
            CancellationToken cancellationToken
        )
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (!TryGetCurrentUserId(out var userId))
            {
                return Challenge();
            }

            var request = new CreatePostRequest
            {
                UserId = userId,
                Content = model.Content,
                Image = model.Image,
            };

            try
            {
                var post = await _postService.CreateAsync(request, cancellationToken);

                TempData["SuccessMessage"] = "文章已成功發表。";

                return RedirectToAction(nameof(Details), new { id = post.PostId });
            }
            catch (ImageValidationException exception)
            {
                ModelState.AddModelError(nameof(model.Image), exception.Message);

                return View(model);
            }
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return Challenge();
            }

            var post = await _postService.GetByIdAsync(id, cancellationToken);

            if (post is null)
            {
                return NotFound();
            }

            if (post.UserId != userId)
            {
                return Forbid();
            }

            var model = new EditPostViewModel
            {
                PostId = post.PostId,
                Content = post.Content,
                ExistingImagePath = post.ImagePath,
            };

            return View(model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            EditPostViewModel model,
            CancellationToken cancellationToken
        )
        {
            if (id != model.PostId)
            {
                return BadRequest();
            }

            if (!TryGetCurrentUserId(out var userId))
            {
                return Challenge();
            }

            if (!ModelState.IsValid)
            {
                var found = await PopulateExistingImagePathAsync(model, userId, cancellationToken);

                if (!found)
                {
                    return NotFound();
                }

                return View(model);
            }

            var request = new UpdatePostRequest
            {
                PostId = model.PostId,
                UserId = userId,
                Content = model.Content,
                NewImage = model.Image,
                RemoveImage = model.RemoveImage,
            };

            try
            {
                var post = await _postService.UpdateAsync(request, cancellationToken);

                TempData["SuccessMessage"] = "文章已成功更新。";

                return RedirectToAction(nameof(Details), new { id = post.PostId });
            }
            catch (ImageValidationException exception)
            {
                ModelState.AddModelError(nameof(model.Image), exception.Message);

                var found = await PopulateExistingImagePathAsync(model, userId, cancellationToken);

                if (!found)
                {
                    return NotFound();
                }

                return View(model);
            }
            catch (PostNotFoundException)
            {
                return NotFound();
            }
            catch (PostAccessDeniedException)
            {
                return Forbid();
            }
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return Challenge();
            }

            try
            {
                await _postService.DeleteAsync(id, userId, cancellationToken);

                TempData["SuccessMessage"] = "文章已成功刪除。";

                return RedirectToAction(nameof(Index));
            }
            catch (PostNotFoundException)
            {
                return NotFound();
            }
            catch (PostAccessDeniedException)
            {
                return Forbid();
            }
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(
            [Bind(Prefix = "NewComment")] CreateCommentViewModel model,
            CancellationToken cancellationToken
        )
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return Challenge();
            }

            if (!ModelState.IsValid)
            {
                return await BuildDetailsViewAsync(model.PostId, model, cancellationToken);
            }

            var request = new CreateCommentRequest
            {
                UserId = userId,
                PostId = model.PostId,
                Content = model.Content,
            };

            try
            {
                await _commentService.CreateAsync(request, cancellationToken);

                TempData["SuccessMessage"] = "留言已新增。";

                return RedirectToAction(nameof(Details), new { id = model.PostId });
            }
            catch (PostNotFoundException)
            {
                return NotFound();
            }
        }

        private bool TryGetCurrentUserId(out int userId)
        {
            var userIdText = User.FindFirstValue(ClaimTypes.NameIdentifier);

            return int.TryParse(userIdText, out userId);
        }

        private async Task<bool> PopulateExistingImagePathAsync(
            EditPostViewModel model,
            int userId,
            CancellationToken cancellationToken
        )
        {
            var post = await _postService.GetByIdAsync(model.PostId, cancellationToken);

            if (post is null)
            {
                return false;
            }

            if (post.UserId != userId)
            {
                return false;
            }

            model.ExistingImagePath = post.ImagePath;
            return true;
        }

        private async Task<IActionResult> BuildDetailsViewAsync(
            int postId,
            CreateCommentViewModel newComment,
            CancellationToken cancellationToken
        )
        {
            var post = await _postService.GetByIdAsync(postId, cancellationToken);

            if (post is null)
            {
                return NotFound();
            }

            var comments = await _commentService.GetByPostIdAsync(postId, cancellationToken);

            var model = new PostDetailsViewModel
            {
                Post = post,
                Comments = comments,
                NewComment = newComment,
            };

            return View(nameof(Details), model);
        }
    }
}
