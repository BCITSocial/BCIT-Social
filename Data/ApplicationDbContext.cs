using BCIT_Social.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BCIT_Social.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Declare the tables
    public DbSet<Profile> Profiles { get; set; }
    public DbSet<Community> Communities { get; set; }
    public DbSet<CommunityMember> CommunityMembers { get; set; }
    public DbSet<CommunityEvent> CommunityEvents { get; set; }
    public DbSet<CommunityMessage> CommunityMessages { get; set; }
    public DbSet<Post> Posts { get; set; }
    public DbSet<Comments> Comments { get; set; }
    public DbSet<Tags> Tags { get; set; }
    public DbSet<PostTags> PostTags { get; set; }
    public DbSet<Bookmarks> Bookmarks { get; set; }
    public DbSet<Chatbox> Chatboxes { get; set; }
    public DbSet<ChatboxUser> ChatboxUsers { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<PostPhoto> PostPhotos { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // One Profile belongs to one ApplicationUser.
        builder.Entity<Profile>()
            .HasKey(p => p.UserId);

        // One Profile belongs to one ApplicationUser through UserId.
        builder.Entity<Profile>()
            .HasOne(p => p.User)
            .WithOne()
            .HasForeignKey<Profile>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // One ApplicationUser can create many Communities.
        builder.Entity<Community>()
            .HasOne(c => c.Creator)
            .WithMany()
            .HasForeignKey(c => c.CreatorId)
            .OnDelete(DeleteBehavior.Restrict);

        // CommunityMember uses UserId and CommunityId as a composite primary key.
        builder.Entity<CommunityMember>()
            .HasKey(cm => new
            {
                cm.UserId,
                cm.CommunityId
            });

        // One ApplicationUser can belong to many Communities through CommunityMember.
        builder.Entity<CommunityMember>()
            .HasOne(cm => cm.User)
            .WithMany()
            .HasForeignKey(cm => cm.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // One Community can have many CommunityMembers.
        builder.Entity<CommunityMember>()
            .HasOne(cm => cm.Community)
            .WithMany(c => c.Members)
            .HasForeignKey(cm => cm.CommunityId)
            .OnDelete(DeleteBehavior.Cascade);

        // One ApplicationUser can create many CommunityEvents.
        builder.Entity<CommunityEvent>()
            .HasOne(e => e.Creator)
            .WithMany()
            .HasForeignKey(e => e.CreatorId)
            .OnDelete(DeleteBehavior.Restrict);

        // One Community can have many CommunityEvents, and an event may exist without a Community.
        builder.Entity<CommunityEvent>()
            .HasOne(e => e.Community)
            .WithMany()
            .HasForeignKey(e => e.CommunityId)
            .OnDelete(DeleteBehavior.SetNull);

        // One Community can have many CommunityMessages.
        builder.Entity<CommunityMessage>()
            .HasOne(m => m.Community)
            .WithMany()
            .HasForeignKey(m => m.CommunityId)
            .OnDelete(DeleteBehavior.Cascade);

        // One ApplicationUser can send many CommunityMessages.
        builder.Entity<CommunityMessage>()
            .HasOne(m => m.Sender)
            .WithMany()
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        // One ApplicationUser can author many Posts.
        builder.Entity<Post>()
            .HasOne(p => p.Author)
            .WithMany()
            .HasForeignKey(p => p.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        // Supports retrieving an author's posts ordered by creation date.
        builder.Entity<Post>()
            .HasIndex(p => new
            {
                p.AuthorId,
                p.CreatedAt
            });

        // One Community can have many Posts, and a Post may exist without a Community.
        builder.Entity<Post>()
            .HasOne(p => p.Community)
            .WithMany()
            .HasForeignKey(p => p.CommunityId)
            .OnDelete(DeleteBehavior.SetNull);

        // Configures the one-to-many relationship between a Post and its photos.
        builder.Entity<PostPhoto>()
            .HasOne(photo => photo.Post)
            .WithMany(post => post.Photos)
            .HasForeignKey(photo => photo.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        // Supports retrieving a post's photos in display order.
        builder.Entity<PostPhoto>()
            .HasIndex(photo => new
            {
                photo.PostId,
                photo.DisplayOrder
            });

        // Ensures each stored photo key refers to only one PostPhoto record.
        builder.Entity<PostPhoto>()
            .HasIndex(photo => photo.StorageKey)
            .IsUnique();

        // One ApplicationUser can author many Comments.
        builder.Entity<Comments>()
            .HasOne(c => c.Author)
            .WithMany()
            .HasForeignKey(c => c.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        // One Post can have many Comments.
        builder.Entity<Comments>()
            .HasOne(c => c.Post)
            .WithMany()
            .HasForeignKey(c => c.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        // One Comment can have many reply Comments through ParentCommentId.
        builder.Entity<Comments>()
            .HasOne(c => c.ParentComment)
            .WithMany(c => c.Replies)
            .HasForeignKey(c => c.ParentCommentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Bookmark uses UserId and PostId as a composite primary key.
        builder.Entity<Bookmarks>()
            .HasKey(b => new
            {
                b.UserId,
                b.PostId
            });

        // One ApplicationUser can have many Bookmarks.
        builder.Entity<Bookmarks>()
            .HasOne(b => b.User)
            .WithMany()
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // One Post can be bookmarked by many users.
        builder.Entity<Bookmarks>()
            .HasOne(b => b.Post)
            .WithMany()
            .HasForeignKey(b => b.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        // PostTags uses PostId and TagId as a composite primary key.
        builder.Entity<PostTags>()
            .HasKey(pt => new
            {
                pt.PostId,
                pt.TagId
            });

        // One Post can have many PostTags entries.
        builder.Entity<PostTags>()
            .HasOne(pt => pt.Post)
            .WithMany()
            .HasForeignKey(pt => pt.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        // One Tag can be used by many PostTags entries.
        builder.Entity<PostTags>()
            .HasOne(pt => pt.Tag)
            .WithMany()
            .HasForeignKey(pt => pt.TagId)
            .OnDelete(DeleteBehavior.Cascade);

        // One ApplicationUser can create many Chatboxes.
        builder.Entity<Chatbox>()
            .HasOne(c => c.Creator)
            .WithMany()
            .HasForeignKey(c => c.CreatorId)
            .OnDelete(DeleteBehavior.Restrict);

        // ChatboxUser uses UserId and ChatboxId as a composite primary key.
        builder.Entity<ChatboxUser>()
            .HasKey(cu => new
            {
                cu.UserId,
                cu.ChatboxId
            });

        // One ApplicationUser can belong to many Chatboxes through ChatboxUser.
        builder.Entity<ChatboxUser>()
            .HasOne(cu => cu.User)
            .WithMany()
            .HasForeignKey(cu => cu.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // One Chatbox can have many users through ChatboxUser.
        builder.Entity<ChatboxUser>()
            .HasOne(cu => cu.Chatbox)
            .WithMany(c => c.ChatboxUsers)
            .HasForeignKey(cu => cu.ChatboxId)
            .OnDelete(DeleteBehavior.Cascade);

        // One Chatbox can contain many Messages.
        builder.Entity<Message>()
            .HasOne(m => m.Chatbox)
            .WithMany(c => c.Messages)
            .HasForeignKey(m => m.ChatboxId)
            .OnDelete(DeleteBehavior.Cascade);

        // One ApplicationUser can send many Messages.
        builder.Entity<Message>()
            .HasOne(m => m.Sender)
            .WithMany()
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

    }
}