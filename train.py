from godot_rl.wrappers.stable_baselines_wrapper import StableBaselinesGodotEnv
from stable_baselines3 import PPO
from stable_baselines3.common.vec_env import VecMonitor
from stable_baselines3.common.callbacks import CheckpointCallback

env = StableBaselinesGodotEnv(
    env_path="./exports/ragdoll.x86_64",
    n_parallel=1,
    speedup=1,
    show_window=True
)
env = VecMonitor(env)

# saves every 50k steps into checkpoints/
checkpoint_callback = CheckpointCallback(
    save_freq=1_000,
    save_path="./checkpoints/",
    name_prefix="ragdoll"
)

# model = PPO(
#     "MultiInputPolicy",
#     env,
#     verbose=1,
#     device="cuda",
#     n_steps=2048,
#     batch_size=64,
#     learning_rate=3e-4,
#     tensorboard_log="./logs/"
# )

# instead of PPO(...) use PPO.load()
model = PPO.load("./checkpoints/ragdoll_517000_steps", env=env)
model.learn(total_timesteps=2_000_000, callback=checkpoint_callback,reset_num_timesteps=False)
model.save("ragdoll_final")
env.close()
