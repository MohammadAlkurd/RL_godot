using Godot;
using Godot.Collections;
using GodotONNX;

public abstract partial class AIController2D : Node2D
{
    // 1. Keep the C# Enum for the Inspector and internal logic
    public enum ControlModesEnum
    {
        INHERIT_FROM_SYNC,
        HUMAN,
        TRAINING,
        ONNX_INFERENCE,
        RECORD_EXPERT_DEMOS
    }

    public Dictionary ControlModes => new()
    {
        { "INHERIT_FROM_SYNC", 0 },
        { "HUMAN", 1 },
        { "TRAINING", 2 },
        { "ONNX_INFERENCE", 3 },
        { "RECORD_EXPERT_DEMOS", 4 }
    };

    [Export] public ControlModesEnum control_mode = ControlModesEnum.INHERIT_FROM_SYNC;
    [Export] public string onnx_model_path = "";
    [Export] public int reset_after = 1000;
    [Export] public string policy_name = "shared_policy";

    // Fields accessed directly by sync.gd via property names
    public GodotObject onnx_model = new();
    public string heuristic = "human";
    public bool done = false;
    public float reward = 0.0f;
    public int n_steps = 0;
    public bool needs_reset = false;

    public override void _Ready()
    {
        AddToGroup("AGENT");
    }

    public override void _PhysicsProcess(double delta)
    {
        n_steps++;
        if (n_steps > reset_after) needs_reset = true;
    }

    // --- Bridge Methods for sync.gd ---

    public Dictionary get_obs()
    {
        var obs = GetObservations();
        return new Dictionary { ["obs"] = obs };
    }

    public float get_reward()
    {
        return reward;
    }

    public virtual Dictionary get_action_space()
    {
        return new Dictionary
        {
            ["action"] = new Dictionary
            {
                // CRITICAL: Python does (v["size"],). If this is an Array, it becomes ([4],) -> ERROR.
                // It MUST be a plain integer.
                ["size"] = 4,
                ["action_type"] = "continuous"
            }
        };
    }

    public virtual Dictionary get_obs_space()
    {
        return new Dictionary
        {
            ["obs"] = new Dictionary
            {
                // CRITICAL: Python does shape=v["size"]. If this is an int, it's not iterable -> ERROR.
                // It MUST be an Array [12].
                ["size"] = new Array { 12 },
                ["space"] = "box"
            }
        };
    }

    // Add this to your AIController2D.cs
    public virtual Array get_action()
    {
        // Return an array of zeros so the system doesn't crash in human mode
        var actions = new Array();
        for (var i = 0; i < GetActionSize(); i++) actions.Add(0.0f);
        return actions;
    }

    public void set_action(Dictionary action) // Dictionary, not Array
    {
        // actions are nested under your action space key "action"
        var arr = action["action"].AsGodotArray();
        SetAction(arr);
    }

    public void set_heuristic(string h)
    {
        heuristic = h;
    }

    public bool get_done()
    {
        return done;
    }

    public void set_done_false()
    {
        done = false;
    }

    public void zero_reward()
    {
        reward = 0.0f;
    }

    public virtual void reset()
    {
        n_steps = 0;
        needs_reset = false;
    }

    // --- Implementation Requirements for your Agent ---
    public abstract Array<float> GetObservations();
    public abstract void SetAction(Array action);
    public abstract int GetObservationSize();
    public abstract int GetActionSize();
}