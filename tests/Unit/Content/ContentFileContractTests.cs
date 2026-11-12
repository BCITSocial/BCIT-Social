using System.Text.Json;
using System.Text.Json.Serialization;
using BCIT_Social.Tests.Support;
using BCIT_Social.ViewModels;

namespace BCIT_Social.Tests.Unit.Content;

/// <summary>
/// wwwroot/content/*.json is edited by non-developers and read at runtime. A typo only shows up when someone opens the page.
/// These tests turn that into a failed build.
/// </summary>
public class ContentFileContractTests
{
    // Same options as EventsController.JsonOptions.
    private static readonly JsonSerializerOptions AppJsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    // T12
    [Fact]
    public void ContentFiles_AreValidJson_AndDeserializeIntoTheirViewModels()
    {
        var folder = TestHelpers.WebProjectPath("wwwroot", "content");
        var files = Directory.GetFiles(folder, "*.json");
        files.ShouldNotBeEmpty();

        foreach (var file in files)
        {
            var json = File.ReadAllText(file);
            Should.NotThrow(() => JsonDocument.Parse(json), $"{Path.GetFileName(file)} is not valid JSON");
        }

        var events = JsonSerializer.Deserialize<EventsContent>(
            File.ReadAllText(Path.Combine(folder, "events-text.json")), AppJsonOptions)!;
        events.Text.PageTitle.ShouldNotBeNullOrWhiteSpace();
        events.Events.Select(e => e.Id).Distinct().Count().ShouldBe(events.Events.Count, "event ids must not repeat");
        events.Events.ShouldAllBe(e => !string.IsNullOrWhiteSpace(e.Title));

        var messages = JsonSerializer.Deserialize<MessagesContent>(
            File.ReadAllText(Path.Combine(folder, "messages-text.json")), AppJsonOptions)!;
        messages.Conversations.ShouldAllBe(c => !string.IsNullOrWhiteSpace(c.Name));
    }
}
