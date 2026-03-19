using Simulation.Utils;

namespace Simulation.Tests.TDFTests;

public class ProjectionTests
{
    public static IEnumerable<object[]> ProjectionPointData => [
        [
            new Vector3D<float>(1, 1, 0),
            new Vector3D<float>(-1, 1, 0),
            new Vector3D<float>(0, 1, 0),
        ],
        [
            new Vector3D<float>(-2, -2, 0),
            new Vector3D<float>(-2, 2, 0),
            new Vector3D<float>(-2, 0, 0),
        ],
        [
            new Vector3D<float>(2, 2, 0),
            new Vector3D<float>(-2, -2, 0),
            new Vector3D<float>(0, 0, 0),
        ],
        [
            new Vector3D<float>(8, 8, 0),
            new Vector3D<float>(4, 4, 0),
            new Vector3D<float>(4, 4, 0),
        ],
        [
            new Vector3D<float>(0, 0, 0),
            new Vector3D<float>(0, 0, 0),
            new Vector3D<float>(0, 0, 0),
        ],
    ];

    [Theory]
    [MemberData(nameof(ProjectionPointData))]
    public void ProjectionTest(Vector3D<float> A, Vector3D<float> B, Vector3D<float> Point)
    {
        var point = VectorExtensions.ProjectPointOntoSegment(new Vector3D<float>(0, 0, 0), A, B);

        Assert.True(point.EqualsWithEpsilon(Point));
    }
}
