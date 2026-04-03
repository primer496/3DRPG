# FinalRPG

Unity 3D RPG project.

## Requirements

- Unity Editor (open the project and use the version in `ProjectSettings/ProjectVersion.txt`)

## Open Project

1. Launch Unity Hub.
2. Add this folder (`FinalRPG`) as a local project.
3. Open the project with the required Unity version.

## Repository Layout

- `Assets/`: game assets, scripts, scenes, prefabs, and resources
- `Packages/`: Unity package manifest and lock files
- `ProjectSettings/`: Unity project settings

## Git Notes

- This repository uses a Unity-focused `.gitignore`.
- Generated folders such as `Library/`, `Temp/`, and build artifacts are ignored.

## Branch Strategy

- `feature/rigidbody-slope`: legacy Rigidbody controller branch (current branch). Slope handling is available, but vault traversal is not implemented and known movement issues remain.
- `feature/cc-slope-vault`: CharacterController branch with slope handling and vault traversal.
- Keep both branches in GitHub for side-by-side testing and incremental fixes.
