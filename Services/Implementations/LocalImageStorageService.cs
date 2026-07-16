using ASP_MessageBoard.Common.Exceptions;
using ASP_MessageBoard.Services.Interfaces;

namespace ASP_MessageBoard.Services.Implementations
{
    public sealed class LocalImageStorageService : IImageStorageService
    {
        private const long MaxFileSize = 5 * 1024 * 1024;

        private static readonly Dictionary<string, byte[][]> AllowedFileSignatures = new(
            StringComparer.OrdinalIgnoreCase
        )
        {
            [".jpg"] =
            [
                [0xFF, 0xD8, 0xFF],
            ],
            [".jpeg"] =
            [
                [0xFF, 0xD8, 0xFF],
            ],
            [".png"] =
            [
                [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A],
            ],
            [".webp"] =
            [
                // WebP 另外檢查 RIFF....WEBP 結構。
                [0x52, 0x49, 0x46, 0x46],
            ],
        };

        private static readonly HashSet<string> AllowedContentTypes = new(
            StringComparer.OrdinalIgnoreCase
        )
        {
            "image/jpeg",
            "image/png",
            "image/webp",
        };

        private readonly string _webRootFullPath;

        public LocalImageStorageService(IWebHostEnvironment environment)
        {
            if (string.IsNullOrWhiteSpace(environment.WebRootPath))
            {
                throw new InvalidOperationException("WebRootPath 尚未設定。");
            }

            _webRootFullPath = Path.GetFullPath(environment.WebRootPath);
        }

        public Task<string> SavePostImageAsync(
            IFormFile image,
            CancellationToken cancellationToken = default
        )
        {
            return SaveImageAsync(image, "posts", cancellationToken);
        }

        public Task<string> SaveCoverImageAsync(
            IFormFile image,
            CancellationToken cancellationToken = default
        )
        {
            return SaveImageAsync(image, "covers", cancellationToken);
        }

        public Task DeleteAsync(string? imagePath, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(imagePath))
            {
                return Task.CompletedTask;
            }

            var isAllowedPath =
                imagePath.StartsWith("/uploads/posts/", StringComparison.OrdinalIgnoreCase)
                || imagePath.StartsWith("/uploads/covers/", StringComparison.OrdinalIgnoreCase);

            if (!isAllowedPath)
            {
                throw new InvalidOperationException("圖片路徑不在允許的目錄內。");
            }

            var relativePath = imagePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);

            var filePath = Path.GetFullPath(Path.Combine(_webRootFullPath, relativePath));

            EnsurePathIsInsideWebRoot(filePath);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            return Task.CompletedTask;
        }

        private static void ValidateBasicFileInformation(IFormFile image)
        {
            if (image.Length <= 0)
            {
                throw new ImageValidationException("圖片檔案不可為空。");
            }

            if (image.Length > MaxFileSize)
            {
                throw new ImageValidationException("圖片大小不可超過 5 MB。");
            }

            var extension = Path.GetExtension(image.FileName);

            if (
                string.IsNullOrWhiteSpace(extension)
                || !AllowedFileSignatures.ContainsKey(extension)
            )
            {
                throw new ImageValidationException("只允許 JPG、PNG 或 WebP 圖片。");
            }

            if (!AllowedContentTypes.Contains(image.ContentType))
            {
                throw new ImageValidationException("圖片格式不正確。");
            }
        }

        private static async Task ValidateFileSignatureAsync(
            IFormFile image,
            string extension,
            CancellationToken cancellationToken
        )
        {
            await using var stream = image.OpenReadStream();

            var header = new byte[12];

            var bytesRead = await stream.ReadAsync(
                header.AsMemory(0, header.Length),
                cancellationToken
            );

            if (extension == ".webp")
            {
                var isWebP =
                    bytesRead >= 12
                    && header.AsSpan(0, 4).SequenceEqual("RIFF"u8)
                    && header.AsSpan(8, 4).SequenceEqual("WEBP"u8);

                if (!isWebP)
                {
                    throw new ImageValidationException("圖片內容與 WebP 格式不符。");
                }

                return;
            }

            var isValid = AllowedFileSignatures[extension]
                .Any(signature =>
                    bytesRead >= signature.Length
                    && header.AsSpan(0, signature.Length).SequenceEqual(signature)
                );

            if (!isValid)
            {
                throw new ImageValidationException("圖片內容與副檔名不符。");
            }
        }

        private async Task<string> SaveImageAsync(
            IFormFile image,
            string category,
            CancellationToken cancellationToken
        )
        {
            ValidateBasicFileInformation(image);

            var extension = Path.GetExtension(image.FileName).ToLowerInvariant();

            await ValidateFileSignatureAsync(image, extension, cancellationToken);

            var uploadDirectory = Path.GetFullPath(
                Path.Combine(_webRootFullPath, "uploads", category)
            );

            EnsurePathIsInsideWebRoot(uploadDirectory);

            Directory.CreateDirectory(uploadDirectory);

            var fileName = $"{Guid.NewGuid():N}{extension}";

            var filePath = Path.GetFullPath(Path.Combine(uploadDirectory, fileName));

            EnsurePathIsInsideDirectory(filePath, uploadDirectory);

            try
            {
                await using var stream = new FileStream(
                    filePath,
                    FileMode.CreateNew,
                    FileAccess.Write,
                    FileShare.None,
                    bufferSize: 81920,
                    useAsync: true
                );

                await image.CopyToAsync(stream, cancellationToken);
            }
            catch
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                throw;
            }

            return $"/uploads/{category}/{fileName}";
        }

        private void EnsurePathIsInsideWebRoot(string path)
        {
            var webRootPrefix =
                _webRootFullPath.TrimEnd(
                    Path.DirectorySeparatorChar,
                    Path.AltDirectorySeparatorChar
                ) + Path.DirectorySeparatorChar;

            if (!path.StartsWith(webRootPrefix, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("偵測到不合法的圖片路徑。");
            }
        }

        private static void EnsurePathIsInsideDirectory(string filePath, string directory)
        {
            var directoryPrefix =
                directory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                + Path.DirectorySeparatorChar;

            if (!filePath.StartsWith(directoryPrefix, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("偵測到不合法的圖片儲存路徑。");
            }
        }
    }
}
