using GW2D.V1;
using System.Collections.Generic;

namespace Simulation.Utils;

public class EventConstructor
{
    public static Event MakeOutOfBoundsEvent(List<long> ids)
    {
        OutOfBoundsEvent OOBEvent = new() { DroneIds = { ids } };
        return new Event { OutOfBoundsEvent = OOBEvent };
    }

    public static Event MakeTargetReachedEvent(List<long> ids)
    {
        TargetReachedEvent eventTarget = new() { DroneIds = { ids } };
        return new Event { TargetReachedEvent = eventTarget };
    }

    public static Event MakePursuerInTargetEvent(List<long> ids)
    {
        PursuerEnteredTargetEvent PursuerTargetEvent = new() { DroneIds = { ids } };
        return new Event { PursuerEnteredTargetEvent = PursuerTargetEvent };
    }

    public static Event MakeObjectCollisionEvent(List<long> ids)
    {
        DroneObjectCollisionEvent objectCollisionEvent = new() { DroneIds = { ids } };
        return new Event { DroneObjectCollisionEvent = objectCollisionEvent };
    }

    public static Event MakeCollisionEvent((long id1, long id2) ids)
    {
        CollisionEvent collisionEvent = new() { DroneIds = { ids.id1, ids.id2 } };
        return new Event { CollisionEvent = collisionEvent };
    }
}
