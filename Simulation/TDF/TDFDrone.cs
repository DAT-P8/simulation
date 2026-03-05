using Godot;
using Simulation.Utils;

namespace Simulation.TDF;

public class TDFDrone(StaticBody3D body, long id)
{
    private readonly StaticBody3D _body = body;
    private readonly long _id = id;
    private readonly Vector3D<float> _position = new(0, 0, 0);

    public StaticBody3D Body => _body;
    public long Id => _id;

    /**
     * <summary>
     * Gets a copy of the position, you cannot set the position through the returned object.
     * </summary>
    */
    public Vector3D<float> GetPosition()
    {
        return new Vector3D<float>(_position.X, _position.Y, _position.Z);
    }

    /**
     * <summary>
     * Sets the position of the actual object, by deferring the SetPosition call to the main thread,
     * and internally updating the position.
     * </summary>
     */
    public void SetPosition(Vector3D<float> position)
    {
        _position.SetVector3D(position);
        _body.CallDeferred(Node3D.MethodName.SetPosition, new Vector3(position.X, position.Y, position.Z));
    }
    
    /**
     * <summary>
     * Sets the position of the actual object, by deferring the SetPosition call to the main thread,
     * and internally updating the position.
     * </summary>
     */
    public void SetPosition(float x, float y, float z)
    {
        _position.SetVector3D(x, y, z);
        _body.CallDeferred(Node3D.MethodName.SetPosition, new Vector3(x, y, z));
    }
}
