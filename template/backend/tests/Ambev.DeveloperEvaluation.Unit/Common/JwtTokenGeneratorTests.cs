using Ambev.DeveloperEvaluation.Common.Security;
using Ambev.DeveloperEvaluation.Unit.TestData;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Common;

public class JwtTokenGeneratorTests
{
    [Fact(DisplayName = "The token carries the user id, name and role")]
    public void GenerateToken_ContainsClaims()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:SecretKey"] = "UnitTestsSecretKeyThatIsLongEnoughForHmacSha256!!"
            })
            .Build();

        var user = UserTestData.User();
        var token = new JwtTokenGenerator(configuration).GenerateToken(user);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        jwt.Claims.Should().Contain(c => c.Value == user.Id.ToString());
        jwt.Claims.Should().Contain(c => c.Value == user.Username);
        jwt.Claims.Should().Contain(c => c.Value == "Customer");
        jwt.ValidTo.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact(DisplayName = "BCrypt hasher verifies the hashed password")]
    public void BCryptPasswordHasher_VerifiesHash()
    {
        var hasher = new BCryptPasswordHasher();
        var hash = hasher.HashPassword(UserTestData.ValidPassword);

        hash.Should().NotBe(UserTestData.ValidPassword);
        hasher.VerifyPassword(UserTestData.ValidPassword, hash).Should().BeTrue();
        hasher.VerifyPassword("wrong", hash).Should().BeFalse();
    }
}
