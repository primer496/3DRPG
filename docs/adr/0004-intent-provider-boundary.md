# Intent Provider as the only writer of per-frame desire

Player input, mobile bridge, and enemy AI all implement Intent Provider and only write Intent into shared context. They must not CrossFade Animator states or bypass HSM states (`Move` / `Combat` / …). Chosen so enemy and player share one motion stack and AI remains a pure decision boundary.
