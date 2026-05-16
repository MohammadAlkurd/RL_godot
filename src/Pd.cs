using Godot;

public static class PD
{
    public static void ApplyPD(RigidBody2D body, float target, float KP, float KD)
    {
        var error = AngleDifference(target, body.Rotation);

        var torque =
            error * KP
            - body.AngularVelocity * KD;

        body.ApplyTorque(torque);
    }

    public static float AngleDifference(float target, float current)
    {
        return Mathf.Atan2(
            Mathf.Sin(target - current),
            Mathf.Cos(target - current)
        );
    }
}