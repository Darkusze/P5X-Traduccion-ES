using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace P5XTESPatcher
{
    class Program
    {
        // 1. CONFIGURACION INICIAL
        static string rutaMod = @"C:\Program Files (x86)\Steam\steamapps\common\P5X\client\pc\BepInEx\Translation\es\Text";
        static string nBase = "Nagisa";
        static string aBase = "Kamisiro";

        static string nAct = nBase;
        static string aAct = aBase;
        static bool skipVal = false;

        // Usamos UTF8 sin BOM (Igual que tu script de PS)
        static UTF8Encoding utf8 = new UTF8Encoding(false);

        static void Main(string[] args)
        {
            // CONFIGURACIÓN DE CONSOLA PARA EL LOGO
            Console.OutputEncoding = Encoding.UTF8;
            try
            {
                Console.SetWindowSize(150, 60); // Ventana grande para el ASCII
            }
            catch { /* Ignorar si la pantalla es pequeña */ }

            Console.Title = "P5XTES Patcher - V1.0.0";

            while (true)
            {
                // Validación de integridad
                ValidarIntegridad();
                Console.Clear();
                JackfrostLogo();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("=========================================================================================");
                Console.WriteLine("                                 P5XTES PATCHER - V1.0.0");
                Console.WriteLine("=========================================================================================");
                Console.WriteLine(" ESTADO DE ARCHIVOS:");

                bool generalOk = File.Exists(Path.Combine(rutaMod, "General.txt"));
                bool subsOk = File.Exists(Path.Combine(rutaMod, "_Substitutions.txt"));

                Console.WriteLine(generalOk ? "  [OK] General.txt" : "  [X] General.txt - NO ENCONTRADO");
                Console.WriteLine(subsOk ? "  [OK] _Substitutions.txt" : "  [X] _Substitutions.txt - NO ENCONTRADO");
                Console.WriteLine("-----------------------------------------------------------------------------------------");

                Console.WriteLine($" NOMBRE ACTUAL EN EL MOD: {nAct} {aAct}");
                Console.WriteLine("=========================================================================================\n");

                Console.WriteLine("    [1] CONFIGURAR PRIVACIDAD (Ocultar ID en UI)");
                Console.WriteLine("    [2] PERSONALIZAR PROTAGONISTA (Nombre y Apellido)");
                Console.WriteLine("    [3] REPARAR / RESETEAR RUTA DEL MOD");
                Console.WriteLine("    [4] SALIR\n");
                Console.WriteLine("=========================================================================================");

                Console.Write(" SELECCIONE UNA OPCION [1-4]: ");
                string opt = Console.ReadLine()?.Trim();

                if (opt == "1") SeccionID();
                else if (opt == "2") SeccionNombres();
                else if (opt == "3") CambiarRuta();
                else if (opt == "4") Environment.Exit(0);
            }
        }

        static void ValidarIntegridad()
        {
            string archivoData = Path.Combine(rutaMod, "patcher_data.txt");
            string archivoSubs = Path.Combine(rutaMod, "_Substitutions.txt");

            if (skipVal)
            {
                skipVal = false;
                if (File.Exists(archivoData))
                {
                    try
                    {
                        string[] datos = File.ReadAllText(archivoData).Split(',');
                        if (datos.Length == 2) { nAct = datos[0].Trim(); aAct = datos[1].Trim(); }
                    }
                    catch { }
                }
                return;
            }

            if (!File.Exists(archivoData))
            {
                nAct = nBase;
                aAct = aBase;
                return;
            }

            // Leer el nombre que queremos buscar
            string[] checkDatos = File.ReadAllText(archivoData).Split(',');
            if (checkDatos.Length != 2) { ResetSilencioso(); return; }

            string nCheck = checkDatos[0].Trim();
            string aCheck = checkDatos[1].Trim();
            string nombreCompleto = $"{nCheck} {aCheck}".ToLower().Trim();

            bool encontrado = false;

            if (File.Exists(archivoSubs))
            {
                // Leemos el archivo completo y lo normalizamos a minúsculas
                // También quitamos caracteres de control invisibles que suelen dejar los editores de texto
                string contenidoSubs = File.ReadAllText(archivoSubs, utf8).ToLower();

                // Limpieza de seguridad: quitamos espacios extra y saltos de línea para la búsqueda
                if (contenidoSubs.Contains(nombreCompleto))
                {
                    encontrado = true;
                }
            }

            if (!encontrado)
            {
                // DEBUG: Esto te dirá exactamente qué intentó buscar y si el archivo existía
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("===========================================================================");
                Console.WriteLine("                [!] ERROR DE SINCRONIZACIÓN [!]");
                Console.WriteLine("===========================================================================");
                Console.WriteLine($" Buscando: [{nombreCompleto}]");
                Console.WriteLine($" En el archivo: {archivoSubs}");
                Console.WriteLine($" ¿El archivo existe?: " + (File.Exists(archivoSubs) ? "SÍ" : "NO"));
                Console.WriteLine("---------------------------------------------------------------------------");
                Console.WriteLine(" [X] No se encontró el nombre. Reajustando a valores base...");

                if (File.Exists(archivoData)) File.Delete(archivoData);
                nAct = nBase;
                aAct = aBase;
                Console.WriteLine(" Presione cualquier tecla para volver recargar...");
                Console.ReadKey(true);
            }
            else
            {
                nAct = nCheck;
                aAct = aCheck;
            }
        }

        static void ResetSilencioso()
        {
            string archivoData = Path.Combine(rutaMod, "patcher_data.txt");
            if (File.Exists(archivoData)) File.Delete(archivoData);
            nAct = nBase;
            aAct = aBase;
        }

        static void SeccionID()
        {
            Console.Clear();
            string archivoUI = Path.Combine(rutaMod, "UI.txt");

            if (!File.Exists(archivoUI))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[!] ERROR: No se encuentra UI.txt");
                Console.WriteLine("Presione una tecla para volver al menu");
                Console.ReadKey(true);
                return;
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("=========================================================================================");
            Console.WriteLine("                         SECCION 1: PRIVACIDAD DE ID (UI.txt)");
            Console.WriteLine("=========================================================================================");
            Console.WriteLine("    [1] OCULTAR ID (Ocultar del interfaz) (Nota: No oculta en el perfil)");
            Console.WriteLine("    [2] MOSTRAR ID (Por defecto)");
            Console.WriteLine("    [3] VOLVER AL MENU PRINCIPAL\n");

            Console.Write(" SELECCION: ");
            string sel = Console.ReadLine()?.Trim();

            if (sel != "1" && sel != "2") return;

            Console.WriteLine("\n[ ANALIZANDO ARCHIVO... ]");

            string buscarV = "r:\"^ID:(\\d+)$\"=\"$1\"";
            string buscarO = "r:\"^ID:(\\d+)$\"=\"\"";
            string contenido = File.ReadAllText(archivoUI, utf8);

            if (sel == "1") // Ocultar
            {
                if (contenido.Contains(buscarO))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("\n[!] AVISO: El ID ya se encuentra OCULTO actualmente.");
                }
                else if (contenido.Contains(buscarV))
                {
                    contenido = contenido.Replace(buscarV, buscarO);
                    File.WriteAllText(archivoUI, contenido, utf8);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("\n[ OK ] Configuracion actualizada con exito.");
                }
                else MostrarErrorID(buscarV, buscarO);
            }
            else if (sel == "2") // Mostrar
            {
                if (contenido.Contains(buscarV))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("\n[!] AVISO: El ID ya se encuentra VISIBLE actualmente.");
                }
                else if (contenido.Contains(buscarO))
                {
                    contenido = contenido.Replace(buscarO, buscarV);
                    File.WriteAllText(archivoUI, contenido, utf8);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("\n[ OK ] Configuracion actualizada con exito.");
                }
                else MostrarErrorID(buscarV, buscarO);
            }

            Console.WriteLine("Presione una tecla para continuar...");
            Console.ReadKey(true);
        }

        static void MostrarErrorID(string v, string o)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n[!] ERROR: No se encontro el ID en el archivo.");
            Console.WriteLine($"Buscado: {v} o {o}");
        }

        static void SeccionNombres()
        {
            Console.Clear();
            string generalTxt = Path.Combine(rutaMod, "General.txt");
            string subsTxt = Path.Combine(rutaMod, "_Substitutions.txt");
            string dataTxt = Path.Combine(rutaMod, "patcher_data.txt");

            if (!File.Exists(generalTxt))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[!] ERROR: No se encontro General.txt");
                Console.WriteLine("Presione una tecla para volver al menu");
                Console.ReadKey(true);
                return;
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("=========================================================================================");
            Console.WriteLine("                           SECCION 2: PERSONALIZACION DE NOMBRE");
            Console.WriteLine("=========================================================================================");
            Console.WriteLine($" NOMBRE ACTUAL DETECTADO: {nAct} {aAct}");
            Console.WriteLine(" Nota: Es solo para aquellos que tenga nombre diferente en su protagonista, los archivos");
            Console.WriteLine(" por default tiene el nombre base Nagisa Kamisiro.");
            Console.WriteLine(" Asegurate de tener el nombre bien escrito como enel juego respetando sus mayúsculas y ");
            Console.WriteLine(" minúsculas");
            Console.WriteLine("-----------------------------------------------------------------------------------------");
            Console.WriteLine(" Cantidad Max Caracteres: 8\n");

            Console.Write(" [+] INGRESA NUEVO NOMBRE: ");
            string nNew = Console.ReadLine()?.Replace(" ", "");
            Console.Write(" [+] INGRESA NUEVO APELLIDO: ");
            string aNew = Console.ReadLine()?.Replace(" ", "");

            if (string.IsNullOrEmpty(nNew) || string.IsNullOrEmpty(aNew)) return;

            if (nNew.Length > 8 || aNew.Length > 8)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n[!] ERROR: El nombre/apellido no puede superar los 8 caracteres.");
                Console.ReadKey(true);
                return;
            }

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n[ PROCESANDO: Aplicando intercambio seguro... ]");

            try
            {
                // 1. Reemplazo en General.txt
                string txt = File.ReadAllText(generalTxt, utf8);

                // Fase de Proteccion
                txt = txt.Replace($"{nAct} {aAct}", "##FULL_NAME##");
                txt = txt.Replace("Nagisa Kamisiro", "##FULL_NAME##");
                txt = txt.Replace(nAct, "##FIRST_NAME##");
                txt = txt.Replace(aAct, "##LAST_NAME##");
                txt = txt.Replace("Nagisa", "##FIRST_NAME##");
                txt = txt.Replace("Kamisiro", "##LAST_NAME##");

                // Fase de Aplicacion
                txt = txt.Replace("##FULL_NAME##", $"{nNew} {aNew}");
                txt = txt.Replace("##FIRST_NAME##", nNew);
                txt = txt.Replace("##LAST_NAME##", aNew);

                File.WriteAllText(generalTxt, txt, utf8);

                // 2. Actualizacion en _Substitutions.txt
                if (File.Exists(subsTxt))
                {
                    string[] lineas = File.ReadAllLines(subsTxt, utf8);
                    string marcador = "++--------Nombre Protagonista------+";
                    List<string> listaFinal = new List<string>();

                    bool encontrado = false;
                    foreach (string linea in lineas)
                    {
                        if (linea.Contains(marcador))
                        {
                            listaFinal.Add(linea);
                            encontrado = true;
                            break;
                        }
                        listaFinal.Add(linea);
                    }

                    if (!encontrado) listaFinal.Add(marcador);

                    listaFinal.Add($"{nNew} {aNew}={nNew} {aNew}");
                    listaFinal.Add($"{nNew}={nNew}");
                    listaFinal.Add($"{aNew}={aNew}");

                    File.WriteAllLines(subsTxt, listaFinal, utf8);
                }

                // Guardar registro
                File.WriteAllText(dataTxt, $"{nNew},{aNew}");
                skipVal = true;

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\n[ OK ] Intercambio realizado con exito.");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n[!] ERROR CRITICO: {ex.Message}");
            }

            Console.WriteLine("Presione una tecla para continuar...");
            Console.ReadKey(true);
        }

        static void CambiarRuta()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("=========================================================================================");
            Console.WriteLine("                                CONFIGURACION DE RUTA");
            Console.WriteLine("=========================================================================================");
            Console.WriteLine(" Arrastra la carpeta 'Text' de tu mod aqui y presiona ENTER.");
            Console.WriteLine(" (Ejemplo: ...\\BepInEx\\Translation\\es\\Text)\n");

            Console.Write(" NUEVA RUTA: ");
            string nRuta = Console.ReadLine()?.Replace("\"", "");

            if (!string.IsNullOrEmpty(nRuta) && Directory.Exists(nRuta))
            {
                rutaMod = nRuta;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\n[ OK ] Ruta actualizada correctamente.");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n[!] ERROR: La ruta no es valida o no existe.");
            }
            Console.ReadKey(true);
        }
        // Método para centrar texto normal
        static void CenterText(string text)
        {
            Console.WriteLine(string.Format("{0," + ((Console.WindowWidth / 2) + (text.Length / 2)) + "}", text));
        }

        static void JackfrostLogo()
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            // Margen para centrar el logo de aprox 45 caracteres en una consola de 100
            string margin = "                         ";
            Console.WriteLine(margin + @"        .:. *###.           :###* ");
            Console.WriteLine(margin + @"       .+%%#%#%%%:        .=##%##-##-        ");
            Console.WriteLine(margin + @"       =#-#%*#%%%%+..::...*%%%%%%%#*#.       ");
            Console.WriteLine(margin + @"       ......*%%#*****++****##%+.:= ..       ");
            Console.WriteLine(margin + @"            .+#####*##*++******:             ");
            Console.WriteLine(margin + @"            +####+=.=-::-:-*****.            ");
            Console.WriteLine(margin + @"          .=##=:--#@-:::*%+=--**+            ");
            Console.WriteLine(margin + @"          :*#:-..-@@*..-@@*..:-+*.           ");
            Console.WriteLine(margin + @"          =#-:..::-+:..:+*-::::-*-           ");
            Console.WriteLine(margin + @"         .*+-:..#@@*@@@@+%@%:::-++           ");
            Console.WriteLine(margin + @"         .=:::.::*@@@@@@@@#::::==:           ");
            Console.WriteLine(margin + @"         .:. ..::::-+**+-:::::.:-            ");
            Console.WriteLine(margin + @"                .+-::::::-*=.  ..            ");
            Console.WriteLine(margin + @"             .:=*+%#+*%%++=#-::..            ");
            Console.WriteLine("");
            Console.ResetColor();
        }
    
    }
}