using Autofac;
using GW2D.V1;
using Serilog.Core;
using Simulation.Services;

namespace Simulation.Tests;

public class MapTests
{
    private readonly IPositionUtility _positionUtility;

    public MapTests()
    {
        var builder = new ContainerBuilder();
        var logger = Logger.None;

        builder.RegisterServices(logger, "localhost", 42069, 420);
        var container = builder.Build();

        _positionUtility = container.Resolve<IPositionUtility>();
    }

    [Fact]
    public void TestBoundsCheckSquareMap()
    {
        var mapSpec = new MapSpec
        {
            SquareMap = new SquareMap
            {
                Height = 10,
                Width = 10,
                TargetX = 5,
                TargetY = 5
            }
        };

        Assert.True(_positionUtility.IsInBounds(mapSpec, new(0, 0, 0)));

        Assert.True(_positionUtility.IsInBounds(mapSpec, new(9, 0, 9)));

        Assert.True(!_positionUtility.IsInBounds(mapSpec, new(0, 0, 10)));

        Assert.True(!_positionUtility.IsInBounds(mapSpec, new(10, 0, 0)));
    }
}
