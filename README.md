# The Long Dark Mod

Base de desarrollo de un mod para **The Long Dark**, por Spelunkern.
Nombre provisional: definiremos el nombre y las funciones con el diseño del mod.
La versión 0.1.0 solo registra su carga y las escenas inicializadas.

## Entorno

- The Long Dark, versión nativa de Linux x64, instalado desde Steam.
- MelonLoader **0.7.3**, con correcciones para Linux nativo y Unity 6.
- Runtime .NET **6.0.36** privado para el cargador; SDK **10.0.x** para compilar a `net6.0`.
- C# y Harmony, con referencias locales a los ensamblados de MelonLoader y del juego.

Los binarios del juego, partidas y ensamblados generados permanecen fuera del repositorio.
El SDK existente del sistema se conserva; el runtime se instala en
`~/.local/share/tld-modding/dotnet`.

## Preparar y compilar

Requisitos de herramientas: `git`, SDK .NET 10, `bash`, `curl`, `unzip`, `sha256sum` y `pgrep`.
Antes de instalar mods, respaldar `~/.local/share/Hinterland/TheLongDark`.

```bash
./scripts/setup.sh
./scripts/deploy.sh
./scripts/launch.sh
```

`deploy.sh` compila en Release y copia únicamente nuestra DLL a `Mods/`.
Exige cerrar el juego antes de desplegar. Para compilar sin instalar:

```bash
./scripts/build.sh
```

La primera ejecución de MelonLoader descarga dependencias y genera
`MelonLoader/Il2CppAssemblies`. Esperar a que termine antes de cerrar el juego.
Estos ensamblados quedan disponibles para implementar parches sobre clases del juego.

Para otra ubicación, exportar `TLD_GAME_DIR` o pasar `-p:GameDir=/ruta/al/juego`
a `build.sh`. El runtime privado puede indicarse con `TLD_DOTNET_ROOT`.

## Arrancar desde Steam

En Propiedades → General → Opciones de lanzamiento, poner la ruta absoluta de
`scripts/launch.sh` entre comillas, seguida de `%command%`. En esta máquina:

```text
"/home/franco/Projects/the-long-dark-mod/scripts/launch.sh" %command%
```

El script configura la precarga de MelonLoader y el runtime .NET antes de ejecutar
el comando de Steam. También permite iniciar el ejecutable directamente.

## Desarrollo y diagnóstico

Entrada del mod: `src/TheLongDarkMod/Mod.cs`.
Proyecto: `src/TheLongDarkMod/TheLongDarkMod.csproj`.
Logs del cargador: `<juego>/MelonLoader/Latest.log` y `<juego>/MelonLoader/Logs/`.
Buscar `Development scaffold loaded successfully` para confirmar la carga.

GitHub Actions compila la base y publica la DLL como artefacto. No dispone de las
DLL del juego: al añadir funciones que las utilicen habrá que adaptar la estrategia
de CI sin publicar archivos propietarios.

Para iniciar sin mods desde Steam, quitar la opción de lanzamiento. Para retirar
solo este mod, borrar `Mods/TheLongDarkMod.dll` con el juego cerrado.

## Referencias

- [MelonLoader 0.7.3](https://github.com/LavaGang/MelonLoader/releases/tag/v0.7.3)
- [Instalación y arranque en Linux](https://github.com/LavaGang/MelonWiki/blob/master/docs/gettingstarted.md#linux-native-games)
- [Desarrollo de mods](https://github.com/LavaGang/MelonWiki/blob/master/docs/modders/quickstart.md)
- [Lista de mods de TLD](https://tldmods.net/)

Proyecto comunitario independiente, sin afiliación con Hinterland.
