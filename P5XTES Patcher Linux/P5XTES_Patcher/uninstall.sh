#!/bin/bash

# ─────────────────────────────────────────
#  P5XTES Patcher - Script de desinstalación
# ─────────────────────────────────────────

INSTALL_DIR="/opt/P5XTES_Patcher"
DESKTOP_FILE="/usr/share/applications/P5XTESPatcher.desktop"
ICON_BASE="/usr/share/icons/hicolor"
PIXMAP="/usr/share/pixmaps/P5XTESPatcher.png"

echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "  Desinstalando P5XTES Patcher..."
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"

if [ "$EUID" -ne 0 ]; then
  echo "❌ Este script necesita permisos de administrador."
  echo "   Ejecuta: sudo ./uninstall.sh"
  exit 1
fi

echo "🗑️  Eliminando archivos del programa..."
rm -rf "$INSTALL_DIR"
rm -f  "$DESKTOP_FILE"
rm -f  "$PIXMAP"

echo "🗑️  Eliminando íconos del sistema..."
for s in 16 32 48 64 128 256; do
  rm -f "$ICON_BASE/${s}x${s}/apps/P5XTESPatcher.png"
done

echo "🔄 Actualizando caché del sistema..."
update-desktop-database /usr/share/applications 2>/dev/null || true
gtk-update-icon-cache -f -t "$ICON_BASE" 2>/dev/null || true

echo ""
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "  ✅ P5XTES Patcher desinstalado."
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
