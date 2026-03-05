using System;
using Godot;
using Simulation.Utils;
using TDFSimulation;

namespace Simulation.TDF;

public class TDFDrone(StaticBody3D body, long id, bool isEvader) : IDisposable
{
    private readonly bool _isEvader = isEvader;
    private readonly StaticBody3D _body = body;
    private readonly long _id = id;
    private readonly Vector3D<float> _position = new(0, 0, 0);
    private readonly Vector3D<float> _velocity = new(0, 0, 0);
    private readonly Vector3D<float> _force = new(0, 0, 0);

    public bool IsEvader => _isEvader;
    public StaticBody3D Body => _body;
    public long Id => _id;

    public bool IsDestroyed = false;

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
     * Gets a copy of the velocity, you cannot set the velocity through this object.
     * </summary>
    */
    public Vector3D<float> GetVelocity()
    {
        return new Vector3D<float>(_velocity.X, _velocity.Y, _velocity.Z);
    }

    /**
     * <summary>
     * Sets the velocity of the object.
     * </summary>
     */
    public void SetVelocity(Vector3D<float> velocity)
    {
        _velocity.SetVector3D(velocity);
    }
    
    /**
     * <summary>
     * Set a constant force to this drone, changing velocity linearly
     * </summary>
     */
    public void SetForce(Vector3D<float> force)
    {
        _force.SetVector3D(force);
    }


    /**
     * <summary>
     * Advances the physics of this drone by some time step.
     * </summary>
     */
    public void AdvanceTime(float step)
    {
        var new_velocity = _velocity.Add(_force.Scale(step));
        var new_position = 1/2 * step * step;
    }

    /*
       a = const
       v = integral a = v0 + x * a
       p = integral v = v0 + 1/2 * x^2 * a
       */

    public TDFDroneState GetState()
    {
        return new TDFDroneState
        {
            X = _position.X,
            Y = _position.Y,
            Z = _position.Z,

            XV = _velocity.X,
            YV = _velocity.Y,
            ZV = _velocity.Z,

            Destroyed = IsDestroyed,
            Id = _id,
            IsEvader = _isEvader,
        };
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _body.QueueFree();
    }
}
