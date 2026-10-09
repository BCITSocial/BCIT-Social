using BCIT_Social.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BCIT_Social.Data;

public static class SeedData
{
    private const int SampleCount = 20;
    private const string DemoPassword = "Demo123!";

    private static readonly (string Email, string DisplayName, string Program, string Campus)[] UserDetails =
    [
        ("maya.chen@example.com", "Maya Chen", "Computer Systems Technology", "Burnaby"),
        ("noah.patel@example.com", "Noah Patel", "Computer Systems Technology", "Burnaby"),
        ("ava.williams@example.com", "Ava Williams", "New Media Design and Web Development", "Downtown"),
        ("liam.garcia@example.com", "Liam Garcia", "Business Information Technology", "Burnaby"),
        ("emma.johnson@example.com", "Emma Johnson", "Computer Systems Technology", "Burnaby"),
        ("oliver.kim@example.com", "Oliver Kim", "Mechanical Engineering", "Burnaby"),
        ("sophia.brown@example.com", "Sophia Brown", "Computer Information Technology", "Downtown"),
        ("ethan.lee@example.com", "Ethan Lee", "Business Information Technology", "Burnaby"),
        ("isabella.martin@example.com", "Isabella Martin", "New Media Design and Web Development", "Downtown"),
        ("lucas.thompson@example.com", "Lucas Thompson", "Computer Systems Technology", "Burnaby"),
        ("mia.anderson@example.com", "Mia Anderson", "Accounting", "Burnaby"),
        ("mason.taylor@example.com", "Mason Taylor", "Computer Information Technology", "Downtown"),
        ("amelia.moore@example.com", "Amelia Moore", "Marketing Management", "Burnaby"),
        ("james.jackson@example.com", "James Jackson", "Computer Systems Technology", "Burnaby"),
        ("harper.white@example.com", "Harper White", "Graphic Communications", "Downtown"),
        ("benjamin.harris@example.com", "Benjamin Harris", "Business Information Technology", "Burnaby"),
        ("evelyn.clark@example.com", "Evelyn Clark", "Computer Information Technology", "Downtown"),
        ("henry.lewis@example.com", "Henry Lewis", "Electrical and Computer Engineering", "Burnaby"),
        ("charlotte.walker@example.com", "Charlotte Walker", "New Media Design and Web Development", "Downtown"),
        ("daniel.hall@example.com", "Daniel Hall", "Computer Systems Technology", "Burnaby")
    ];

    public static async Task InitializeAsync(
        ApplicationDbContext db,
        UserManager<ApplicationUser> userManager)
    {
        await db.Database.MigrateAsync();

        var users = await SeedUsersAsync(userManager);
        await SeedProfilesAsync(db, users);

        var communities = await SeedCommunitiesAsync(db, users);
        await SeedCommunityMembersAsync(db, users, communities);
        await SeedCommunityEventsAsync(db, users, communities);
        await SeedCommunityMessagesAsync(db, users, communities);

        var tags = await SeedTagsAsync(db);
        var posts = await SeedPostsAsync(db, users, communities);
        await SeedCommentsAsync(db, users, posts);
        await SeedPostTagsAsync(db, posts, tags);
        await SeedBookmarksAsync(db, users, posts);

        var chatboxes = await SeedChatboxesAsync(db, users);
        await SeedChatboxUsersAsync(db, users, chatboxes);
        await SeedMessagesAsync(db, users, chatboxes);
    }

    private static async Task<List<ApplicationUser>> SeedUsersAsync(
        UserManager<ApplicationUser> userManager)
    {
        var users = new List<ApplicationUser>(SampleCount);
        foreach (var detail in UserDetails)
        {
            var user = await userManager.FindByEmailAsync(detail.Email);
            if (user is null)
            {
                user = new ApplicationUser
                {
                    UserName = detail.Email,
                    Email = detail.Email,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, DemoPassword);
                if (!result.Succeeded)
                {
                    var errors = string.Join("; ", result.Errors.Select(error => error.Description));
                    throw new InvalidOperationException(
                        $"Could not create demo user '{detail.Email}': {errors}");
                }
            }

            users.Add(user);
        }

        return users;
    }

    private static async Task SeedProfilesAsync(
        ApplicationDbContext db,
        IReadOnlyList<ApplicationUser> users)
    {
        for (var i = 0; i < users.Count; i++)
        {
            var userId = users[i].Id;
            if (await db.Profiles.AnyAsync(profile => profile.UserId == userId))
            {
                continue;
            }

            var detail = UserDetails[i];
            db.Profiles.Add(new Profile
            {
                UserId = userId,
                DisplayName = detail.DisplayName,
                Bio = $"{detail.DisplayName.Split(' ')[0]} is a BCIT student who enjoys learning, collaborating, and getting involved on campus.",
                Program = detail.Program,
                Campus = detail.Campus
            });
        }

        await db.SaveChangesAsync();
    }

    private static async Task<List<Community>> SeedCommunitiesAsync(
        ApplicationDbContext db,
        IReadOnlyList<ApplicationUser> users)
    {
        var communities = new List<Community>(SampleCount);
        for (var i = 0; i < SampleCount; i++)
        {
            var name = i switch
            {
                0 => "BCIT Developers",
                1 => "Burnaby Study Circle",
                _ => $"BCIT Student Community {i + 1:00}"
            };
            var community = await db.Communities.FirstOrDefaultAsync(item => item.Name == name);
            if (community is null)
            {
                community = new Community
                {
                    Name = name,
                    Type = i % 2 == 0 ? "Interest" : "Study",
                    CreatorId = users[i % users.Count].Id
                };
                db.Communities.Add(community);
                await db.SaveChangesAsync();
            }

            communities.Add(community);
        }

        return communities;
    }

    private static async Task SeedCommunityMembersAsync(
        ApplicationDbContext db,
        IReadOnlyList<ApplicationUser> users,
        IReadOnlyList<Community> communities)
    {
        for (var i = 0; i < SampleCount; i++)
        {
            await AddCommunityMemberAsync(db, users[i], communities[i], i % 5 == 0);
            await AddCommunityMemberAsync(db, users[i], communities[(i + 1) % SampleCount], false);
        }
    }

    private static async Task AddCommunityMemberAsync(
        ApplicationDbContext db,
        ApplicationUser user,
        Community community,
        bool isModerator)
    {
        if (await db.CommunityMembers.AnyAsync(member =>
                member.UserId == user.Id && member.CommunityId == community.Id))
        {
            return;
        }

        db.CommunityMembers.Add(new CommunityMember
        {
            UserId = user.Id,
            CommunityId = community.Id,
            IsModerator = isModerator,
            JoinedAt = DateTime.UtcNow.AddDays(-14)
        });
        await db.SaveChangesAsync();
    }

    private static async Task SeedCommunityEventsAsync(
        ApplicationDbContext db,
        IReadOnlyList<ApplicationUser> users,
        IReadOnlyList<Community> communities)
    {
        for (var i = 0; i < SampleCount; i++)
        {
            var title = $"BCIT Sample Event {i + 1:00}";
            if (await db.CommunityEvents.AnyAsync(item => item.Title == title))
            {
                continue;
            }

            db.CommunityEvents.Add(new CommunityEvent
            {
                Title = title,
                CreatorId = users[i].Id,
                CommunityId = communities[i].Id,
                EventDate = DateTime.UtcNow.AddDays(i + 1)
            });
        }

        await db.SaveChangesAsync();
    }

    private static async Task SeedCommunityMessagesAsync(
        ApplicationDbContext db,
        IReadOnlyList<ApplicationUser> users,
        IReadOnlyList<Community> communities)
    {
        for (var i = 0; i < SampleCount; i++)
        {
            var body = $"Sample community message {i + 1:00}: who's joining the next campus meetup?";
            if (await db.CommunityMessages.AnyAsync(item => item.MessageBody == body))
            {
                continue;
            }

            db.CommunityMessages.Add(new CommunityMessage
            {
                CommunityId = communities[i].Id,
                SenderId = users[i].Id,
                MessageBody = body,
                SentAt = DateTime.UtcNow.AddHours(-i)
            });
        }

        await db.SaveChangesAsync();
    }

    private static async Task<List<Tags>> SeedTagsAsync(ApplicationDbContext db)
    {
        var names = new[]
        {
            "projects", "study", "campus-life", "help",
            "technology", "careers", "events", "housing",
            "food", "clubs", "design", "coding",
            "networking", "wellness", "resources", "questions",
            "announcements", "volunteering", "sports", "arts"
        };
        var tags = new List<Tags>(names.Length);

        foreach (var name in names)
        {
            var tag = await db.Tags.FirstOrDefaultAsync(item => item.Name == name);
            if (tag is null)
            {
                tag = new Tags { Name = name };
                db.Tags.Add(tag);
                await db.SaveChangesAsync();
            }

            tags.Add(tag);
        }

        return tags;
    }

    private static async Task<List<Post>> SeedPostsAsync(
        ApplicationDbContext db,
        IReadOnlyList<ApplicationUser> users,
        IReadOnlyList<Community> communities)
    {
        var posts = new List<Post>(SampleCount);
        for (var i = 0; i < SampleCount; i++)
        {
            var content = $"Sample post {i + 1:00}: sharing ideas, questions, and updates from the BCIT community.";
            var post = await db.Posts.FirstOrDefaultAsync(item => item.Content == content);
            if (post is null)
            {
                post = new Post
                {
                    AuthorId = users[i].Id,
                    CommunityId = communities[i].Id,
                    Content = content,
                    CreatedAt = DateTime.UtcNow.AddDays(-i)
                };
                db.Posts.Add(post);
                await db.SaveChangesAsync();
            }

            posts.Add(post);
        }

        return posts;
    }

    private static async Task SeedCommentsAsync(
        ApplicationDbContext db,
        IReadOnlyList<ApplicationUser> users,
        IReadOnlyList<Post> posts)
    {
        for (var i = 0; i < SampleCount; i++)
        {
            var content = $"Sample comment {i + 1:00}: thanks for sharing this with everyone.";
            if (await db.Comments.AnyAsync(item => item.Content == content))
            {
                continue;
            }

            db.Comments.Add(new Comments
            {
                AuthorId = users[(i + 1) % users.Count].Id,
                PostId = posts[i].Id,
                Content = content,
                CreatedAt = DateTime.UtcNow.AddHours(-i)
            });
        }

        await db.SaveChangesAsync();
    }

    private static async Task SeedPostTagsAsync(
        ApplicationDbContext db,
        IReadOnlyList<Post> posts,
        IReadOnlyList<Tags> tags)
    {
        for (var i = 0; i < SampleCount; i++)
        {
            var postId = posts[i].Id;
            var tagId = tags[i].Id;
            if (!await db.PostTags.AnyAsync(item => item.PostId == postId && item.TagId == tagId))
            {
                db.PostTags.Add(new PostTags { PostId = postId, TagId = tagId });
            }
        }

        await db.SaveChangesAsync();
    }

    private static async Task SeedBookmarksAsync(
        ApplicationDbContext db,
        IReadOnlyList<ApplicationUser> users,
        IReadOnlyList<Post> posts)
    {
        for (var i = 0; i < SampleCount; i++)
        {
            var userId = users[(i + 1) % users.Count].Id;
            var postId = posts[i].Id;
            if (!await db.Bookmarks.AnyAsync(item => item.UserId == userId && item.PostId == postId))
            {
                db.Bookmarks.Add(new Bookmarks
                {
                    UserId = userId,
                    PostId = postId,
                    SavedAt = DateTime.UtcNow.AddDays(-i)
                });
            }
        }

        await db.SaveChangesAsync();
    }

    private static async Task<List<Chatbox>> SeedChatboxesAsync(
        ApplicationDbContext db,
        IReadOnlyList<ApplicationUser> users)
    {
        var chatboxes = new List<Chatbox>(SampleCount);
        for (var i = 0; i < SampleCount; i++)
        {
            var creatorId = users[i].Id;
            var chatbox = await db.Chatboxes.FirstOrDefaultAsync(item => item.CreatorId == creatorId);
            if (chatbox is null)
            {
                chatbox = new Chatbox
                {
                    CreatorId = creatorId,
                    CreatedAt = DateTime.UtcNow.AddDays(-i)
                };
                db.Chatboxes.Add(chatbox);
                await db.SaveChangesAsync();
            }

            chatboxes.Add(chatbox);
        }

        return chatboxes;
    }

    private static async Task SeedChatboxUsersAsync(
        ApplicationDbContext db,
        IReadOnlyList<ApplicationUser> users,
        IReadOnlyList<Chatbox> chatboxes)
    {
        for (var i = 0; i < chatboxes.Count; i++)
        {
            await AddChatboxUserAsync(db, users[i], chatboxes[i]);
            await AddChatboxUserAsync(db, users[(i + 1) % users.Count], chatboxes[i]);
        }
    }

    private static async Task AddChatboxUserAsync(
        ApplicationDbContext db,
        ApplicationUser user,
        Chatbox chatbox)
    {
        if (await db.ChatboxUsers.AnyAsync(item =>
                item.UserId == user.Id && item.ChatboxId == chatbox.Id))
        {
            return;
        }

        db.ChatboxUsers.Add(new ChatboxUser { UserId = user.Id, ChatboxId = chatbox.Id });
        await db.SaveChangesAsync();
    }

    private static async Task SeedMessagesAsync(
        ApplicationDbContext db,
        IReadOnlyList<ApplicationUser> users,
        IReadOnlyList<Chatbox> chatboxes)
    {
        for (var i = 0; i < chatboxes.Count; i++)
        {
            var text = $"Sample direct message {i + 1:00}: see you around campus!";
            if (await db.Messages.AnyAsync(item =>
                    item.ChatboxId == chatboxes[i].Id && item.Text == text))
            {
                continue;
            }

            db.Messages.Add(new Message
            {
                ChatboxId = chatboxes[i].Id,
                SenderId = users[i].Id,
                Text = text,
                CreatedAt = DateTime.UtcNow.AddMinutes(-i * 5)
            });
        }

        await db.SaveChangesAsync();
    }
}
