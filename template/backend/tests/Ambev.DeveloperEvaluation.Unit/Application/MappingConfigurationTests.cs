using Ambev.DeveloperEvaluation.Unit.TestData;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

public class MappingConfigurationTests
{
    [Fact(DisplayName = "AutoMapper configuration is valid")]
    public void Configuration_IsValid()
    {
        TestMapper.Configuration.AssertConfigurationIsValid();
    }
}
