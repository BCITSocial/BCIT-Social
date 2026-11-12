using BCIT_Social.Tests.Support;
using BCIT_Social.ViewModels;

namespace BCIT_Social.Tests.Unit.Presentation;

/// <summary>Declarative rules are code too: a removed [Required] silently lets blank posts into the database.</summary>
public class ViewModelValidationTests
{
    // T10
    [Theory]
    //     title           tag content   → invalid members
    [InlineData("Hello", 1, "Body", "")]
    [InlineData("", 1, "Body", "Title")]
    [InlineData(null, 1, "Body", "Title")]
    [InlineData("#150", 1, "Body", "")]            // exactly 150 chars is allowed
    [InlineData("#151", 1, "Body", "Title")]       // 151 is not
    [InlineData("Hello", 0, "Body", "TagId")]       // 0 = "no post type chosen"
    [InlineData("Hello", -3, "Body", "TagId")]
    [InlineData("Hello", 1, null, "Content")]
    [InlineData("", 0, "", "Content,TagId,Title")]
    public void CreatePostViewModel_Validation(string? title, int tagId, string? content, string expectedInvalid)
    {
        title = title switch { "#150" => new string('a', 150), "#151" => new string('a', 151), _ => title };
        var model = new CreatePostViewModel { Title = title!, TagId = tagId, Content = content! };

        string.Join(",", TestHelpers.InvalidMembers(model)).ShouldBe(expectedInvalid);
    }

    // T11
    [Theory]
    [InlineData("maya@my.bcit.ca", true)]
    [InlineData("A@BCIT.CA", true)]
    [InlineData("", false)]
    [InlineData("plain-text", false)]
    [InlineData("two@@bcit.ca", false)]
    [InlineData("white space@bcit.ca", true)]   // [EmailAddress] is lenient (accepts spaces), which is why BcitEmailPolicy (T19) is needed
    public void SendFriendRequestViewModel_RequiresAWellFormedEmail(string email, bool valid)
    {
        var model = new SendFriendRequestViewModel { Email = email };

        TestHelpers.InvalidMembers(model).Length.ShouldBe(valid ? 0 : 1);
    }
}
