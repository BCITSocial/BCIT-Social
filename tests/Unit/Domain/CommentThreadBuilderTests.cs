using BCIT_Social.Core.Comments;
using BCIT_Social.ViewModels;

namespace BCIT_Social.Tests.Unit.Domain;

public class CommentThreadBuilderTests
{
    private static CommentViewModel C(int id) => new() { Id = id, AuthorName = $"user{id}", Content = $"c{id}" };

    // T23  – uses the REAL CommentViewModel, proving the Core builder plugs into the web layer without referencing it.
    [Fact]
    public void Build_NestsRepliesAtAnyDepth_KeepsOrder_AndNeverLosesAComment()
    {
        var rows = new (CommentViewModel View, int? Parent)[]
        {
            (C(1), null),
            (C(2), 1),
            (C(3), 2),      // reply to a reply
            (C(4), null),
            (C(5), 99),     // parent missing (deleted/filtered) → must still be visible
            (C(6), 6),      // self-reference → must not vanish into itself
            (C(7), 1),
        };

        var top = CommentThreadBuilder.Build(
            rows.Select(r => r.View),
            idOf: view => view.Id,
            parentIdOf: view => rows.Single(r => r.View.Id == view.Id).Parent,
            addReply: (parent, reply) => parent.Replies.Add(reply));

        top.Select(c => c.Id).ShouldBe([1, 4, 5, 6]);
        top[0].Replies.Select(c => c.Id).ShouldBe([2, 7]);
        top[0].Replies[0].Replies.Select(c => c.Id).ShouldBe([3]);
        top[0].ReplyCount.ShouldBe(3);                       // 2, 3 and 7
        top.Sum(c => 1 + c.ReplyCount).ShouldBe(rows.Length);
    }
}
