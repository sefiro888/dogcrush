# DOGCRUSH — Plan nuevo de juego

## Objetivo general

Transformar DOGCRUSH en un juego móvil vertical tipo match-3 basado en intercambio de fichas vecinas, manteniendo la identidad visual de perros y parque, y aprovechando la base actual de tablero, niveles, potenciadores, vidas y publicación WebGL.

La nueva interacción será:

1. El jugador pulsa una ficha.
2. Desliza hacia una ficha vecina horizontal o vertical.
3. Las fichas intercambian sus posiciones.
4. Si se forma una combinación de tres o más fichas iguales, se eliminan.
5. Si no se forma combinación, las fichas vuelven a su posición.

La prioridad será que jugar en móvil resulte cómodo, claro y fácil de aprender.

## Estado de partida

La versión anterior está protegida en:

- Repositorio: `dogcrush-backup`
- Rama: `juegoantesdelcambiodesistema`
- Etiqueta: `juegoantesdelcambiodesistema`
- Commit de referencia: `c1499a4`

La rama anterior no debe modificarse. El nuevo sistema se desarrollará en una rama separada, por ejemplo:

`codex/intercambio-match3`

La beta pública seguirá separada en `dogcrush-beta` y solo recibirá compilaciones cuando el lote esté revisado.

## Reglas de trabajo

- No modificar la copia de seguridad.
- Trabajar en lotes de cambios relacionados.
- No publicar cada cambio individual.
- Crear commits claros después de cada fase estable.
- Realizar solo comprobaciones técnicas automáticas durante el desarrollo.
- El usuario hará las comprobaciones visuales y jugables en el móvil.
- No reemplazar la versión anterior hasta aceptar la nueva mecánica.
- Mantener una compilación WebGL funcional al cerrar cada bloque importante.

## Uso recomendado de modelos

### Sol

Usarlo para decisiones estructurales y cambios de alto riesgo:

- Diseño de la nueva mecánica de intercambio.
- Reorganización de la lógica de combinaciones.
- Migración de entrada táctil.
- Integración con gravedad, objetivos y potenciadores.
- Diagnóstico de errores complejos.
- Decisiones de arquitectura.

### Terra

Usarlo como modelo principal para la mayor parte del proyecto:

- Implementación de fases ya diseñadas.
- Ajustes de niveles.
- Correcciones de interfaz.
- Mejoras de rendimiento.
- Revisión de código.
- Preparación de compilaciones.
- Mantenimiento posterior.

### Luna

Usarlo para tareas sencillas y de bajo riesgo:

- Consultas sobre el proyecto.
- Cambios pequeños de texto o colores.
- Documentación.
- Revisiones rápidas.
- Preparación de listas de comprobación.

## Fase 0 — Preparación y nueva rama

**Modelo recomendado:** Terra.

### Trabajo

- Crear la rama `codex/intercambio-match3` desde el estado actual.
- Confirmar que la copia de seguridad permanece intacta.
- Registrar la versión inicial del nuevo sistema.
- Separar claramente código de juego, compilación y recursos generados.

### Resultado esperado

Podemos experimentar con el intercambio sin poner en peligro la versión anterior.

## Fase 1 — Nuevo sistema de entrada móvil

**Modelo recomendado:** Sol.

### Trabajo

- Sustituir la selección de cadenas por selección de una ficha y una dirección.
- Detectar únicamente vecinos horizontales y verticales.
- Interpretar el gesto corto como intercambio de celdas.
- Evitar movimientos diagonales.
- Ignorar gestos que salgan fuera del tablero.
- Mantener una zona táctil cómoda alrededor de cada celda.
- Hacer que el sistema funcione igual con ratón y pantalla táctil.

### Criterios de terminado

- Una ficha solo puede intercambiarse con una vecina.
- El dedo no tapa una cadena completa.
- Los movimientos diagonales no producen intercambios.
- No hay selecciones erróneas entre celdas cercanas.

## Fase 2 — Detección de combinaciones match-3

**Modelo recomendado:** Sol.

### Trabajo

- Detectar líneas horizontales de tres o más.
- Detectar líneas verticales de tres o más.
- Detectar combinaciones simultáneas.
- Unificar combinaciones que se cruzan.
- Cancelar y devolver el intercambio si no se crea ninguna combinación.
- Eliminar todas las fichas válidas en una misma resolución.

### Criterios de terminado

- Tres fichas iguales en horizontal se eliminan.
- Tres fichas iguales en vertical se eliminan.
- Un intercambio sin combinación vuelve atrás.
- Las combinaciones cruzadas no generan eliminaciones duplicadas.

## Fase 3 — Gravedad, reposición y cascadas

**Modelo recomendado:** Terra.

### Trabajo

- Reutilizar la caída actual adaptándola al resultado del intercambio.
- Reponer las fichas eliminadas.
- Detectar cascadas automáticas.
- Resolver varias explosiones consecutivas.
- Bloquear la entrada mientras se resuelve una cascada.
- Garantizar que no aparecen huecos ni fichas antiguas.
- Recalcular movimientos posibles después de cada cascada.

### Criterios de terminado

- Las fichas caen correctamente.
- Las nuevas fichas ocupan todas las celdas válidas.
- Las cascadas se resuelven sin intervención del jugador.
- El jugador puede volver a mover cuando termina la animación.

## Fase 4 — Puntuación, combos y feedback

**Modelo recomendado:** Terra.

### Trabajo

- Puntuar cada combinación eliminada.
- Crear bonus para combinaciones de cuatro y cinco.
- Crear multiplicadores por cascadas.
- Mostrar puntuación flotante.
- Adaptar el contador de cadena actual al nuevo sistema.
- Retirar la línea visual de arrastre si ya no es necesaria.
- Añadir feedback claro al intercambio válido o inválido.

### Ideas de puntuación

- Tres fichas: puntuación base.
- Cuatro fichas: pieza especial de línea.
- Cinco fichas: comodín o pieza explosiva.
- Cascada: multiplicador creciente.
- Objetivo completado: bonus adicional.

## Fase 5 — Potenciadores adaptados al match-3

**Modelo recomendado:** Terra.

### Trabajo

- Pata: barajar o regenerar el tablero.
- Bolsa: añadir tiempo.
- Hueso: limpiar una fila o columna.
- Adaptar los potenciadores a las nuevas piezas especiales.
- Mostrar cantidades disponibles.
- Descontar cantidades al usar un potenciador.
- Impedir su uso durante animaciones.
- Permitir configurarlos por nivel.

### Futuras ideas

- Bomba de área.
- Rayo horizontal.
- Rayo vertical.
- Congelar el temporizador.
- Comodín de cualquier tipo.

## Fase 6 — Objetivos y niveles

**Modelo recomendado:** Terra.

### Trabajo

- Mantener objetivos de puntuación.
- Añadir objetivos de eliminar tipos concretos.
- Añadir objetivos de realizar combinaciones especiales.
- Añadir objetivos de cascadas.
- Configurar tiempo y dificultad por nivel.
- Mantener los niveles como datos independientes.
- Guardar niveles desbloqueados y estrellas.

### Tipos de tablero

- Tablero completo.
- Tablero diamante.
- Tablero con esquinas bloqueadas.
- Tablero con huecos centrales.
- Tablero con zonas bloqueadas.

Las formas nuevas se añadirán solo cuando la gravedad y la reposición sean estables.

## Fase 7 — Obstáculos y dificultad progresiva

**Modelo recomendado:** Sol para diseñar; Terra para implementar.

### Trabajo

- Celdas bloqueadas.
- Hielo o barro con varios impactos.
- Cajas que deben romperse.
- Objetivos combinados.
- Menos movimientos en niveles avanzados.
- Nuevas reglas cada cinco niveles.

### Curva propuesta

- Niveles 1–5: aprendizaje del intercambio.
- Niveles 6–10: objetivos variados.
- Niveles 11–20: formas y obstáculos sencillos.
- Niveles 21–30: combinaciones especiales.
- Niveles 31–50: objetivos combinados y dificultad avanzada.

## Fase 8 — Vidas, progreso y rejugabilidad

**Modelo recomendado:** Terra.

### Trabajo

- Mantener pérdida de vida al fallar.
- Recuperación de vidas.
- Guardado entre sesiones.
- Recompensas por superar niveles.
- Estrellas por rendimiento.
- Bonus por terminar con tiempo restante.
- Desafíos opcionales y objetivos secundarios.

## Fase 9 — Interfaz móvil y presentación

**Modelo recomendado:** Terra.

### Trabajo

- Mantener el logo reducido y el tablero protagonista.
- Ajustar el HUD para el nuevo sistema.
- Mostrar claramente el intercambio seleccionado.
- Mejorar animaciones de intercambio.
- Añadir animaciones de combinación y cascada.
- Ajustar botones para uso con una mano.
- Revisar pantallas 360×800 y móviles más altos.

## Fase 10 — Menús y experiencia completa

**Modelo recomendado:** Terra.

### Trabajo

- Menú inicial.
- Selector de niveles.
- Tutorial del intercambio.
- Pantalla de victoria.
- Pantalla de derrota.
- Pantalla de pausa.
- Ajustes de sonido y vibración.
- Pantalla final de campaña.

## Fase 11 — Pruebas manuales del usuario

**Modelo recomendado:** Luna para preparar listas; Terra para corregir.

### El usuario comprobará

- Facilidad de intercambio.
- Movimientos inválidos.
- Combinaciones horizontales y verticales.
- Cascadas.
- Potenciadores.
- Objetivos.
- Dificultad y tiempo.
- Legibilidad del HUD.
- Tamaño de fichas y tablero.
- Funcionamiento en móvil real.

Cada fallo se registrará con nivel, acción realizada, resultado esperado y resultado obtenido.

## Fase 12 — Publicación progresiva

**Modelo recomendado:** Terra.

### Trabajo

- Compilar WebGL.
- Ejecutar auditoría técnica.
- Crear commit de la fase.
- Actualizar la rama de respaldo.
- Publicar una beta solo cuando haya varios cambios agrupados.
- Probar la beta en móvil.
- Corregir los fallos encontrados.
- Repetir hasta tener una versión estable.

## Fase 13 — Lanzamiento

**Modelo recomendado:** Sol para la decisión final; Terra para preparar el lanzamiento.

### Trabajo

- Confirmar que el sistema de intercambio es mejor que la versión anterior.
- Mantener disponible la copia `juegoantesdelcambiodesistema`.
- Fusionar la rama nueva cuando esté aprobada.
- Publicar la versión estable.
- Crear etiqueta de versión.
- Preparar enlace público.
- Documentar cómo restaurar cada versión.

## Decisión recomendada

No se debe cambiar todo de una vez. El orden correcto es:

1. Entrada de intercambio.
2. Detección de combinaciones.
3. Caída y cascadas.
4. Puntuación.
5. Potenciadores.
6. Objetivos y niveles.
7. Obstáculos.
8. Pulido y publicación.

La copia anterior permanece protegida durante todo el proceso. Si el intercambio no ofrece una experiencia mejor, podremos volver al sistema de cadenas sin perder el trabajo anterior.
