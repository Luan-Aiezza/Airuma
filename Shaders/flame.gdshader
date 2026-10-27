shader_type canvas_item;

// Red flames that burn round a silhouette. TEXTURE is the serpent's art on a larger transparent canvas; only its alpha is used.
// The flames are worked out per art pixel (the UV is snapped to the texel grid, so they are as blocky as the rest of the
// game): "heat" is how much of the body lies near this pixel (below it, for flames that rise, and all round it), broken up by
// noise that scrolls upwards, and is then cut into a few flat colours. Inside the body only a rim of flame is drawn, so the
// fire seems to lick over the green skin and not to cover it.
uniform float reach = 26.0;   // how far the flames reach from the body, in art pixels
uniform float fury = 1.0;     // how fierce they are (they grow with it)
uniform float rim = 7.0;      // how deep inside the body the rim of flame goes
uniform float speed = 1.7;    // how fast the flames rise
uniform float cell = 8.0;     // size of the flames' tongues, in art pixels

float hash21(vec2 p) {
	return fract(sin(dot(p, vec2(12.9898, 78.233))) * 43758.5453);
}

float vnoise(vec2 p) {
	vec2 i = floor(p);
	vec2 f = fract(p);
	f = f * f * (3.0 - 2.0 * f);
	float a = hash21(i);
	float b = hash21(i + vec2(1.0, 0.0));
	float c = hash21(i + vec2(0.0, 1.0));
	float d = hash21(i + vec2(1.0, 1.0));
	return mix(mix(a, b, f.x), mix(c, d, f.x), f.y);
}

float fbm(vec2 p) {
	return 0.55 * vnoise(p) + 0.3 * vnoise(p * 2.1 + 3.7) + 0.15 * vnoise(p * 4.3 + 9.1);
}

float body(sampler2D tex, vec2 uv) {
	return texture(tex, uv).a;
}

// Pixels that burn on their own (the red channel of the mask), such as the slits of the eyes.
float emit(sampler2D tex, vec2 uv) {
	vec4 c = texture(tex, uv);
	return c.r * c.a;
}

void fragment() {
	vec2 px = TEXTURE_PIXEL_SIZE;
	vec2 cellxy = floor(UV / px);
	vec2 uv = (cellxy + 0.5) * px;
	float t = TIME * speed;

	// Two noise fields that scroll up: one makes the tongues, the other sways them from side to side.
	vec2 np = vec2(cellxy.x / cell, cellxy.y / cell + t);
	float n = fbm(np);
	float sway = fbm(vec2(cellxy.x / (cell * 1.6) + 11.0, cellxy.y / (cell * 1.6) + t * 0.7)) - 0.5;

	// Flames rise: the body below this pixel feeds them, the further the weaker.
	float below = 0.0, w = 0.0;
	for (int i = 1; i <= 8; i++) {
		float a = float(i) / 8.0;
		float weight = 1.0 - a * a;
		below += body(TEXTURE, uv + vec2(sway * 2.0 * a * reach, a * reach * 0.9) * px) * weight;
		w += weight;
	}
	below /= w;

	// And they spread to the sides and above, a little, from the nearest part of the body.
	float ring = 0.0;
	for (int k = 0; k < 8; k++) {
		float ang = float(k) * 0.785398 + n * 0.8;
		float r = reach * (0.28 + 0.3 * n);
		ring += body(TEXTURE, uv + vec2(cos(ang), sin(ang)) * r * px);
	}
	ring /= 8.0;

	float inside = body(TEXTURE, uv);
	float heat;
	if (inside > 0.5) {
		// Only a rim, where the body is close to its edge.
		float depth = 0.0;
		for (int k = 0; k < 8; k++) {
			float ang = float(k) * 0.785398;
			depth += body(TEXTURE, uv + vec2(cos(ang), sin(ang)) * rim * px);
		}
		depth /= 8.0;
		heat = (1.0 - depth) * (0.25 + 1.3 * n) * 1.15 * fury - 0.12;
	} else {
		heat = (below * 1.0 + ring * 0.7) * (0.3 + 1.3 * n) * fury - 0.04;
	}

	// The eyes pour out flames of their own: they rise from the slits, far, and swell the nearer the slit.
	float eyes = 0.0, ew = 0.0;
	for (int i = 0; i <= 10; i++) {
		float a = float(i) / 10.0;
		float weight = 1.0 - a * 0.8;
		eyes += emit(TEXTURE, uv + vec2(sway * 2.4 * a * reach, a * reach * 1.25) * px) * weight;
		ew += weight;
	}
	eyes /= ew * 0.55;
	heat = max(heat, eyes * (0.4 + 1.5 * n) * fury - 0.05);

	// Flat colours, hot at the root, dark red at the tips.
	vec4 col = vec4(0.0);
	if (heat > 0.8) col = vec4(1.0, 0.82, 0.25, 1.0);
	else if (heat > 0.58) col = vec4(1.0, 0.45, 0.1, 1.0);
	else if (heat > 0.36) col = vec4(0.88, 0.15, 0.09, 1.0);
	else if (heat > 0.2) col = vec4(0.5, 0.06, 0.07, 0.9);
	COLOR = col;
}
