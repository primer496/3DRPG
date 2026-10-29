# Matt skills 文档布局接入

## 做了什么

- 新增 `docs/agents/`（issue-tracker / triage-labels / domain）与 `AGENTS.md` 的 `## Agent skills` 段。
- 新增根目录 `GLOSSARY.md` 与 `docs/adr/0001`–`0005`（EventBus、轮询、UI Toolkit、Intent Provider、Capability Config）。
- 将 archive / 过时结构说明 / practices / CapabilityConfigMigration 中仍有效的坑位并入 `AGENTS.md` 第八节与 ADR，并删除冗余文件。

## 保留

- `docs/architecture/Dialogue_Quest_System_Explanation.md`（Yarn/任务流程深读）
- `docs/changelog/`（变更史）
- `docs/resume-project-section.md`（简历素材，非 agent 规范）

## 未完成（需本机）

- `gh auth login` 后创建五类 triage 标签（当前 CLI 未登录）。
