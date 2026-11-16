extends SceneTree
# The first card of the video: black, a small waning crescent moon at the left, and "Aiezza Studios".
const W := 480
const H := 270
const K := 8
const FPS := 60
var vp: SubViewport
var frames := 0
func _initialize() -> void:
	vp = SubViewport.new()
	vp.size = Vector2i(200 * K, 40 * K)
	vp.render_target_update_mode = SubViewport.UPDATE_ALWAYS
	root.add_child(vp)
	var n := Node2D.new()
	vp.add_child(n)
	var bg := ColorRect.new(); bg.color = Color.WHITE; bg.size = Vector2(vp.size); n.add_child(bg)
	var f := FontFile.new()
	f.load_dynamic_font("/System/Library/Fonts/Supplemental/Bodoni 72.ttc")
	var fv := FontVariation.new()
	fv.base_font = f
	fv.variation_embolden = 0.4
	var l := Label.new()
	l.text = "Aiezza Studios"
	l.add_theme_font_override("font", fv)
	l.add_theme_font_size_override("font_size", 190)
	l.add_theme_color_override("font_color", Color.BLACK)
	l.position = Vector2(20, -10)
	n.add_child(l)
func _process(_d: float) -> bool:
	frames += 1
	if frames == 6:
		build(vp.get_texture().get_image())
		quit()
	return false
func build(hi: Image) -> void:
	var gw := 200
	var gh := 40
	var mask: Array = []
	var minx := 9999; var maxx := -1; var miny := 9999; var maxy := -1
	for y in gh:
		var row := []
		for x in gw:
			var dark := 0
			for yy in K:
				for xx in K:
					if hi.get_pixel(x * K + xx, y * K + yy).r < 0.5: dark += 1
			var on: bool = dark > K * K / 2
			row.append(on)
			if on:
				minx = mini(minx, x); maxx = maxi(maxx, x); miny = mini(miny, y); maxy = maxi(maxy, y)
		mask.append(row)
	var tw := maxx - minx + 1
	var th := maxy - miny + 1
	var moon := 22
	var gap := 12
	var total := moon + gap + tw
	var x0 := (W - total) / 2
	var cy := H / 2
	var layer := Image.create(W, H, false, Image.FORMAT_RGBA8)
	# the text
	var tx := x0 + moon + gap
	var ty := cy - th / 2 - 1
	for y in th:
		for x in tw:
			if mask[miny + y][minx + x]:
				var shade := 0.82 + 0.18 * (1.0 - float(y) / th)
				layer.set_pixel(tx + x, ty + y, Color(0.93 * shade, 0.92 * shade, 0.88 * shade, 1.0))
	# the crescent: the lit rim at the left (a waning moon), a disc less a disc
	var c := Vector2(x0 + moon / 2.0, cy)
	for y in H:
		for x in W:
			var p := Vector2(x + 0.5, y + 0.5)
			if p.distance_to(c) <= moon / 2.0 and p.distance_to(c + Vector2(5.5, -1.5)) > moon / 2.0 - 1.2:
				var lit := clampf((p.x - (c.x - moon / 2.0)) / 9.0, 0.0, 1.0)
				layer.set_pixel(x, y, Color(0.96, 0.92, 0.74).lerp(Color(0.78, 0.72, 0.52), lit))
	var total_frames := 4 * FPS
	for fr in total_frames:
		var t := float(fr) / FPS
		var a := smooth(t, 0.4, 1.4) * (1.0 - smooth(t, 3.0, 3.9))
		var frame := Image.create(W, H, false, Image.FORMAT_RGBA8)
		frame.fill(Color.BLACK)
		for y in H:
			for x in W:
				var px := layer.get_pixel(x, y)
				if px.a > 0.0: frame.set_pixel(x, y, Color.BLACK.lerp(Color(px.r, px.g, px.b, 1.0), a))
		frame.resize(W * 4, H * 4, Image.INTERPOLATE_NEAREST)
		frame.save_png("/tmp/cover/studio/s%04d.png" % fr)
	print("studio ", tw, "x", th, " frames ", total_frames)
func smooth(t: float, a: float, b: float) -> float:
	var u := clampf((t - a) / (b - a), 0.0, 1.0)
	return u * u * (3.0 - 2.0 * u)
