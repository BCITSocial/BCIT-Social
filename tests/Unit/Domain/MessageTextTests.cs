using BCIT_Social.Core.Messaging;

namespace BCIT_Social.Tests.Unit.Domain;

public class MessageTextTests
{
    // T20
    public static TheoryData<string?, MessageTextError, int> Cases => new()
    {
        { null, MessageTextError.Empty, 0 },
        { "", MessageTextError.Empty, 0 },
        { "  \n\t ", MessageTextError.Empty, 0 },
        { "hi", MessageTextError.None, 2 },
        { "  hi  ", MessageTextError.None, 2 },                                    // trimmed
        { new string('a', MessageText.MAX_LENGTH), MessageTextError.None, 5000 },   // boundary: allowed
        { new string('a', MessageText.MAX_LENGTH + 1), MessageTextError.TooLong, 0 },// boundary: rejected
        { "  " + new string('a', MessageText.MAX_LENGTH) + "  ", MessageTextError.None, 5000 }, // length is measured after trimming
    };

    [Theory]
    [MemberData(nameof(Cases))]
    public void Normalize_TrimsThenEnforcesOneToFiveThousandCharacters(
        string? raw, MessageTextError expectedError, int expectedLength)
    {
        var result = MessageText.Normalize(raw);

        result.Error.ShouldBe(expectedError);
        result.IsValid.ShouldBe(expectedError == MessageTextError.None);
        (result.Text?.Length ?? 0).ShouldBe(expectedLength);
    }
}
