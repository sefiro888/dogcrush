# DOGCRUSH — Plan maestro de trabajo

Última actualización: 27/07/2026

Este documento es la hoja de ruta oficial de DOGCRUSH. La versión publicada actual queda protegida como nueva línea base: todo el trabajo futuro debe avanzar desde ella, sin recuperar composiciones o recursos visuales anteriores.

## Línea base protegida

- Estado: jugable, publicado y visualmente aprobado como punto de continuación.
- Rama oficial: `main`.
- Commit base: `18dfb052cbfceb29ebb57257e0c91f13d6de2eb1`.
- Etiqueta de restauración: `visual-base-aprobada-2026-07-27`.
- Juego público: `https://sefiro888.github.io/dogcrush/`.
- Repositorio público: `https://github.com/sefiro888/dogcrush`.
- Motor: Unity `6000.5.5f1`.
- Escena oficial: `Assets/_DogCrush/Scenes/Gameplay.unity`.
- Referencia artística: `ejemplo/resultadodeseado.png`.
- Orientación prioritaria: móvil vertical.

### Regla de protección

Antes de comenzar cada bloque importante se creará una rama `codex/...`. Solo se integrará en `main` después de superar las pruebas y una revisión visual móvil. No se regenerará la escena completa ni se recuperarán generadores o recursos heredados que puedan sobrescribir la composición aprobada.

## Estado funcional confirmado

- [x] Tablero cuadrado `8x8` con 64 fichas.
- [x] Cinco tipos de ficha visibles e interactivos.
- [x] Selección por arrastre y multiplicador vivo.
- [x] Puntuación, eliminación, gravedad y reposición.
- [x] Temporizador, nivel, récord y reinicio.
- [x] Reinicio sin conservar fichas de la partida anterior.
- [x] Fondo vertical, marco, logo y controles inferiores integrados.
- [x] Tres pruebas PlayMode aprobadas.
- [x] Compilación WebGL con cero errores.
- [x] Validación manual en resolución móvil `390x844`.
- [x] Publicación mediante GitHub Pages.

## Fase 0 — Seguridad y continuidad

- [x] Fijar la versión actual como línea base protegida.
- [x] Mantener `main` como rama estable y publicada.
- [x] Crear y verificar un repositorio privado de seguridad.
- [x] Mantener una etiqueta de restauración para cada versión aprobada.
- [ ] Documentar al cerrar cada sesión el último estado validado y el siguiente paso.
- [ ] No incluir adjuntos temporales, cachés de Unity ni cambios ajenos al bloque activo.

**Criterio de salida:** el proyecto puede restaurarse aunque una rama de trabajo o el repositorio local resulte dañado.

## Fase 1 — Base jugable estable

- [x] Reparar TextMeshPro y los textos principales.
- [x] Eliminar las excepciones bloqueantes de arranque.
- [x] Restaurar el tablero completo e interactivo.
- [x] Validar selección, puntuación, eliminación, caída y reposición.
- [x] Validar temporizador, récord y reinicio.
- [x] Añadir pruebas PlayMode de llenado, reinicio y ciclo completo.
- [x] Conseguir una compilación WebGL sin errores.

**Resultado:** fase terminada. La lógica jugable queda congelada salvo correcciones demostrables.

## Fase 2 — Composición móvil definitiva

- [ ] Definir una composición maestra vertical basada en `resultadodeseado.png`.
- [x] Separar claramente fondo, cabecera, logo, tablero, fichas y barra inferior.
- [ ] Ajustar jerarquía y separación entre HUD, logo y tablero.
- [x] Mantener el tablero cuadrado sin deformación en todas las resoluciones.
- [ ] Aplicar zonas seguras superiores e inferiores.
- [x] Evitar fondos blancos, huecos accidentales y recortes del marco.
- [x] Validar `360x800`, `390x844` y `430x932`.
- [ ] Mantener una composición de escritorio razonable sin perjudicar el móvil.

**Criterio de salida:** la pantalla completa se ve equilibrada y sin deformaciones en los tres tamaños móviles.

## Fase 3 — Tablero definitivo

- [x] Convertir el tablero en un componente independiente y predecible.
- [x] Separar marco de madera, fondo interior y cuadrícula.
- [ ] Ajustar el marco al aspecto cálido, compacto y redondeado de la referencia.
- [x] Hacer visibles las 64 casillas sin competir con las fichas.
- [x] Normalizar márgenes interiores y separación entre fichas.
- [x] Impedir que una sustitución del marco altere posiciones o tamaño de las fichas.

**Criterio de salida:** el tablero puede cambiar de arte sin romper la cuadrícula ni la interacción.

## Fase 4 — Familia visual de fichas

- [ ] Preparar una hoja comparativa con las cinco fichas dentro de una casilla real.
- [ ] Igualar escala visual, márgenes transparentes, iluminación y volumen.
- [ ] Mejorar el hueso: grueso, legible y ligeramente diagonal.
- [ ] Convertir el cachorro en una ficha más compacta y caricaturesca.
- [ ] Mejorar la bola/huella para que se lea claramente como pieza.
- [ ] Reducir el peso visual de collar y comedero.
- [ ] Verificar recorte, transparencia y área táctil antes de integrarlas.
- [ ] Sustituir las fichas como un único conjunto, no individualmente.

**Criterio de salida:** las cinco fichas parecen pertenecer al mismo juego y tienen una presencia equivalente.

## Fase 5 — HUD y marcadores

- [ ] Sustituir la barra superior provisional por módulos visuales independientes.
- [ ] Diseñar placa azul de nivel.
- [ ] Diseñar puntuación con estrella y formato legible.
- [ ] Diseñar temporizador verde con icono y estados de aviso.
- [ ] Diseñar vidas con corazón.
- [ ] Integrar correctamente puntuación o movimientos en la zona inferior.
- [ ] Mantener cifras legibles en pantallas pequeñas.
- [ ] Colocar el multiplicador vivo junto a la selección sin tapar fichas.

**Criterio de salida:** nivel, puntuación, tiempo, vidas y multiplicador se entienden de un vistazo y mantienen el estilo de la referencia.

## Fase 6 — Sensación de juego

- [ ] Añadir contorno o brillo controlado durante la selección.
- [ ] Añadir línea suave entre las fichas seleccionadas.
- [ ] Aplicar un pequeño aumento de escala a la cadena activa.
- [ ] Animar el multiplicador vivo.
- [ ] Añadir desaparición, destello y partículas al completar una cadena.
- [ ] Añadir caída con rebote ligero.
- [ ] Incorporar sonidos coherentes y control de volumen.
- [ ] Añadir vibración opcional en dispositivos compatibles.
- [ ] Mantener estabilidad y claridad con cadenas largas.

**Criterio de salida:** seleccionar y eliminar fichas transmite respuesta inmediata sin saturar la pantalla.

## Fase 7 — Contenido y progresión

- [ ] Decidir el sistema definitivo: tiempo, movimientos o combinación de ambos.
- [ ] Diseñar objetivos de nivel.
- [ ] Incorporar estrellas, vidas y progresión solamente cuando el HUD esté consolidado.
- [ ] Definir potenciadores y su comportamiento antes de producir más ilustraciones.
- [ ] Añadir niveles y dificultad progresiva.
- [ ] Diseñar pantallas de inicio, pausa, victoria y derrota.

## Fase 8 — Calidad y publicación

- [ ] Ejecutar pruebas PlayMode después de cada integración.
- [ ] Mantener cero errores relevantes en Unity y navegador.
- [ ] Revisar rendimiento y memoria en móvil.
- [ ] Optimizar el tamaño de la descarga WebGL.
- [ ] Comprobar carga limpia, reinicio y varias partidas consecutivas.
- [ ] Validar táctilmente en al menos un Android real.
- [ ] Publicar únicamente desde una rama revisada.
- [ ] Actualizar el repositorio de seguridad después de cada versión aprobada.

## Próximo objetivo inmediato

Trabajar únicamente en **Fase 2 — Composición móvil definitiva**, conservando la jugabilidad y las fichas actuales. Cuando las tres resoluciones objetivo estén aprobadas, se cerrará esa fase antes de modificar el tablero o volver a generar ilustraciones.

**En revisión local:** la rama `codex/tablero-adaptable` sustituye la imagen rígida por un marco, panel y cuadrícula generados según las dimensiones del nivel. Se ha verificado en 8×8 y 7×9, pero no se publicará hasta recibir aprobación visual.

## Método de trabajo

1. Crear una rama para un solo bloque claramente delimitado.
2. Realizar cambios pequeños y comprobables.
3. Compilar y ejecutar las pruebas.
4. Revisar visualmente en las tres resoluciones móviles.
5. Mostrar el resultado antes de integrarlo.
6. Fusionar en `main`, publicar y crear una nueva etiqueta estable.
7. Replicar la versión aprobada en el repositorio de seguridad.
