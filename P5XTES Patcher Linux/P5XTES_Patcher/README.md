# P5XTES Patcher

Patcher para Persona 5 X. Compatible con distribuciones Linux de 64 bits.

---

## Contenido de la carpeta

| Archivo | Descripción |
|---|---|
| `P5XTESPatcher` | Ejecutable principal |
| `libHarfBuzzSharp.so` | Librería requerida |
| `libSkiaSharp.so` | Librería requerida |
| `P5XTES_Patcher.png` | Ícono de la aplicación |
| `P5XTESPatcher.desktop` | Acceso directo del menú |
| `install.sh` | Script de instalación |
| `uninstall.sh` | Script de desinstalación |

---

## Instalación

1. Abre una terminal en la carpeta `P5XTES_Patcher/`

2. Dale permisos de ejecución al script (solo la primera vez):
   ```bash
   chmod +x install.sh
   ```

3. Ejecuta el instalador con permisos de administrador:
   ```bash
   sudo ./install.sh
   ```

El programa quedará instalado en `/opt/P5XTES_Patcher/` y aparecerá con su ícono en el menú de aplicaciones de tu escritorio.

---

## Desinstalación

1. Abre una terminal en la carpeta `P5XTES_Patcher/`

2. Dale permisos de ejecución al script (solo la primera vez):
   ```bash
   chmod +x uninstall.sh
   ```

3. Ejecuta el desinstalador con permisos de administrador:
   ```bash
   sudo ./uninstall.sh
   ```

Esto eliminará todos los archivos instalados, incluyendo el ícono y el acceso directo del menú.

---

## Ejecución manual (sin instalar)

Si prefieres no instalar el programa en el sistema, puedes ejecutarlo directamente desde la carpeta:

```bash
chmod +x P5XTESPatcher
./P5XTESPatcher
```

> **Nota:** En este modo el programa no aparecerá en el menú de aplicaciones ni tendrá ícono en el explorador de archivos.

---

## Requisitos del sistema

- Linux 64 bits (x86_64)
- Entorno de escritorio con soporte `.desktop` (GNOME, KDE, XFCE, etc.)
- `sudo` / permisos de administrador para instalar
