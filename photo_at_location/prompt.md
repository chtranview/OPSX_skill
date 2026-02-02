# Gemini 2.0 Flash Prompt Documentation

**璅∪??**: `gemini-2.0-flash-exp`
**API Endpoint**: `https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash-exp:generateContent`

?祆?隞嗉???Photo at Location 撠?銝剖?策 Google Gemini 2.0 Flash API ??蝷箄???
**甇斗?隞嗆???server.js 銝剔??內閰?甇交?啜?*

---

## Step 1: ????內閰?(Background Generation)

```text
Generate a tourist photo background at: ${locationName}
GPS: ${lat}, ${lng}

Requirements:
- Famous landmark "${locationName}" clearly visible in background
- Foreground area with solid ground (pavement, rocks, grass) where a tourist can stand
- Eye-level camera angle, like someone taking a photo for a friend
- Bottom 20% should be walkable ground
- Season: ${season}, Weather: ${weather}
- 4K resolution, realistic photography
- NO people in the image
```

---

## Step 2: ???內閰?(Composite Prompt)

```text
TASK: Place a person into a travel photo background.

You are given TWO images:
- IMAGE 1: A photo of a person (THIS IS THE REFERENCE - the face MUST be preserved exactly)
- IMAGE 2: A background scene at ${locationName}

FACE PRESERVATION IS THE #1 PRIORITY:
- The person's face in the output MUST look IDENTICAL to IMAGE 1
- Same eyes, same nose, same mouth, same face shape, same skin tone
- Same hair style and hair color
- Do NOT generate a different face - COPY the face from IMAGE 1
- The face should be recognizable as the same person
- If you cannot preserve the face exactly, do not proceed

COMPOSITION:
- Place the person standing in front of the ${locationName} background (IMAGE 2)
- Person should be in the lower center of the image
- Person occupies about 40% of image height
- Person's feet on solid ground with natural shadow
- Clothing: ${clothing} (for ${weather})
- Person facing forward, natural expression

QUALITY:
- The final image should look like a real photograph
- Match lighting and colors between person and background
- 4K resolution

Generate the composite image.
```

---

## 霈隤芣?

| 霈 | 靘? | 隤芣? |
|------|------|------|
| `${locationName}` | OpenStreetMap Nominatim API ??嗆?撠?| ?圈??迂 |
| `${lat}` | ?冽?典??暺??蝵?| 蝺臬漲 |
| `${lng}` | ?冽?典??暺??蝵?| 蝬漲 |
| `${season}` | ?寞??嗅??交??楝摨西?蝞?| ?亙予/憭予/蝘予/?砍予 |
| `${weather}` | ?寞?摮???除???函? | 憭拇除?膩 |
| `${clothing}` | ?寞?摮???予瘞??蝞?| ?拙??忽?遣霅?|

---

## ?銵???

Gemini 2.0 Flash ?⊥?摰?靽???孵噩嚗??綽?
1. 摰?????嚗??鞎澆???
2. 瘥活???賣??冽???
3. ?⊥???Photoshop 蝎曄Ⅱ銴ˊ??

---

## ?航??脫獢?

1. **??璅∪?** - 雿輻 Replicate insightface/face-fusion
2. **?惜??** - ??? + remove.bg ?餉? + Canvas ??
3. **Imagen 3** - 雿輻 Google Vertex AI嚗?隞祥嚗?

---

## ?湔?亥?

- **2026-01-19**: ????Gemini 2.0 Flash (gemini-2.0-flash-exp)
- **2026-01-15**: ???嚗蝙??Gemini 2.0 Flash
