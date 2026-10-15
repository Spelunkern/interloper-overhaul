# Cambios

## 0.5.0

- Restaura los conjuntos de carga normales de Timberwolf Mountain y oculta las alternativas exclusivas de Interloper.
- Recupera la mochila de piel de alce en ambas ubicaciones posibles de Mysterious Signal Fire, conservando la selección original.
- Permite casquillos vacíos de rifle y revólver en las tablas y ubicaciones originales, sin habilitar munición utilizable ni armas globalmente.
- Habilita la elección de región inicial con la elegibilidad normal de Stalker, manteniendo los inicios en exteriores.
- Permite tres feats ya desbloqueadas para partidas nuevas de Interloper.
- Corrige la generación de recompensas de Tales evitando parches sobre getters privados cuyos argumentos optimiza el compilador nativo; el contexto de generación se restaura al salir.
- Prueba los filtros con prefabs reales de casquillos y munición, y verifica la selección nativa de región.

## 0.4.0

- Recupera las recompensas únicas de Tales excluidas en Interloper, incluida la chaqueta táctica y la funda del jefe de seguridad.
- Restaura las versiones normales de cápsulas de radio y cajas de accidentes; desactiva sus alternativas reducidas para evitar duplicados.
- Recupera el contenido excluido por dificultad en los cuatro búnkeres de Signal Void.
- Genera las recompensas de estos contenedores con los filtros y reducciones de Stalker, usando clones privados de las tablas para conservar el loot global.
- Restaura los intercambios que desbloquean los encargos de Sutherland; conserva los requisitos de confianza, entrega y progreso.
- Mantiene los filtros ajenos al equipo, la selección de investigación, cerraduras y progreso de misiones. No regenera contenedores ya saqueados.

## 0.3.0

- Arranque general directo al menú principal: omitir logos, avisos iniciales y video de introducción.
- Continuación automática al terminar la carga, sin esperar una tecla.
- Mantener el flujo de carga de perfiles, archivos y errores del juego.
- Validación automática del menú visible sin enviar entradas de teclado o ratón.
- Lanzador Linux con ReadyToRun desactivado para evitar el fallo de arranque del CLR 6 observado en las pruebas.

## 0.2.0

- Nombre definitivo: Interloper Overhaul.
- Casas originales intactas en Coastal Highway para Interloper, con recarga de selecciones guardadas.
- Restauración selectiva de contenedores, mementos, escondites y objetos del mapa omitidos por la dificultad.
- Distribución normal de búnkeres en mundos nuevos, conservando las asignaciones existentes al cargar.
- Tablas y parámetros de loot sin rebalanceo.
- Pruebas de políticas y validación dentro del juego, separadas del build de distribución.

## 0.1.0

- Base inicial de C#, MelonLoader, scripts para Linux y repositorio público.
