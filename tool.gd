@tool
extends EditorScript

func _run():
	var tex = load("res://Assets/Spritesheet/platformPack_tilesheet.png")  # your imported texture
	ResourceSaver.save(tex, "res://Assets/Spritesheet/atlas.tres")
