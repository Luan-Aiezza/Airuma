shader_type canvas_item;

// TEXTURE = the sky and the far trees, flipped. Row 0 is at the horizon; rows grow towards us.
uniform float grid_w = 278.0;
uniform float grid_h = 110.0;
uniform float moon_x = 182.0;

float hash(vec2 p) {
	return fract(sin(dot(p, vec2(12.9898, 78.233))) * 43758.5453);
}

void fragment() {
	vec2 grid = vec2(grid_w, grid_h);
	// Godot's UV y grows downward: k is the row counted from the horizon;
	vec2 pix = clamp(floor(UV * grid), vec2(0.0), grid - 1.0);
	float k = pix.y;
	vec2 cell = vec2(pix.x, grid.y - 1.0 - pix.y);
	float t = TIME;

	// Ripples whose spacing grows towards us (perspective), in whole pixels.
	float depth = log(k + 3.0);
	float w1 = sin(depth * 11.0 - t * 1.5 + cell.x * 0.03);
	float w2 = sin(depth * 5.0 + t * 0.8 + cell.x * 0.09);
	float amp = 0.5 + k * 0.03;
	float dx = floor((w1 * 0.6 + w2 * 0.4) * amp + 0.5);
	vec2 sc = vec2(clamp(cell.x + dx, 0.0, grid.x - 1.0), pix.y);
	vec3 col = texture(TEXTURE, (sc + 0.5) / grid).rgb;

	// The moon's glitter, breaking up into dashes.
	float rh = hash(vec2(k, floor(t * 3.0)));
	float spread = 4.0 + k * 0.06;
	if (k > 2.0 && abs(cell.x - moon_x - dx) < spread * (0.3 + rh * 0.7) && rh > 0.45) {
		col += vec3(0.5, 0.52, 0.4) * (1.0 - k / 120.0);
	}
	// Sparkles on the surface.
	if (hash(cell + floor(t * 2.5)) > 0.9985) col = vec3(0.85, 0.95, 1.0);
	// Dark water by the boat.
	col *= 1.0 - 0.25 * smoothstep(0.55, 1.0, k / grid.y);
	COLOR = vec4(col, 1.0);
}
