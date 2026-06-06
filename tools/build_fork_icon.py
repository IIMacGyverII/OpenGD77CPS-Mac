"""Generate DMR_Fork.ico — stock OpenGD77 icon + orange PriInterPhone P badge."""
from PIL import Image, ImageDraw, ImageFont
import os
import sys

root = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
base_path = os.path.join(root, "DMR_32512.ico")
out_path = os.path.join(root, "DMR_Fork.ico")

base = Image.open(base_path).convert("RGBA")
sizes = [256, 48, 32, 16]
frames = []
font_path = os.path.join(os.environ.get("WINDIR", r"C:\Windows"), "Fonts", "arialbd.ttf")

for size in sizes:
    img = base.resize((size, size), Image.Resampling.LANCZOS)
    badge = max(size // 3, 7)
    x0 = size - badge - 2
    y0 = size - badge - 2
    draw = ImageDraw.Draw(img)
    draw.ellipse((x0, y0, size - 2, size - 2), fill=(230, 81, 0, 255), outline=(255, 224, 178, 255))
    fs = max(int(badge * 0.72), 6)
    try:
        font = ImageFont.truetype(font_path, fs)
    except OSError:
        font = ImageFont.load_default()
    bbox = draw.textbbox((0, 0), "P", font=font)
    tw = bbox[2] - bbox[0]
    th = bbox[3] - bbox[1]
    tx = x0 + (badge - tw) // 2
    ty = y0 + (badge - th) // 2 - 1
    draw.text((tx, ty), "P", fill=(255, 255, 255, 255), font=font)
    frames.append(img)

frames[0].save(out_path, format="ICO", sizes=[(s, s) for s in sizes])
print("Wrote", out_path)