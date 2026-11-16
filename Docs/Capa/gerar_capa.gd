extends SceneTree
# The cover of the game, 630 x 500, in pixel art: a night sky of stars, a great moon, and the name, drawn on a grid of 315 x 250 (each pixel 2 x 2).
const W := 315
const H := 250
const K := 8                     # hi-res pixels per grid pixel when rasterising the font
const TITLE := "AIRUMA"
var vp: SubViewport
var frames := 0
var rng := RandomNumberGenerator.new()
var img: Image
var title_mask: Array = []       # [y][x] bool, in grid coordinates
var title_origin := Vector2i.ZERO
var title_w := 0
var title_h := 0
const BAYER := [[0, 8, 2, 10], [12, 4, 14, 6], [3, 11, 1, 9], [15, 7, 13, 5]]

func bayer(x: int, y: int) -> float:
	return (BAYER[y % 4][x % 4] + 0.5) / 16.0

func _initialize() -> void:
	rng.seed = 20261115
	# 1. the name, at high resolution, in a viewport
	title_w = 300
	title_h = 100
	vp = SubViewport.new()
	vp.size = Vector2i(title_w * K, title_h * K)
	vp.transparent_bg = false
	vp.render_target_update_mode = SubViewport.UPDATE_ALWAYS
	root.add_child(vp)
	var n := Node2D.new()
	vp.add_child(n)
	var bg := ColorRect.new()
	bg.color = Color.WHITE
	bg.size = Vector2(vp.size)
	n.add_child(bg)
	var f := FontFile.new()
	f.load_dynamic_font("/System/Library/Fonts/Supplemental/Bodoni 72.ttc")
	var fv := FontVariation.new()
	fv.base_font = f
	fv.variation_embolden = 0.9
	fv.variation_transform = Transform2D(Vector2(0.52, 0), Vector2(0, 1.18), Vector2.ZERO)
	var size := 640
	var widths: Array = []
	var total := 0.0
	var gap := 62.0
	for ch in TITLE:
		var w := fv.get_string_size(ch, HORIZONTAL_ALIGNMENT_LEFT, -1, size).x
		w *= 0.52
		widths.append(w)
		total += w
	total += gap * (TITLE.length() - 1)
	var x := (vp.size.x - total) / 2.0
	for i in TITLE.length():
		var l := Label.new()
		l.text = TITLE[i]
		l.add_theme_font_override("font", fv)
		l.add_theme_font_size_override("font_size", size)
		l.add_theme_color_override("font_color", Color.BLACK)
		l.position = Vector2(x, 110.0)
		n.add_child(l)
		x += widths[i] + gap

func _process(_d: float) -> bool:
	frames += 1
	if frames == 6:
		build_mask(vp.get_texture().get_image())
		compose()
		quit()
	return false

func build_mask(hi: Image) -> void:
	# a grid pixel is title when more than half of its hi-res block is dark
	for y in title_h:
		var row := []
		for x in title_w:
			var dark := 0
			for yy in K:
				for xx in K:
					if hi.get_pixel(x * K + xx, y * K + yy).r < 0.5: dark += 1
			row.append(dark > K * K / 2)
		title_mask.append(row)
	# the tilde of the last letter, drawn by hand: a wave, three pixels thick, above it
	var last_right := -1
	for x in range(title_w - 1, -1, -1):
		var any := false
		for y in title_h: if title_mask[y][x]: any = true
		if any:
			last_right = x
			break
	var last_left := last_right
	for x in range(last_right, -1, -1):
		var any2 := false
		for y in title_h: if title_mask[y][x]: any2 = true
		if not any2: break
		last_left = x
	var top_y := title_h
	for x in range(last_left, last_right + 1):
		for y in title_h:
			if title_mask[y][x]:
				top_y = mini(top_y, y)
				break
	var cx := (last_left + last_right) / 2 + 1
	for dx in range(-12, 13):
		var wy := int(round(-3.2 * sin(float(dx) * PI / 12.0))) + top_y - 8
		for t in 5:
			var yy := wy + t
			if yy >= 0 and yy < title_h and cx + dx < title_w: title_mask[yy][cx + dx] = true
	# the bounding box, to centre it
	var minx := 9999; var maxx := -1; var miny := 9999; var maxy := -1
	for y in title_h:
		for x in title_w:
			if title_mask[y][x]:
				minx = mini(minx, x); maxx = maxi(maxx, x); miny = mini(miny, y); maxy = maxi(maxy, y)
	print("title box ", minx, " ", miny, " ", maxx, " ", maxy)
	title_origin = Vector2i((W - (maxx - minx + 1)) / 2 - minx, 128 - (miny + maxy) / 2)

func mask_at(x: int, y: int) -> bool:
	var tx := x - title_origin.x
	var ty := y - title_origin.y
	return tx >= 0 and ty >= 0 and tx < title_w and ty < title_h and title_mask[ty][tx]

func hex(s: String) -> Color:
	return Color.html(s)

func compose() -> void:
	img = Image.create(W, H, false, Image.FORMAT_RGBA8)
	var moon := Vector2(157, 126)
	var radius := 86.0
	sky(moon, radius)
	stars(moon, radius)
	draw_moon(moon, radius)
	draw_title()
	var big := img.duplicate()
	big.resize(W * 2, H * 2, Image.INTERPOLATE_NEAREST)
	big.save_png("/tmp/cover/cover.png")
	img.save_png("/tmp/cover/cover_1x.png")
	print("saved ", big.get_size())

func sky(moon: Vector2, radius: float) -> void:
	var ramp := ["#050814", "#080F26", "#0C1838", "#11264F", "#173761", "#1F4D70", "#296578", "#347D80"]
	for y in H:
		for x in W:
			var t: float = pow(float(y) / (H - 1), 1.1) * (ramp.size() - 1)
			var base := int(floor(t + bayer(x, y) - 0.5))
			var c := hex(ramp[clampi(base, 0, ramp.size() - 1)])
			# the glow of the moon over the sky, in dithered steps
			var d := Vector2(x, y).distance_to(moon)
			var g := clampf(1.0 - (d - radius) / 95.0, 0.0, 1.0)
			g = g * g
			var step: float = floor(g * 5.0 + bayer(x + 1, y + 2) - 0.5) / 5.0
			if step > 0.0: c = c.lerp(hex("#5C93B8"), step * 0.62)
			img.set_pixel(x, y, c)

func stars(moon: Vector2, radius: float) -> void:
	var colors := ["#FFFFFF", "#DCE8FF", "#B7CFFF", "#FFEFC2"]
	for i in 230:
		var x := rng.randi_range(1, W - 2)
		# more of them high up, where the sky is darker
		var y := int(pow(rng.randf(), 1.45) * (H - 4)) + 1
		if Vector2(x, y).distance_to(moon) < radius + 3.0: continue
		var c := hex(colors[rng.randi() % colors.size()])
		var bright := rng.randf()
		c = c.darkened(0.55 * (1.0 - bright))
		img.set_pixel(x, y, c)
	# a few great stars, as little crosses
	for i in 14:
		var x := rng.randi_range(8, W - 9)
		var y := rng.randi_range(6, 170)
		if Vector2(x, y).distance_to(moon) < radius + 8.0: continue
		var core := hex("#FFFFFF")
		var arm := hex("#9DB9E8")
		img.set_pixel(x, y, core)
		for d in [Vector2i(1, 0), Vector2i(-1, 0), Vector2i(0, 1), Vector2i(0, -1)]: img.set_pixel(x + d.x, y + d.y, arm)
		if i % 3 == 0:
			for d in [Vector2i(2, 0), Vector2i(-2, 0), Vector2i(0, 2), Vector2i(0, -2)]: img.set_pixel(x + d.x, y + d.y, arm.darkened(0.35))

func draw_moon(c: Vector2, r: float) -> void:
	var tones := ["#F7F1D9", "#E7DEBC", "#D1C7A2", "#B5AB88", "#948C6E"]
	var light := Vector2(-0.55, -0.62).normalized()
	# the craters: position (as a share of the radius), size
	var craters := [[-0.35, -0.38, 15], [0.30, -0.45, 10], [0.45, 0.05, 14], [-0.50, 0.15, 9], [-0.05, 0.40, 17], [0.12, -0.05, 7], [-0.22, -0.10, 5],
		[0.55, -0.35, 5], [-0.62, -0.30, 6], [0.30, 0.62, 8], [-0.45, 0.58, 6], [0.05, -0.68, 6], [0.68, 0.38, 6]]
	for y in H:
		for x in W:
			var p := Vector2(x + 0.5, y + 0.5)
			var d := p.distance_to(c)
			if d > r: continue
			var n := (p - c) / r
			var z := sqrt(maxf(0.0, 1.0 - n.length_squared()))
			var lit: float = clampf(n.dot(light) * 0.55 + z * 0.65, 0.0, 1.0)
			var level := (1.0 - lit) * 3.0 + 0.1
			var tone := int(floor(level + bayer(x, y) - 0.5))
			# craters: one tone darker inside, with a lit lip on the lower right
			for cr in craters:
				var cc: Vector2 = c + Vector2(cr[0], cr[1]) * r
				var cd := p.distance_to(cc)
				var cs: float = cr[2] * 0.62
				if cd < cs:
					var inside := (p - cc).dot(light)
					tone += 1 if inside > -cs * 0.35 else 0
					tone += 1
				elif cd < cs + 1.3 and (p - cc).dot(light) < 0.0:
					tone -= 1
			# the rim of the disc is a little darker
			if d > r - 1.6: tone += 1
			img.set_pixel(x, y, hex(tones[clampi(tone, 0, tones.size() - 1)]))

func draw_title() -> void:
	var shadow := hex("#03060C")
	# a shadow under the name, so it stands off the moon
	for y in H:
		for x in W:
			if mask_at(x - 3, y - 4) and not mask_at(x, y):
				img.set_pixel(x, y, img.get_pixel(x, y).lerp(shadow, 0.78))
	var gold_top := hex("#F1DC9A")
	var gold_bottom := hex("#8F6B3A")
	var inner_line := hex("#0B2A2E")
	for y in H:
		var run_start := -1
		for x in W:
			if not mask_at(x, y):
				run_start = -1
				continue
			if run_start < 0: run_start = x
			var edge := not (mask_at(x - 1, y) and mask_at(x + 1, y) and mask_at(x, y - 1) and mask_at(x, y + 1))
			# how deep inside the stroke the pixel is: 0 for the edge
			var depth := 0
			if not edge:
				depth = 1
				if mask_at(x - 2, y) and mask_at(x + 2, y) and mask_at(x, y - 2) and mask_at(x, y + 2): depth = 2
			var t := clampf((float(y) - (title_origin.y + 4)) / 56.0, 0.0, 1.0)
			var col: Color
			if edge:
				col = gold_top.lerp(gold_bottom, t)
				# the lit edge at the top left, the shaded one at the bottom right
				if not mask_at(x + 1, y + 1): col = col.darkened(0.35)
				elif not mask_at(x - 1, y - 1): col = col.lightened(0.18)
			elif depth == 1:
				col = inner_line
			else:
				# the verdigris of the letters: lighter at the top, darker at the bottom, with a lit side at the left of each stroke
				var top := hex("#4FA391")
				var bottom := hex("#17504F")
				col = top.lerp(bottom, floorf(t * 5.0 + bayer(x, y) - 0.5) / 5.0)
				var left_gap := 0
				while mask_at(x - left_gap - 1, y) and left_gap < 6: left_gap += 1
				var right_gap := 0
				while mask_at(x + right_gap + 1, y) and right_gap < 6: right_gap += 1
				if left_gap <= 2: col = col.lightened(0.22)
				elif right_gap <= 2: col = col.darkened(0.28)
				# the worn, mottled surface
				var noise := rng.randf()
				if noise > 0.955: col = col.lightened(0.18)
				elif noise < 0.04: col = col.darkened(0.2)
			img.set_pixel(x, y, col)
