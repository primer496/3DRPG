# Task polling instead of await on the main loop

`Update` / `LateUpdate` / `FixedUpdate` must not use `async/await`. Multi-frame work (state-transition activities) uses `Task` plus per-frame `IsCompleted` polling (see `TransitionSequencer`). Chosen over coroutines/await to keep the frame loop deterministic, cancelable, and free of hidden continuation timing.
