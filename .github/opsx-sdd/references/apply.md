# 任務實作 · 逐步執行

實作 OpenSpec 變更中的任務。逐一執行 tasks.md 中的待辦事項，
撰寫程式碼並標記完成。支援中斷後續做、設計議題暫停、
以及與其他 opsx 指令交錯使用的流動式工作流程。

---

**輸入**：可選指定一個 change 名稱。若省略，從對話脈絡推斷或列出讓使用者選擇。

## 步驟

### 1. 選取 Change

依以下優先順序解析：
1. 使用者明確指定 → 直接使用
2. 從對話脈絡推斷（提過的 change 名稱）
3. 僅一個活躍 change → 自動選取
4. 多個活躍 changes → 列出讓使用者選擇

**定位專案目錄**：在主工作目錄下尋找 `<workspace>/openspec/changes/<name>/`。

宣布：「使用 change: `<name>`（可用 `/opsx-apply <other>` 切換）」

### 2. 確認狀態與 Schema

讀取 `<workspace>/openspec/changes/<name>/.openspec.yaml`，確認：
- `schema: spec-driven`
- artifacts 狀態（哪些已 `done`）

**若 tasks.md 不存在**：提示使用者先執行 `/opsx-propose` 建立變更提案。

### 3. 讀取背景 Artifacts

讀取以下檔案作為實作背景：
- `<workspace>/openspec/changes/<name>/proposal.md` — 了解 WHY 和 WHAT
- `<workspace>/openspec/changes/<name>/design.md` — 了解 HOW（架構決策）
- `<workspace>/openspec/changes/<name>/tasks.md` — 了解任務清單與進度
- `<workspace>/openspec/changes/<name>/specs/` — 了解需求規格（若有）

**重要**：開始實作前必須讀取這些背景檔案。

### 4. 顯示當前進度

```
┌──────────────────────────────────────────┐
│  🔨 開始實作                              │
│  Change：{name}                           │
│  Schema：spec-driven                      │
│  進度：{complete}/{total} 任務完成         │
└──────────────────────────────────────────┘
```

列出所有任務與狀態概覽。

### 5. 逐一實作任務（核心迴圈）

對每個狀態為 `- [ ]`（未完成）的任務：

**a. 顯示當前任務**
```
▶ [{index}/{total}] 實作 {task_description}...
```

**b. 實作程式碼**
- 撰寫程式碼完成任務- **程式碼寫在專案根目錄 `<workspace>/` 下**（不是 openspec/ 裡面）- 變更最小化且聚焦於單一任務
- 遵循 design.md 中的架構決策
- 程式碼風格與專案現有模式一致

**c. 標記完成**
在 `tasks.md` 中更新：`- [ ] {task}` → `- [x] {task}`

**d. 報告完成**
```
  ✓ 任務完成
```

**e. 繼續下一個任務**

### 6. 暫停條件

遇到以下情況時暫停並報告：

| 情況 | 動作 |
|------|------|
| **任務不明確** | 暫停並詢問使用者釐清 |
| **設計議題** | 暫停並建議更新 design.md 或 proposal.md |
| **錯誤或阻塞** | 回報錯誤並等待使用者指引 |
| **使用者中斷** | 儲存進度，顯示當前狀態 |

暫停時的輸出：

```
──────────────────────────────────────────
⏸ 實作暫停
Change：{name}
進度：{complete}/{total} 任務完成

原因：{pause_reason}

選項：
1. {option_1}
2. {option_2}
──────────────────────────────────────────
```

### 7. 完成或暫停時顯示狀態

**全部完成**：

```
══════════════════════════════════════════
✅ 實作完成！
Change：{name}
進度：{total}/{total} 任務全部完成 ✓

下一步：
- /opsx-verify 驗證實作
- /opsx-archive 歸檔變更
══════════════════════════════════════════
```

**部分完成**：顯示已完成與未完成的任務清單。

---

## 護欄

- 持續推進任務直到全部完成或遇到阻塞
- **開始前必須讀取背景 artifacts**（proposal、design、tasks、specs）
- 任務模糊時暫停詢問，不猜測
- 實作揭露設計問題時暫停並建議更新 artifacts
- 程式碼變更最小化且只針對單一任務
- **完成每個任務後立即更新 tasks.md 的 checkbox**
- 遇到錯誤、阻塞或不明需求時暫停 — 不猜測
- 不假設特定檔名，讀取實際存在的檔案

---

## 流動式工作流程

此指令支援「actions on a change」模型：

- **可隨時呼叫**：所有 artifacts 完成前即可呼叫（只要 tasks.md 存在）
- **支援中斷續做**：部分實作後再次呼叫，從上次停下的地方繼續
- **可交錯使用**：與其他 opsx 指令交錯使用（例如中途 `/opsx-explore` 思考再回來）
- **支援 artifact 更新**：實作揭露設計議題時可建議更新 artifacts，不受階段鎖定
