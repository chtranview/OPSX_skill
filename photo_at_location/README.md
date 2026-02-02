# Photo at Location (local)

這是一個本地範例專案，可以：
- 在 OpenStreetMap 上選取一個地點
- 上傳一張個人參考照片
- 選擇使用 **Google Gemini 2.5 Flash (Nano Banana)** 或 **Hugging Face** API
- 生成該地點的高品質旅遊照片

重要：本範例不會保存使用者的 API Token。每個執行此專案的人需輸入自己的 API Token。

## 快速開始

### 1. 取得 API Token

#### 選項 A：Google Gemini 2.5 Flash (Nano Banana)
- 前往 https://aistudio.google.com/
- 登入 Google 帳號（免費）
- 點擊「Get API Key」
- 複製 API Key

#### 選項 B：Hugging Face (Stable Diffusion XL)
- 前往 https://huggingface.co/settings/tokens
- 登入或建立帳號（免費）
- 建立新的 API Token（選擇 "Fine-grained"）
- 複製 Token

### 2. 安裝並執行專案

```powershell
cd photo_at_location
npm install
npm start
```

### 3. 在瀏覽器開啟

前往 `http://localhost:3000`

## 使用說明

1. **選擇 API**：在下拉選單中選擇：
   - `Google Gemini 2.5 Flash (Nano Banana)` - **推薦，完全免費快速**
   - `Hugging Face (Stable Diffusion XL)` - 備選

2. **載入地圖**：點擊「載入地圖」按鈕以顯示 OpenStreetMap（無需 API Key）

3. **選擇地點**：在地圖上點選想要的旅遊地點（會放置一個標記，並顯示經緯度）

4. **上傳照片**：選擇一張你的參考照片（人像或全身最佳）

5. **輸入 Token**：在「API Token」欄位貼上對應 API 的 Token

6. **生成照片**：點擊「產生 4K 照片」，等待伺服器呼叫相應 API 生成結果

7. **預覽與保存**：生成的 4K 照片會在 Canvas 上預覽，可右鍵另存新檔

## API 比較

| 功能 | Google Gemini 2.5 Flash | Hugging Face |
|------|-------------|--------|
| **開發公司** | Google | Hugging Face |
| **API 類型** | 文本+視覺模型 | 文本到圖像 |
| **免費額度** | 完全免費（無限制） | 每月充足 |
| **生成速度** | 快速 | 30-60 秒 |
| **品質** | 優秀 | 很好 |
| **設置難度** | 簡單 | 簡單 |
| **推薦** | ⭐ 首選 | 次選 |

## 後端與圖片生成

- **後端**：`server.js`（Node.js + Express）
- **支援 API**：
  - Google Gemini 2.5 Flash API (推薦 - 完全免費)
  - Hugging Face Inference API + SDXL (備選)

## 分享給其他人

1. 將整個 `photo_at_location` 資料夾打包或推到 Git
2. 其他使用者只需執行：
   ```powershell
   npm install
   npm start
   ```
3. 在頁面上選擇 API 並輸入自己的 Token 即可使用

## 常見問題

### Banana 相關
- **"Banana API error: Application not found"**：已改用官方 Google Gemini 2.5 Flash API
  - 無需部署，直接使用 Google AI Studio 的 API Key

### Hugging Face 相關
- **"Hugging Face API error" 或 "410 Gone"**：模型可能在冷啟動，已自動重試 3 次
  - 若仍失敗，改用 **Banana** 或 **Replicate**
- **生成很慢**：1024×1024 圖片通常需要 30-60 秒
- **Token 無效**：到 https://huggingface.co/settings/tokens 檢查並重新建立

### Replicate 相關
- **"Version does not exist"**：Replicate 的版本 ID 經常失效且無法追蹤
  - **已移除此選項，改用 Hugging Face 或 Banana**

## 環境變數設定（可選）

若要設定自訂端點，可以在啟動伺服器前設定環境變數：

```powershell
# Google Gemini
$env:GOOGLE_API_KEY = "your-api-key-here"

# Hugging Face
$env:HF_MODEL = "stabilityai/stable-diffusion-xl-base-1.0"

# 啟動伺服器
npm start
```

## 技術棧

- **前端**：HTML5 + JavaScript + Leaflet.js（OpenStreetMap）
- **後端**：Node.js + Express + Axios
- **圖片生成**：
  - Banana.dev API + SDXL (Nano Banana)
  - Hugging Face Inference API + SDXL
  - Replicate API + SDXL

## 進階：自訂模型

如要使用其他模型，編輯 `server.js`：

### 更換 Hugging Face 模型
在 `generateImageWithHuggingFace` 函式中改變 `MODEL` 變數

### 更換 Google Gemini 模型
在 `generateImageWithGemini` 函式中改變 `model` 值

## 許可證

MIT - 自由使用與修改
