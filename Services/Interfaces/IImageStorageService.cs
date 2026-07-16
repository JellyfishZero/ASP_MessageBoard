namespace ASP_MessageBoard.Services.Interfaces
{
    public interface IImageStorageService
    {
        Task<string> SavePostImageAsync(
            IFormFile image,
            CancellationToken cancellationToken = default
        );

        Task DeleteAsync(string? imagePath, CancellationToken cancellationToken = default);
    }
}
