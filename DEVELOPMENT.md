# Guía de Desarrollo para Principiantes — DOGCRUSH

## Cómo Abrir y Probar el Proyecto

### 1. Requisitos Previos
- **Unity Hub** instalado.
- **Unity 6000.5.5f1 (Unity 6.5)** instalado a través de Unity Hub.

### 2. Abrir el Proyecto
1. Abre Unity Hub.
2. Haz clic en **Add** ➔ **Add project from disk**.
3. Selecciona la carpeta: `C:\Users\sefir\Desktop\DOGCRUSH`.
4. Abre el proyecto utilizando la versión Unity 6000.5.5f1.

### 3. Probar en el Editor
1. En la ventana **Project**, navega a:
   `Assets` ➔ `_DogCrush` ➔ `Scenes` ➔ `Gameplay.unity`
2. Doble clic para abrir la escena `Gameplay.unity`.
3. Haz clic en el botón **Play** (triángulo superior central) para iniciar el juego.
4. Usa el ratón (o la pantalla táctil si estás en un dispositivo táctil) para hacer clic en una ficha y arrastrar conectando fichas iguales.

---

## Herramientas Automatizadas del Editor

Si necesitas regenerar o reconstruir los assets y la escena automáticamente:
1. En la barra de menú superior de Unity, haz clic en **DOGCRUSH**.
2. Selecciona **Build Playable Prototype**.
3. Se generarán los sprites, materiales, prefabs, el lienzo UI y la escena configurada automáticamente.

---

## Control de Versiones con Git

### Crear un Commit
```bash
git add .
git commit -m "feat: descripción de los cambios realizados"
```

### Subir Cambios a GitHub
```bash
git push origin main
```
