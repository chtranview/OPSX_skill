# 變更歸檔 · Delta Spec 同步

歸檔已完成的變更。檢查 artifact 與 task 完成度，
評估 delta spec 同步狀態，將變更資料夾移至 archive 並
可選地將 delta specs 合併至主 specs。

---

**輸入**：可選指定一個 change 名稱。若省略，必須讓使用者選擇。

## 步驟

### 1. 選取 Change — 必須由使用者確認

在主工作目錄下尋找專案目錄，讀取 `<workspace>/openspec/changes/` 目錄（排除 `archive/`），列出所有活躍 changes。

顯示每個 change 的名稱與 schema（從 `.openspec.yaml` 讀取）。

讓使用者選擇。**不自動選取，不猜測 — 永遠讓使用者選擇。**

### 2. 檢查 Artifact 完成狀態

讀取 `<workspace>/openspec/changes/<name>/.openspec.yaml`，檢查所有 artifacts 狀態。

**若有 artifacts 未完成（status 不是 `done`）**：
- 顯示警告，列出未完成的 artifacts
- 詢問使用者是否仍要繼續歸檔
- 選項：「繼續歸檔」/「取消，先完成 artifacts」
- **警告不阻擋歸檔** — 只告知並確認

### 3. 檢查 Task 完成狀態

讀取 `<workspace>/openspec/changes/<name>/tasks.md`，統計：
- `- [ ]`（未完成）vs `- [x]`（已完成）
- 計算 complete / total

**若有未完成任務**：
- 顯示警告：「有 {incomplete}/{total} 個任務未完成」
- 詢問使用者是否仍要繼續歸檔
- **警告不阻擋歸檔** — 只告知並確認

**若無 tasks.md**：跳過任務檢查，無警告。

### 4. 評估 Delta Spec 同步狀態

檢查 `<workspace>/openspec/changes/<name>/specs/` 目錄是否存在。

**若無 delta specs**：跳過同步，直接進行歸檔。

**若有 delta specs**：

#### 4a. 分析差異

比對每個 delta spec 與對應的主 spec（`<workspace>/openspec/specs/<capability>/spec.md`）：
- 列出要套用的變更（ADDED / MODIFIED / REMOVED / RENAMED）
- 顯示合併摘要

#### 4b. 詢問使用者

**若需要同步**：
- 選項 1：「立即同步（建議）」
- 選項 2：「跳過同步直接歸檔」

**若已同步**：
- 選項 1：「直接歸檔」
- 選項 2：「重新同步」
- 選項 3：「取消」

#### 4c. 執行同步（若使用者選擇同步）

按以下順序合併 delta specs 至主 specs：

| 順序 | 操作 | 說明 |
|------|------|------|
| 1 | **RENAMED** | 先套用重命名，以便後續 MODIFIED 能對應新名稱 |
| 2 | **REMOVED** | 透過 normalized header 匹配刪除需求 |
| 3 | **MODIFIED** | 透過 normalized header 匹配替換需求（使用 RENAMED 後的新名稱） |
| 4 | **ADDED** | 附加至主 spec 末尾 |

**Header 匹配規則**：trim + case-sensitive equality

**目標檔案**：`<workspace>/openspec/specs/<capability>/spec.md`
- 若主 spec 不存在，建立新檔案
- 若存在，依操作類型修改

#### 4d. 處理合併衝突（Merge Conflicts）

在同步過程中，若遇到以下狀態不一致的情況，將觸發合併衝突：
- **MODIFIED / REMOVED / RENAMED**：在主 spec 中找不到對應的 `### Requirement: <name>`
- **ADDED**：主 spec 中已存在同名的 `### Requirement: <name>`

**若發生衝突**：
1. **暫停同步與歸檔**：停止修改主 spec 檔案，且不進行後續的 archive 目錄移動。
2. **顯示衝突報告**：列出發生衝突的 Capability、具體的 Requirement 名稱及原因。
   ```
   ⚠️ Delta Spec 合併衝突！
   檔案：specs/<capability>/spec.md
   - 找不到目標：MODIFIED Requirement: <name> 不存在於主 spec 中。
   ```
3. **提供解決建議**：指示使用者手動介入。
   > 「請手動檢查並修正 delta spec 或主 spec，解決衝突後再次執行 `/opsx-archive`。」
   > 「或者，您可以選擇『跳過同步直接歸檔』，稍後再手動整併。」

### 5. 執行歸檔

#### 5a. 確保 archive 目錄存在

建立 `<workspace>/openspec/changes/archive/` 目錄（若不存在）。

#### 5b. 生成目標名稱

格式：`YYYY-MM-DD-<change-name>`
例如：`2026-04-06-add-dark-mode`

#### 5c. 檢查目標是否已存在

若 `<workspace>/openspec/changes/archive/{target_name}/` 已存在：
- **停止並報錯**
- 建議：重命名既有歸檔或使用不同日期

#### 5d. 移動至 archive

將整個 `<workspace>/openspec/changes/<name>/` 目錄移至 `<workspace>/openspec/changes/archive/{target_name}/`。

保留所有檔案（包括 `.openspec.yaml`）。

更新 `.openspec.yaml` 中的 `status: archived`。

### 6. 顯示完成摘要

```
══════════════════════════════════════════
✅ 歸檔完成！

Change：{name}
Schema：spec-driven
歸檔至：openspec/changes/archive/{target_name}/
Specs：{sync_status}

{completion_notes}
══════════════════════════════════════════
```

其中 `{sync_status}` 為：
- `✓ 已同步至主 specs`
- `無 delta specs`
- `跳過同步`

---

## 護欄

- **永遠詢問使用者選擇 change** — 不自動選取
- 警告不阻擋歸檔 — 只告知並確認
- 移動時保留 `.openspec.yaml`（隨目錄一起移動）
- 顯示清晰的最終摘要
- 有 delta specs 時**必須執行同步評估**並呈現合併摘要再讓使用者選擇
- 歸檔目標已存在時**必須停止並報錯**
- **遇到合併衝突時必須暫停** — 不盲目覆寫，提供明確的錯誤報告並讓使用者手動介入

---

## 輸出格式

```
┌──────────────────────────────────────────┐
│  📦 開始歸檔                              │
│  Change：{name}                           │
│  Schema：spec-driven                      │
└──────────────────────────────────────────┘

📄 Artifacts 檢查：
   ✅ proposal.md — 完成
   ✅ design.md — 完成
   ✅ tasks.md — {complete}/{total} 完成

📋 Delta Spec 同步：
   {sync_details}

══════════════════════════════════════════
✅ 歸檔完成！

Change：{name}
Schema：spec-driven
歸檔至：openspec/changes/archive/{target_name}/
Specs：{sync_status}
══════════════════════════════════════════
```
