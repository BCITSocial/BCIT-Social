using BCIT_Social.Core.Identity;

namespace BCIT_Social.Tests.Unit.Domain;

public class BcitEmailPolicyTests
{
    // T19
    [Theory]
    [InlineData("maya@bcit.ca", true)]
    [InlineData("maya@my.bcit.ca", true)]
    [InlineData("MAYA@BCIT.CA", true)]                
    [InlineData("  maya@my.bcit.ca  ", true)]          
    [InlineData("maya@gmail.com", false)]
    [InlineData("maya@bcit.ca.chipilon.com", false)]   
    [InlineData("maya@chipilon.bcit.ca", false)]            
    [InlineData("maya@sub.bcit.ca", false)]            
    [InlineData("maya@bcit.com", false)]
    [InlineData("@bcit.ca", false)]                    
    [InlineData("a@@bcit.ca", false)]
    [InlineData("chipilon@x.com@bcit.ca", false)]          
    [InlineData("bcit.ca", false)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    [InlineData(null, false)]
    public void IsBcitEmail_AcceptsOnlyOfficialBcitAddresses(string? email, bool expected)
    {
        BcitEmailPolicy.IsBcitEmail(email).ShouldBe(expected);
    }
}
