# opsx-sdd 合併稿（完整性優先 + 護欄不降級）

版本目標
- 文件完整性以遠端 .github_copilot/opsx-sdd 為主骨架。
- 安全護欄維持本機強度，不可退化。

適用範圍
- 本機來源: .github/skills/opsx-sdd
- 遠端基底: OPSX_skill@bf8e4a72/.github_copilot/opsx-sdd

---

## A. 最終來源矩陣

直接採用遠端基底（完整性優先）
- SKILL.md
- README.md
- references/archive.md
- references/explore.md
- references/verify.md
- references/yaml/_index.yaml
- references/yaml/opsx-apply.yaml
- references/yaml/opsx-archive.yaml
- references/yaml/opsx-explore.yaml
- references/yaml/opsx-propose.yaml
- references/yaml/opsx-verify.yaml

採用本機版本（護欄優先）
- references/propose.md
- references/apply.md

保留本機管理文件
- MERGE_3WAY_CHECKLIST.md
- 本檔案（MERGE_DRAFT_integrity_first_guardrails_preserved.md）

決策理由
- 遠端 SKILL.md 與 README.md 在流程、路由、章節與表格覆蓋顯著較完整。
- 本機 references/propose.md 與 references/apply.md 含致命路徑防護宣告與 create-ps 協同規範，遠端不足。

---

## B. 必須保留的護欄條款（不可刪）

檔案: references/propose.md
- 必保留條款 1: 致命路徑防護宣告整段。
- 必保留條款 2: 專案目錄必須是 <workspace>/<name>/openspec/... 的封裝路徑語義。
- 必保留條款 3: 介面防呆機制（新專案先確保底層資料夾存在，避免 IDE Diff 靜默失敗）。
- 必保留條款 4: create-ps 協同提醒。

檔案: references/apply.md
- 必保留條款 1: 致命路徑防護宣告整段。
- 必保留條款 2: 背景 artifacts 讀取路徑必須落在 <workspace>/<name>/openspec/changes/<name>/。
- 必保留條款 3: create-ps 協同提醒與約束。

檔案: SKILL.md
- 必增補條款 1: 鐵律區塊中納入三條本機護欄語意：
  - 絕對工作區定錨
  - 100% 目錄封裝
  - 介面防呆機制
- 必增補條款 2: 路徑示意一致化為 <workspace>/<name>/openspec/...（避免誤導成 <workspace>/openspec/...）。

---

## C. 合併步驟（建議一次完成）

步驟 1
- 先以遠端版本覆蓋以下檔案:
  - SKILL.md
  - README.md
  - references/archive.md
  - references/explore.md
  - references/verify.md
  - references/yaml/_index.yaml
  - references/yaml/opsx-apply.yaml
  - references/yaml/opsx-archive.yaml
  - references/yaml/opsx-explore.yaml
  - references/yaml/opsx-propose.yaml
  - references/yaml/opsx-verify.yaml

步驟 2
- 將本機版本的下列檔案覆蓋回來:
  - references/propose.md
  - references/apply.md

步驟 3
- 在 SKILL.md 補入護欄段落（若遠端骨架缺少）。

步驟 4
- 驗收路徑語義一致性，避免任何指令文件導向 <workspace>/openspec/... 作為唯一落點。

---

## D. SKILL.md 補強草稿（可直接貼入）

建議插入在流程總覽或全域設定之前的護欄區塊:

標題
- 鐵律（Guardrails）

內容
1. 絕對工作區定錨
- 本文件中的 <workspace> 一律表示 VS Code 最上層工作區根目錄。
- 禁止以 .github/skills/... 的相對路徑作為專案建立起點。

2. 100% 專案目錄封裝
- 所有變更 artifacts 必須封裝在 <workspace>/<name>/openspec/ 結構下。
- 禁止將 proposal、design、tasks、specs 直接寫到工作區根目錄或技能目錄。

3. 介面防呆機制
- 若 <workspace>/<name>/ 尚未存在，先提示建立空目錄，再套用深層檔案變更。
- 避免 IDE 對深層新檔案出現靜默失敗或路徑漂移。

4. PowerShell 協同
- 任務涉及 PowerShell 時，需提示或導向 create-ps 規範流程，避免腳本格式與錯誤攔截退化。

---

## E. 驗收清單（合併後必跑）

文件靜態驗收
- references/propose.md 中可找到致命路徑防護宣告。
- references/apply.md 中可找到致命路徑防護宣告。
- SKILL.md 中可找到 Guardrails 或等價鐵律段落。

語義一致性驗收
- references/propose.md 與 references/apply.md 的主要路徑敘述皆指向 <workspace>/<name>/openspec/...。
- 不得出現將 .github/skills 當成專案工作根目錄的敘述。

流程可用性驗收
- 指令路由完整: /opsx-sdd, /opsx-explore, /opsx-propose, /opsx-apply, /opsx-verify, /opsx-archive。
- Artifacts 與 archive 說明存在且可追溯。

---

## F. 合併結果定位

此合併稿是執行藍圖，不直接改動現有核心檔。
若要落地，可依本稿逐步套用，或由代理一次自動合併並回傳差異摘要。
