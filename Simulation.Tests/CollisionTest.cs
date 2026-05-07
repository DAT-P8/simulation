using Simulation.Utils;
using Godot;

namespace Simulation.Tests;

public class CollisionTest
{
    [Fact]
    public void TestDroneCollisions()
    {
        (Vector2I, Vector2I) drone1;
        (Vector2I, Vector2I) drone2;

        //=== Collision ===
        //Same square
        drone1 = (new Vector2I(-1, 0), new Vector2I(1, 1));
        drone2 = (new Vector2I(1, 0), new Vector2I(-1, 1));
        Assert.True(SweepDrones(drone1, drone2), "Same square");

        //Cross at border
        drone1 = (new Vector2I(0, 0), new Vector2I(1, 1));
        drone2 = (new Vector2I(1, 0), new Vector2I(-1, 1));
        Assert.True(SweepDrones(drone1, drone2), "Cross at border");

        //One moves right fast
        drone1 = (new Vector2I(0, 0), new Vector2I(3, 0));
        drone2 = (new Vector2I(1, 0), new Vector2I(1, 0));
        Assert.True(SweepDrones(drone1, drone2), "Fast right");

        //Move in front of path
        drone1 = (new Vector2I(0, 1), new Vector2I(3, 0));
        drone2 = (new Vector2I(2, 0), new Vector2I(1, 1));
        Assert.True(SweepDrones(drone1, drone2), "Move in front");

        //Head on
        drone1 = (new Vector2I(0, 0), new Vector2I(3, 0));
        drone2 = (new Vector2I(4, 0), new Vector2I(-2, 0));
        Assert.True(SweepDrones(drone1, drone2), "Head on");

        //=== No collision ===
        //Both move up
        drone1 = (new Vector2I(0, 0), new Vector2I(0, 1));
        drone2 = (new Vector2I(1, 0), new Vector2I(0, 1));
        Assert.False(SweepDrones(drone1, drone2), "Both move up");

        //Both move right
        drone1 = (new Vector2I(0, 0), new Vector2I(1, 0));
        drone2 = (new Vector2I(1, 0), new Vector2I(1, 0));
        Assert.False(SweepDrones(drone1, drone2), "Both move right");
    }

    private bool SweepDrones((Vector2I, Vector2I) drone1, (Vector2I, Vector2I) drone2)
    {
        var sweep = CollisionChecker.SweepPair(drone1, drone2);
        var dot = sweep.Dot(sweep);
        return dot < 0.5f;
    }

    [Fact]
    public void TestObjectCollisions()
    {
        // X - object, O - free space
        //  1 X X O
        //  0 X O X
        // -1 O O X
        //   -1 0 1

        List<(int, int)> objectPos = [(-1, 1), (0, 1), (-1, 0), (1, 0), (1, -1)];
        var objects = GenerateObjects(objectPos);
        List<(long, (int, int), (int, int), long)> collidingDrones = [
            (0,(0,0),(1,0),2),
            (1,(0,0),(-1,1),1),
            (2,(0,-1),(0,1),2)
        ];
        List<(long, (int, int), (int, int), long)> otherDrones = [
            (3,(0,0),(0,0),0),
            (4,(0,0),(1,1),2),
            (5,(1,1),(-1,-1),3)
        ];

        var allDroneData = GenerateDrones(collidingDrones.Concat(otherDrones).ToList());
        var collisionIds = CollisionChecker.IntersectAnyPoint(allDroneData, objects);
        List<long> actualIds = [0, 1, 2];
        Assert.False(collisionIds.Except(actualIds).Any(), "Object collisions");
    }

    private List<Vector2I> GenerateObjects(List<(int x, int y)> pos)
    {
        List<Vector2I> objects = [];
        foreach (var (x, y) in pos)
            objects.Add(new Vector2I(x, y));
        return objects;
    }

    private List<(long, Vector2I, Vector2I, long)> GenerateDrones(
            List<(long id, (int dx, int dy), (int ax, int dy), long velocity)> pos
            )
    {
        List<(long droneId, Vector2I dronePos, Vector2I actionVec, long velocity)> drones = [];
        foreach (var (id, (dx, dy), (ax, ay), velocity) in pos)
            drones.Add((id, new Vector2I(dx, dy), new Vector2I(ax, ay), velocity));
        return drones;
    }
}
