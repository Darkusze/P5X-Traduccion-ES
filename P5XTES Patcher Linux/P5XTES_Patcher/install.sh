#!/bin/bash

# ─────────────────────────────────────────
#  P5XTES Patcher - Script de instalación
# ─────────────────────────────────────────

INSTALL_DIR="/opt/P5XTES_Patcher"
DESKTOP_DIR="/usr/share/applications"
ICON_BASE="/usr/share/icons/hicolor"
APP_NAME="P5XTESPatcher"

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
export SCRIPT_DIR # Necesario para que Python lo detecte

echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "  Instalando P5XTES Patcher..."
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"

if [ "$EUID" -ne 0 ]; then
  echo "❌ Este script necesita permisos de administrador."
  echo "   Ejecuta: sudo ./install.sh"
  exit 1
fi

# ── Dependencia: python3 + pillow para redimensionar el ícono ──
echo "🔍 Verificando dependencias..."
if ! python3 -c "from PIL import Image" 2>/dev/null; then
  echo "   Instalando python3-pillow..."
  apt-get install -y python3-pillow 2>/dev/null || \
  pip3 install pillow --break-system-packages -q 2>/dev/null || \
  dnf install -y python3-pillow 2>/dev/null || true
fi

# ── Copiar archivos del programa ──
echo "📁 Creando directorio $INSTALL_DIR..."
mkdir -p "$INSTALL_DIR"

echo "📋 Copiando archivos..."
cp "$SCRIPT_DIR/$APP_NAME"           "$INSTALL_DIR/" 2>/dev/null || cp "$SCRIPT_DIR/P5XTES Patcher" "$INSTALL_DIR/$APP_NAME"
cp "$SCRIPT_DIR/libHarfBuzzSharp.so" "$INSTALL_DIR/"
cp "$SCRIPT_DIR/libSkiaSharp.so"     "$INSTALL_DIR/"
cp "$SCRIPT_DIR/P5XTES_Patcher.png"  "$INSTALL_DIR/"
chmod +x "$INSTALL_DIR/$APP_NAME"

# ── Instalar ícono en todos los tamaños estándar (hicolor theme) ──
echo "🖼️  Instalando íconos en tamaños estándar..."
python3 - << 'PYEOF'
import sys
import os
import shutil

# Variables globales para asegurar disponibilidad en el bloque 'except'
script_dir = os.environ.get("SCRIPT_DIR", ".")
src = os.path.join(script_dir, "P5XTES_Patcher.png")

try:
    from PIL import Image

    img = Image.open(src).convert("RGBA")

    sizes = [16, 32, 48, 64, 128, 256]
    for s in sizes:
        dest_dir = f"/usr/share/icons/hicolor/{s}x{s}/apps"
        os.makedirs(dest_dir, exist_ok=True)
        img.resize((s, s), Image.Resampling.LANCZOS).save(f"{dest_dir}/P5XTESPatcher.png", "PNG")
        print(f"   ✓ {s}x{s}")

    # También en pixmaps como respaldo
    os.makedirs("/usr/share/pixmaps", exist_ok=True)
    img.resize((48, 48), Image.Resampling.LANCZOS).save("/usr/share/pixmaps/P5XTESPatcher.png", "PNG")
    print("   ✓ pixmaps/48x48")

except Exception as e:
    print(f"   ⚠ No se pudieron redimensionar los tamaños (Falta Pillow): {e}", file=sys.stderr)
    print("   📦 Aplicando respaldo: Copiando ícono en tamaño original...")

    # Respaldo absoluto: Coloca el archivo original en los directorios clave del sistema
    os.makedirs("/usr/share/pixmaps", exist_ok=True)
    shutil.copy(src, "/usr/share/pixmaps/P5XTESPatcher.png")

    os.makedirs("/usr/share/icons/hicolor/256x256/apps", exist_ok=True)
    shutil.copy(src, "/usr/share/icons/hicolor/256x256/apps/P5XTESPatcher.png")
    print("   ✓ Ícono de respaldo instalado correctamente.")
PYEOF

# ── Instalar .desktop ──
echo "🔗 Instalando acceso directo..."

# Estructura estricta para capturar el .desktop sin importar cómo esté nombrado en el origen
if [ -f "$SCRIPT_DIR/$APP_NAME.desktop" ]; then
    cp "$SCRIPT_DIR/$APP_NAME.desktop" "$DESKTOP_DIR/$APP_NAME.desktop"
elif [ -f "$SCRIPT_DIR/P5XTES Patcher.desktop" ]; then
    cp "$SCRIPT_DIR/P5XTES Patcher.desktop" "$DESKTOP_DIR/$APP_NAME.desktop"
elif [ -f "$SCRIPT_DIR/P5XTES Patcher" ]; then
    cp "$SCRIPT_DIR/P5XTES Patcher" "$DESKTOP_DIR/$APP_NAME.desktop"
else
    echo "❌ Error crítico: No se encontró ningún archivo .desktop en la carpeta de origen."
fi

# Permisos de lectura estándar para accesos directos globales
chmod 644 "$DESKTOP_DIR/$APP_NAME.desktop"

# ── Actualizar caché de íconos y aplicaciones ──
echo "🔄 Actualizando caché del sistema..."
update-desktop-database "$DESKTOP_DIR" 2>/dev/null || true
gtk-update-icon-cache -f -t "$ICON_BASE" 2>/dev/null || true
xdg-icon-resource forceupdate 2>/dev/null || true

echo ""
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "  ✅ ¡Instalación completada!"
echo "     El programa aparece en el menú de aplicaciones."
echo "     También puedes ejecutarlo desde terminal con:"
echo "     $INSTALL_DIR/$APP_NAME"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
