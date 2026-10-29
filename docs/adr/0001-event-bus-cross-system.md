# Cross-system communication via Event Bus

Systems (inventory, quest, dialogue, player stats, mobile input) must not call each other via `GetComponent` / `FindObjectOfType` or singleton managers. All cross-system signals go through the Event Bus (legacy namespace `TaskManager`). Chosen over direct references so subsystems stay swappable and AI agents cannot invent new hard couplings.
