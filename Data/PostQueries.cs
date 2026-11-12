using BCIT_Social.Models;
using BCIT_Social.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BCIT_Social.Data;

// Post lookups shared by the Home feed and community pages.
public static class PostQueries
{
    // Maps posts to feed-card data (Views/Home/_PostCards.cshtml). Keeps the query's order.
    public static IQueryable<PostCardViewModel> ProjectCards(
        this ApplicationDbContext context, IQueryable<Post> posts, string? userId)
    {
        return posts.AsNoTracking().Select(post => new PostCardViewModel
        {
            Id = post.Id,
            Title = post.Title,
            Content = post.Content,
            AuthorName = context.Profiles
                .Where(profile => profile.UserId == post.AuthorId)
                .Select(profile => profile.DisplayName)
                .FirstOrDefault() ?? post.Author.UserName ?? "",
            Photos = post.Photos.OrderBy(photo => photo.DisplayOrder).ToList(),
            CommunityName = post.Community != null ? post.Community.Name : null,
            CreatedAt = post.CreatedAt,
            Tags = post.PostTags.Select(postTag => postTag.Tag.Name).ToList(),
            CommentCount = context.Comments.Count(comment => comment.PostId == post.Id),
            IsAuthor = userId != null && post.AuthorId == userId,
            AuthorId = post.AuthorId,
            IsSaved = userId != null && context.Bookmarks
                .Any(bookmark => bookmark.UserId == userId && bookmark.PostId == post.Id),
        });
    }
}
