# OpenSpec OPSX-SDD · 全域 Agent Skill 使用指南
> README.md · v1.0 · 2026-04-06

這份文件說明 **opsx-sdd** Agent Skill 的功能摘要、檔案結構、以及六個 `/opsx-*` 指令的用法。適用於 **Cursor**（亦相容 OpenSpec spec-driven 流程）。

> 📌 本 Skill 以 spec-driven 模式管理從「探索→提案→實作→驗證→歸檔」的完整變更生命週期。AI 直接操作檔案系統，不依賴外部 CLI。delta spec 同步併入 `/opsx-archive`（無獨立 `/opsx-sync`）。

---

## 目錄

1. [功能摘要](#1-功能摘要)
2. [檔案結構](#2-檔案結構)
3. [快速開始](#3-快速開始)
4. [指令說明與用法](#4-指令說明與用法)
5. [完整流程步驟](#5-完整流程步驟)
6. [Artifacts 結構](#6-artifacts-結構)
7. [常見問題](#7-常見問題)

---

## 1. 功能摘要

| 項目 | 說明 |
|------|------|
| **Skill 名稱** | `opsx-sdd` |
| **適用範圍** | 全域（所有工作區） |
| **觸發方式** | 聊天室輸入 `/opsx-sdd`（入口）、`/opsx-explore`、`/opsx-propose`、`/opsx-apply`、`/opsx-verify`、`/opsx-archive` |
| **CLI 依賴** | 無 — AI 直接操作檔案系統 |
| **輸出語言** | zh-TW |
| **Schema** | spec-driven |

### 核心特性

- **六指令生命週期** — 入口 `/opsx-sdd` + 探索、提案、實作、驗證、歸檔
- **零 CLI 依賴** — AI 直接讀寫 `openspec/` 目錄，不需安裝任何工具
- **自動初始化** — 首次執行時自動建立 `openspec/` 結構
- **YAML Pipeline 參考** — 每個指令都有對應的 YAML 結構化定義作為補充
- **流動式工作流程** — 支援中斷續做、平行變更、跳過可選階段

---

## 2. 檔案結構（Cursor 部署）

```
~/.cursor/skills/opsx-sdd/                 ← Skill 核心（全域，所有工作區可用）
├── SKILL.md                               ← 主 Skill 定義（frontmatter + 指令路由）
├── README.md                              ← 本文件
└── references/                            ← 詳細指引
    ├── explore.md / propose.md / apply.md / verify.md / archive.md
    └── yaml/
        ├── _index.yaml
        ├── opsx-explore.yaml
        ├── opsx-propose.yaml
        ├── opsx-apply.yaml
        ├── opsx-verify.yaml
        └── opsx-archive.yaml

<workspace>/.cursor/
├── commands/                              ← Slash Command 入口（專案級）
│   ├── opsx-sdd.md                        ← /opsx-sdd
│   ├── opsx-explore.md                    ← /opsx-explore
│   ├── opsx-propose.md                    ← /opsx-propose
│   ├── opsx-apply.md                      ← /opsx-apply
│   ├── opsx-verify.md                     ← /opsx-verify
│   └── opsx-archive.md                    ← /opsx-archive
└── skills/opsx-sdd/                       ← 連結至 ~/.cursor/skills/opsx-sdd（供 command 相對路徑引用）
```

### 檔案角色對照

| 檔案 | 角色 | 說明 |
|------|------|------|
| `SKILL.md` | 主入口 | 指令路由表、Artifacts 結構、檔案系統操作規範 |
| `references/*.md` | 行為指引 | 定義 AI「怎麼做」— 立場、步驟、護欄 |
| `references/yaml/*.yaml` | 結構化定義 | 定義「做什麼」— 輸入、步驟、輸出、錯誤處理 |
| `.cursor/commands/*.md` | Slash Command | 註冊為 `/opsx-*` 指令，以 `#skill:opsx-sdd` 載入 Skill |

> **與 OpenSpec CLI 的差異**：本 skill 在 `<workspace>/<name>/openspec/` 建立結構（支援一個 workspace 多個子專案）；OpenSpec CLI 預設在專案根目錄直接建立 `openspec/`。

---

## 3. 快速開始（Cursor）

### 前置條件

- Cursor IDE
- Skill 已安裝於 `~/.cursor/skills/opsx-sdd/`
- 專案已包含 `.cursor/commands/opsx-*.md`（本 workspace 已設定）
- 建議建立 `.cursor/skills/opsx-sdd` 連結至全域 skill（供 command 內相對路徑引用）

### Step 1：開啟 Cursor Chat

在 Cursor 中開啟 Agent Chat（`Ctrl+L` 或側邊欄）。

### Step 2：輸入指令

在聊天輸入框輸入 `/`，選擇或輸入 `/opsx-propose`：

```
/opsx-propose add-user-auth
```

### Step 3：觀察輸出

AI 依照 Skill 指引自動執行整個提案流程：

```
┌──────────────────────────────────────────┐
│  📝 建立變更提案                          │
│  名稱：add-user-auth                      │
│  Schema：spec-driven                      │
└──────────────────────────────────────────┘

  ▶ 確認意圖...
  ✓ 建立 openspec/changes/add-user-auth/
  ✓ 寫入 .openspec.yaml

  ▶ 生成 proposal.md...
  ✓ proposal 已完成

  ▶ 建立 delta specs...
  ✓ specs/<capability>/spec.md 已生成

  ▶ 生成 design.md...
  ✓ design 已完成

  ▶ 生成 tasks.md...
  ✓ tasks 已完成

══════════════════════════════════════════
✅ 提案完成！
📁 位置：openspec/changes/add-user-auth/
📄 Artifacts：proposal.md, specs/, design.md, tasks.md
下一步：執行 /opsx-apply 開始實作
══════════════════════════════════════════
```

---

## 4. 指令說明與用法

### 指令總覽

| 指令 | 用途 | 參數 | 前置條件 |
|------|------|------|---------|
| `/opsx-sdd` | 入口指令 — 載入指令路由表，引導選擇正確指令 | 自然語言描述（可選） | 無 |
| `/opsx-explore` | 探索模式 — 思考、調查、不寫程式碼 | `[topic]`（可選） | 無 |
| `/opsx-propose` | 建立變更提案並生成 artifacts | `[name]`（kebab-case 或描述） | 無 |
| `/opsx-apply` | 逐一實作 tasks.md 中的任務 | `[change-name]`（可選） | tasks.md 存在 |
| `/opsx-verify` | 三維度驗證實作正確性 | `[change-name]`（可選） | 有 artifacts |
| `/opsx-archive` | 歸檔變更並同步 delta specs | `[change-name]`（可選） | change 存在 |

---

### 4.0 `/opsx-sdd` — 入口指令

**用途：** 由 `.cursor/commands/opsx-sdd.md` 註冊的入口 slash command。當你不確定該用哪個 `/opsx-*` 指令時，輸入 `/opsx-sdd` 即可。

**用法：**
```
/opsx-sdd                          ← 顯示指令路由表，引導選擇
/opsx-sdd 我想開始一個新功能        ← AI 判斷後引導至 /opsx-propose
/opsx-sdd 目前有哪些進行中的變更？   ← AI 列出 active changes 並建議下一步
```

**AI 行為：**
- 載入 SKILL.md 的完整指令路由表與流程總覽
- 根據使用者描述，推薦最適合的 `/opsx-*` 指令
- 可作為「不知道從哪開始」時的起點

> 💡 此指令由 `.cursor/commands/opsx-sdd.md` 註冊。不確定用哪個指令時，從 `/opsx-sdd` 開始。

---

### 4.1 `/opsx-explore` — 探索模式

**用途：** 在正式提案前思考問題、調查程式庫、比較方案。適合需求尚不明確的場景。

**用法：**
```
/opsx-explore              ← 開放式探索
/opsx-explore 認證系統越來越複雜，想重構
```

**AI 行為：**
- 扮演思考夥伴 — 好奇、開放、耐心
- **禁止寫程式碼**（建立 OpenSpec artifacts 允許）
- 大量使用 ASCII 圖表視覺化
- 自然地讀取既有 artifacts 作為討論背景
- 洞察結晶時提議捕捉至適當 artifact

**範例對話：**
```
User: /opsx-explore 我們的 API 回應時間變慢了

AI:   讓我先看看現有的程式碼結構…

      ┌─ API Request Flow ─────────────────┐
      │  Client → Gateway → Service → DB   │
      │              ↓                      │
      │          Cache Layer ← 目前無快取    │
      └────────────────────────────────────┘

      我注意到三個瓶頸點：
      1. 資料庫查詢沒有分頁
      2. 缺少快取層
      3. N+1 查詢問題

      你覺得哪個影響最大？要深入看哪一個？
```

**結束方式：** 探索沒有固定結尾。可能流向 `/opsx-propose`、更新現有 artifacts、或提供清晰度後結束。

**參考：** [references/explore.md](references/explore.md) · [references/yaml/opsx-explore.yaml](references/yaml/opsx-explore.yaml)

---

### 4.2 `/opsx-propose` — 建立變更提案

**用途：** 一步到位建立 change 目錄並生成所有 planning artifacts。

**用法：**
```
/opsx-propose add-user-auth                  ← 直接指定 kebab-case 名稱
/opsx-propose 我想加入雙因素認證功能           ← 描述需求，AI 自動產生名稱
```

**執行步驟：**

```
Step 1  確認意圖 — 自然確認使用者想做什麼
          ↓
Step 2  檢查既有 — 檢查主工作目錄下是否已有同名專案目錄
          ↓
Step 3  建立專案 — 建立 <name>/ 專案目錄 + openspec/ 結構 + change 目錄
          ↓
Step 4  生成 Artifacts — 依序生成：
          proposal.md → specs/ → design.md → tasks.md
          ↓
Step 5  驗證完整性 — 確認所有 artifacts 已建立且相互一致
          ↓
Step 6  顯示狀態 — 列出專案目錄、已建立的檔案與下一步建議
```

**生成的 Artifacts：**

| Artifact | 內容 | 用途 |
|----------|------|------|
| `.openspec.yaml` | 元資料（schema, dates） | 變更索引 |
| `proposal.md` | Intent / Scope / Approach | WHY + WHAT |
| `specs/` | Delta specs（`### Requirement:` + `#### Scenario:`） | 結構化行為規格（**必要**） |
| `design.md` | Architecture / Decisions / Tradeoffs | HOW |
| `tasks.md` | checkbox 任務清單 `- [ ]` / `- [x]` | 實作追蹤 |

**重要規則：**
- **Delta specs 為必要步驟** — 每個 change 必須建立 `specs/<capability>/spec.md`，定義結構化的行為規格
- Specs 使用 RFC 2119 關鍵字（SHALL/MUST/SHOULD/MAY）與 GIVEN/WHEN/THEN 場景格式
- 同名 change 已存在時 → 詢問「繼續」或「新建」
- `context` 與 `rules` 是 AI 約束，**不會寫入** artifact 檔案

**參考：** [references/propose.md](references/propose.md) · [references/yaml/opsx-propose.yaml](references/yaml/opsx-propose.yaml)

---

### 4.3 `/opsx-apply` — 實作任務

**用途：** 逐一執行 tasks.md 中的待辦事項並標記完成。支援中斷續做。

**用法：**
```
/opsx-apply                    ← 只有一個 change 時自動選取
/opsx-apply add-user-auth      ← 指定 change 名稱
```

**執行步驟：**

```
Step 1  選取 Change — 讀取 openspec/changes/ 讓使用者確認
          ↓
Step 2  檢查狀態 — 確認 .openspec.yaml 與 tasks.md 存在
          ↓
Step 3  讀取上下文 — 載入 proposal/design/tasks/specs
          ↓
Step 4  顯示進度 — 統計已完成/未完成任務數
          ↓
Step 5  實作循環 ← ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ┐
        │ 5a. 顯示當前任務                    │
        │ 5b. 撰寫程式碼實作                  │
        │ 5c. 標記 - [ ] → - [x]             │
        │ 5d. 回報完成，若有下一個 → ─ ─ ─ ─ ┘
          ↓
Step 6  最終狀態 — 全部完成後提示下一步
```

**暫停條件：**
- 任務描述不明確 → 詢問使用者
- 實作揭露設計問題 → 建議更新 artifacts
- 錯誤或阻塞 → 回報並等待
- 使用者中斷

**流動式特性：** 可在任何時間點呼叫。即使 artifacts 未全部完成，只要 `tasks.md` 存在即可開始。

**參考：** [references/apply.md](references/apply.md) · [references/yaml/opsx-apply.yaml](references/yaml/opsx-apply.yaml)

---

### 4.4 `/opsx-verify` — 驗證實作

**用途：** 三維度檢核實作是否符合變更 artifacts 的要求。

**用法：**
```
/opsx-verify                   ← 提示選擇 change
/opsx-verify add-user-auth     ← 指定 change 名稱
```

**三維度驗證：**

| 維度 | 檢查內容 | 問題嚴重度 |
|------|---------|-----------|
| **Completeness** | tasks checkbox 完成度、spec 需求覆蓋 | 🔴 CRITICAL |
| **Correctness** | 實作與 spec 意圖一致、場景覆蓋 | 🟡 WARNING |
| **Coherence** | 遵循 design 決策、程式碼模式一致 | 🔵 SUGGESTION |

**輸出範例：**
```
## 驗證報告：add-user-auth

| 維度       | 狀態              |
|-----------|-------------------|
| 完整性     | 7/8 任務, 3 需求   |
| 正確性     | 3/3 需求覆蓋       |
| 一致性     | 已遵循             |

🔴 CRITICAL: 1（未完成任務）
🟡 WARNING:  0
🔵 SUGGESTION: 2（模式建議）

修復 1 個嚴重問題後可進行歸檔。
```

**優雅降級：**
- 只有 `tasks.md` → 僅驗任務完成度（🔴 **缺少 specs 產生 CRITICAL**）
- `tasks.md` + `specs/` → 驗完整性與正確性
- 全部 artifacts → 三維度全面驗證

**參考：** [references/verify.md](references/verify.md) · [references/yaml/opsx-verify.yaml](references/yaml/opsx-verify.yaml)

---

### 4.5 `/opsx-archive` — 歸檔變更

**用途：** 完成變更生命週期，歸檔至 archive 並可選同步 delta specs 至主 specs。

**用法：**
```
/opsx-archive                  ← 提示選擇 change
/opsx-archive add-user-auth    ← 指定 change 名稱
```

**執行步驟：**

```
Step 1  選取 Change — 列出所有 active changes 讓使用者選擇
          ↓
Step 2  檢查 Artifacts — 確認完成度（警告但不阻擋）
          ↓
Step 3  檢查 Tasks — 統計未完成任務（警告但不阻擋）
          ↓
Step 4  Delta Spec 評估（若有）
        │ 比對 delta specs 與主 specs
        │ 顯示合併摘要
        │ 詢問是否同步
        │ 同步順序：RENAMED → REMOVED → MODIFIED → ADDED
          ↓
Step 5  執行歸檔
        │ 建立 archive/ 目錄（若不存在）
        │ 移動至 openspec/changes/archive/YYYY-MM-DD-<name>/
        │ 更新 .openspec.yaml → status: archived
          ↓
Step 6  顯示完成摘要
```

**重要規則：**
- **永遠詢問使用者選擇** — 不自動選取 change
- 警告不阻擋歸檔 — 只告知並確認
- 歸檔目標已存在 → 停止並報錯

**參考：** [references/archive.md](references/archive.md) · [references/yaml/opsx-archive.yaml](references/yaml/opsx-archive.yaml)

---

## 5. 完整流程步驟

### 快速路徑（大多數場景）

```
1. /opsx-propose <change-name>    → 建立專案目錄 + 生成提案 artifacts
2. /opsx-apply                    → 在專案目錄下逐一實作任務
3. /opsx-archive                  → 歸檔並同步 specs
```

### 完整路徑（複雜變更）

```
1. /opsx-explore <topic>          → 思考、調查、釐清需求
2. /opsx-propose <change-name>    → 初始化結構 + 生成提案 artifacts
3. /opsx-apply                    → 在主工作目錄下逐一實作任務
4. /opsx-verify                   → 三維度驗證
5. /opsx-archive                  → 歸檔並同步 specs
```

### 迭代修復流程

```
/opsx-verify → 發現問題
  ↓
/opsx-apply  → 修復 CRITICAL issues
  ↓
/opsx-verify → 確認通過
  ↓
/opsx-archive
```

### 平行變更流程

```
Change A: /opsx-propose add-auth → /opsx-apply add-auth
   [暫停 A，切換到 B]
Change B: /opsx-propose fix-bug → /opsx-apply fix-bug → /opsx-archive fix-bug
   [回到 A]
Change A: /opsx-apply add-auth → /opsx-archive add-auth
```

---

## 6. Artifacts 結構

所有 artifacts 統一存放於 `<workspace>/<name>/openspec/` 結構下：

```
<workspace>/
└── <name>/                                  ← 專案根目錄（propose 建立）
    ├── openspec/
    │   ├── config.yaml                      ← 專案配置（首次自動建立）
    │   ├── specs/                           ← 主規格（source of truth）
    │   │   └── <capability>/spec.md
    │   ├── changes/                         ← 變更（活躍 + 歸檔）
    │   │   ├── <name>/                      ← 活躍變更
    │   │   │   ├── .openspec.yaml            ← 元資料（schema, dates, status）
    │   │   │   ├── proposal.md               ← Intent / Scope / Approach
    │   │   │   ├── design.md                ← Architecture / Decisions
    │   │   │   ├── tasks.md                 ← - [ ] / - [x] 任務清單
    │   │   │   └── specs/                   ← Delta specs
    │   │   │       └── <capability>/spec.md ← ADDED / MODIFIED / REMOVED / RENAMED
    │   │   └── archive/                     ← 已歸檔變更
    │   │       └── YYYY-MM-DD-<name>/
    └── (原始碼)                              ← apply 時在此目錄寫程式碼
```

### .openspec.yaml 格式

```yaml
schema: spec-driven
name: <change-name>
created: "YYYY-MM-DD"
status: active          # active | archived
artifacts:
  proposal: pending     # pending | done
  specs: pending        # pending | done
  design: pending       # pending | done
  tasks: pending        # pending | done
```

### config.yaml 格式

```yaml
schema: spec-driven

context: |
  Tech stack: [語言、框架、平台]
  Build: [建置工具/命令]
  Platform: [目標平台]
  [其他專案背景約束]
```

`context` 段落簡潔描述專案技術棧與約束，供後續 artifact 撰寫時參考。

---

## 7. 常見問題

### Q：輸入 `/opsx-*` 指令沒有反應？

**確認：**
1. `.cursor/commands/opsx-*.md` 檔案存在（檔名使用連字號，如 `opsx-apply.md`）
2. `~/.cursor/skills/opsx-sdd/SKILL.md` 的 `name` 欄位為 `opsx-sdd`
3. `.cursor/skills/opsx-sdd` 連結或複本存在（供 reference 路徑解析）
4. 重新載入 Cursor 視窗（`Ctrl+Shift+P` → `Developer: Reload Window`）

---

### Q：/opsx-propose 時出現「同名 change 已存在」？

選擇「繼續既有 change」接續作業，或先用 `/opsx-archive` 歸檔後再新建。

---

### Q：/opsx-apply 找不到 tasks.md？

先執行 `/opsx-propose <change-name>` 建立完整 artifacts。

---

### Q：想同時處理多個 change？

每個指令後加上 change 名稱即可切換：
```
/opsx-apply add-auth
/opsx-apply fix-payment
/opsx-apply add-auth        ← 切回
```

---

### Q：explore 模式中 AI 開始寫程式碼？

提醒：
```
請停止寫程式碼。目前在探索模式，只思考不實作。
若要實作，請退出探索並執行 /opsx-propose。
```
