# 變更提案 · 產生 Artifacts

建立變更提案 — 一步到位建立 change 資料夾並生成所有 planning artifacts。
依序產出 proposal.md（what & why）、design.md（how）、tasks.md（implementation steps），
完成後即可進入 `/opsx-apply` 開始實作。

---

**輸入**：使用者的請求應包含一個變更名稱（kebab-case）或描述想建置的內容。

## 步驟

### 1. 確認使用者意圖

若無明確輸入，詢問使用者：
> 「你想建置或修復什麼？請描述你想做的事。」

從描述推導 kebab-case 名稱（例如「加入使用者認證」→ `add-user-auth`）。

**重要**：沒有理解使用者要建置什麼之前，不要繼續。

### 2. 檢查同名 change 是否存在

檢查主工作目錄下是否已有同名變更目錄 `<workspace>/openspec/changes/<name>/`。

**若衝突**：詢問使用者：
> 「已存在同名變更目錄「{name}」。要繼續該變更還是建立新的？」

選項：
- 繼續既有變更
- 建立新變更（自動加數字後綴，如 `add-user-auth-2`）

### 3. 初始化與建立 openspec 結構

**Step 3a. 初始化 openspec 結構（若尚未存在）**：
若 `<workspace>/openspec/` 不存在，則在主工作目錄下建立：
```
<workspace>/<name>/openspec/
├── config.yaml          ← schema: spec-driven + context
├── specs/               ← 主規格（source of truth）
└── changes/             ← 活躍變更
```

`config.yaml` 內容：

```yaml
schema: spec-driven

context: |
  Tech stack: [語言、框架、平台]
  Build: [建置工具/命令]
  Platform: [目標平台]
  [其他專案背景約束]
```

**撰寫準則**：`context` 段落應簡潔描述專案技術棧與約束，供後續 artifact 撰寫時參考。

**Step 3c. 建立 change 目錄與元資料**：
```
<workspace>/openspec/changes/<name>/
└── .openspec.yaml
```

`.openspec.yaml` 內容：

```yaml
schema: spec-driven
name: <name>
created: "YYYY-MM-DD"
status: active
artifacts:
  proposal: pending
  specs: pending
  design: pending
  tasks: pending
```

**若 `openspec/` 目錄不存在**，已在 Step 3a 建立，不需額外操作。

### 4. 依序生成 Artifacts

按以下順序建立，每個 artifact 建立前先讀取已完成的前置 artifact 作為背景：

#### 4a. proposal.md（WHY + WHAT）

建立於 `<workspace>/openspec/changes/<name>/proposal.md`。

**結構**：

```markdown
# 變更提案：<name>

## Intent
[為什麼要做這個變更？解決什麼問題？]

## Scope

### In scope
- [包含的功能/範圍，條列式]

### Out of scope
- [不包含的功能/範圍]

## Approach
[高層次的實作方向，不涉及技術細節]
```

**撰寫準則**：
- 說明 WHY 與 WHAT，不涉及 HOW
- 範圍要明確：什麼做、什麼不做
- 從探索模式的對話中提取洞察（若有）

完成後更新 `.openspec.yaml`：`proposal: done`

#### 4b. Delta Specs（WHAT — 行為規格）

**前置**：先讀取 `<workspace>/openspec/changes/<name>/proposal.md` 作為背景。

**此步驟為必要步驟。** 每個 change 都必須建立 delta specs，定義系統「應該做什麼」的結構化行為規格。

根據 proposal.md 的 Scope，識別需要規格化的 capability 領域，
在 `<workspace>/openspec/changes/<name>/specs/<capability>/spec.md` 建立。

**結構**：

```markdown
# <Capability 名稱>

## ADDED Requirements

### Requirement: <需求名稱>

The system SHALL <行為描述>。

#### Scenario: <場景名稱>

- **GIVEN** <前置條件>
- **WHEN** <觸發動作>
- **THEN** <預期結果>
- **AND** <附加驗證>（選用）
```

**撰寫準則**：

| 規則 | 說明 |
|------|------|
| **RFC 2119 關鍵字** | 使用 `SHALL` / `MUST`（絕對要求）、`SHOULD`（建議）、`MAY`（可選） |
| **Requirement 標記** | 必須使用 `### Requirement: <name>`（3 個 `#`） |
| **Scenario 標記** | 必須使用 `#### Scenario: <name>`（**恰好 4 個 `#`**，用 3 個會在歸檔時靜默失敗） |
| **每個需求至少一個場景** | 場景是可測試的驗收條件 |
| **GIVEN/WHEN/THEN 格式** | 場景使用結構化的前置條件 → 觸發 → 預期結果 |
| **Capability 命名** | kebab-case（如 `game-mechanics`、`ui-rendering`） |

**Delta 操作類型**（使用 `##` headers）：

| 操作 | 用途 | 說明 |
|------|------|------|
| `## ADDED Requirements` | 新增功能 | 全新的需求規格 |
| `## MODIFIED Requirements` | 修改行為 | 必須包含完整更新內容，不能只寫差異 |
| `## REMOVED Requirements` | 移除功能 | 必須包含 `**Reason**` 和 `**Migration**` |
| `## RENAMED Requirements` | 重命名 | 使用 `FROM:` / `TO:` 格式 |

**新專案（greenfield）**：所有需求使用 `## ADDED Requirements`。

**Capability 劃分指引**：
- 按功能領域劃分，每個 capability 對應一個 `spec.md`
- 例如遊戲專案可分為：`game-mechanics`、`ui-rendering`、`input-controls`、`scoring-system`
- 每個 capability 包含 3-8 個 requirements 為佳
- 過少 → 考慮合併；過多 → 考慮拆分

完成後更新 `.openspec.yaml`：`specs: done`

#### 4c. design.md（HOW）

**前置**：先讀取 `<workspace>/openspec/changes/<name>/proposal.md` 和 delta specs 作為背景。

建立於 `<workspace>/openspec/changes/<name>/design.md`。

**結構**：

```markdown
# 技術設計：<name>

## Architecture
[系統架構概覽，使用 ASCII 圖表]

## Key Decisions

### Decision 1: [決策名稱]
- **選擇**：[選了什麼]
- **理由**：[為什麼]
- **替代方案**：[考慮過但不選的]

## Tradeoffs
- [取捨 1]
- [取捨 2]

## Dependencies
- [外部依賴]
```

**撰寫準則**：
- 說明 HOW — 技術方案、架構決策、取捨
- 善用 ASCII 圖表視覺化架構
- 每個決策記錄「選了什麼」、「為什麼」、「不選的替代方案」

完成後更新 `.openspec.yaml`：`design: done`

#### 4d. tasks.md（實作步驟）

**前置**：先讀取 `<workspace>/openspec/changes/<name>/proposal.md`、delta specs 和 `design.md` 作為背景。

建立於 `<workspace>/openspec/changes/<name>/tasks.md`。

**結構**：

```markdown
# 實作任務：<name>

## [任務群組 1 名稱]
- [ ] 任務描述 1
- [ ] 任務描述 2

## [任務群組 2 名稱]
- [ ] 任務描述 3
- [ ] 任務描述 4

## 驗證與測試
- [ ] 驗證任務
```

**撰寫準則**：
- 每個任務以 `- [ ]` 開頭
- 粒度適中：每個任務可在一次 apply 中逐一完成
- 依邏輯分群：基礎建設 → 核心功能 → 進階功能 → 驗證
- 任務之間有合理的執行順序

完成後更新 `.openspec.yaml`：`tasks: done`

### 5. 驗證所有 Artifact 已建立

確認以下檔案都存在：
- `<workspace>/openspec/changes/<name>/.openspec.yaml`（所有 artifacts 狀態為 `done`）
- `<workspace>/openspec/changes/<name>/proposal.md`
- `<workspace>/openspec/changes/<name>/specs/`（至少一個 `<capability>/spec.md`）
- `<workspace>/openspec/changes/<name>/design.md`
- `<workspace>/openspec/changes/<name>/tasks.md`

### 6. 顯示最終狀態

```
══════════════════════════════════════════
✅ 提案完成！
📂 Artifacts：openspec/changes/<name>/
📄 檔案：proposal.md, specs/, design.md, tasks.md

下一步：執行 /opsx-apply 開始實作
══════════════════════════════════════════
```

---

## 護欄

- 必須建立所有 artifacts（proposal、specs、design、tasks）
- **Delta specs 為必要步驟**，不可跳過
- 建立新 artifact 前必須讀取其前置 artifacts
- 若使用者意圖嚴重不明確才提問，盡量做合理決策保持推進力
- 若同名 change 已存在，必須詢問使用者意圖
- 寫入 artifact 後必須驗證檔案存在才繼續下一個
- **context 與 rules 為撰寫時的約束，絕不寫入 artifact 檔案中**

---

## 輸出格式

```
┌──────────────────────────────────────────┐
│  📝 建立變更提案                          │
│  名稱：{name}                             │
│  Schema：spec-driven                      │
└──────────────────────────────────────────┘

  ▶ 建立 proposal.md...
  ✓ proposal.md 已生成

  ▶ 建立 delta specs...
  ✓ specs/<capability>/spec.md 已生成

  ▶ 建立 design.md...
  ✓ design.md 已生成

  ▶ 建立 tasks.md...
  ✓ tasks.md 已生成

══════════════════════════════════════════
✅ 提案完成！
📁 位置：openspec/changes/{name}/
📄 Artifacts：proposal.md, specs/, design.md, tasks.md

下一步：執行 /opsx-apply 開始實作
══════════════════════════════════════════
```
