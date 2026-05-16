import torch
from stable_baselines3 import PPO

model = PPO.load("./checkpoints/ragdoll_517000_steps", device="cpu")

class FullActor(torch.nn.Module):
    def __init__(self, policy):
        super().__init__()
        self.policy = policy

    def forward(self, obs, state_ins):
        # wrap in dict — this is how godot-rl sends observations
        obs_dict = {"obs": obs}
        features = self.policy.extract_features(obs_dict, self.policy.pi_features_extractor)
        latent = self.policy.mlp_extractor.forward_actor(features)
        action = self.policy.action_net(latent)
        state_outs = torch.zeros(state_ins.shape)
        return action, state_outs

actor = FullActor(model.policy)
actor.eval()
actor.cpu()

dummy_obs       = torch.randn(1, 12).cpu()
dummy_state_ins = torch.zeros(1).cpu()

torch.onnx.export(
    actor,
    (dummy_obs, dummy_state_ins),
    "ragdoll_actor.onnx",
    opset_version=15,
    input_names=["obs", "state_ins"],
    output_names=["action", "state_outs"],
    dynamic_axes={
        "obs":        {0: "batch_size"},
        "state_ins":  {0: "batch_size"},
        "action":     {0: "batch_size"},
        "state_outs": {0: "batch_size"},
    }
)
print("ONNX export successful!")
