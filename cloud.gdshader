shader_type canvas_item;

uniform vec4 clouds_color : source_color = vec4(1.0);

void fragment() {
    // UV goes from 0 -> 1
    // convert to -1 -> 1 centered coordinates
    vec2 p = UV * 5.0 - 2.5;

    float r = length(p);
    float theta = atan(p.y, p.x);

    float border =
        sin(theta)
        + 1.0
        + pow(cos(2.0 * theta), 3.0);

    if (r <= border) {
        COLOR.rgb = clouds_color.rgb;
        COLOR.a = clouds_color.a;
    } else {
        COLOR.a = 0.0;
    }
}