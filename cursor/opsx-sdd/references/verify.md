# 實作驗證 · 三維度檢核

> 路徑均相對於專案根目錄 `<workspace>/<name>/`。

驗證實作是否符合變更 artifacts。透過三維度檢核 —
Completeness（完整性）、Correctness（正確性）、Coherence（一致性）—
產生優先順序化的驗證報告，判定是否可進入歸檔。

---

**輸入**：可選指定一個 change 名稱。若省略，必須讓使用者選擇。

## 步驟

### 1. 選取 Change — 必須由使用者確認

在主工作目錄下尋找專案目錄，讀取 `<workspace>/<name>/openspec/changes/` 目錄（排除 `archive/`），列出所有活躍 changes。

顯示：
- Change 名稱
- Schema（從 `.openspec.yaml` 讀取）
- 標記有未完成任務的 changes 為 `(In Progress)`

讓使用者選擇。**不自動選取，不猜測。**

### 2. 確認 Schema 與可用 Artifacts

讀取 `<workspace>/<name>/openspec/changes/<name>/.openspec.yaml`，確認：
- Schema 類型（spec-driven）
- 哪些 artifacts 已存在（proposal、specs、design、tasks）

**必要檢查：** 若 `specs/` 目錄不存在或為空，立即產生 **🔴 CRITICAL** issue：
> 「缺少 delta specs。spec-driven schema 要求必須建立 `specs/<capability>/spec.md`，定義結構化的 `### Requirement:` 和 `#### Scenario:` 。請執行 /opsx-propose 補建。」

### 3. 載入所有可用的 Change Artifacts

讀取以下檔案（存在的才讀）：
- `<workspace>/<name>/openspec/changes/<name>/tasks.md`
- `<workspace>/<name>/openspec/changes/<name>/proposal.md`
- `<workspace>/<name>/openspec/changes/<name>/design.md`
- `<workspace>/<name>/openspec/changes/<name>/specs/` 下的所有 spec 檔案

### 4. 初始化驗證報告結構

建立三維度報告：
- **Completeness**：追蹤任務與需求覆蓋
- **Correctness**：追蹤需求實作與場景覆蓋
- **Coherence**：追蹤設計遵循與模式一致

每個維度可出現 CRITICAL、WARNING 或 SUGGESTION 等級的 issues。

### 5. 驗證完整性（Completeness）

#### 5a. 任務完成度

解析 `tasks.md` 中的 checkbox：
- `- [ ]`（未完成）vs `- [x]`（已完成）
- 統計 complete / total

每個未完成的任務 → **🔴 CRITICAL** issue
> 建議：「完成任務：{description}」或「確認已實作後標記完成」

#### 5b. 需求覆蓋（Spec Coverage）

檢查 `<workspace>/<name>/openspec/changes/<name>/specs/` 目錄：

- 提取所有 `### Requirement:` 標記的需求
- 對每個需求，在程式庫中搜尋相關關鍵字
- 評估實作是否可能存在

**規格結構檢查**：
- 每個 spec.md 必須含有 `### Requirement:` 標記（使用 3 個 `#`）
- 每個 Requirement 必須至少有一個 `#### Scenario:`（使用 4 個 `#`）
- Scenario 必須使用 GIVEN/WHEN/THEN 格式
- 需求描述應使用 RFC 2119 關鍵字（SHALL/MUST/SHOULD/MAY）

未找到實作的需求 → **🔴 CRITICAL**
> 建議：「實作需求 {name}：{description}」

規格格式不符 → **🟡 WARNING**
> 建議：「修正 spec 格式：{issue_detail}」

### 6. 驗證正確性（Correctness）

#### 6a. 需求實作映射

條件：delta specs 存在

- 對每個需求，搜尋程式庫找出實作證據（檔案路徑與行號）
- 評估實作是否符合需求意圖
- 若偵測到分歧 → **🟡 WARNING**
> 建議：「檢查 {file}:{lines} 與需求 {name} 的一致性」

#### 6b. 場景覆蓋

條件：delta specs 含有 `#### Scenario:` 標記

- 對每個場景，檢查條件是否在程式碼中被處理
- 檢查是否有對應測試
- 若場景未覆蓋 → **🟡 WARNING**
> 建議：「為場景新增測試或實作：{scenario_description}」

### 7. 驗證一致性（Coherence）

#### 7a. 設計遵循

條件：`design.md` 存在

- 提取 design.md 中的關鍵決策（尋找 Decision:、Approach:、Architecture: 等區段）
- 驗證實作是否遵循這些決策
- 若偵測到矛盾 → **🟡 WARNING**
> 建議：「更新實作或修改 design.md 以符合現實」

若無 design.md：跳過此檢查，註記「無 design.md 可驗證」

#### 7b. 模式一致性

- 檢視新程式碼的檔案命名、目錄結構、程式碼風格
- 若有顯著偏離 → **🔵 SUGGESTION**
> 建議：「考慮遵循專案模式：{example_pattern}」

### 8. 生成驗證報告

#### 摘要計分卡

```
## 驗證報告：{name}

### 摘要
| 維度       | 狀態              |
|-----------|-------------------|
| 完整性     | {X}/{Y} 任務, {N} 需求 |
| 正確性     | {M}/{N} 需求覆蓋  |
| 一致性     | {status}          |
```

#### Issues 依嚴重度分組

1. **🔴 CRITICAL**（必須修復才能歸檔）
   - 未完成的任務
   - 未實作的需求
   - 每個附有具體、可執行的建議

2. **🟡 WARNING**（建議修復）
   - Spec/設計與實作的分歧
   - 未覆蓋的場景
   - 每個附有具體建議

3. **🔵 SUGGESTION**（可考慮改善）
   - 模式不一致
   - 小幅改進機會
   - 每個附有具體建議

#### 最終評估

- 有 CRITICAL issues：「發現 {count} 個嚴重問題。請修復後再歸檔。」
- 僅有 warnings：「無嚴重問題。{count} 個警告待考慮。可進行歸檔（附註改善項目）。」
- 全部通過：「所有檢查通過。可進行歸檔。」

---

## 驗證啟發法

| 維度 | 策略 |
|------|------|
| **Completeness** | 聚焦客觀清單項目（checkbox、需求列表） |
| **Correctness** | 使用關鍵字搜尋、檔案路徑分析、合理推斷 — 不要求完美確定性 |
| **Coherence** | 尋找明顯不一致，不吹毛求疵 |
| **誤報處理** | 不確定時偏好 SUGGESTION > WARNING > CRITICAL |
| **可執行性** | 每個 issue 必須有具體建議，含檔案/行號參考（若適用） |

---

## 優雅降級

依可用 artifacts 自動降級驗證範圍：

| 可用 Artifacts | 執行的驗證 | 跳過的驗證 | 注意 |
|---------------|-----------|-----------|------|
| 僅 tasks.md | 任務完成度 | spec 覆蓋、正確性、一致性 | 🔴 **缺少 specs 產生 CRITICAL** |
| tasks + specs | 完整性、正確性 | 設計遵循 | |
| 全部 artifacts | 三維度全面驗證 | 無 | |

永遠註明跳過了哪些檢查及原因。

---

## 護欄

- **不自動選取 change** — 必須讓使用者確認
- 每個 issue 必須有具體且可執行的建議
- 程式碼參考使用 `file:line` 格式
- 不確定時偏好較低嚴重度（避免誤報）
- **不給模糊建議**如「考慮檢視一下」
- 永遠註明跳過了哪些檢查及原因

---

## 輸出格式

```
┌──────────────────────────────────────────┐
│  🔍 開始驗證                              │
│  Change：{name}                           │
│  Schema：spec-driven                      │
└──────────────────────────────────────────┘

▶ 驗證完整性...
▶ 驗證正確性...
▶ 驗證一致性...

══════════════════════════════════════════
驗證完成：{name}
🔴 CRITICAL: {critical_count}
🟡 WARNING:  {warning_count}
🔵 SUGGESTION: {suggestion_count}

{final_assessment}
══════════════════════════════════════════
```
