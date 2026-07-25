# Documento de Diseño de Juego (GDD) — DOGCRUSH

## 1. Visión General y Fantasía
- **Título**: DOGCRUSH
- **Género**: Match-Chain Puzzle / Casual Mobile
- **Temática**: Perros y cuidado canino
- **Público Objetivo**: Jugadores casuales de todas las edades que disfrutan de juegos de puzles tipo Match-3 y aman a los perros.
- **Pilar Principal**: Sensación táctil fluida (Game Feel), colores vibrantes, encadenamientos gratificantes y respuesta visual inmediata.

---

## 2. Reglas del Tablero y Fichas

### Cuadrícula
- Dimensiones: **7 columnas x 9 filas** (63 casillas).
- Orientación: Vertical (diseñado para móviles con relación de aspecto 9:16 / 19.5:9).

### Tipos de Fichas (5 iniciales)
1. 🐶 **Perro**: Ficha principal (naranja cálido).
2. 🦴 **Hueso**: Premio canino (blanco hueso).
3. 🎾 **Pelota**: Juguete favorito (azul brillante).
4. 🍖 **Pienso**: Comida sabrosa (rojo/rojo frambuesa).
5. 🦮 **Collar**: Accesorio de paseo (verde menta).

---

## 3. Mecánica de Selección por Arrastre (Chain Matching)

1. El jugador pulsa una ficha inicial.
2. Mientras mantiene pulsado, mueve el dedo/ratón sobre fichas adyacentes del mismo tipo.
3. Se admite adyacencia horizontal, vertical y diagonal.
4. **Regla de no duplicado**: No se puede seleccionar la misma ficha dos veces dentro de una misma cadena.
5. **Retroceso / Deshacer**: Arrastrar el puntero de vuelta a la penúltima ficha deselecciona la última ficha añadida.
6. **Confirmación**:
   - Longitud < 3 fichas: La selección se cancela sin consumo.
   - Longitud >= 3 fichas: La cadena se valida, se elimina y otorga puntos.

---

## 4. Sistema de Puntuación y Combos

### Puntuación Base
- **3 fichas**: 300 pts (100 por ficha).
- **4 fichas**: 500 pts (+200 bonus).
- **5 fichas**: 900 pts (+400 bonus).
- **6 fichas**: 1,400 pts (+800 bonus).
- **7+ fichas**: 2,200+ pts.

### Multiplicadores de Combo
- **5–6 fichas**: `COMBO x2`
- **7–8 fichas**: `COMBO x3`
- **9+ fichas**: `SUPERCOMBO x4`

### Sistema de Racha Temporal
- Realizar cadenas exitosas consecutivas en menos de 4 segundos incrementa el multiplicador de racha activa.

---

## 5. Bucle de Partida (Contrarreloj)
- Duración por partida: **60 segundos**.
- Al llegar a 0s, se completa la resolución pendiente de fichas y se muestra la pantalla de **Game Over** con el resumen de puntuación y nuevo récord guardado localmente en PlayerPrefs.
