shader_type canvas_item;

// TEXTURE = everything the water reflects (sky, crowns, boat, boss, lights), rendered by the SubViewport.
// bed     = the river bed, seen faintly through the water
// moon    = the moon's reflection, in grid pixels (column, row from the top)
// UV y grows downward, so the row from the top is simply cell.y.
uniform sampler2D bed : filter_nearest, repeat_disable;
uniform vec2 moon = vec2(124.0, 56.0);
uniform vec2 grid = vec2(190.0, 194.0);
uniform vec2 bed_size = vec2(190.0, 256.0);
uniform float scroll_rows = 9.1;
uniform float time = 0.0;

float hash(vec2 p) {
	return fract(sin(dot(p, vec2(12.9898, 78.233))) * 43758.5453);
}

void fragment() {
	vec2 cell = clamp(floor(UV * grid), vec2(0.0), grid - 1.0);
	float rowTop = cell.y;
	float t = time;

	// Ripples in whole grid pixels; they swell around the moon's reflection.
	float w1 = sin(rowTop * 0.33 + t * 1.2);
	float w2 = sin(rowTop * 0.85 - t * 1.9 + cell.x * 0.11);
	vec2 toMoon = vec2(cell.x, rowTop) - moon;
	float amp = 0.9 + 3.6 * exp(-dot(toMoon, toMoon) / 1500.0);
	float dx = floor((w1 * 0.6 + w2 * 0.4) * amp + 0.5);
	float dy = floor(sin(cell.x * 0.17 + t * 0.8 + rowTop * 0.05) * 0.8 + 0.5);

	// Now and then, small rings spread from a few random spots of the river: each slot waits a while, pops up somewhere new,
	// sends out a little ripple that fades, and starts waiting again.
	float ring_light = 0.0;
	for (int i = 0; i < 5; i++) {
		float fi = float(i);
		float period = 6.5 + fi * 1.3;                    // seconds between two ripples of this slot
		float life = 2.2;
		float shifted = t + fi * 3.1;
		float cycle = floor(shifted / period);
		float age = shifted - cycle * period;
		if (age > life) continue;
		vec2 centre = vec2(18.0 + hash(vec2(fi, cycle)) * (grid.x - 36.0), 25.0 + hash(vec2(cycle, fi + 7.0)) * (grid.y - 40.0));
		vec2 d = vec2(cell.x, rowTop) - centre;
		float dist = length(d);
		float r = age * 9.0;                              // the ring's radius in grid pixels
		float fade = 1.0 - age / life;
		float band = (dist - r);
		float prof = sin(band * 1.6) * exp(-band * band / 5.0) * fade;
		vec2 dir = dist > 0.001 ? d / dist : vec2(0.0);
		dx += floor(dir.x * prof * 1.6 + 0.5);
		dy += floor(dir.y * prof * 1.6 + 0.5);
		ring_light += prof;
	}

	vec2 sc = vec2(clamp(cell.x + dx, 0.0, grid.x - 1.0), clamp(rowTop - dy, 0.0, grid.y - 1.0));
	vec4 refl = texture(TEXTURE, (sc + 0.5) / grid);

	// The bed, scrolling slower than the surface, refracted a little less.
	float bedRow = mod(rowTop - floor(t * scroll_rows), bed_size.y);
	float bx = clamp(cell.x + floor(dx * 0.5), 0.0, bed_size.x - 1.0);
	vec3 bedCol = texture(bed, vec2((bx + 0.5) / bed_size.x, (bedRow + 0.5) / bed_size.y)).rgb;
	vec3 under = mix(vec3(0.03, 0.09, 0.12), bedCol, 0.8);

	// Clear water: mostly the reflection, with the bed showing through.
	vec3 col = mix(under, refl.rgb, refl.a * 0.82);

	col += vec3(0.14, 0.21, 0.27) * clamp(ring_light, -0.5, 1.0);

	// Stars twinkle.
	float lum = dot(refl.rgb, vec3(0.34));
	if (lum > 0.7 && hash(cell + floor(t * 1.5)) > 0.75) col *= 0.7;

	// Glitter path under the moon.
	float below = rowTop - moon.y;
	if (below > 3.0) {
		float rh = hash(vec2(rowTop, floor(t * 4.0)));
		float spread = 3.0 + below * 0.07;
		float centre = moon.x + (rh - 0.5) * 6.0 + dx;
		float hw = 0.8 + rh * 3.0;
		if (rh > 0.35 && abs(cell.x - centre) < hw && abs(cell.x - moon.x) < spread) {
			col += vec3(0.55, 0.55, 0.42) * (1.0 - below / 190.0);
		}
	}

	// Sparkles on the surface.
	if (hash(cell + floor(t * 3.0)) > 0.9985) col = vec3(0.9, 0.97, 1.0);

	// Slightly darker towards the banks.
	col *= 1.0 - 0.22 * pow(abs(UV.x * 2.0 - 1.0), 3.0);

	COLOR = vec4(col, 1.0);
}
