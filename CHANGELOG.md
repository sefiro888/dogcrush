# Changelog — DOGCRUSH

All notable changes to this project will be documented in this file.

## [0.1.0] - 2026-07-25

### Added
- Complete playable v0.1 prototype for DOGCRUSH.
- 7x9 interactive board with 5 dog-themed piece types (Dog, Bone, Ball, Food, Collar).
- Unified mouse drag & touch chain selection controller.
- Chain adjacency rules, non-repeat constraint, and backtrack/undo support.
- LineRenderer connection view displaying the active selection chain in real-time.
- Column gravity drop & top replenishment mechanics.
- Scoring system with long-chain bonuses, streak system, and Combo multipliers (x2, x3, Supercombo x4).
- 60-second timed game loop with HUD countdown, 10s warning, Game Over overlay, and Play Again restart trigger.
- Persistent local high score saving using PlayerPrefs.
- Automated `DogCrushProjectSetup` Editor tool (`DOGCRUSH/Build Playable Prototype`) generating sprites, prefabs, Canvas UI, and `Gameplay.unity` scene setup.
- NUnit EditMode unit tests for board adjacency, scoring, and save controller.
- Documentation: `README.md`, `GAME_DESIGN.md`, `ROADMAP.md`, `DEVELOPMENT.md`, `CHANGELOG.md`, `SECURITY.md`.
- Isolated Git repository setup and `.github/workflows/repository-check.yml` CI workflow.
