using System.Globalization;

namespace BCIT_Social.Tests.Support;

/// <summary>
/// Runs a block of a test under a fixed culture so date/number formatting assertions never depend on the machine
/// (a Vancouver laptop, a French-Canadian CI agent and an Azure container must all behave the same).
/// </summary>
public sealed class CultureScope : IDisposable
{
    private readonly CultureInfo _previous = CultureInfo.CurrentCulture;
    private readonly CultureInfo _previousUi = CultureInfo.CurrentUICulture;

    public CultureScope(string name = "")
    {
        var culture = CultureInfo.GetCultureInfo(name);
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
    }

    public void Dispose()
    {
        CultureInfo.CurrentCulture = _previous;
        CultureInfo.CurrentUICulture = _previousUi;
    }
}
