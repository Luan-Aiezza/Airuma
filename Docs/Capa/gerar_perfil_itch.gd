extends SceneTree
# The profile of the studio, in the colours of the last stage of the game (the river of stars): banner, background tile and icon.
var W := 480
var H := 125
var mode := "banner"
var img: Image
var rng := RandomNumberGenerator.new()
var vp: SubViewport
var frames := 0
var mask: Array = []
var mw := 0
var mh := 0
const BAYER := [[0, 8, 2, 10], [12, 4, 14, 6], [3, 11, 1, 9], [15, 7, 13, 5]]
const MOON := ["#F6F2FF", "#DCD6F2", "#B9B1DC", "#9087BB", "#6C6498"]

func bayer(x: int, y: int) -> float: return (BAYER[y % 4][x % 4] + 0.5) / 16.0
func hex(s: String) -> Color: return Color.html(s)

func _initialize() -> void:
	for a in OS.get_cmdline_user_args():
		if a.begins_with("--mode="): mode = a.substr(7)
	rng.seed = 6
	if mode == "icon": W = 128; H = 128
	elif mode == "tile": W = 128; H = 128
	if mode != "banner":
		call_deferred("finish")
		return
	# the name, rendered large and read back as pixels
	vp = SubViewport.new()
	vp.size = Vector2i(220 * 8, 40 * 8)
	vp.render_target_update_mode = SubViewport.UPDATE_ALWAYS
	root.add_child(vp)
	var n := Node2D.new()
	vp.add_child(n)
	var bg := ColorRect.new(); bg.color = Color.WHITE; bg.size = Vector2(vp.size); n.add_child(bg)
	var f := FontFile.new()
	f.load_dynamic_font("/System/Library/Fonts/Supplemental/Bodoni 72.ttc")
	var fv := FontVariation.new()
	fv.base_font = f
	fv.variation_embolden = 0.5
	var l := Label.new()
	l.text = "Aiezza Studios"
	l.add_theme_font_override("font", fv)
	l.add_theme_font_size_override("font_size", 250)
	l.add_theme_color_override("font_color", Color.BLACK)
	l.position = Vector2(20, -20)
	n.add_child(l)

func _process(_d: float) -> bool:
	frames += 1
	if mode == "banner" and frames == 6:
		read_mask(vp.get_texture().get_image())
		finish()
	return false

func read_mask(hi: Image) -> void:
	var minx := 9999; var maxx := -1; var miny := 9999; var maxy := -1
	var rows: Array = []
	for y in 40:
		var row := []
		for x in 220:
			var dark := 0
			for yy in 8:
				for xx in 8:
					if hi.get_pixel(x * 8 + xx, y * 8 + yy).r < 0.5: dark += 1
			var on: bool = dark > 32
			row.append(on)
			if on:
				minx = mini(minx, x); maxx = maxi(maxx, x); miny = mini(miny, y); maxy = maxi(maxy, y)
		rows.append(row)
	mw = maxx - minx + 1
	mh = maxy - miny + 1
	for y in mh:
		var r := []
		for x in mw: r.append(rows[miny + y][minx + x])
		mask.append(r)

func finish() -> void:
	img = Image.create(W, H, false, Image.FORMAT_RGBA8)
	if mode == "tile": tile()
	elif mode == "icon": icon()
	else: banner()
	var out := img.duplicate()
	out.resize(W * 4, H * 4, Image.INTERPOLATE_NEAREST)
	out.save_png("/tmp/cover/profile_%s.png" % mode)
	print("saved ", mode, out.get_size())
	quit()

# ---- the sky of the last stage: deep indigo, with the Milky Way across it
func noise(x: float, y: float) -> float:
	return 0.5 + 0.25 * sin(x * 0.11 + 1.3) * cos(y * 0.17) + 0.15 * sin(x * 0.29 + y * 0.23) + 0.1 * sin(x * 0.57 - y * 0.41 + 2.0)

func sky(angle: float, width: float, strength: float) -> void:
	var ramp := ["#04051A", "#070925", "#0B0E33", "#11144A", "#181A5C", "#20206E"]
	var dir := Vector2(cos(angle), sin(angle))
	var perp := Vector2(-dir.y, dir.x)
	var centre := Vector2(W * 0.5, H * 0.5)
	for y in H:
		for x in W:
			var t := float(y) / (H - 1) * 0.75 + 0.1 * noise(x, y)
			var level := int(floor(t * (ramp.size() - 1) + bayer(x, y) - 0.5))
			var c := hex(ramp[clampi(level, 0, ramp.size() - 1)])
			var d := (Vector2(x, y) - centre).dot(perp)
			var along := (Vector2(x, y) - centre).dot(dir)
			var band := exp(-d * d / (width * width)) * strength * (0.55 + 0.9 * noise(x * 0.8 + 40.0, y * 0.8))
			var step: float = floor(band * 6.0 + bayer(x + 2, y + 1) - 0.5) / 6.0
			if step > 0.0:
				var col := hex("#5A47B8").lerp(hex("#C7B6F2"), clampf(step * 1.4, 0, 1))
				if noise(x * 1.7, y * 1.3 + along * 0.05) > 0.78: col = hex("#B782D6")
				c = c.lerp(col, clampf(step * 0.9, 0, 0.85))
			img.set_pixel(x, y, c)

func stars(count: int, keep_out: Rect2) -> void:
	var cols := ["#FFFFFF", "#DCE6FF", "#B7CCFF", "#E7D8FF", "#9CCBFF"]
	for i in count:
		var x := rng.randi_range(1, W - 2)
		var y := rng.randi_range(1, H - 2)
		if keep_out.has_point(Vector2(x, y)): continue
		img.set_pixel(x, y, hex(cols[rng.randi() % cols.size()]).darkened(0.5 * (1.0 - rng.randf())))
	for i in maxi(2, count / 16):
		var x := rng.randi_range(5, W - 6)
		var y := rng.randi_range(5, H - 6)
		if keep_out.has_point(Vector2(x, y)): continue
		img.set_pixel(x, y, hex("#FFFFFF"))
		for d in [Vector2i(1, 0), Vector2i(-1, 0), Vector2i(0, 1), Vector2i(0, -1)]: img.set_pixel(x + d.x, y + d.y, hex("#A9BDF2"))
		if i % 3 == 0:
			for d in [Vector2i(2, 0), Vector2i(-2, 0), Vector2i(0, 2), Vector2i(0, -2)]: img.set_pixel(x + d.x, y + d.y, hex("#6F86C8"))

func planet(c: Vector2, r: float, base: String, ring := false) -> void:
	var light := Vector2(-0.6, -0.7).normalized()
	for y in range(int(c.y - r * 2), int(c.y + r * 2) + 1):
		for x in range(int(c.x - r * 2), int(c.x + r * 2) + 1):
			if x < 0 or y < 0 or x >= W or y >= H: continue
			var p := Vector2(x + 0.5, y + 0.5)
			var n := (p - c) / r
			if ring and absf(n.y * 3.0 + n.x * 0.6) < 0.5 and n.length() < 1.9 and (n.length() > 1.25 or n.y * 1.0 > 0.0):
				if n.length() > 1.25: img.set_pixel(x, y, hex("#C9B58A").darkened(0.15 + 0.3 * bayer(x, y))); continue
			if n.length() > 1.0: continue
			var z := sqrt(maxf(0.0, 1.0 - n.length_squared()))
			var lit := clampf(n.dot(light) * 0.6 + z * 0.55, 0.0, 1.0)
			var level := int(floor((1.0 - lit) * 3.0 + bayer(x, y) - 0.4))
			img.set_pixel(x, y, hex(base).darkened(clampf(level * 0.22, 0.0, 0.7)).lightened(0.12 if level <= 0 else 0.0))

func glow(c: Vector2, r: float, reach: float) -> void:
	for y in H:
		for x in W:
			var d := Vector2(x, y).distance_to(c)
			var g := clampf(1.0 - (d - r) / reach, 0.0, 1.0)
			g = g * g
			var step: float = floor(g * 5.0 + bayer(x + 1, y + 2) - 0.5) / 5.0
			if step > 0.0: img.set_pixel(x, y, img.get_pixel(x, y).lerp(hex("#8F83E0"), step * 0.5))

func moon_disc(c: Vector2, r: float, crescent_shift := Vector2.ZERO) -> void:
	var light := Vector2(-0.55, -0.62).normalized()
	var craters := [[-0.35, -0.38, 14], [0.30, -0.45, 9], [0.45, 0.05, 13], [-0.5, 0.15, 8], [-0.05, 0.4, 16], [0.12, -0.05, 6], [-0.22, -0.1, 5], [0.55, -0.35, 5], [-0.62, -0.3, 6], [0.3, 0.62, 7], [-0.45, 0.58, 6]]
	for y in H:
		for x in W:
			var p := Vector2(x + 0.5, y + 0.5)
			var d := p.distance_to(c)
			if d > r: continue
			if crescent_shift != Vector2.ZERO and p.distance_to(c + crescent_shift) < r * 0.93: continue
			var n := (p - c) / r
			var z := sqrt(maxf(0.0, 1.0 - n.length_squared()))
			var lit := clampf(n.dot(light) * 0.5 + z * 0.7, 0.0, 1.0)
			var tone := int(floor((1.0 - lit) * 3.0 + 0.1 + bayer(x, y) - 0.5))
			for cr in craters:
				var cc: Vector2 = c + Vector2(cr[0], cr[1]) * r
				var cd := p.distance_to(cc)
				var cs: float = cr[2] * r / 86.0
				if cd < cs: tone += 2 if (p - cc).dot(light) > -cs * 0.35 else 1
				elif cd < cs + 1.2 and (p - cc).dot(light) < 0.0: tone -= 1
			if d > r - 1.5: tone += 1
			img.set_pixel(x, y, hex(MOON[clampi(tone, 0, MOON.size() - 1)]))

# ---- the three pieces
func tile() -> void:
	for y in H:
		for x in W: img.set_pixel(x, y, hex("#070920"))
	# faint, soft dithered clouds of colour, away from the edges so that the tile repeats without a seam
	for y in range(12, H - 12):
		for x in range(12, W - 12):
			var fade := minf(minf(x - 12, W - 13 - x), minf(y - 12, H - 13 - y)) / 20.0
			var v := (noise(x * 1.3, y * 1.1) - 0.55) * clampf(fade, 0, 1)
			var step: float = floor(v * 6.0 + bayer(x, y) - 0.5) / 6.0
			if step > 0.0: img.set_pixel(x, y, hex("#070920").lerp(hex("#2A2470"), step * 1.3))
	stars(40, Rect2(-1, -1, 0, 0))

func icon() -> void:
	sky(-0.7, 22.0, 0.8)
	var c := Vector2(64, 64)
	stars(60, Rect2(14, 14, 100, 100))
	planet(Vector2(104, 100), 6.0, "#6E5BB8")
	planet(Vector2(20, 22), 3.5, "#3F7FC8")
	glow(c, 40.0, 38.0)
	moon_disc(c, 42.0, Vector2(15, -5))
	# a few stars in front of the dark side of the moon
	for p in [Vector2(70, 44), Vector2(82, 70), Vector2(60, 90)]:
		img.set_pixel(int(p.x), int(p.y), hex("#FFFFFF"))
	# the frame of the icon is the screen: nothing more

func banner() -> void:
	sky(-0.45, 20.0, 0.9)
	var moon := Vector2(392, 68)
	stars(150, Rect2(moon.x - 56, moon.y - 56, 112, 112))
	planet(Vector2(196, 24), 7.0, "#3F7FC8")
	planet(Vector2(444, 104), 9.0, "#8C6E48")
	planet(Vector2(34, 100), 4.5, "#B0507A")
	glow(moon, 46.0, 60.0)
	moon_disc(moon, 46.0)
	# the studio: a waning crescent, and the name beside it
	var base_x := 40
	var cy := 64
	var tx := base_x + 34
	var ty := cy - mh / 2
	# a soft shadow, then the letters
	for y in mh:
		for x in mw:
			if mask[y][x] and tx + x + 2 < W and ty + y + 2 < H:
				img.set_pixel(tx + x + 2, ty + y + 2, img.get_pixel(tx + x + 2, ty + y + 2).lerp(hex("#04051A"), 0.8))
	for y in mh:
		for x in mw:
			if mask[y][x]:
				var shade := 0.84 + 0.16 * (1.0 - float(y) / mh)
				img.set_pixel(tx + x, ty + y, Color(0.93 * shade, 0.91 * shade, 1.0 * shade))
	var c := Vector2(base_x + 14, cy)
	for y in H:
		for x in W:
			var p := Vector2(x + 0.5, y + 0.5)
			if p.distance_to(c) <= 13.0 and p.distance_to(c + Vector2(8.0, -2.0)) > 11.5:
				var lit := clampf((p.x - (c.x - 13.0)) / 12.0, 0.0, 1.0)
				img.set_pixel(x, y, hex("#F6F2FF").lerp(hex("#9087BB"), lit))
