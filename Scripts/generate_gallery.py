import os, json, re, shutil
from pathlib import Path
from PIL import Image

ROOT = Path(__file__).resolve().parents[1]  # repo root
SRC  = ROOT / "gallery-src"
OUT  = ROOT / "docs" / "gallery"
THUMBS = OUT / "thumbs"
PAPERPRO = OUT / "paperpro"
RM2 = OUT / "rm2"

SITE_BASE = f"https://{os.getenv('GITHUB_REPOSITORY','user/repo').replace('/', '.github.io/', 1)}"
# ^ remplacé par GitHub Pages (on écrase plus bas par une méthode robuste)
# Mieux: calcule depuis remote, mais ici on mettra via env var si besoin.

# Si tu veux forcer l’URL propre:
USER_REPO = os.getenv("GALLERY_BASE", "").strip()  # optionnel: "roropastis/ReMarkable-Sleep-Screen-Manager"
if USER_REPO:
    SITE_BASE = f"https://{USER_REPO.split('/')[0]}.github.io/{USER_REPO.split('/')[1]}"

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

def process_device(device: str, src_dir: Path, out_dir: Path, items: list):
    if not src_dir.exists():
        return
    target_w, target_h = TARGETS[device]
    for src_path in sorted(src_dir.glob("*.png")):
        slug = kebab(src_path.name)
        dst_png = out_dir / f"{slug}.png"
        thumb_jpg = THUMBS / f"{slug}.jpg"

        # Ouvre et redimensionne au canvas cible (fond blanc)
        with Image.open(src_path) as im:
            im = im.convert("RGB")  # E-ink, PNG final, mais convert pour ops
            # Resize letterbox vers cible
            ratio = min(target_w / im.width, target_h / im.height)
            new_w, new_h = int(im.width * ratio), int(im.height * ratio)
            canvas = Image.new("RGB", (target_w, target_h), (255, 255, 255))
            im_resized = im.resize((new_w, new_h), Image.LANCZOS)
            off_x = (target_w - new_w) // 2
            off_y = (target_h - new_h) // 2
            canvas.paste(im_resized, (off_x, off_y))

            # Sauve PNG final
            canvas.save(dst_png, "PNG", optimize=True)

            # Crée vignette 600 px de large (conserve ratio)
            thumb_w = 600
            thumb_ratio = thumb_w / canvas.width
            thumb_size = (thumb_w, max(1, int(canvas.height * thumb_ratio)))
            thumb = canvas.resize(thumb_size, Image.LANCZOS)
            thumb.save(thumb_jpg, "JPEG", quality=82, optimize=True, progressive=True)

        # Ajout au catalogue
        items.append({
            "id": slug,
            "title": slug.replace('-', ' ').title(),
            "author": "",  # à remplir si tu veux
            "license": "CC0",  # par défaut
            "device": device,
            "resolution": f"{target_w}x{target_h}",
            "preview_url": f"{SITE_BASE}/gallery/thumbs/{thumb_jpg.name}",
            "download_url": f"{SITE_BASE}/gallery/{device}/{dst_png.name}",
            "tags": []
        })

def main():
    ensure_dirs()

    items = []
    process_device("paperpro", SRC / "paperpro", PAPERPRO, items)
    process_device("rm2", SRC / "rm2", RM2, items)

    # Écrit index.json
    index_path = OUT / "index.json"
    index_obj = {"updated": __import__("datetime").datetime.utcnow().strftime("%Y-%m-%d"), "items": items}
    with open(index_path, "w", encoding="utf-8") as f:
        json.dump(index_obj, f, ensure_ascii=False, indent=2)

    print(f"Generated {index_path} with {len(items)} items")

if __name__ == "__main__":
    main()
