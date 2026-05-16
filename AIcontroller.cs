using Godot;
using Godot.Collections;
using GodotONNX;

public partial class AIcontroller : AIController2D
{
    [Export] private float KP = 5000f;
    [Export] private float KD = 300f;
    [Export] private RichTextLabel label;
    [Export] private PackedScene RagdollScene; // drag your ragdoll .tscn here in inspector
    private Node2D _ragdoll;
    private RigidBody2D Torso => _ragdoll.GetNode<RigidBody2D>("Torso");
    private RigidBody2D Left_leg => _ragdoll.GetNode<RigidBody2D>("Left_Leg");
    private RigidBody2D Right_leg => _ragdoll.GetNode<RigidBody2D>("Right_Leg");
    private RigidBody2D Left_feet => _ragdoll.GetNode<RigidBody2D>("Left_Feet");
    private RigidBody2D Right_feet => _ragdoll.GetNode<RigidBody2D>("Right_Feet");

    private float[] _actions = new float[4]; // 4 joint targets
    private Vector2 _spawnPosition; // set this to where ragdoll should spawn


    public override void _Ready()
    {
        base._Ready();
        SetPhysicsProcess(true);
        _spawnPosition = new Vector2(-250, 0); // your ground spawn point
        SpawnRagdoll();
    }

    private void SpawnRagdoll()
    {
        // remove old one if exists
        if (_ragdoll != null)
        {
            _ragdoll.QueueFree();
            _ragdoll = null;
        }

        // instance fresh copy
        _ragdoll = RagdollScene.Instantiate<Node2D>();
        _ragdoll.GlobalPosition = _spawnPosition;
        Torso.ContactMonitor = true;
        Torso.MaxContactsReported = 4;
        GetParent().CallDeferred("add_child", _ragdoll); // deferred so physics is ready
    }

    public override int GetObservationSize()
    {
        return 12;
    }

    public override int GetActionSize()
    {
        return 4;
    }

    // ✅ What the AI SEES — called every step
    public override Array<float> GetObservations()
    {
        return new Array<float>
        {
            // Torso state
            (float)Torso.LinearVelocity.X,
            (float)Torso.LinearVelocity.Y,
            (float)Torso.AngularVelocity,
            (float)Torso.Rotation,

            // Leg angles & velocities
            (float)Left_leg.Rotation,
            (float)Left_leg.AngularVelocity,
            (float)Right_leg.Rotation,
            (float)Right_leg.AngularVelocity,

            // Feet angles & velocities
            (float)Left_feet.Rotation,
            (float)Left_feet.AngularVelocity,
            (float)Right_feet.Rotation,
            (float)Right_feet.AngularVelocity
        };
    }

    public override void SetAction(Array action)
    {
        // SAFETY CHECK: If the array is empty or too small, abort this frame.
        if (action == null || action.Count < GetActionSize()) return;


        // Now it is safe to index the array
        _actions[0] = Mathf.Clamp((float)action[0], -1f, 1f);
        _actions[1] = Mathf.Clamp((float)action[1], -1f, 1f);
        _actions[2] = Mathf.Clamp((float)action[2], -1f, 1f);
        _actions[3] = Mathf.Clamp((float)action[3], -1f, 1f);
    }

    private bool IsTorsoOnGround()
    {
        var collider = Torso.GetCollidingBodies();
        foreach (var col in collider)
        {
            var body = InstanceFromId(col.GetInstanceId()) as PhysicsBody2D;
            if (body.CollisionMask == 2) return true;
        }


        return false;
    }

    public override void _PhysicsProcess(double delta)
    {
        // guard — ragdoll may be null for 1 frame after respawn
        if (_ragdoll == null || !IsInstanceValid(_ragdoll))
            return;

        // if (Input.IsKeyPressed(Key.R)) needs_reset = true;
        // if (Input.IsKeyPressed(Key.Right)) _actions[1] = 1.0f;

        base._PhysicsProcess(delta);


        if (needs_reset)
        {
            ResetAgent();
            return;
        }

        if (Torso.GlobalPosition.Y > 125f)
        {
            done = true;
            needs_reset = true;
            return;
        }

        // Apply AI actions to joints (same as your PD controller)
        PD.ApplyPD(Left_leg, _actions[0], KP, KD);
        PD.ApplyPD(Right_leg, _actions[1], KP, KD);
        PD.ApplyPD(Left_feet, _actions[2], KP, KD);
        PD.ApplyPD(Right_feet, _actions[3], KP, KD);


        if (IsTorsoOnGround())
        {
            reward -= 1f; // penalty for falling
            done = true;
            needs_reset = true;
            return;
        }

        var forwardVelocity = (float)Torso.LinearVelocity.X;
        var uprightBonus = Mathf.Cos(Torso.Rotation); // 1.0 upright, -1.0 upside down
        var energyPenalty = (_actions[0] * _actions[0]
                             + _actions[1] * _actions[1]
                             + _actions[2] * _actions[2]
                             + _actions[3] * _actions[3]) * 0.001f;

        reward += forwardVelocity * 0.2f // move forward
                  + uprightBonus * 0.1f // stay upright (increased from 0.01)
                  - energyPenalty; // don't waste energy
    }

    private void ResetAgent()
    {
        _actions = new float[4];
        needs_reset = false;
        done = false;
        reward = 0f;
        SpawnRagdoll();
        // Reset logic for positions/velocities...
        reset(); // Call the base reset to clear n_steps and needs_reset
    }
}