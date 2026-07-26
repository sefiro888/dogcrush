# DOGCRUSH — Plan de acción

Este documento es la hoja de ruta de referencia del proyecto. Se actualizará al cerrar cada fase para poder continuar el trabajo en futuras conversaciones sin perder el contexto.

## Estado de partida

- Rama base auditada: `main`
- Commit base: `ba9c611` (`Deploy-verified-HD-park-and-3D-wooden-board-build`)
- Rama de trabajo: `codex/fase1-estabilizacion`
- Motor: Unity `6000.5.5f1`
- Escena actual: `Assets/_DogCrush/Scenes/Gameplay.unity`
- Configuración actual: tablero `7x9`, 5 tipos, 60 segundos
- Referencia visual: `ejemplo/resultadodeseado.png`
- Referencia adicional guardada: `Assets/_DogCrush/Art/References/dogcrush-reference-2026-07-26.png`

## Diagnóstico confirmado

- WebGL ya muestra las 63 fichas y el HUD. Se fijó el escalado lineal para WebGL; en una sesión de navegador todavía puede aparecer una advertencia FSR heredada de la caché o del pipeline de URP, pero no bloquea la partida.
- El arranque produce `ArgumentNullException` y una advertencia de shader FSR.
- TextMeshPro no tiene una fuente esencial claramente incluida en `Assets`.
- El HUD se destruye y se reconstruye dinámicamente durante `Awake`.
- Las fichas están duplicadas entre `Art/Pieces` y `Resources/Pieces` con importaciones distintas.
- Solo existen pruebas EditMode; todavía no hay pruebas PlayMode.
- Hay dos escenas habilitadas en Build Settings, aunque la compilación WebGL fuerza `Gameplay.unity`.

## Fase 0 — Protección y control del trabajo

- [x] Auditar el estado del repositorio.
- [x] Confirmar que local y `origin/main` coincidían.
- [x] Crear la rama `codex/fase1-estabilizacion`.
- [x] Crear el primer commit de esta hoja de ruta.
- [ ] No publicar en `main` hasta validar la fase.

## Fase 1 — Recuperar una base jugable estable

- [x] Reparar y fijar TextMeshPro con una fuente incluida.
- [x] Corregir textos mal codificados.
- [ ] Obtener stack traces de desarrollo y localizar el origen exacto de `ArgumentNullException`.
- [x] Corregir los rectángulos y el PPU de las cinco fichas usadas por la escena.
- [ ] Unificar la fuente de las cinco fichas y eliminar referencias ambiguas.
- [x] Conseguir 63 fichas visibles, con sprite válido y collider activo.
- [ ] Validar selección por arrastre y cancelación.
- [ ] Validar eliminación, gravedad y reposición.
- [ ] Validar puntuación, combos y temporizador.
- [x] Añadir una prueba PlayMode de llenado del tablero.
- [x] Conseguir WebGL sin excepciones de consola.

**Criterio de salida:** una partida completa puede iniciarse, jugarse y reiniciarse, mostrando 63 fichas interactivas y HUD legible.

**Nota de progreso:** Unity EditMode (5/5) y PlayMode (1/1) pasan. La primera compilación WebGL se completó sin errores; la recompilación limpia después de corregir las texturas quedó detenida durante el postprocesado prolongado de shaders y aún debe repetirse de forma controlada.

La prueba PlayMode también verifica ahora que cada sprite tiene geometría superior a 0,5 unidades, además de collider e interacción.

Actualización 26/07/2026: se reparó el reinicio para reciclar las fichas anteriores y se añadió una prueba PlayMode específica. URP WebGL quedó fijado a escalado lineal. La compilación WebGL resultante terminó correctamente (`Build Finished, Result: Success`; auditoría: `errors: 0`) y se verificó visualmente el tablero completo en navegador. La advertencia FSR se ha vuelto a comprobar y queda anotada como limpieza técnica no bloqueante para una sesión posterior.

## Fase 2 — Escena y arquitectura limpias

- [ ] Crear una escena de trabajo limpia sin destruir UI en tiempo de ejecución.
- [ ] Mantener una única escena oficial de juego.
- [ ] Separar prefabs, lógica, presentación y herramientas del editor.
- [ ] Hacer que el generador no sobrescriba escenas sin confirmación.
- [ ] Usar `BoardConfig` como única fuente de dimensiones, tiempo y reglas.
- [ ] Ampliar las pruebas de integración.

## Fase 3 — Composición visual de DOGCRUSH

- [ ] Usar `resultadodeseado.png` como guía de composición.
- [x] Guardar fondo vertical aprobado: `Assets/_DogCrush/Art/Backgrounds/dogcrush-park-background-v1.png`.
- [x] Guardar marco de tablero limpio con transparencia: `Assets/_DogCrush/Art/UI/dogcrush-board-frame-v1.png`.
- [ ] Separar fondo, tablero, marco transparente, logo y HUD.
- [ ] Resolver los formatos cuadrado, horizontal y vertical.
- [ ] Definir un sistema de escalado y zonas seguras.

## Fase 4 — Fichas y sensación de juego

- [ ] Normalizar tamaño, iluminación y escala de las fichas.
- [ ] Añadir selección, brillo, caída elástica y desaparición.
- [ ] Añadir partículas, línea de cadena, sonidos y vibración.

## Fase 5 — Mecánicas de la referencia

Primero se conserva el modo actual de conectar cadenas. Después se decidirá si se implementan realmente tablero `8x8`, movimientos, vidas, niveles y potenciadores.

## Fase 6 — Calidad y publicación

- [ ] Pruebas en móvil, horizontal y vertical.
- [ ] Cero errores y advertencias relevantes en consola.
- [ ] Optimización del tamaño WebGL.
- [ ] Publicación desde rama validada y posterior integración en `main`.

## Próximo objetivo inmediato

Devolver una versión interna con 63 fichas visibles, interacción, eliminación, caída, reposición, puntuación, temporizador y reinicio. No se empezará el acabado artístico hasta superar este criterio.
