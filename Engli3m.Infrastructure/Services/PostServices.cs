using Engli3m.Application.DTOs.Post;
using Engli3m.Application.Interfaces;
using Engli3m.Domain.Enities;
using Engli3m.Infrastructure.Helper;
using Microsoft.EntityFrameworkCore;

namespace Engli3m.Infrastructure.Services
{
    public class PostServices : IPostServices
    {
        private readonly string _videoFolder;
        private readonly string _imageFolder;
        private readonly EnglishDbContext _dbContext;
        private readonly INotificationService _notificationService;
        public PostServices(EnglishDbContext dbContext, INotificationService notificationService)
        {
            _dbContext    = dbContext;
            _videoFolder  = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "Posts", "videos");
            _imageFolder  = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "Posts", "images");

            // make sure both exist up front
            Directory.CreateDirectory(_videoFolder);
            Directory.CreateDirectory(_imageFolder);
            _notificationService=notificationService;
        }

        public async Task<bool> CreatePostAsync(int userId, CreatePostDto dto)
        {
            _ = await _dbContext.Users.FindAsync(userId)
                       ?? throw new InvalidOperationException("User not found.");

            await using var tx = await _dbContext.Database.BeginTransactionAsync();

            // 1) create the Post
            var post = new Post
            {
                Content   = dto.Content,
                CreatedAt = DateTime.UtcNow,
                UserId    = userId,
                Media     = []
            };
            _dbContext.Posts.Add(post);
            await _dbContext.SaveChangesAsync();

            foreach (var file in dto.MediaUrls)
            {
                var ext = FileHelper.GetExtension(file).ToLowerInvariant();
                string folder = ext is ".mp4" or ".avi" or ".mov"
                                ? _videoFolder
                                : _imageFolder;

                string saved = await FileHelper.SaveImageAsync(file, folder)
                                  ?? throw new InvalidOperationException("File saving failed.");

                string relativeDir = folder.Contains("videos") ? "videos" : "images";
                string relativeUrl = Path.Combine("uploads", "Posts", relativeDir, saved)
                                        .Replace("\\", "/");

                post.Media.Add(new PostMedia
                {
                    Url       = "/" + relativeUrl,
                    PostId    = post.Id,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _dbContext.SaveChangesAsync();
            await tx.CommitAsync();
            await _notificationService.SendToAllGradesAsync(
    "📢 إعلان هام",
    "تم إضافة منشور جديد، راجع التطبيق الآن."
);

            return true;
        }

        public async Task<bool> DeletePostAsync(int postId)
        {
            // load post including its media
            var post = await _dbContext.Posts
                .Include(p => p.Media)
                .FirstOrDefaultAsync(p => p.Id == postId);

            if (post == null)
                return false;
            await using var trx = await _dbContext.Database.BeginTransactionAsync();


            foreach (var media in post.Media)
            {
                var filePath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    media.Url.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString())
                );

                if (File.Exists(filePath))
                    File.Delete(filePath);
            }

            _dbContext.Posts.Remove(post);
            await _dbContext.SaveChangesAsync();
            await trx.CommitAsync();
            return true;
        }

        public async Task<List<PostResponseDto>> GetAllPostAsync(int pageNumber, int pageSize, int currentUserId)
        {
            if (pageNumber < 1 || pageSize < 1)
                throw new ArgumentException("Page number and size must be greater than zero.");
            if (pageSize > 100)
                throw new ArgumentException("Page size cannot exceed 100.");

            var posts = await _dbContext.Posts
                .AsNoTracking()
                .Include(p => p.User)
                .Include(p => p.Media)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new PostResponseDto
                {
                    PostId            = p.Id,
                    FirstName         = p.User.FirstName,
                    LastName          = p.User.LastName,
                    ProfilePictureUrl = p.User.ProfilePictureUrl??string.Empty,
                    Content           = p.Content,
                    CreatedAt         = p.CreatedAt,
                    IsFav = p.FavPosts.Any(f => f.UserId == currentUserId),
                    MediaUrls = p.Media != null
    ? p.Media.Select(m => m.Url).ToList()
    : new List<string>()
                }).OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return posts;
        }

        public async Task<bool> MarkPostFavAsync(int postID, int userId)
        {
            try
            {
                // ✅ Check if the post is already favorited by this user
                bool alreadyExists = await _dbContext.FavPosts
                    .AnyAsync(fp => fp.PostId == postID && fp.UserId == userId);

                if (alreadyExists)
                    return false; // Don't add again

                var favPost = new FavPost
                {
                    PostId = postID,
                    UserId = userId,
                    DateTime = DateTime.Now
                };

                await _dbContext.FavPosts.AddAsync(favPost);
                await _dbContext.SaveChangesAsync();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

    }
}