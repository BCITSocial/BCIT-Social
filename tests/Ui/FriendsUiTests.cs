using BCIT_Social.Tests.Support;

namespace BCIT_Social.Tests.Ui;

[Collection("PlaywrightUi")]
public class FriendsUiTests(PlaywrightFixture fx)
{
    [Theory]
    [InlineData(false, 0)]
    [InlineData(true, 1)]
    public async Task RemoveFriend_SubmitsOnlyWhenConfirmed(bool confirm, int expectedSubmissions)
    {
        const string html = """
            <form data-remove-friend data-friend-name="Maya Chen">
                <button type="submit">Remove</button>
            </form>
            """;
        var page = await fx.OpenComponentPageAsync("/Friends", html, "friends.js");
        try
        {
            await page.EvaluateAsync("""
                () => {
                    window.submissions = 0;
                    document.querySelector('form').addEventListener('submit', event => {
                        if (!event.defaultPrevented) window.submissions++;
                        event.preventDefault();
                    });
                }
                """);
            string? confirmationMessage = null;
            page.Dialog += async (_, dialog) =>
            {
                confirmationMessage = dialog.Message;
                if (confirm) await dialog.AcceptAsync();
                else await dialog.DismissAsync();
            };

            await page.GetByRole(Microsoft.Playwright.AriaRole.Button).ClickAsync();

            confirmationMessage.ShouldBe("Remove Maya Chen from your friends?");
            (await page.EvaluateAsync<int>("() => window.submissions")).ShouldBe(expectedSubmissions);
        }
        finally
        {
            await page.Context.CloseAsync();
        }
    }
}
