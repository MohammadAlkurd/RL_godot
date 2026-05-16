shader_type canvas_item;


uniform sampler2D atlas_array : source_color,filter_nearest_mipmap_anisotropic,repeat_enable;
uniform float repeat = 10.0;      // how many times to tile
uniform float repeat_y =1.0;
void fragment() {
	vec2 tiled_uv = vec2(UV.x * repeat, UV.y*repeat_y);
    // 3rd component = layer index (the tile ID)
    vec4 color = texture(atlas_array, tiled_uv);
    
    COLOR = color;
}
