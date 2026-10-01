using Ambev.DeveloperEvaluation.Application;
using AutoMapper;

namespace Ambev.DeveloperEvaluation.Unit.TestData;

/// <summary>
/// Real AutoMapper configuration of the application layer.
/// </summary>
public static class TestMapper
{
    public static readonly MapperConfiguration Configuration =
        new(cfg => cfg.AddMaps(typeof(ApplicationLayer).Assembly));

    public static IMapper Create() => Configuration.CreateMapper();
}
