#  P5X Traducción fan ES (Global) (PC)
<h1 align="center">
<br>
<img width="500" height="252" alt="image" src="https://github.com/user-attachments/assets/d090757b-0b69-40bf-b925-3330996b17b0" />
<br>
</h1>

## introducción

P5XTES es un proyecto fan-made para llevar a cabo la traduccion al español de la versión global de Persona 5 The Phantom X (P5X) mediante el uso del plugins mod [BepInEx](https://github.com/BepInEx/BepInEx) y [XUnity](https://github.com/bbepis/XUnity.AutoTranslator) para crear y cargar las traducciones automaticas generadas y manuales.

## ¿A que se debe este proyecto?

Todos sabemos que la versión global de Persona 5: The Phantom X fue lanzada únicamente en inglés, lo que representa un problema significativo para los jugadores hispanohablantes. Muchos abandonan el juego o pierden el interés porque no dominan el idioma lo suficiente para disfrutar de los diálogos profundos, misiones, eventos y otros elementos narrativos. Por eso, decidí crear esta traducción al español para abarcar a más personas, permitiendo que un mayor número de jugadores acceda al contenido completo y se sumerja en la experiencia sin frustraciones al no entenderlo.

## ¿Estare actualizando las traducciones del juego?
Sí, planeo actualizar las traducciones regularmente para mantener compatibilidad con los parches oficiales del juego y agregar mejoras basadas en feedback.

Si quieres ver mas detalles visita a mi [Ko-fi](https://ko-fi.com/shinoesp)

Vista a mi discord pueden contactarme por el servidor y tengo mas información [Discord](https://discord.gg/9MFSd3AAc4) 

## Guía de instalación

### Opcion 1
1. Descargar los archivos del repositorio junto al plugins de mod de BenInEx y XUnity que esta en el Archivo .zip llamado P5XTES.zip. y la carpeta Translate
2. Busque la ruta del juego donde lo tenga instalado hacia la carpeta \P5X\client\pc y extraen los archivos de P5XTES.zip hacia la carpeta
3. Luego la carpeta Translate lo colocan al \P5X\client\pc\BepInEx
4. Y Listo ya pueden iniciar el juego.

### Opcion 2 (sencillo)
1. Descargas los archivos en el apartado de [Release](https://github.com/Darkusze/P5XTES/releases) vX.X - P5XTES.zip
2. Busque la ruta del juego donde lo tenga instalado hacia la carpeta \P5X\client\pc y extraen los archivos de X.X.P5XTES.zip hacia la carpeta.
3. Y Iniciar el juego.

### Opcion para actualizar (Para los que ya instalaron el mod, solo se actualizra los archivo de texto del mod)
Si ya tienes instalado el mod solo necesita actualizar los archivos de Translation (traducción) de la siguiente manera:
- Desde el [Release](https://github.com/Darkusze/P5XTES/releases): Por el repositorio tendras un archivo llamado Only-Translation.x.x.zip, extrae el archivo .zip y tendras la carpeta de Translation, lo copias y pegas a la siguiente ruta P5X\client\pc\BepInEx

  
## El mod ha modificado mi nombre y tambien afecta el chat general del juego ¿que puedo hacer?
El mod usa un auto traductor como mencione en el proyecto agarra las traducciones automaticas por si hay alguna traducción  que no tenga a la mano (si se fijan el nombre de mi protagonista se llama Nagisa Kamisiro por default) por lo que el texto de algunos dialogos o nombres de los personaje (mas que todo el protagonista) no lo tomara por no tener el nombre tal cual como esta escrito en la traducción manual, que puede afectar los que tienen nombre japones o en ingles.

### Soluciones
Hay formas de solucionarlo por lo que puedo mencionar lo siguiente:
- Agreguen manualmente sus nombre en el archivo de _Substitutions.txt localizado en \P5X\client\pc\BepInEx\Translation\es\Text
<img width="707" height="269" alt="image" src="https://github.com/user-attachments/assets/ce0c7161-70b2-40f4-bb54-88804ae73fdb" />

-Y escribes tu nombre del pj como se ve en el siguiente ejemplo:

<img width="365" height="86" alt="image" src="https://github.com/user-attachments/assets/ecc5b52c-7e1c-4bda-822e-bef146602b7b" />

- Otra Solución es usar otro Endpoint para la traducción pueden usar las de Deepl o alguna de su preferencia si tienes conocimiento puedes cambiarlo a tu gusto para cambiarlo tienes que ir
<string name="text">P5X\client\pc\BepInEx\config <b></b>.</string> y busquen el archivo llamado AutoTranslatorConfig.ini, Tengo una guía subida en mi Servidor de discord

<img width="483" height="95" alt="image" src="https://github.com/user-attachments/assets/5cb5372c-aef4-4682-9fe9-245649c6cbf2" />



## Advertencia Legal

Este proyecto de traducción fan-made para *Persona 5: The Phantom X* (P5X) no está afiliado, respaldado ni autorizado por Perfect World Games, Atlus ni SEGA. Todo el contenido de este mod ha sido creado con recursos y materiales propios, utilizando herramientas como BepInEx y XUnity.AutoTranslator para aplicar traducciones en tiempo real sin modificar, reemplazar ni alterar de ninguna forma los archivos originales del juego.

Este mod cumple con los términos de servicio del juego, no modifica ni reemplaza ningun archivo original del juego. No obstante, queda a discreción de Perfect World Games, Atlus o SEGA interpretar lo contrario y tomar acciones como solicitudes de eliminación (DMCA). No me hago responsable de cualquier problema derivado del uso de este mod, incluyendo bans en cuentas o crasheos en el juego.
