import os, json, re
from pathlib import Path
from datetime import datetime
from PIL import Image

ROOT = Path(__file__).resolve().parents[1]   # repo root
SRC  = ROOT / "gallery-src"
OUT  = ROOT / "docs" / "gallery"
THUMBS   = OUT / "thumbs"
PAPERPRO = OUT / "paperpro"
RM2      = OUT / "rm2"

# Déduit automatiquement l'URL GitHub Pages depuis GITHUB_REPOSITORY
# Ex: roropastis/ReMarkable-Sleep-Screen-Manager -> https://roropastis.github.io/ReMarkable-Sleep-Screen-Manager
repo = os.getenv("GITHUB_REPOSITORY", "").strip()
if not repo or "/" not in repo:
    # fallback pour exécution locale
    repo = "user/repo"
user, name = repo.split("/", 1)
SITE_BASE = f"https://{user}.github.io/{name}"

TARGETS = {
    "paperpro": (2160, 1620),
    "rm2": (1404, 1872),
}

def kebab(name: str) -> str:
    base = re.sub(r'\.[^.]+$', '', name)
    base = re.sub(r'[_\s]+', '-', base)
    base = re.sub(r'[^a-zA-Z0-9\-]+', '', base)
    base = re.sub(r'-{2,}', '-', base).strip('-')
    return base.lower()

def ensure_dirs():
    for p in [OUT, THUMBS, PAPERPRO, RM2]:
        p.mkdir(parents=True, exist_ok=True)

def make_canvas_fit(im: Image.Image, tw: int, th: int) -> Image.Image:
    ratio = min(tw / im.width, th / im.height)
    nw = max(1, int(im.width * ratio))
    nh = max(1, int(im.height * ratio))
    canvas = Image.new("RGB", (tw, th), (255, 255, 255))
    im_resized = im.resize((nw, nh), Image.LANCZOS)
    ox = (tw - nw) // 2
    oy = (th - nh) // 2
    canvas.paste(im_resized, (ox, oy))
    return canvas

def process_device(device: str, src_dir: Path, out_dir: Path, items: list):
    if not src_dir.exists():
        return
    tw, th = TARGETS[device]
    for src_path in sorted(src_dir.glob("*.png")):
        slug = kebab(src_path.name)
        dst_png = out_dir / f"{slug}.png"
        dst_thumb = THUMBS / f"{slug}.jpg"

        with Image.open(src_path) as im:
            im = im.convert("RGB")
            canvas = make_canvas_fit(im, tw, th)
            canvas.save(dst_png, "PNG", optimize=True)

            # vignette 600 px de large
            thumb_w = 600
            tr = thumb_w / canvas.width
            thumb_h = max(1, int(canvas.height * tr))
            thumb = canvas.resize((thumb_w, thumb_h), Image.LANCZOS)
            thumb.save(dst_thumb, "JPEG", quality=82, optimize=True, progressive=True)

        items.append({
            "id": slug,
            "title": slug.replace("-", " ").title(),
            "author": "",
            "license": "CC0",
            "device": device,
            "resolution": f"{tw}x{th}",
            "preview_url": f"{SITE_BASE}/gallery/thumbs/{dst_thumb.name}",
            "download_url": f"{SITE_BASE}/gallery/{device}/{dst_png.name}",
            "tags": []
        })

def main():
    ensure_dirs()
    items = []
    process_device("paperpro", SRC / "paperpro", PAPERPRO, items)
    process_device("rm2", SRC / "rm2", RM2, items)

    index = {
        "updated": datetime.utcnow().strftime("%Y-%m-%d"),
        "items": items
    }
    OUT.mkdir(parents=True, exist_ok=True)
    with open(OUT / "index.json", "w", encoding="utf-8") as f:
        json.dump(index, f, ensure_ascii=False, indent=2)
    print(f"Generated docs/gallery/index.json with {len(items)} items")

if __name__ == "__main__":
    main()
