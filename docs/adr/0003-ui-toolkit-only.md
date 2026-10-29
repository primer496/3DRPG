# UI Toolkit only (no uGUI)

All runtime UI uses UIDocument + VisualElement. Canvas/Image/Text/Button (uGUI) is forbidden. Chosen for one UI stack, consistent panel coordinate math (`RuntimePanelUtils`), and to avoid mixed Show/Hide patterns (`DisplayStyle` vs `SetActive` on UIDocument hosts, which rebuilds the visual tree and stale-caches element refs).
