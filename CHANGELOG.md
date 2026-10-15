# Cambios

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
