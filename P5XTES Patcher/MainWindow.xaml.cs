using System;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Diagnostics;

namespace P5XTESPatcher
{
    public partial class MainWindow : Window
    {
        // ─── URLs comunidad ──────────────────────────────────────────────────────
        const string RepoApiUrl = "https://api.github.com/repos/Darkusze/P5XTES/releases/latest";
        const string UrlGitHub = "https://github.com/Darkusze/P5XTES";
        const string UrlTwitter = "https://x.com/Katzuro32";
        const string UrlDiscord = "https://discord.com/invite/9MFSd3AAc4";
        const string UrlKofi = "https://ko-fi.com/shinoesp";
        const string UrlYouTube = "https://www.youtube.com/@Shinoesp";

        const string NombreBase = "Nagisa";
        const string ApellidoBase = "Kamisiro";

        // ─── Estado runtime ──────────────────────────────────────────────────────
        string rutaRaiz = @"C:\Program Files (x86)\Steam\steamapps\common\P5X\client\pc";

        // El config se guarda en AppData para que siempre sea encontrado,
        // independientemente de donde este instalado el juego.
        static readonly string ConfigPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "P5XTESPatcher", "config_P5XTES.ini");
        string RutaMod => Path.Combine(rutaRaiz, @"BepInEx\Translation\es\Text");
        // Launcher: sube dos niveles desde \P5X\client\pc → \P5X, luego entra en P5XLaunch
        string RutaLauncher => Path.Combine(
            Directory.GetParent(Directory.GetParent(rutaRaiz)!.FullName)!.FullName,
            "P5XLaunch", "P5XLauncher.exe");

        // Nombre guardado EN MEMORIA (lo que está aplicado actualmente en los archivos)
        string nActual = NombreBase;
        string aActual = ApellidoBase;

        string urlTraductor = "", urlTextos = "", urlTexturas = "";
        string verTraductorRemoto = "", verTextosRemoto = "", verTexturasRemoto = "";

        static readonly UTF8Encoding Utf8NoBom = new(false);
        bool _ignorarCheckChange = false;

        // ─── Colores ─────────────────────────────────────────────────────────────
        static readonly SolidColorBrush ColorVerde = new(Color.FromRgb(0x2e, 0xcc, 0x71));
        static readonly SolidColorBrush ColorNaranja = new(Color.FromRgb(0xf3, 0x9c, 0x12));
        static readonly SolidColorBrush ColorRojo = new(Color.FromRgb(0xe7, 0x4c, 0x3c));
        static readonly SolidColorBrush ColorGris = new(Color.FromRgb(0x55, 0x55, 0x55));

        // ─── Constructor ─────────────────────────────────────────────────────────
        public MainWindow()
        {
            InitializeComponent();
            Loaded += async (s, e) => await Inicializar();
        }

        async Task Inicializar()
        {
            CargarIcono();

            // ── Migrar config antiguo (estaba junto al juego) ──
            MigrarConfigAntiguo();

            // ── Cargar ruta guardada ──
            string rutaGuardada = GetCfg("ruta_raiz");
            if (!string.IsNullOrEmpty(rutaGuardada))
                rutaRaiz = rutaGuardada;

            TxtRuta.Text = rutaRaiz;
            TxtRutaStatus.Text = rutaRaiz;

            // ── Verificar que la ruta del juego existe ──
            bool rutaOk = VerificarRutaJuego();

            // Aunque la ruta no sea válida, igualmente cargamos estado y conectamos a GitHub
            // para que el usuario pueda cambiar la ruta y descargar sin reiniciar la app.
            if (rutaOk)
            {
                CargarNombreDesdeConfig();
                CargarEstadoCheckID();
            }
            ActualizarVersionesLocales();
            SetStatus("● Conectando con GitHub...", ColorGris);

            await ComprobarVersionesRemotas();
        }

        // ─── Verificación de ruta ─────────────────────────────────────────────────
        // Comprueba que la ruta del juego existe Y termina en \P5X\client\pc
        // (sin importar la unidad ni las carpetas intermedias).
        // Devuelve true si es válida, false si no (y muestra aviso).
        bool VerificarRutaJuego(bool silencioso = false)
        {
            // Normalizar separadores para la comparación
            string ruta = rutaRaiz.TrimEnd('\\', '/');
            string rutaNorm = ruta.Replace('/', '\\').ToLowerInvariant();
            const string sufijo = @"\p5x\client\pc";

            bool rutaCorrecta = rutaNorm.EndsWith(sufijo, StringComparison.OrdinalIgnoreCase);
            bool existe = Directory.Exists(rutaRaiz);

            if (rutaCorrecta && existe) return true;

            if (!silencioso)
            {
                string msg;
                if (!rutaCorrecta)
                    msg = $"La ruta seleccionada no es válida:\n{rutaRaiz}\n\n" +
                          "La ruta debe terminar en ...\\P5X\\client\\pc\n" +
                          "Ejemplo: D:\\Juegos\\P5X\\client\\pc\n\n" +
                          "Usa \"Cambiar ruta\" para corregirla.";
                else
                    msg = $"La carpeta del juego no existe en:\n{rutaRaiz}\n\n" +
                          "Asegúrate de que el juego esté instalado en esa ruta.";

                SetStatus("● Ruta del juego no válida", ColorRojo);
                MessageBox.Show(msg, "P5XTES Patcher — Ruta no válida",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return false;
        }

        // ─── Icono del titlebar ───────────────────────────────────────────────────
        // Lee el icono desde los recursos embebidos en el propio .exe.
        // No necesita ningún archivo suelto junto al ejecutable.
        void CargarIcono()
        {
            try
            {
                // El nombre del recurso embebido es: <Namespace>.<NombreArchivo>
                // Ejemplo: P5XTESPatcher.P5XTES_Patcher_v2.ico
                var ensamblado = System.Reflection.Assembly.GetExecutingAssembly();
                string recurso = "P5XTESPatcher.P5XTES_Patcher_v2.ico";

                using var stream = ensamblado.GetManifestResourceStream(recurso);
                if (stream == null) return; // recurso no encontrado, queda el fallback "P5"

                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.StreamSource = stream;
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.EndInit();

                IconImg.Source = bmp;
                IconImg.Visibility = Visibility.Visible;
                IconFallback.Visibility = Visibility.Collapsed;
            }
            catch { /* mantiene el fallback "P5" */ }
        }

        // ═════════════════════════════════════════════════════════════════════════
        // SECCIÓN 1 — VERSIONES
        // ═════════════════════════════════════════════════════════════════════════

        // Devuelve la versión de un componente cruzando ini y disco:
        // 1. Si el ini tiene versión válida, la devuelve.
        // 2. Si no, comprueba si la carpeta/archivo existe fisicamente.
        //    Si existe, guarda "desconocida" en el ini y la devuelve.
        // 3. Si no existe en disco, devuelve "".
        // rutaRelativa: carpeta relativa a rutaRaiz a comprobar (null = solo ini).
        // cfgKey:       clave del ini donde guardar si se detecta del disco.
        // Escribe p5xtes_version.txt junto al juego con las versiones actuales.
        // Sirve como fuente de verdad cuando el ini en AppData no tiene la versión
        // (p.ej. el usuario movió el juego a otro disco).
        void EscribirVersionTxt()
        {
            try
            {
                string path = Path.Combine(rutaRaiz, "p5xtes_version.txt");
                var lineas = new[]
                {
                    $"mod_version={GetCfg("mod_version")}",
                    $"text_version={GetCfg("text_version")}",
                    $"texture_version={GetCfg("texture_version")}"
                };
                File.WriteAllLines(path, lineas, Utf8NoBom);
            }
            catch { /* no crítico */ }
        }

        // Lee p5xtes_version.txt del directorio del juego y devuelve el valor
        // de la clave pedida, o "" si no existe o no se puede leer.
        string LeerVersionTxt(string key)
        {
            try
            {
                string path = Path.Combine(rutaRaiz, "p5xtes_version.txt");
                if (!File.Exists(path)) return "";
                foreach (var linea in File.ReadAllLines(path, Utf8NoBom))
                {
                    var partes = linea.Split('=', 2);
                    if (partes.Length == 2 && partes[0].Trim() == key)
                        return partes[1].Trim();
                }
            }
            catch { }
            return "";
        }

        // Devuelve la versión de un componente cruzando ini → p5xtes_version.txt → disco:
        // 1. Si el ini tiene versión válida, la devuelve.
        // 2. Si no, intenta leerla de p5xtes_version.txt (junto al juego).
        //    Si la encuentra, la sincroniza al ini y la devuelve.
        // 3. Si tampoco, comprueba si la carpeta existe físicamente.
        //    Si existe pero sin versión conocida, devuelve "" → UI muestra "—"
        //    (instalado pero sin versión → no se puede comparar con remoto).
        // 4. Si no existe en disco, devuelve "".
        string ResolverVersionLocal(string cfgKey, string rutaRelativa, string? cfgKeyGuardar)
        {
            string ver = GetCfg(cfgKey);
            if (EsVersionValida(ver)) return ver;

            // Intentar recuperar la versión desde p5xtes_version.txt
            string verTxt = LeerVersionTxt(cfgKey);
            if (EsVersionValida(verTxt))
            {
                // Sincronizar al ini para no releer el txt en cada arranque
                string cfgTarget = cfgKeyGuardar ?? cfgKey;
                SetCfg(cfgTarget, verTxt);
                return verTxt;
            }

            // Sin versión en ninguna fuente: comprobar si el componente existe en disco
            string rutaFisica = Path.Combine(rutaRaiz, rutaRelativa);

            // Para texturas comprobar también Texture2D (nombre alternativo en BepInEx)
            string rutaFisicaAlt = rutaRelativa.EndsWith("Texture", StringComparison.OrdinalIgnoreCase)
                ? Path.Combine(rutaRaiz, rutaRelativa + "2D")
                : "";

            bool TieneArchivos(string ruta) =>
                Directory.Exists(ruta) && Directory.EnumerateFiles(ruta, "*", SearchOption.AllDirectories).Any();

            bool existeEnDisco = rutaRelativa == "BepInEx"
                ? Directory.Exists(rutaFisica)
                : TieneArchivos(rutaFisica) || (!string.IsNullOrEmpty(rutaFisicaAlt) && TieneArchivos(rutaFisicaAlt));

            // Existe en disco pero la versión es desconocida → no la guardamos en el ini
            // para no bloquear futuras detecciones. La UI mostrará "—" y el dot en gris.
            return existeEnDisco ? "instalado-sin-version" : "";
        }

        void ActualizarVersionesLocales()
        {
            string vMod = ResolverVersionLocal("mod_version", "BepInEx", null);
            string vText = ResolverVersionLocal("text_version", @"BepInEx\Translation\es\Text", "text_version");
            string vTex = ResolverVersionLocal("texture_version", @"BepInEx\Translation\es\Texture", "texture_version");

            TxtVerTraductorLocal.Text = EsVersionValida(vMod) ? vMod : vMod == "instalado-sin-version" ? "?" : "—";
            TxtVerTextosLocal.Text = EsVersionValida(vText) ? vText : vText == "instalado-sin-version" ? "?" : "—";
            TxtVerTexturasLocal.Text = EsVersionValida(vTex) ? vTex : vTex == "instalado-sin-version" ? "?" : "—";
        }

        async Task ComprobarVersionesRemotas()
        {
            try
            {
                using var client = CrearHttpClient();
                string json = await client.GetStringAsync(RepoApiUrl);

                var assets = ExtraerTodosLosAssets(json);

                urlTraductor = BuscarAsset(assets, n =>
                    n.Contains("P5XTES", StringComparison.OrdinalIgnoreCase) &&
                    !n.StartsWith("Only", StringComparison.OrdinalIgnoreCase));

                urlTextos = BuscarAsset(assets, n =>
                    n.StartsWith("Only", StringComparison.OrdinalIgnoreCase) &&
                    n.Contains("text", StringComparison.OrdinalIgnoreCase) &&
                    n.IndexOf("texture", StringComparison.OrdinalIgnoreCase) < 0);

                urlTexturas = BuscarAsset(assets, n =>
                    n.StartsWith("Only", StringComparison.OrdinalIgnoreCase) &&
                    n.Contains("texture", StringComparison.OrdinalIgnoreCase));

                verTraductorRemoto = ExtraerVersion(Path.GetFileName(urlTraductor));
                verTextosRemoto = ExtraerVersion(Path.GetFileName(urlTextos));
                verTexturasRemoto = ExtraerVersion(Path.GetFileName(urlTexturas));

                ActualizarUI_Versiones();
                SetStatus("● Conectado a GitHub", ColorVerde);
            }
            catch (Exception ex)
            {
                SetStatus($"● Sin conexión: {ex.Message}", ColorRojo);
                ActualizarUI_SoloLocal();
            }
        }

        static Dictionary<string, string> ExtraerTodosLosAssets(string json)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var urlMatches = Regex.Matches(json,
                @"""browser_download_url""\s*:\s*""(https://[^""]+)""");
            foreach (Match m in urlMatches.Cast<Match>())
            {
                string url = m.Groups[1].Value;
                string nombre = Path.GetFileName(url);
                if (!nombre.StartsWith("source", StringComparison.OrdinalIgnoreCase))
                    result[nombre] = url;
            }
            return result;
        }

        static string BuscarAsset(Dictionary<string, string> assets, Func<string, bool> predicado)
        {
            foreach (var kv in assets)
                if (predicado(kv.Key)) return kv.Value;
            return "";
        }

        void ActualizarUI_Versiones()
        {
            // Usa ResolverVersionLocal para cruzar ini + disco
            string vMod = ResolverVersionLocal("mod_version", "BepInEx", null);
            string vText = ResolverVersionLocal("text_version", @"BepInEx\Translation\es\Text", "text_version");
            string vTex = ResolverVersionLocal("texture_version", @"BepInEx\Translation\es\Texture", "texture_version");
            bool modOk = EsVersionValida(vMod);
            bool modPresente = modOk || vMod == "instalado-sin-version";

            // ── Traductor ──
            TxtVerTraductorRemoto.Text = string.IsNullOrEmpty(verTraductorRemoto) ? "—" : verTraductorRemoto;
            if (vMod == "instalado-sin-version")
                SetEstado(DotTraductor, TxtStatusTraductor, BtnTraductor, TxtBtnTraductor,
                    ColorNaranja, "Instalado — versión desconocida", "↑  Reinstalar", true);
            else if (!modOk)
                SetEstado(DotTraductor, TxtStatusTraductor, BtnTraductor, TxtBtnTraductor,
                    ColorRojo, "No instalado", "↓  Descargar", true);
            else if (VersionMayor(verTraductorRemoto, vMod))
                SetEstado(DotTraductor, TxtStatusTraductor, BtnTraductor, TxtBtnTraductor,
                    ColorNaranja, "Actualización disponible", "↑  Actualizar", true);
            else
                SetEstado(DotTraductor, TxtStatusTraductor, BtnTraductor, TxtBtnTraductor,
                    ColorVerde, "Al día  ✓", "✓  Traductor", false);

            // ── Textos — solo habilitado si el mod (traductor) está instalado ──
            TxtVerTextosRemoto.Text = string.IsNullOrEmpty(verTextosRemoto) ? "—" : verTextosRemoto;
            if (!modPresente)
            {
                SetEstado(DotTextos, TxtStatusTextos, BtnTextos, TxtBtnTextos,
                    ColorGris, "Instala el traductor primero", "—  Textos", false);
            }
            else if (vText == "instalado-sin-version")
                SetEstado(DotTextos, TxtStatusTextos, BtnTextos, TxtBtnTextos,
                    ColorNaranja, "Instalado — versión desconocida", "↑  Reinstalar", true);
            else if (!EsVersionValida(vText))
                SetEstado(DotTextos, TxtStatusTextos, BtnTextos, TxtBtnTextos,
                    ColorRojo, "No instalado", "↓  Descargar", true);
            else if (VersionMayor(verTextosRemoto, vText))
                SetEstado(DotTextos, TxtStatusTextos, BtnTextos, TxtBtnTextos,
                    ColorNaranja, "Actualización disponible", "↑  Actualizar", true);
            else
                SetEstado(DotTextos, TxtStatusTextos, BtnTextos, TxtBtnTextos,
                    ColorVerde, "Al día  ✓", "✓  Textos", false);

            // ── Texturas — solo habilitado si el mod está instalado ──
            TxtVerTexturasRemoto.Text = string.IsNullOrEmpty(verTexturasRemoto) ? "—" : verTexturasRemoto;
            if (!modPresente)
            {
                SetEstado(DotTexturas, TxtStatusTexturas, BtnTexturas, TxtBtnTexturas,
                    ColorGris, "Instala el traductor primero", "—  Texturas", false);
            }
            else if (vTex == "instalado-sin-version")
                SetEstado(DotTexturas, TxtStatusTexturas, BtnTexturas, TxtBtnTexturas,
                    ColorNaranja, "Instalado — versión desconocida", "↑  Reinstalar", true);
            else if (!EsVersionValida(vTex))
                SetEstado(DotTexturas, TxtStatusTexturas, BtnTexturas, TxtBtnTexturas,
                    ColorRojo, "No instalado", "↓  Descargar", true);
            else if (VersionMayor(verTexturasRemoto, vTex))
                SetEstado(DotTexturas, TxtStatusTexturas, BtnTexturas, TxtBtnTexturas,
                    ColorNaranja, "Actualización disponible", "↑  Actualizar", true);
            else
                SetEstado(DotTexturas, TxtStatusTexturas, BtnTexturas, TxtBtnTexturas,
                    ColorVerde, "Al día  ✓", "✓  Texturas", false);

            BtnActualizarTodo.IsEnabled = BtnTraductor.IsEnabled || BtnTextos.IsEnabled || BtnTexturas.IsEnabled;
        }

        void SetEstado(
            System.Windows.Shapes.Ellipse dot,
            System.Windows.Controls.TextBlock lblStatus,
            System.Windows.Controls.Button btn,
            System.Windows.Controls.TextBlock lblBtn,
            SolidColorBrush color, string statusText, string btnText, bool enabled)
        {
            dot.Fill = color;
            lblStatus.Text = statusText;
            lblStatus.Foreground = color;
            lblBtn.Text = btnText;
            btn.IsEnabled = enabled;
            btn.Style = enabled
                ? (Style)FindResource("PrimaryButton")
                : (Style)FindResource("PatcherButton");
        }

        void ActualizarUI_SoloLocal()
        {
            static void SetDot(System.Windows.Shapes.Ellipse dot,
                        System.Windows.Controls.TextBlock lbl,
                        System.Windows.Controls.TextBlock lblVer, string ver)
            {
                bool ok = EsVersionValida(ver);
                bool sinVer = ver == "instalado-sin-version";
                dot.Fill = ok ? ColorVerde : sinVer ? ColorNaranja : ColorRojo;
                lbl.Text = ok ? "Instalado" : sinVer ? "Instalado — versión desconocida" : "No instalado";
                lbl.Foreground = ok ? ColorVerde : sinVer ? ColorNaranja : ColorRojo;
                lblVer.Text = "—";
            }
            SetDot(DotTraductor, TxtStatusTraductor, TxtVerTraductorRemoto, ResolverVersionLocal("mod_version", "BepInEx", null));
            SetDot(DotTextos, TxtStatusTextos, TxtVerTextosRemoto, ResolverVersionLocal("text_version", @"BepInEx\Translation\es\Text", "text_version"));
            SetDot(DotTexturas, TxtStatusTexturas, TxtVerTexturasRemoto, ResolverVersionLocal("texture_version", @"BepInEx\Translation\es\Texture", "texture_version"));
        }

        // ═════════════════════════════════════════════════════════════════════════
        // SECCIÓN 2 — DESCARGA + EXTRACCIÓN
        // ═════════════════════════════════════════════════════════════════════════

        async Task EjecutarDescarga(string url, string tipo)
        {
            if (string.IsNullOrEmpty(url))
            {
                MsgError("No se encontró el archivo en GitHub.\nComprueba tu conexión o visita el repositorio manualmente.");
                return;
            }

            // Guardia extra: textos y texturas requieren que el traductor esté instalado
            if ((tipo == "TEXTO" || tipo == "TEXTURA") && !EsVersionValida(GetCfg("mod_version")))
            {
                MsgError("Debes instalar el Traductor (BepInEx) antes de descargar los Textos o las Texturas.");
                return;
            }

            BloquearBotones(true);
            string fileName = Path.GetFileName(url);
            string tmpFile = Path.Combine(Path.GetTempPath(), fileName);
            string version = ExtraerVersion(fileName);

            if (tipo == "TEXTO")
                LimpiarDirectorio(Path.Combine(rutaRaiz, @"BepInEx\Translation\es\Text"));
            else if (tipo == "TEXTURA")
                LimpiarDirectorio(Path.Combine(rutaRaiz, @"BepInEx\Translation\es\Texture"));
            LimpiarDirectorio(Path.Combine(rutaRaiz, @"BepInEx\Translation\es\Texture2D"));

            try
            {
                SetStatus($"● Descargando {fileName}...", ColorNaranja);
                SetProgreso(0, $"Iniciando descarga de {tipo.ToLower()}...");

                using var client = CrearHttpClient();
                var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();

                long totalBytes = response.Content.Headers.ContentLength ?? -1;
                using (var netStream = await response.Content.ReadAsStreamAsync())
                using (var fileStream = File.Create(tmpFile))
                {
                    byte[] buf = new byte[81920];
                    long leido = 0;
                    int n;
                    while ((n = await netStream.ReadAsync(buf)) > 0)
                    {
                        await fileStream.WriteAsync(buf.AsMemory(0, n));
                        leido += n;
                        if (totalBytes > 0)
                            SetProgreso((double)leido / totalBytes,
                                $"Descargando... {leido / 1048576.0:F1} MB / {totalBytes / 1048576.0:F1} MB");
                    }
                }

                SetProgreso(0.9, "Extrayendo archivos...");
                SetStatus("● Extrayendo...", ColorNaranja);

                string targetDir = (tipo == "CORE")
                    ? rutaRaiz
                    : Path.Combine(rutaRaiz, "BepInEx");

                Directory.CreateDirectory(targetDir);

                await Task.Run(() =>
                {
                    using var archive = ZipFile.OpenRead(tmpFile);
                    var entries = archive.Entries
                        .Where(e => !string.IsNullOrEmpty(e.Name))
                        .ToList();
                    int total = entries.Count, i = 0;
                    foreach (var entry in entries)
                    {
                        string dest = Path.Combine(targetDir, entry.FullName);
                        string? dir = Path.GetDirectoryName(dest);
                        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
                        entry.ExtractToFile(dest, overwrite: true);
                        i++;
                        SetProgreso(0.9 + 0.1 * i / total, $"Extrayendo {i}/{total}...");
                    }
                });

                if (tipo == "CORE") SetCfg("mod_version", version);
                else if (tipo == "TEXTO") SetCfg("text_version", version);
                else if (tipo == "TEXTURA") SetCfg("texture_version", version);

                // Escribir p5xtes_version.txt en la raíz del juego con las versiones
                // actuales para que el patcher pueda recuperarlas si el ini se pierde
                // (cambio de disco, reinstalación, etc.)
                EscribirVersionTxt();

                try { File.Delete(tmpFile); } catch { }

                // ── Post-instalación de textos: re-aplicar nombre y ocultar ID ──
                if (tipo == "TEXTO")
                {
                    SetProgreso(0.98, "Re-aplicando personalizaciones...");
                    AplicarNombreEnArchivos(nActual, aActual, NombreBase, ApellidoBase);
                    if (ChkOcultarID.IsChecked == true)
                        AplicarOcultarID(ocultar: true, silencioso: true);
                }

                SetProgreso(1.0, $"✓ {tipo} instalado — v{version}");
                SetStatus($"● {tipo} actualizado a v{version}", ColorVerde);
                ActualizarVersionesLocales();
                ActualizarUI_Versiones();
            }
            catch (Exception ex)
            {
                SetStatus($"● Error: {ex.Message}", ColorRojo);
                SetProgreso(0, "");
                MsgError($"Error al instalar {tipo}:\n\n{ex.Message}");
            }
            finally
            {
                // No usar BloquearBotones(false) aquí: rehabilitaría botones sin comparar
                // versiones. ActualizarUI_Versiones ya habilita/deshabilita correctamente
                // según el estado real (al día, pendiente, no instalado).
                Dispatcher.Invoke(() => BtnDesinstalar.IsEnabled = true);
                ActualizarUI_Versiones();
            }
        }

        async void BtnTraductor_Click(object s, RoutedEventArgs e)
            => await EjecutarDescarga(urlTraductor, "CORE");
        async void BtnTextos_Click(object s, RoutedEventArgs e)
            => await EjecutarDescarga(urlTextos, "TEXTO");
        async void BtnTexturas_Click(object s, RoutedEventArgs e)
            => await EjecutarDescarga(urlTexturas, "TEXTURA");

        async void BtnActualizarTodo_Click(object s, RoutedEventArgs e)
        {
            if (BtnTraductor.IsEnabled) await EjecutarDescarga(urlTraductor, "CORE");
            if (BtnTextos.IsEnabled) await EjecutarDescarga(urlTextos, "TEXTO");
            if (BtnTexturas.IsEnabled) await EjecutarDescarga(urlTexturas, "TEXTURA");
        }

        // ═════════════════════════════════════════════════════════════════════════
        // SECCIÓN 3 — DESINSTALAR
        // ═════════════════════════════════════════════════════════════════════════

        void BtnDesinstalar_Click(object s, RoutedEventArgs e)
        {
            var res = MessageBox.Show(
                "Se eliminarán las carpetas BepInEx y dotnet, y los archivos de configuración del mod.\n\n¿Estás seguro?",
                "Desinstalar P5XTES",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (res != MessageBoxResult.Yes) return;

            string[] carpetas = { "BepInEx", "dotnet" };
            string[] archivos = {
                "changelog.txt", "doorstop_config.ini", "winhttp.dll",
                ".doorstop_version", "p5xtes_version.txt"
                // config_P5XTES.ini NO se borra: conserva ruta y preferencias del patcher
            };

            int errores = 0;
            foreach (var f in archivos)
                try { string p = Path.Combine(rutaRaiz, f); if (File.Exists(p)) File.Delete(p); }
                catch { errores++; }

            foreach (var d in carpetas)
                try { string p = Path.Combine(rutaRaiz, d); if (Directory.Exists(p)) Directory.Delete(p, true); }
                catch { errores++; }

            // Limpiar versiones en config pero conservar ruta y nombre
            SetCfg("mod_version", "");
            SetCfg("text_version", "");
            SetCfg("texture_version", "");

            // Resetear UI de nombre
            nActual = NombreBase; aActual = ApellidoBase;
            TxtNombre.Text = NombreBase;
            TxtApellido.Text = ApellidoBase;
            SetCfg("prota_nombre", "");
            SetCfg("prota_apellido", "");

            _ignorarCheckChange = true;
            ChkOcultarID.IsChecked = false;
            _ignorarCheckChange = false;

            // Recargar versiones y estado de botones desde cero
            ActualizarVersionesLocales();
            // Si hay conexión, volver a evaluar la UI completa; si no, modo local
            if (!string.IsNullOrEmpty(verTraductorRemoto))
                ActualizarUI_Versiones();
            else
                ActualizarUI_SoloLocal();

            if (errores == 0)
            {
                SetStatus("● Mod desinstalado correctamente", ColorVerde);
                MessageBox.Show("Mod desinstalado correctamente.\nYa puedes volver a descargar los componentes.",
                    "P5XTES Patcher", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                SetStatus("● Desinstalado con algunos errores", ColorNaranja);
                MessageBox.Show($"Desinstalado con {errores} error(es).\nPuede que algún archivo estuviera en uso.",
                    "P5XTES Patcher", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // ═════════════════════════════════════════════════════════════════════════
        // SECCIÓN 4 — NOMBRE PROTAGONISTA
        // ═════════════════════════════════════════════════════════════════════════

        // Carga el nombre guardado en config_P5XTES.ini
        // (migración: si existe patcher_data.txt antiguo, lo importa y lo borra)
        void CargarNombreDesdeConfig()
        {
            string n = GetCfg("prota_nombre");
            string a = GetCfg("prota_apellido");

            // (patcher_data.txt ya no se usa — el nombre vive en config_P5XTES.ini)

            if (!string.IsNullOrEmpty(n) && !string.IsNullOrEmpty(a))
            {
                nActual = n; aActual = a;
                TxtNombre.Text = n;
                TxtApellido.Text = a;
            }
        }

        void BtnAplicarNombre_Click(object s, RoutedEventArgs e)
        {
            string nNuevo = TxtNombre.Text.Replace(" ", "").Trim();
            string aNuevo = TxtApellido.Text.Replace(" ", "").Trim();

            if (string.IsNullOrEmpty(nNuevo) || string.IsNullOrEmpty(aNuevo))
            { MsgError("Introduce un nombre y apellido válidos."); return; }

            // Bloquear si el usuario introduce exactamente el nombre por defecto
            if (nNuevo == NombreBase && aNuevo == ApellidoBase)
            { MsgError($"\"{NombreBase} {ApellidoBase}\" es el nombre por defecto y ya está aplicado.\nIntroduce un nombre diferente."); return; }

            string generalTxt = Path.Combine(RutaMod, "General.txt");
            if (!File.Exists(generalTxt))
            { MsgError($"No se encontró General.txt en:\n{RutaMod}\n\nInstala los textos primero."); return; }

            // Si el nombre nuevo es igual al actual, igual lo aplicamos
            // (permite "corregir" si se instalaron textos nuevos sin reaplicar)
            string nAnterior = nActual;
            string aAnterior = aActual;

            if (AplicarNombreEnArchivos(nNuevo, aNuevo, nAnterior, aAnterior))
            {
                nActual = nNuevo; aActual = aNuevo;
                SetCfg("prota_nombre", nNuevo);
                SetCfg("prota_apellido", aNuevo);
                SetStatus($"● Nombre aplicado: {nNuevo} {aNuevo}", ColorVerde);
            }
        }

        // Aplica el nombre en General.txt.
        // Usa SOLO reemplazo del nombre COMPLETO "Nombre Apellido" como unidad atómica.
        // Hacer replace de partes sueltas (nombre solo, apellido solo) provoca doble
        // sustitución cuando el nuevo nombre coincide con alguna parte del anterior.
        // Ej: Josk Can → Can Josk haría: "Josk Can"→"Can Josk" OK,
        //     luego "Josk"→"Can" convertiría "Can Josk" en "Can Can". BUG evitado.
        bool AplicarNombreEnArchivos(string nNuevo, string aNuevo, string nAnterior, string aAnterior)
        {
            string generalTxt = Path.Combine(RutaMod, "General.txt");
            string substitutionsTxt = Path.Combine(RutaMod, "_Substitutions.txt");

            if (!File.Exists(generalTxt)) return false;
            try
            {
                // ── General.txt ──────────────────────────────────
                string txt = File.ReadAllText(generalTxt, Utf8NoBom);
                txt = txt.Replace($"{nAnterior} {aAnterior}", $"{nNuevo} {aNuevo}");
                File.WriteAllText(generalTxt, txt, Utf8NoBom);

                // ── _Substitutions.txt ─────────────────────────
                // Inserta las lineas de nombre justo despues del marcador
                // +---Nombre Protagonista Personalizado---+
                // Formato: Nombre Apellido=NombreNuevo ApellidoNuevo
                if (File.Exists(substitutionsTxt))
                {
                    const string marcador = "+---Nombre Protagonista Personalizado---+";
                    string sub = File.ReadAllText(substitutionsTxt, Utf8NoBom);

                    int idxMarcador = sub.IndexOf(marcador, StringComparison.Ordinal);
                    if (idxMarcador >= 0)
                    {
                        // Todo lo anterior al marcador (incluido el propio marcador)
                        string cabecera = sub[..(idxMarcador + marcador.Length)];

                        // Buscar si hay una seccion siguiente para no machacarla
                        int inicioResto = idxMarcador + marcador.Length;
                        while (inicioResto < sub.Length &&
                               (sub[inicioResto] == '\r' || sub[inicioResto] == '\n'))
                            inicioResto++;
                        int finBloque = sub.IndexOf("++", inicioResto, StringComparison.Ordinal);
                        string resto = finBloque >= 0 ? sub[finBloque..] : "";

                        // Construir las sustituciones (nombre completo, nombre, apellido)
                        // Si el nuevo nombre es el base, dejamos la seccion vacia
                        string lineas;
                        if (nNuevo == NombreBase && aNuevo == ApellidoBase)
                        {
                            lineas = "\r\n";
                        }
                        else
                        {
                            // Omitir partes que ya cubre la seccion Por Defecto:
                            // - Si nNuevo == NombreBase ("Nagisa") -> no escribir linea de nombre
                            // - Si aNuevo == ApellidoBase ("Kamisiro") -> no escribir linea de apellido
                            // - El nombre completo siempre se escribe (es combinacion unica)
                            var sb = new System.Text.StringBuilder();
                            sb.Append("\r\n");
                            sb.Append($"{nNuevo} {aNuevo}={nNuevo} {aNuevo}\r\n");
                            if (nNuevo != NombreBase)
                                sb.Append($"{nNuevo}={nNuevo}\r\n");
                            if (aNuevo != ApellidoBase)
                                sb.Append($"{aNuevo}={aNuevo}\r\n");
                            lineas = sb.ToString();
                        }

                        File.WriteAllText(substitutionsTxt,
                            cabecera + lineas + (string.IsNullOrEmpty(resto) ? "" : resto),
                            Utf8NoBom);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                MsgError($"Error al aplicar nombre:\n{ex.Message}");
                return false;
            }
        }

        // ═════════════════════════════════════════════════════════════════════════
        // SECCIÓN 5 — OCULTAR ID
        // ═════════════════════════════════════════════════════════════════════════

        void CargarEstadoCheckID()
        {
            string archivoUI = Path.Combine(RutaMod, "UI.txt");
            if (!File.Exists(archivoUI)) return;
            try
            {
                string contenido = File.ReadAllText(archivoUI, Utf8NoBom);
                _ignorarCheckChange = true;
                ChkOcultarID.IsChecked = contenido.Contains("r:\"^ID:(\\d+)$\"=\"\"");
                _ignorarCheckChange = false;
            }
            catch { }
        }

        void ChkOcultarID_Changed(object s, RoutedEventArgs e)
        {
            if (_ignorarCheckChange) return;
            bool ocultar = ChkOcultarID.IsChecked == true;
            AplicarOcultarID(ocultar, silencioso: false);
        }

        // Aplica o revierte el ocultamiento del ID en UI.txt.
        // silencioso=true → no muestra SetStatus (usado tras instalar textos).
        void AplicarOcultarID(bool ocultar, bool silencioso)
        {
            string archivoUI = Path.Combine(RutaMod, "UI.txt");
            if (!File.Exists(archivoUI))
            {
                if (!silencioso)
                    SetStatus("● UI.txt no encontrado — instala los textos primero.", ColorRojo);
                return;
            }
            try
            {
                const string buscarV = "r:\"^ID:(\\d+)$\"=\"ID:$1\"";
                const string buscarO = "r:\"^ID:(\\d+)$\"=\"\"";
                string c = File.ReadAllText(archivoUI, Utf8NoBom);
                c = ocultar ? c.Replace(buscarV, buscarO) : c.Replace(buscarO, buscarV);
                File.WriteAllText(archivoUI, c, Utf8NoBom);
                if (!silencioso)
                    SetStatus(ocultar ? "● ID ocultado" : "● ID visible", ColorVerde);
            }
            catch (Exception ex)
            {
                if (!silencioso) MsgError($"Error al modificar UI.txt:\n{ex.Message}");
            }
        }

        // ═════════════════════════════════════════════════════════════════════════
        // SECCIÓN 6 — RUTA
        // ═════════════════════════════════════════════════════════════════════════

        async void BtnCambiarRuta_Click(object s, RoutedEventArgs e)
        {
            var dlg = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Selecciona la carpeta raíz del juego (donde está el .exe)",
                SelectedPath = Directory.Exists(rutaRaiz) ? rutaRaiz : ""
            };
            if (dlg.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

            string nuevaRuta = dlg.SelectedPath;
            if (!Directory.Exists(nuevaRuta))
            {
                MsgError("La carpeta seleccionada no existe.");
                return;
            }

            // Validar que termina en \P5X\client\pc antes de aceptar
            string nuevaNorm = nuevaRuta.TrimEnd('\\', '/').Replace('/', '\\').ToLowerInvariant();
            if (!nuevaNorm.EndsWith(@"\p5x\client\pc", StringComparison.OrdinalIgnoreCase))
            {
                MsgError($"La ruta seleccionada no es válida:\n{nuevaRuta}\n\n" +
                         "Debe terminar en ...\\P5X\\client\\pc");
                return;
            }

            rutaRaiz = nuevaRuta;
            TxtRuta.Text = rutaRaiz;
            TxtRutaStatus.Text = rutaRaiz;
            SetCfg("ruta_raiz", rutaRaiz);

            // Limpiar versiones del ini para que ResolverVersionLocal
            // las detecte desde cero en la nueva ruta, sin arrastrar
            // versiones de la instalación anterior.
            SetCfg("mod_version", "");
            SetCfg("text_version", "");
            SetCfg("texture_version", "");

            CargarNombreDesdeConfig();
            CargarEstadoCheckID();
            ActualizarVersionesLocales();

            // Reconectar siempre a GitHub con la nueva ruta (no condicional)
            SetStatus("● Conectando con GitHub...", ColorGris);
            await ComprobarVersionesRemotas();

            SetStatus("● Ruta actualizada", ColorVerde);
        }

        // ═════════════════════════════════════════════════════════════════════════
        // SECCIÓN 7 — AYUDA (handlers — tooltips definidos en XAML)
        // ═════════════════════════════════════════════════════════════════════════

        void BtnHelp_Componentes_Click(object s, RoutedEventArgs e) { }

        // ═════════════════════════════════════════════════════════════════════════
        // SECCIÓN 8 — LINKS
        // ═════════════════════════════════════════════════════════════════════════

        void BtnGitHub_Click(object s, RoutedEventArgs e) => AbrirUrl(UrlGitHub);
        void BtnTwitter_Click(object s, RoutedEventArgs e) => AbrirUrl(UrlTwitter);
        void BtnDiscord_Click(object s, RoutedEventArgs e) => AbrirUrl(UrlDiscord);
        void BtnKofi_Click(object s, RoutedEventArgs e) => AbrirUrl(UrlKofi);
        void BtnYouTube_Click(object s, RoutedEventArgs e) => AbrirUrl(UrlYouTube);

        static void AbrirUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return;
            try { Process.Start(new ProcessStartInfo(url) { UseShellExecute = true }); }
            catch { }
        }

        // ═════════════════════════════════════════════════════════════════════════
        // INICIAR JUEGO
        // ═════════════════════════════════════════════════════════════════════════

        void BtnIniciarJuego_Click(object s, RoutedEventArgs e)
        {
            string launcher = RutaLauncher;

            if (!File.Exists(launcher))
            {
                MsgError($"No se encontró el launcher en:\n{launcher}\n\n" +
                         "Asegúrate de que el juego esté correctamente instalado.");
                return;
            }

            try
            {
                Process.Start(new ProcessStartInfo(launcher) { UseShellExecute = true });
                SetStatus("● Juego iniciado", ColorVerde);
            }
            catch (Exception ex)
            {
                MsgError($"No se pudo iniciar el launcher:\n{ex.Message}");
            }
        }

        // ═════════════════════════════════════════════════════════════════════════
        // TITLEBAR
        // ═════════════════════════════════════════════════════════════════════════

        void TitleBar_MouseDown(object s, MouseButtonEventArgs e)
        { if (e.ChangedButton == MouseButton.Left) DragMove(); }

        void BtnMinimize_Click(object s, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        void BtnClose_Click(object s, RoutedEventArgs e) => Application.Current.Shutdown();

        // ═════════════════════════════════════════════════════════════════════════
        // HELPERS — Config INI
        // ═════════════════════════════════════════════════════════════════════════

        // Migra el config_P5XTES.ini antiguo (guardado junto al juego)
        // al nuevo lugar fijo en AppData. Se ejecuta una sola vez.
        void MigrarConfigAntiguo()
        {
            // Rutas posibles donde el patcher antiguo pudo haber guardado el ini
            string[] rutasCandidatas = {
                Path.Combine(rutaRaiz, "config_P5XTES.ini"),
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "P5XTESPatcher", "config_P5XTES.ini")
            };

            if (File.Exists(ConfigPath)) return; // ya existe el nuevo, no hay que migrar

            foreach (var candidato in rutasCandidatas)
            {
                if (!File.Exists(candidato)) continue;
                try
                {
                    string? dir = Path.GetDirectoryName(ConfigPath);
                    if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
                    File.Copy(candidato, ConfigPath);
                    return; // migrado con exito
                }
                catch { }
            }
        }

        string GetCfg(string key)
        {
            if (!File.Exists(ConfigPath)) return "";
            try
            {
                foreach (var l in File.ReadAllLines(ConfigPath))
                    if (l.StartsWith(key + "="))
                        return l[(key.Length + 1)..].Trim();
            }
            catch { }
            return "";
        }

        void SetCfg(string key, string value)
        {
            var config = new Dictionary<string, string>();
            if (File.Exists(ConfigPath))
                foreach (var l in File.ReadAllLines(ConfigPath))
                {
                    var p = l.Split('=');
                    if (p.Length >= 2) config[p[0].Trim()] = string.Join("=", p.Skip(1)).Trim();
                }
            config[key] = value;
            string? dir = Path.GetDirectoryName(ConfigPath);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
            File.WriteAllLines(ConfigPath, config.Select(x => $"{x.Key}={x.Value}"), Utf8NoBom);
        }

        // ═════════════════════════════════════════════════════════════════════════
        // HELPERS — UI
        // ═════════════════════════════════════════════════════════════════════════

        void SetStatus(string texto, SolidColorBrush color) => Dispatcher.Invoke(() =>
        {
            TxtStatus.Text = texto;
            TxtStatus.Foreground = color;
        });

        void SetProgreso(double fraccion, string texto) => Dispatcher.Invoke(() =>
        {
            double total = ProgressFill.Parent is System.Windows.Controls.Border parent
                ? parent.ActualWidth : 660;
            ProgressFill.Width = Math.Max(0, Math.Min(total, total * fraccion));
            TxtProgreso.Text = texto;
        });

        void BloquearBotones(bool bloquear) => Dispatcher.Invoke(() =>
        {
            bool modOk = EsVersionValida(GetCfg("mod_version"));
            bool t = !string.IsNullOrEmpty(urlTraductor);
            bool tx = !string.IsNullOrEmpty(urlTextos) && modOk;
            bool te = !string.IsNullOrEmpty(urlTexturas) && modOk;

            BtnTraductor.IsEnabled = !bloquear && t;
            BtnTextos.IsEnabled = !bloquear && tx;
            BtnTexturas.IsEnabled = !bloquear && te;
            BtnActualizarTodo.IsEnabled = !bloquear && (t || tx || te);
            BtnDesinstalar.IsEnabled = !bloquear;
        });

        static void MsgError(string msg)
            => MessageBox.Show(msg, "P5XTES Patcher", MessageBoxButton.OK, MessageBoxImage.Warning);

        // ═════════════════════════════════════════════════════════════════════════
        // HELPERS — Versiones / Strings
        // ═════════════════════════════════════════════════════════════════════════

        static bool EsVersionValida(string v)
            => !string.IsNullOrEmpty(v) && v != "0.0.0";

        static string ExtraerVersion(string nombre)
        {
            var m = Regex.Match(nombre ?? "", @"(\d+\.\d+(?:\.\d+)?)");
            return m.Success ? m.Value : "";
        }

        static bool VersionMayor(string remota, string local)
        {
            if (string.IsNullOrEmpty(remota) || string.IsNullOrEmpty(local)) return false;
            if (Version.TryParse(remota, out var vR) && Version.TryParse(local, out var vL))
                return vR > vL;
            return string.Compare(remota, local, StringComparison.OrdinalIgnoreCase) > 0;
        }

        static void LimpiarDirectorio(string path)
        {
            if (Directory.Exists(path))
            { Directory.Delete(path, recursive: true); Directory.CreateDirectory(path); }
        }

        static HttpClient CrearHttpClient()
        {
            var c = new HttpClient();
            c.DefaultRequestHeaders.Add("User-Agent", "P5XTES-Patcher");
            return c;
        }
    }
}
