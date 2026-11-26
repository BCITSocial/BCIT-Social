using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using BCIT_Social.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SixLabors.ImageSharp;

namespace BCIT_Social.Data;

/// <summary>
/// Creates the BCIT Social demonstration dataset.
/// </summary>
public static class SeedData
{
    private const string SEED_BLOB_FOLDER = "seed-v2";
    private const string ADMIN_ROLE = "Admin";

    private sealed class SeedSecretsMarker
    {
    }

    private sealed record SeedAsset(
        string RelativePath,
        string StorageKey,
        string OriginalFileName,
        string ContentType,
        long FileSizeBytes);

    /// <summary>
    /// Applies migrations, uploads seed images, and inserts the seed dataset.
    /// </summary>
    public static async Task InitializeAsync(
        ApplicationDbContext db,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(userManager);
        ArgumentNullException.ThrowIfNull(roleManager);

        await db.Database.MigrateAsync();

        var configuration = BuildConfiguration();
        var assets = await UploadSeedAssetsAsync(configuration);

        await using var transaction =
            await db.Database.BeginTransactionAsync();

        try
        {
            var seedNow = DateTime.UtcNow;

            var users =
                await SeedUsersAsync(
                    userManager,
                    seedNow);

            await SeedAdministratorsAsync(
                userManager,
                roleManager,
                users);

            await SeedProfilesAsync(
                db,
                users,
                assets);

            await SeedFriendshipsAsync(
                db,
                users,
                seedNow);

            var communities =
                await SeedCommunitiesAsync(
                    db,
                    users,
                    assets);

            await SeedCommunityMembersAsync(
                db,
                users,
                communities,
                seedNow);

            var tags =
                await SeedTagsAsync(db);

            var posts =
                await SeedPostsAsync(
                    db,
                    users,
                    communities,
                    seedNow);

            await SeedPostTagsAsync(
                db,
                posts,
                tags);

            await SeedPostPhotosAsync(
                db,
                posts,
                assets,
                seedNow);

            await SeedCommentsAsync(
                db,
                users,
                posts);

            await SeedBookmarksAsync(
                db,
                users,
                posts,
                seedNow);

            var events =
                await SeedEventsAsync(
                    db,
                    users,
                    communities);

            await SeedEventRsvpsAsync(
                db,
                users,
                events);

            await transaction.CommitAsync();

            Console.WriteLine(
                "BCIT Social seed data was created successfully.");
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var environmentName =
            Environment.GetEnvironmentVariable(
                "ASPNETCORE_ENVIRONMENT")
            ?? "Development";

        return new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile(
                "appsettings.json",
                optional: true,
                reloadOnChange: false)
            .AddJsonFile(
                $"appsettings.{environmentName}.json",
                optional: true,
                reloadOnChange: false)
            .AddUserSecrets<SeedSecretsMarker>(
                optional: true)
            .AddEnvironmentVariables()
            .Build();
    }

    private static async Task<
        Dictionary<string, SeedAsset>>
        UploadSeedAssetsAsync(
            IConfiguration configuration)
    {
        var connectionString =
            configuration["AzureStorage:ConnectionString"];

        var containerName =
            configuration["AzureStorage:ContainerName"];

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "AzureStorage:ConnectionString is not configured.");
        }

        if (string.IsNullOrWhiteSpace(containerName))
        {
            throw new InvalidOperationException(
                "AzureStorage:ContainerName is not configured.");
        }

        var configuredAssetPath =
            configuration["SeedData:AssetsPath"];

        var assetRoot =
            string.IsNullOrWhiteSpace(configuredAssetPath)
                ? Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "Data",
                    "SeedAssets")
                : Path.GetFullPath(
                    configuredAssetPath,
                    Directory.GetCurrentDirectory());

        if (!Directory.Exists(assetRoot))
        {
            throw new DirectoryNotFoundException(
                $"Seed asset directory not found: {assetRoot}");
        }

        var pendingAssets =
            new List<(SeedAsset Asset, string FullPath)>();

        foreach (var relativePath in
                 SeedDataCatalog.ImageFiles)
        {
            var fullPath =
                Path.Combine(
                    assetRoot,
                    relativePath.Replace(
                        '/',
                        Path.DirectorySeparatorChar));

            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException(
                    $"Required seed image was not found: {fullPath}",
                    fullPath);
            }

            var fileInfo = new FileInfo(fullPath);
            var contentType = GetContentType(fullPath);

            await using var validationStream =
                File.OpenRead(fullPath);

            var imageInfo =
                await Image.IdentifyAsync(validationStream);

            if (imageInfo is null)
            {
                throw new InvalidOperationException(
                    $"Seed asset is not a valid image: {fullPath}");
            }

            var isCommunityBanner =
                relativePath.StartsWith(
                    "communities/",
                    StringComparison.OrdinalIgnoreCase);

            var isPostImage =
                relativePath.StartsWith(
                    "posts/",
                    StringComparison.OrdinalIgnoreCase);

            var minimumWidth =
                isCommunityBanner ? 800 : 300;

            var minimumHeight =
                isCommunityBanner ? 200 : 300;

            if (imageInfo.Width < minimumWidth ||
                imageInfo.Height < minimumHeight)
            {
                throw new InvalidOperationException(
                    $"Seed image '{relativePath}' is " +
                    $"{imageInfo.Width}x{imageInfo.Height}. " +
                    $"It must be at least " +
                    $"{minimumWidth}x{minimumHeight}.");
            }

            var maximumBytes =
                isPostImage
                    ? 20L * 1024 * 1024
                    : 10L * 1024 * 1024;

            if (fileInfo.Length > maximumBytes)
            {
                throw new InvalidOperationException(
                    $"Seed image '{relativePath}' is too large. " +
                    $"Its maximum size is " +
                    $"{maximumBytes / (1024 * 1024)} MiB.");
            }

            var normalizedRelativePath =
                relativePath.Replace('\\', '/');

            var storageKey =
                $"{SEED_BLOB_FOLDER}/" +
                normalizedRelativePath;

            pendingAssets.Add((
                new SeedAsset(
                    normalizedRelativePath,
                    storageKey,
                    Path.GetFileName(fullPath),
                    contentType,
                    fileInfo.Length),
                fullPath));
        }

        var container =
            new BlobContainerClient(
                connectionString,
                containerName);

        await container.CreateIfNotExistsAsync(
            PublicAccessType.None);

        var uploadedAssets =
            new Dictionary<string, SeedAsset>(
                StringComparer.OrdinalIgnoreCase);

        foreach (var pendingAsset in pendingAssets)
        {
            var blob =
                container.GetBlobClient(
                    pendingAsset.Asset.StorageKey);

            if (!(await blob.ExistsAsync()).Value)
            {
                await using var uploadStream =
                    File.OpenRead(
                        pendingAsset.FullPath);

                await blob.UploadAsync(
                    uploadStream,
                    new BlobUploadOptions
                    {
                        HttpHeaders =
                            new BlobHttpHeaders
                            {
                                ContentType =
                                    pendingAsset
                                        .Asset
                                        .ContentType
                            }
                    });
            }

            uploadedAssets[
                pendingAsset.Asset.RelativePath] =
                pendingAsset.Asset;
        }

        Console.WriteLine(
            $"Validated and prepared {uploadedAssets.Count} seed images.");

        return uploadedAssets;
    }

    private static string GetContentType(
        string filePath)
    {
        return Path.GetExtension(filePath)
            .ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",

            _ => throw new InvalidOperationException(
                $"Unsupported seed image type: {filePath}")
        };
    }

    private static async Task<
        Dictionary<string, ApplicationUser>>
        SeedUsersAsync(
            UserManager<ApplicationUser> userManager,
            DateTime seedNow)
    {
        var users =
            new Dictionary<string, ApplicationUser>(
                StringComparer.OrdinalIgnoreCase);

        for (var index = 0;
             index < SeedDataCatalog.Users.Length;
             index++)
        {
            var seed =
                SeedDataCatalog.Users[index];

            var user =
                await userManager.FindByEmailAsync(
                    seed.Email);

            if (user is null)
            {
                user = new ApplicationUser
                {
                    UserName = seed.Email,
                    Email = seed.Email,
                    EmailConfirmed = true,
                    FirstName = seed.FirstName,
                    LastName = seed.LastName,
                    CreatedAt =
                        seedNow.AddDays(
                            -(180 - index * 3))
                };

                var result =
                    await userManager.CreateAsync(
                        user,
                        SeedDataCatalog.DEMO_PASSWORD);

                ThrowIfIdentityFailed(
                    result,
                    $"create seed user '{seed.Email}'");
            }
            else
            {
                user.UserName = seed.Email;
                user.Email = seed.Email;
                user.EmailConfirmed = true;
                user.FirstName = seed.FirstName;
                user.LastName = seed.LastName;
                user.DateOfDel = null;

                var result =
                    await userManager.UpdateAsync(user);

                ThrowIfIdentityFailed(
                    result,
                    $"update seed user '{seed.Email}'");
            }

            users[seed.Key] = user;
        }

        return users;
    }

    private static async Task SeedAdministratorsAsync(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IReadOnlyDictionary<string, ApplicationUser> users)
    {
        if (!await roleManager.RoleExistsAsync(ADMIN_ROLE))
        {
            var result =
                await roleManager.CreateAsync(
                    new IdentityRole(ADMIN_ROLE));

            ThrowIfIdentityFailed(
                result,
                $"create the '{ADMIN_ROLE}' role");
        }

        string[] administratorKeys =
        [
            "maya",
            "noah"
        ];

        foreach (var administratorKey in administratorKeys)
        {
            var user = users[administratorKey];

            if (await userManager.IsInRoleAsync(
                    user,
                    ADMIN_ROLE))
            {
                continue;
            }

            var result =
                await userManager.AddToRoleAsync(
                    user,
                    ADMIN_ROLE);

            ThrowIfIdentityFailed(
                result,
                $"assign '{user.Email}' to '{ADMIN_ROLE}'");
        }
    }

    private static void ThrowIfIdentityFailed(
        IdentityResult result,
        string operation)
    {
        if (result.Succeeded)
        {
            return;
        }

        var errors =
            string.Join(
                "; ",
                result.Errors.Select(
                    error => error.Description));

        throw new InvalidOperationException(
            $"Could not {operation}: {errors}");
    }

    private static async Task SeedProfilesAsync(
        ApplicationDbContext db,
        IReadOnlyDictionary<string, ApplicationUser> users,
        IReadOnlyDictionary<string, SeedAsset> assets)
    {
        foreach (var seed in SeedDataCatalog.Users)
        {
            var user = users[seed.Key];

            var profile =
                await db.Profiles.FirstOrDefaultAsync(
                    item => item.UserId == user.Id);

            if (profile is null)
            {
                profile = new Profile
                {
                    UserId = user.Id
                };

                db.Profiles.Add(profile);
            }

            profile.DisplayName =
                $"{seed.FirstName} {seed.LastName}";

            profile.Bio = seed.Bio;
            profile.Program = seed.Program;
            profile.Campus = seed.Campus;

            if (seed.PhotoFile is not null)
            {
                var asset = assets[seed.PhotoFile];

                profile.ProfilePhotoStorageKey =
                    asset.StorageKey;

                profile.ProfilePhotoOriginalFileName =
                    asset.OriginalFileName;

                profile.ProfilePhotoContentType =
                    asset.ContentType;

                profile.ProfilePhotoFileSizeBytes =
                    asset.FileSizeBytes;

                profile.ProfilePhotoUpdatedAt =
                    DateTime.UtcNow;
            }
        }

        await db.SaveChangesAsync();
    }

    private static async Task SeedFriendshipsAsync(
        ApplicationDbContext db,
        IReadOnlyDictionary<string, ApplicationUser> users,
        DateTime seedNow)
    {
        foreach (var seed in
                 SeedDataCatalog.Friendships)
        {
            var requester =
                users[seed.RequesterKey];

            var recipient =
                users[seed.RecipientKey];

            var relationshipKey =
                GetRelationshipKey(
                    requester.Id,
                    recipient.Id);

            var relationship =
                await db.FriendRelationships
                    .FirstOrDefaultAsync(
                        item =>
                            item.RelationshipKey ==
                            relationshipKey);

            var createdAt =
                seedNow.AddDays(-seed.DaysAgo);

            if (relationship is null)
            {
                relationship =
                    new FriendRelationship
                    {
                        RelationshipKey =
                            relationshipKey
                    };

                db.FriendRelationships.Add(
                    relationship);
            }

            relationship.RequesterId = requester.Id;
            relationship.RecipientId = recipient.Id;
            relationship.Status = seed.Status;
            relationship.CreatedAt = createdAt;

            relationship.UpdatedAt =
                seed.Status == "Accepted"
                    ? createdAt.AddHours(12)
                    : null;

            relationship.DeactivatedAt = null;
        }

        await db.SaveChangesAsync();
    }

    private static string GetRelationshipKey(
        string firstUserId,
        string secondUserId)
    {
        return string.CompareOrdinal(
            firstUserId,
            secondUserId) < 0
            ? $"{firstUserId}:{secondUserId}"
            : $"{secondUserId}:{firstUserId}";
    }

    private static async Task<
        Dictionary<string, Community>>
        SeedCommunitiesAsync(
            ApplicationDbContext db,
            IReadOnlyDictionary<string, ApplicationUser> users,
            IReadOnlyDictionary<string, SeedAsset> assets)
    {
        var communities =
            new Dictionary<string, Community>(
                StringComparer.OrdinalIgnoreCase);

        foreach (var seed in
                 SeedDataCatalog.Communities)
        {
            var community =
                await db.Communities
                    .FirstOrDefaultAsync(
                        item =>
                            item.Name == seed.Name);

            if (community is null)
            {
                community = new Community();
                db.Communities.Add(community);
            }

            community.CreatorId =
                users[seed.OwnerKey].Id;

            community.Name = seed.Name;
            community.Type = seed.Type;
            community.Description = seed.Description;
            community.DateOfDel = null;

            if (seed.BannerFile is not null)
            {
                var asset = assets[seed.BannerFile];

                community.BannerPhotoStorageKey =
                    asset.StorageKey;

                community.BannerPhotoOriginalFileName =
                    asset.OriginalFileName;

                community.BannerPhotoContentType =
                    asset.ContentType;

                community.BannerPhotoFileSizeBytes =
                    asset.FileSizeBytes;

                community.BannerPhotoUpdatedAt =
                    DateTime.UtcNow;
            }

            communities[seed.Key] = community;
        }

        await db.SaveChangesAsync();
        return communities;
    }

    private static async Task SeedCommunityMembersAsync(
        ApplicationDbContext db,
        IReadOnlyDictionary<string, ApplicationUser> users,
        IReadOnlyDictionary<string, Community> communities,
        DateTime seedNow)
    {
        foreach (var seed in
                 SeedDataCatalog.Communities)
        {
            var community =
                communities[seed.Key];

            var memberKeys =
                seed.MemberKeys
                    .Append(seed.OwnerKey)
                    .Distinct(
                        StringComparer.OrdinalIgnoreCase);

            var moderatorKeys =
                seed.ModeratorKeys
                    .Append(seed.OwnerKey)
                    .ToHashSet(
                        StringComparer.OrdinalIgnoreCase);

            var memberIndex = 0;

            foreach (var memberKey in memberKeys)
            {
                var user = users[memberKey];

                var membership =
                    await db.CommunityMembers.FindAsync(
                        user.Id,
                        community.Id);

                if (membership is null)
                {
                    membership =
                        new CommunityMember
                        {
                            UserId = user.Id,
                            CommunityId = community.Id,
                            JoinedAt =
                                seedNow.AddDays(
                                    -(95 - memberIndex * 3))
                        };

                    db.CommunityMembers.Add(membership);
                }

                membership.IsModerator =
                    moderatorKeys.Contains(memberKey);

                memberIndex++;
            }
        }

        await db.SaveChangesAsync();
    }

    private static async Task<
        Dictionary<string, Tags>>
        SeedTagsAsync(ApplicationDbContext db)
    {
        var tags =
            new Dictionary<string, Tags>(
                StringComparer.OrdinalIgnoreCase);

        foreach (var name in SeedDataCatalog.Tags)
        {
            var tag =
                await db.Tags.FirstOrDefaultAsync(
                    item => item.Name == name);

            if (tag is null)
            {
                tag = new Tags
                {
                    Name = name
                };

                db.Tags.Add(tag);
            }

            tags[name] = tag;
        }

        await db.SaveChangesAsync();
        return tags;
    }

    private static async Task<
        Dictionary<string, Post>>
        SeedPostsAsync(
            ApplicationDbContext db,
            IReadOnlyDictionary<string, ApplicationUser> users,
            IReadOnlyDictionary<string, Community> communities,
            DateTime seedNow)
    {
        var posts =
            new Dictionary<string, Post>(
                StringComparer.OrdinalIgnoreCase);

        foreach (var seed in SeedDataCatalog.Posts)
        {
            var post =
                await db.Posts.FirstOrDefaultAsync(
                    item => item.Title == seed.Title);

            if (post is null)
            {
                post = new Post
                {
                    CreatedAt =
                        seedNow.AddHours(
                            -seed.HoursAgo)
                };

                db.Posts.Add(post);
            }

            post.AuthorId =
                users[seed.AuthorKey].Id;

            post.CommunityId =
                communities[seed.CommunityKey].Id;

            post.Title = seed.Title;
            post.Content = seed.Content;
            post.DateOfDel = null;

            posts[seed.Key] = post;
        }

        await db.SaveChangesAsync();
        return posts;
    }

    private static async Task SeedPostTagsAsync(
        ApplicationDbContext db,
        IReadOnlyDictionary<string, Post> posts,
        IReadOnlyDictionary<string, Tags> tags)
    {
        foreach (var seed in SeedDataCatalog.Posts)
        {
            var post = posts[seed.Key];

            foreach (var tagName in seed.Tags)
            {
                var tag = tags[tagName];

                var exists =
                    await db.PostTags.AnyAsync(
                        item =>
                            item.PostId == post.Id &&
                            item.TagId == tag.Id);

                if (!exists)
                {
                    db.PostTags.Add(
                        new PostTags
                        {
                            PostId = post.Id,
                            TagId = tag.Id
                        });
                }
            }
        }

        await db.SaveChangesAsync();
    }

    private static async Task SeedPostPhotosAsync(
        ApplicationDbContext db,
        IReadOnlyDictionary<string, Post> posts,
        IReadOnlyDictionary<string, SeedAsset> assets,
        DateTime seedNow)
    {
        foreach (var seed in SeedDataCatalog.Posts)
        {
            var post = posts[seed.Key];

            for (var index = 0;
                 index < seed.PhotoFiles.Length;
                 index++)
            {
                var asset =
                    assets[seed.PhotoFiles[index]];

                var photo =
                    await db.PostPhotos
                        .FirstOrDefaultAsync(
                            item =>
                                item.StorageKey ==
                                asset.StorageKey);

                if (photo is null)
                {
                    photo = new PostPhoto
                    {
                        StorageKey =
                            asset.StorageKey
                    };

                    db.PostPhotos.Add(photo);
                }

                photo.PostId = post.Id;
                photo.OriginalFileName =
                    asset.OriginalFileName;
                photo.ContentType = asset.ContentType;
                photo.FileSizeBytes =
                    asset.FileSizeBytes;
                photo.DisplayOrder = index;
                photo.CreatedAt = seedNow;
            }
        }

        await db.SaveChangesAsync();
    }

    private static async Task SeedCommentsAsync(
        ApplicationDbContext db,
        IReadOnlyDictionary<string, ApplicationUser> users,
        IReadOnlyDictionary<string, Post> posts)
    {
        var comments =
            new Dictionary<string, Comments>(
                StringComparer.OrdinalIgnoreCase);

        foreach (var seed in SeedDataCatalog.Comments
                     .Where(item =>
                         item.ParentKey is null))
        {
            var post = posts[seed.PostKey];
            var user = users[seed.AuthorKey];

            var comment =
                await db.Comments.FirstOrDefaultAsync(
                    item =>
                        item.PostId == post.Id &&
                        item.AuthorId == user.Id &&
                        item.Content == seed.Content);

            if (comment is null)
            {
                comment = new Comments();
                db.Comments.Add(comment);
            }

            comment.PostId = post.Id;
            comment.AuthorId = user.Id;
            comment.Content = seed.Content;

            comment.CreatedAt =
                post.CreatedAt.AddHours(
                    seed.HoursAfterPost);

            comment.ParentCommentId = null;
            comments[seed.Key] = comment;
        }

        await db.SaveChangesAsync();

        foreach (var seed in SeedDataCatalog.Comments
                     .Where(item =>
                         item.ParentKey is not null))
        {
            var post = posts[seed.PostKey];
            var user = users[seed.AuthorKey];
            var parent = comments[seed.ParentKey!];

            var comment =
                await db.Comments.FirstOrDefaultAsync(
                    item =>
                        item.PostId == post.Id &&
                        item.AuthorId == user.Id &&
                        item.Content == seed.Content);

            if (comment is null)
            {
                comment = new Comments();
                db.Comments.Add(comment);
            }

            comment.PostId = post.Id;
            comment.AuthorId = user.Id;
            comment.Content = seed.Content;
            comment.ParentCommentId = parent.Id;

            comment.CreatedAt =
                post.CreatedAt.AddHours(
                    seed.HoursAfterPost);

            comments[seed.Key] = comment;
        }

        await db.SaveChangesAsync();
    }

    private static async Task SeedBookmarksAsync(
        ApplicationDbContext db,
        IReadOnlyDictionary<string, ApplicationUser> users,
        IReadOnlyDictionary<string, Post> posts,
        DateTime seedNow)
    {
        foreach (var seed in SeedDataCatalog.Bookmarks)
        {
            var user = users[seed.UserKey];
            var post = posts[seed.PostKey];

            var bookmark =
                await db.Bookmarks.FindAsync(
                    user.Id,
                    post.Id);

            if (bookmark is null)
            {
                bookmark = new Bookmarks
                {
                    UserId = user.Id,
                    PostId = post.Id
                };

                db.Bookmarks.Add(bookmark);
            }

            bookmark.SavedAt =
                seedNow.AddDays(-seed.DaysAgo);
        }

        await db.SaveChangesAsync();
    }

    private static async Task<
        Dictionary<string, CommunityEvent>>
        SeedEventsAsync(
            ApplicationDbContext db,
            IReadOnlyDictionary<string, ApplicationUser> users,
            IReadOnlyDictionary<string, Community> communities)
    {
        var events =
            new Dictionary<string, CommunityEvent>(
                StringComparer.OrdinalIgnoreCase);

        var vancouverToday =
            TimeZoneInfo.ConvertTimeBySystemTimeZoneId(
                DateTime.UtcNow,
                "America/Vancouver").Date;

        foreach (var seed in SeedDataCatalog.Events)
        {
            var community =
                communities[seed.CommunityKey];

            var communityEvent =
                await db.CommunityEvents
                    .FirstOrDefaultAsync(
                        item =>
                            item.Title == seed.Title &&
                            item.CommunityId ==
                            community.Id);

            var startsAt =
                vancouverToday
                    .AddDays(seed.DaysFromToday)
                    .AddHours(seed.StartHour);

            if (communityEvent is null)
            {
                communityEvent =
                    new CommunityEvent();

                db.CommunityEvents.Add(
                    communityEvent);
            }

            communityEvent.CreatorId =
                users[seed.CreatorKey].Id;

            communityEvent.CommunityId =
                community.Id;

            communityEvent.Title = seed.Title;
            communityEvent.EventDate = startsAt;

            communityEvent.EndsAt =
                startsAt.AddMinutes(
                    seed.DurationMinutes);

            communityEvent.Campus = seed.Campus;
            communityEvent.Location = seed.Location;
            communityEvent.Description =
                seed.Description;

            communityEvent.DateOfDel = null;

            events[seed.Key] = communityEvent;
        }

        await db.SaveChangesAsync();
        return events;
    }

    private static async Task SeedEventRsvpsAsync(
        ApplicationDbContext db,
        IReadOnlyDictionary<string, ApplicationUser> users,
        IReadOnlyDictionary<string, CommunityEvent> events)
    {
        foreach (var seed in SeedDataCatalog.Events)
        {
            var communityEvent = events[seed.Key];

            foreach (var userKey in seed.GoingUserKeys)
            {
                await AddOrUpdateRsvpAsync(
                    db,
                    communityEvent.Id,
                    users[userKey].Id,
                    going: true);
            }

            foreach (var userKey in
                     seed.InterestedUserKeys)
            {
                await AddOrUpdateRsvpAsync(
                    db,
                    communityEvent.Id,
                    users[userKey].Id,
                    going: false);
            }
        }

        await db.SaveChangesAsync();
    }

    private static async Task AddOrUpdateRsvpAsync(
        ApplicationDbContext db,
        int eventId,
        string userId,
        bool going)
    {
        var rsvp =
            await db.EventRsvps.FindAsync(
                eventId,
                userId);

        if (rsvp is null)
        {
            rsvp = new EventRsvp
            {
                EventId = eventId,
                UserId = userId
            };

            db.EventRsvps.Add(rsvp);
        }

        rsvp.Going = going;
    }
}