# DOGCRUSH 🐶✨

**DOGCRUSH** es un juego móvil 2D de puzles de conectar cadenas de fichas caninas por arrastre, creado con Unity 6.5.

![DOGCRUSH Banner Placeholder](Assets/_DogCrush/Art/Backgrounds/board_bg.png)

## 📌 Estado del Proyecto

- **Versión actual**: `0.1.0-prototype`
- **Motor**: Unity 6.5 (`6000.5.5f1`)
- **Render Pipeline**: Universal Render Pipeline (URP 2D)
- **Orientación**: Vertical (Portrait 1080 x 1920)
- **Licencia**: Licencia no definida aún (todos los derechos reservados).

---

## 🎮 Mecánica de Juego

1. Un tablero de **7 columnas x 9 filas** se llena con 5 tipos de fichas caninas:
   - 🐶 **Perro**
   - 🦴 **Hueso**
   - 🎾 **Pelota**
   - 🍖 **Pienso**
   - 🦮 **Collar**
2. Haz clic o toca una ficha, mantén pulsado y arrastra hacia fichas iguales adyacentes (horizontal, vertical o diagonal).
3. **Deshacer/Retroceso**: Si vuelves a la ficha anterior de la cadena, se elimina la última selección.
4. **Validación**:
   - Menos de 3 fichas: Se cancela la selección.
   - 3 o más fichas: Se eliminan las fichas seleccionadas.
5. **Gravedad y Reposición**: Las fichas superiores caen y aparecen nuevas fichas desde arriba.
6. **Combos y Rachas**: Cadenas de 5+, 7+ y 9+ activan multiplicadores `Combo x2`, `Combo x3` y `Supercombo x4`.
7. **Modo Contrarreloj**: Consigue la máxima puntuación posible en **60 segundos**.

---

## 💻 Requisitos y Configuración

- Unity 6.5 (`6000.5.5f1`) o posterior.
- Plantilla Universal 2D.
- Soporte para entrada por Ratón y Pantalla Táctil.

### Cómo Ejecutar en Unity Editor

1. Clona o abre el repositorio en Unity Hub.
2. Abre la escena principal: [Gameplay.unity](file:///c:/Users/sefir/Desktop/DOGCRUSH/Assets/_DogCrush/Scenes/Gameplay.unity)
3. Pulsa el botón **Play**.
4. *(Opcional)* Si la escena no está configurada, ejecuta el menú del editor:
   `DOGCRUSH` ➔ `Build Playable Prototype`

---

## 📁 Estructura del Proyecto

```text
Assets/_DogCrush/
├── Art/              # Texturas, iconos de fichas y fondos
├── Audio/            # Música y efectos de sonido
├── Data/             # ScriptableObjects (BoardConfig)
├── Materials/        # Materiales 2D
├── Prefabs/          # Prefabs del tablero, fichas y UI
├── Scenes/           # Escena principal Gameplay.unity
├── Scripts/          # Arquitectura C# (Core, Board, Gameplay, Presentation, UI, Editor)
└── Tests/            # Pruebas unitarias EditMode y PlayMode
```

---

## 🚀 Roadmap Resumido

- [x] **v0.1.0**: Prototipo jugable básico (tablero, arrastre, gravedad, puntuación, temporizador, récord local).
- [ ] **v0.2.0**: Arte final ilustrado, animaciones fluidas y SFX caninos.
- [ ] **v0.3.0**: Niveles con objetivos variados (rescatar perritos, romper obstáculos).
- [ ] **v0.4.0**: Metaprogresión y coleccionable de razas de perros.
- [ ] **v1.0.0**: Publicación oficial para Android (Google Play).
