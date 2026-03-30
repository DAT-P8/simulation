namespace Simulation.Lib.NGW.V1;

public record MapConfiguration;

public record SquareMapConfiguration(
    long Width,
    long Height
) : MapConfiguration;
