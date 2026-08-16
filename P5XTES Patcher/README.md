# P5XTES Patcher — WPF

## Estructura del proyecto

```
P5XTESPatcher/
├── App.xaml
├── App.xaml.cs
├── MainWindow.xaml          ← Interfaz visual (XAML)
├── MainWindow.xaml.cs       ← Lógica completa
└── P5XTESPatcher.csproj     ← Proyecto .NET 6 WPF
```

## Requisitos

- **Visual Studio 2022** (Community gratuito sirve) con la carga de trabajo:
  - "Desarrollo de escritorio de .NET"
- **.NET 6 SDK** (se instala junto a VS2022)

## Cómo compilar

### Opción A — Visual Studio
1. Abre `P5XTESPatcher.csproj` con Visual Studio 2022
2. `Ctrl+Shift+B` para compilar
3. El `.exe` aparece en `bin\Debug\net6.0-windows\`

### Opción B — Línea de comandos
```bash
dotnet build -c Release
```

## Publicar como EXE único (para distribuir)

```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

El `.exe` final aparece en:
`bin\Release\net6.0-windows\win-x64\publish\P5XTESPatcher.exe`

Este archivo es autónomo — no requiere .NET instalado en el PC del usuario.

## Personalizar links de comunidad

En `MainWindow.xaml.cs`, al principio del archivo:

```csharp
static readonly string UrlGitHub  = "https://github.com/Darkusze/P5XTES";
static readonly string UrlTwitter = "https://twitter.com/TU_USUARIO";
static readonly string UrlDiscord = "https://discord.gg/TU_SERVIDOR";
static readonly string UrlKofi    = "https://ko-fi.com/TU_USUARIO";
```

## Estructura de releases en GitHub esperada

El patcher busca en el último release 3 assets con estos patrones de nombre:

| Componente | Patrón buscado |
|------------|----------------|
| Traductor  | `P5XTES.zip` |
| Textos     | archivo que contenga `only` y `text` en el nombre |
| Texturas   | archivo que contenga `only` y `texture` en el nombre |

Ejemplo de nombres válidos:
- `P5XTES-2.4.0.zip`
- `P5XTES-only.text-2.5.0.zip`
- `P5XTES-only.texture-4.0.6.zip`

## Archivo de configuración generado

El patcher guarda su estado en:
`[ruta_juego]\config_P5XTES.ini`

```ini
mod_version=2.4.0
text_version=2.5.0
texture_version=4.0.6
ruta_raiz=C:\...\P5X\client\pc
```
