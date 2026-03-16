using Simulation.Utils;

namespace Simulation.Tests.TDFTests;

public class CollisionTests
{
    public static IEnumerable<object[]> NonCollidingPointData =>
        [
            // Two points moving in the same direction, no crossing
            [
                new List<Vector3D<float>> { new(-2f, 0f, 0f), new(2f, 0f, 0f) },
                new List<Vector3D<float>> { new(-1f, 0f, 0f), new(3f, 0f, 0f) }
            ],
            // Two points moving away from each other
            [
                new List<Vector3D<float>> { new(-1f, 0f, 0f), new(1f, 0f, 0f) },
                new List<Vector3D<float>> { new(-3f, 0f, 0f), new(3f, 0f, 0f) }
            ],
            // Two points moving parallel
            [
                new List<Vector3D<float>> { new(-2f, 1.5f, 0f), new(-2f, -1.5f, 0f) },
                new List<Vector3D<float>> { new(2f, 1.5f, 0f),  new(2f, -1.5f, 0f) }
            ],
            // Two points both stationary and far apart
            [
                new List<Vector3D<float>> { new(-5f, 0f, 0f), new(5f, 0f, 0f) },
                new List<Vector3D<float>> { new(-5f, 0f, 0f), new(5f, 0f, 0f) }
            ],
            // Three points all moving in same direction
            [
                new List<Vector3D<float>> { new(-3f, 0f, 0f), new(0f, 0f, 0f), new(3f, 0f, 0f) },
                new List<Vector3D<float>> { new(-2f, 0f, 0f), new(1f, 0f, 0f), new(4f, 0f, 0f) }
            ],
            // Chase sequence, non-colliding but overlapping
            [
                new List<Vector3D<float>> { new(10f, 10f, 10f), new(0f, 0f, 0f) },
                new List<Vector3D<float>> { new(20f, 20f, 20f), new(15f, 15f, 15f) }
            ],
            // Stationary points
            [
                new List<Vector3D<float>> { new(0f, 0f, 0f), new(2f, 0f, 0f) },
                new List<Vector3D<float>> { new(0f, 0f, 0f), new(2f, 0f, 0f) }
            ],
            // A crossing but crosses before other drone comes
            [
                new List<Vector3D<float>> { new(-5f, 0f, 0f), new(5f, 0f, 0f) },
                new List<Vector3D<float>> { new(0f, -1f, 0f), new(0f, 11f, 0f) }
            ]
        ];

    public static IEnumerable<object[]> CollidingPointData =>
        [
            // Two points moving directly toward each other along X axis
            [
                new List<Vector3D<float>> { new(-2f, 0f, 0f), new(2f, 0f, 0f) },
                new List<Vector3D<float>> { new(2f, 0f, 0f),  new(-2f, 0f, 0f) }
            ],
            // Two points moving toward each other along Y axis
            [
                new List<Vector3D<float>> { new(0f, -2f, 0f), new(0f, 2f, 0f) },
                new List<Vector3D<float>> { new(0f, 2f, 0f),  new(0f, -2f, 0f) }
            ],
            // Two points moving toward each other along Z axis
            [
                new List<Vector3D<float>> { new(0f, 0f, -2f), new(0f, 0f, 2f) },
                new List<Vector3D<float>> { new(0f, 0f, 2f),  new(0f, 0f, -2f) }
            ],
            // One stationary point at origin, one sweeping through it
            [
                // TODO: failing
                new List<Vector3D<float>> { new(0f, 0f, 0f), new(-1.5f, 0f, 0f) },
                new List<Vector3D<float>> { new(0f, 0f, 0f), new(1.5f, 0f, 0f) }
            ],
            // Diagonal approach — both moving toward origin from opposite corners
            [
                new List<Vector3D<float>> { new(-2f, -2f, 0f), new(2f, 2f, 0f) },
                new List<Vector3D<float>> { new(2f, 2f, 0f),   new(-2f, -2f, 0f) }
            ],
            // Three points, two collide while third passes by safely
            [
                new List<Vector3D<float>> { new(-2f, 0f, 0f), new(2f, 0f, 0f) },
                new List<Vector3D<float>> { new(2f, 0f, 0f),  new(-2f, 0f, 0f) }
            ],
            // Near-miss that just grazes the unit sphere boundary
            [
                new List<Vector3D<float>> { new(-2f, 0.99f, 0f), new(2f, 0.99f, 0f) },
                new List<Vector3D<float>> { new(2f, 0.99f, 0f),  new(-2f, 0.99f, 0f) }
            ],
            // Stationary points
            [
                new List<Vector3D<float>> { new(0f, 0f, 0f), new(0f, 0f, 0f) },
                new List<Vector3D<float>> { new(.9f, 0f, 0f), new(.9f, 0f, 0f) }
            ],
        ];

    [Theory]
    [MemberData(nameof(CollidingPointData))]
    public void TestColliding(List<Vector3D<float>> beforePositions, List<Vector3D<float>> afterPositions)
    {
        var points = TDF.TDFSimulation.SweepTests(beforePositions, afterPositions);

        foreach (var (point, _, _) in points)
            Assert.InRange(point.Dot(point), 0f, 1f);
    }
    
    [Theory]
    [MemberData(nameof(NonCollidingPointData))]
    public void TestNonColliding(List<Vector3D<float>> beforePositions, List<Vector3D<float>> afterPositions)
    {
        var points = TDF.TDFSimulation.SweepTests(beforePositions, afterPositions);

        foreach (var (point, _, _) in points)
            Assert.NotInRange(point.Dot(point), 0f, 1f);
    }
}
