# Interloper Overhaul

Mod para **The Long Dark** centrado en cambios de la dificultad **Interloper**,
con espacio para modificaciones generales en futuras versiones. Autor: Spelunkern.

## Versión 0.4.0 — Recompensas de Tales

En Interloper estándar se recuperan las recompensas únicas excluidas por dificultad,
incluidas la **chaqueta táctica** y la **funda del jefe de seguridad**. Los guantes
tácticos y el cinturón del capataz mantienen sus ubicaciones y requisitos originales.

- **Cápsulas de radio y cajas de accidentes:** se habilitan las variantes normales
  creadas por Hinterland y se deshabilitan las variantes reducidas de Interloper.
  Se conserva la selección original por investigación, transmisores y señal.
- **Búnkeres Alpha, Beta, Gamma y Omega:** se recupera el contenido excluido por
  dificultad, incluida la recompensa de Signal Void. Las misiones, puertas,
  documentos y la sala final conservan sus requisitos.
- **Contenido de estos contenedores:** usa los filtros y reducciones de Stalker
  durante su generación. Las tablas se clonan para que sus filtros almacenados
  no alteren contenedores del resto del mundo. No se inventan tablas ni cantidades.
- **Sutherland:** los cinco encargos y sus intercambios desbloqueados admiten sus
  recompensas normales, incluidas las flechas, el cuchillo y el rifle de búnker.
  Se mantienen la confianza, costes, entregas y desbloqueos originales. El catálogo
  común del comerciante sigue con las restricciones de Interloper.

Estos cambios no regeneran recompensas ya recogidas ni rellenan contenedores
inspeccionados. Para comprobar toda la generación se recomienda una partida nueva.

## Arranque directo

Al iniciar el juego se omiten logos, avisos iniciales, la espera de una tecla y el
video de introducción del menú. El menú principal aparece automáticamente cuando
termina la carga de perfiles y archivos. Este cambio es general, antes de elegir
una dificultad. Las cinemáticas durante la partida conservan su reproducción normal.
El script de lanzamiento también omite la pantalla inicial propia de MelonLoader.
En Linux desactiva ReadyToRun para el runtime .NET 6, configuración usada durante
la validación tras observar fallos intermitentes del cargador antes de cargar mods.
Se puede probar la configuración original con `DOTNET_ReadyToRun=1`.

## Restauración del mundo

Los cambios de restauración se aplican únicamente a Interloper estándar:

- **Coastal Highway:** todas las variantes de casas de los cinco grupos de edificios
  usan las estructuras originales intactas, con sus accesos al interior. Se desactiva
  el grupo especial de edificios quemados de Interloper y se recuperan las dos casas
  independientes que la dificultad oculta.
- **Contenedores y coleccionables del mundo:** se eliminan los filtros de Interloper
  sobre contenedores, mementos, notas de visera, grupos de escondites y objetos de mapa
  elegibles. En grupos aleatorios compuestos íntegramente por este contenido se usa
  como mínimo la cantidad del nivel medio de recursos.
- **Búnkeres de preparacionistas:** se usa la distribución normal de Stalker:
  **tres interiores amueblados y seis abandonados**, con ubicaciones elegidas por
  el distribuidor original del juego. No se convierten los nueve en búnkeres llenos.

Fuera de las recompensas de Tales y sus cápsulas, no se ajustan las tablas de loot,
sus probabilidades, la condición ni las cantidades por contenedor. Restaurar una
casa o un interior de preparacionista habilita el contenido original asociado a ese
lugar, sujeto a los filtros normales de Interloper. No se habilitan globalmente armas,
herramientas, plantas ni fauna.
Pilgrim, Voyageur, Stalker, Misery, desafíos, historia y Custom mantienen sus reglas.

### Partidas existentes

Se recomienda **una partida nueva** para obtener la generación completa del mundo.
Las selecciones de casas quemadas se redirigen a sus variantes intactas al cargar
Coastal Highway. No se regenera loot de escenas visitadas ni se limpian datos guardados.
Los búnkeres ya asignados en partidas existentes se conservan, incluidos los interiores
exclusivos de Interloper; la distribución normal se aplica al generar un mundo nuevo.
Las variantes nuevas de búnker también se reconocen al guardar y recargar.

## Preparación y uso

Entorno verificado: The Long Dark **2.55**, Unity **6000.0.60f1**, Linux x64,
MelonLoader **0.7.3**, runtime .NET **6.0.36**, SDK .NET **10.0.x**.

Herramientas: `git`, SDK .NET 10, `bash`, `curl`, `unzip`, `sha256sum`, `pgrep`.
Respaldar `~/.local/share/Hinterland/TheLongDark` antes de jugar con mods.

```bash
./scripts/setup.sh
./scripts/launch.sh  # Primera ejecución: esperar la generación de ensamblados y cerrar.
./scripts/deploy.sh
./scripts/launch.sh
```

El runtime privado se instala en `~/.local/share/tld-modding/dotnet`.
`deploy.sh` exige cerrar el juego, compila Release e instala `Mods/InterloperOverhaul.dll`.
Si se conserva el scaffold inicial, retirar `Mods/TheLongDarkMod.dll` para cargar solo
el mod nuevo. Para compilar sin instalar: `./scripts/build.sh`.

Variables opcionales: `TLD_GAME_DIR` para otra ubicación del juego,
`TLD_DOTNET_ROOT` para otro runtime. `build.sh` también acepta `-p:GameDir=/ruta/al/juego`.
Los archivos del juego, las partidas y las referencias generadas quedan fuera del repo.

### Steam

En Propiedades → General → Opciones de lanzamiento, usar la ruta absoluta del script
entre comillas, seguida de `%command%`. Con el checkout local actual:

```text
"/home/franco/Projects/the-long-dark-mod/scripts/launch.sh" %command%
```

Quitar esa opción permite iniciar sin MelonLoader. Para retirar solo este mod,
borrar `Mods/InterloperOverhaul.dll` con el juego cerrado.

## Desarrollo y validación

- Código: `src/InterloperOverhaul/`.
- Política de selección del mundo: `shared/WorldPolicy.cs`.
- Referencias: `<juego>/MelonLoader/Il2CppAssemblies/`.
- Log: `<juego>/MelonLoader/Latest.log`.

```bash
./scripts/build.sh -p:RestoreLockedMode=true
dotnet run --project tests/PolicyChecks/PolicyChecks.csproj -c Release
./scripts/validate-runtime.sh
```

La validación de runtime requiere el juego cerrado y una DLL Release ya desplegada.
Compila temporalmente una versión de prueba, abre el menú, comprueba el arranque automático del menú y los parches reales
sobre objetos de prueba y clones de la distribución de búnkeres, y cierra el juego.
No abre partidas. Restaura la DLL instalada al terminar y deja el informe en
`.local/runtime-validation.log`. El código de validación no se incluye en Release normal.

GitHub Actions verifica las políticas y scripts sin distribuir DLL propietarias.
La compilación completa y las pruebas con MelonLoader se ejecutan localmente, usando
una instalación legítima del juego. La DLL del mod se genera en
`src/InterloperOverhaul/bin/Release/net6.0/InterloperOverhaul.dll`.

## Fuentes de compatibilidad

- [Interloper: restricciones y recompensas](https://thelongdark.fandom.com/wiki/Interloper)
- La implementación se contrasta con los componentes y escenas de la instalación local 2.55.


- [MelonLoader 0.7.3](https://github.com/LavaGang/MelonLoader/releases/tag/v0.7.3)
- [Arranque nativo en Linux](https://github.com/LavaGang/MelonWiki/blob/master/docs/gettingstarted.md#linux-native-games)
- [RerollBurntBuildings, referencia de los grupos de Coastal Highway](https://github.com/GruffCassquatch/RerollBurntBuildings)

Proyecto comunitario independiente, sin afiliación con Hinterland.
