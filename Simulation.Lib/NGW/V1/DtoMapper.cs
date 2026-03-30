using GW2D.V1;

namespace Simulation.Lib.NGW.V1;

public static class DtoMapper
{
    public static MapConfiguration MapSpecToMapConfiguration(MapSpec mapSpec)
    {
        return mapSpec.MapOneofCase switch
        {
            MapSpec.MapOneofOneofCase.SquareMap => MapSpecToMapConfiguration(mapSpec.SquareMap),

            MapSpec.MapOneofOneofCase.None => throw new Exception("Received a no map case!"),
            _ => throw new Exception($"Did not recognize the one of case: {mapSpec.MapOneofCase}!"),
        };
    }
    
    public static MapConfiguration MapSpecToMapConfiguration(SquareMap mapSpec)
    {
        return new SquareMapConfiguration(
            mapSpec.Width,
            mapSpec.Height
        );
    }
}
