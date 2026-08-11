# opsx-sdd 三方合併清單

日期: 2026-08-11

比較對象:
- A: 本機現行版 (.github/skills/opsx-sdd)
- B: GitHub Copilot 版 (OPSX_skill/.github_copilot/opsx-sdd)
- C: Cursor 版 (OPSX_skill/cursor/opsx-sdd)

決策原則:
- 以 B 作為文件完整度基底（frontmatter、路由說明、整體文件較完整）。
- 保留 A 的路徑封裝鐵律（<workspace>/<name>/openspec/...）與防呆語義。
- C 僅抽取 Cursor 專用補強，不覆蓋 A/B 的通用邏輯。

---

## 1) 直接保留 A（不動）

- references/archive.md
- references/explore.md
- references/verify.md
- references/yaml/opsx-apply.yaml
- references/yaml/opsx-archive.yaml
- references/yaml/opsx-explore.yaml
- references/yaml/opsx-verify.yaml

理由:
- 與 B 語意等價或僅行尾差異。
- 這批檔案在 A/B/C 沒有明顯功能落差。

---

## 2) A 與 B 需人工合併（高優先）

- SKILL.md
- README.md
- references/apply.md
- references/propose.md
- references/yaml/_index.yaml
- references/yaml/opsx-propose.yaml

建議合併方向:
- SKILL.md: 以 B 為骨架，補回 A 的三條鐵律與路徑防呆語義。
- README.md: 以 B 為主文件，保留 A 的「重要核心變更與防護機制」段落。
- references/apply.md: 以 A 為主，避免路徑退化成 <workspace>/openspec/...
- references/propose.md: 以 A 為主，保留 <workspace>/<name>/openspec/... 與建立專案根目錄防呆。
- references/yaml/_index.yaml: 以 C 的擴充欄位為參考，必要時回填到 A。
- references/yaml/opsx-propose.yaml: 以 A 為主，逐段比對 C 的任務拆解與錯誤處理補強。

---

## 3) 從 B 補齊（你工作區已存在，僅驗證）

- .github/prompts/opsx-apply.prompt.md
- .github/prompts/opsx-archive.prompt.md
- .github/prompts/opsx-explore.prompt.md
- .github/prompts/opsx-propose.prompt.md
- .github/prompts/opsx-verify.prompt.md

狀態:
- 本機已與 B 完全一致（hash match），不需動作。

---

## 4) 從 C 或 antigravity 視需求新增（可選）

- Cursor 指令層:
  - commands/opsx-sdd.md
  - commands/opsx-explore.md
  - commands/opsx-propose.md
  - commands/opsx-apply.md
  - commands/opsx-verify.md
  - commands/opsx-archive.md

- antigravity 工作流層:
  - workflows/opsx-explore.md
  - workflows/opsx-propose.md
  - workflows/opsx-apply.md
  - workflows/opsx-verify.md
  - workflows/opsx-archive.md

注意:
- 這兩組不是 VS Code Copilot 必要檔案。
- 僅在你要同時支援 Cursor/antigravity 生態時新增。

---

## 5) 合併落地順序（建議）

1. 先合併 SKILL.md（先確立路由與路徑鐵律）。
2. 再合併 references/propose.md 與 references/apply.md（避免錯路徑寫檔）。
3. 補 README.md（更新文件，不影響執行）。
4. 視需求導入 C/antigravity 的 commands/workflows。
5. 最後用 /opsx-propose 做一次 dry-run 驗證目錄是否落在 <workspace>/<name>/openspec/。

---

## 6) 驗收檢查點

- /opsx-propose <name> 是否只在 <workspace>/<name>/openspec/ 下產生 artifacts。
- 不可在 .github/skills/ 內誤生專案檔案。
- tasks.md 若含 PowerShell 任務，是否仍有 create-ps 協同提醒。
- /opsx-apply, /opsx-archive 對同一 change-name 的路徑解析是否一致。
