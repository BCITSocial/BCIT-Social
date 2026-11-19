namespace BCIT_Social.Core.Comments;

/// <summary>
/// Nests a flat, already-ordered list of comments into threads.
/// Generic on purpose: the web layer keeps using its own CommentViewModel; Core only knows ids.
/// (Extracted from HomeController.LoadCommentThreadsAsync, which mixed this with the EF query.)
/// </summary>
public static class CommentThreadBuilder
{
    public static IReadOnlyList<TNode> Build<TNode>(
        IEnumerable<TNode> rows,
        Func<TNode, int> idOf,
        Func<TNode, int?> parentIdOf,
        Action<TNode, TNode> addReply)
    {
        var list = rows.ToList();
        var byId = new Dictionary<int, TNode>();
        foreach (var row in list) byId[idOf(row)] = row;

        var topLevel = new List<TNode>();
        foreach (var row in list)
        {
            if (parentIdOf(row) is { } parentId && parentId != idOf(row) && byId.TryGetValue(parentId, out var parent))
            {
                addReply(parent, row);
            }
            else
            {
                topLevel.Add(row);
            }
        }

        return topLevel;
    }
}
