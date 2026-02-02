import express from 'express';
import multer from 'multer';
import path from 'node:path';
import axios from 'axios';
import { HttpsProxyAgent } from 'https-proxy-agent';

// Proxy configuration (only used for REST API calls)
const PROXY_URL = process.env.HTTPS_PROXY || process.env.HTTP_PROXY || null;
let httpsAgent = null;
if (PROXY_URL) {
  try {
    httpsAgent = new HttpsProxyAgent(PROXY_URL);
    console.log(`[INFO] Using proxy: ${PROXY_URL}`);
  } catch (e) {
    console.warn(`[WARN] Failed to create proxy agent: ${e.message}`);
  }
}

const app = express();
const port = process.env.PORT || 3000;

const upload = multer({ storage: multer.memoryStorage(), limits: { fileSize: 20 * 1024 * 1024 } });

app.use(express.static(path.join(process.cwd(), 'public')));

// POST /generate
// Accepts form-data: photo (image file), lat, lng, apiKey
app.post('/generate', upload.single('photo'), async (req, res) => {
  try {
    const { apiKey } = req.body;
    let { lat, lng } = req.body;
    
    // Convert lat/lng to numbers
    lat = parseFloat(lat);
    lng = parseFloat(lng);
    
    if (!req.file) return res.status(400).json({ error: 'No photo uploaded' });
    if (isNaN(lat) || isNaN(lng)) return res.status(400).json({ error: 'Invalid location data' });
    if (!apiKey) return res.status(400).json({ error: 'Missing Gemini API Key' });

    // Convert uploaded photo to base64
    const photoBase64 = req.file.buffer.toString('base64');
    const mimeType = req.file.mimetype || 'image/jpeg';

    console.log(`[INFO] Request: lat=${lat}, lng=${lng}, file=${req.file.originalname}, size=${req.file.size} bytes`);

    // Get location name using reverse geocoding
    const locationName = await getLocationNameFromAPI(lat, lng);
    const { season, weather, clothing } = getSeasonAndWeather(lat, lng);
    
    console.log(`[INFO] Location: ${locationName}, Season: ${season}, Weather: ${weather}`);
    console.log(`[INFO] Using TWO-STEP generation to avoid background contamination`);
    
    // ===== Step 1: Generate background image only (no user photo) =====
    console.log(`[STEP 1/2] Generating background for: ${locationName}...`);
    
    const backgroundPrompt = `Generate a tourist photo background at ${locationName}.

REQUIREMENTS:
1. Show the famous landmark of ${locationName} in the background (middle to upper portion)
2. Include a clear FOREGROUND AREA with solid ground (stone path, pavement, grass, sand, or viewing platform) where a person could naturally stand
3. Camera angle: eye-level, as if taking a photo of someone standing in front of the landmark
4. The foreground ground should be at the bottom 20% of the image
5. Landmark visible but at realistic distance (not too close)
6. Weather: ${weather}, Season: ${season}
7. NO PEOPLE in this image - completely empty scene
8. 4K quality, realistic photography style

This should look like a typical tourist photo spot where someone would stand to take a picture with ${locationName} behind them.`;

    const bgResult = await generateBackgroundOnly(apiKey, backgroundPrompt);
    
    if (!bgResult.success) {
      throw new Error(`Background generation failed: ${bgResult.error}`);
    }
    
    console.log(`[STEP 1/2] Background generated successfully`);
    
    // ===== Step 2: Composite person into background =====
    console.log(`[STEP 2/2] Compositing person into ${locationName} background...`);
    
    const compositePrompt = `TASK: Place a person into a travel photo background.

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

Generate the composite image.`;

    const result = await generateComposite(apiKey, compositePrompt, photoBase64, mimeType, bgResult.imageBase64);

    if (result.success) {
      res.json({
        image: result.image,
        message: `Successfully generated travel photo - ${locationName}`,
        location: locationName
      });
    } else {
      throw new Error(result.error || 'Composite failed');
    }

  } catch (err) {
    console.error(`[ERROR] Server error:`, err.message);
    res.status(500).json({ error: 'Server error', details: err.message });
  }
});

/**
 * Step 1: Generate background image ONLY (no user photo input to avoid contamination)
 */
async function generateBackgroundOnly(apiKey, prompt) {
  const GEMINI_API_URL = 'https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash-exp:generateContent';
  
  try {
    console.log(`[INFO] Generating background (text-only prompt, no reference image)...`);
    
    const requestBody = {
      contents: [{
        parts: [{ text: prompt }]
      }],
      generationConfig: {
        responseModalities: ['TEXT', 'IMAGE']
      }
    };

    const response = await axios.post(`${GEMINI_API_URL}?key=${apiKey}`, requestBody, {
      headers: { 'Content-Type': 'application/json' },
      timeout: 120000,
      httpsAgent: httpsAgent,
      proxy: false
    });

    const data = response.data;
    
    if (data.candidates?.[0]?.content?.parts) {
      for (const part of data.candidates[0].content.parts) {
        if (part.inlineData?.data) {
          return {
            success: true,
            imageBase64: part.inlineData.data,
            mimeType: part.inlineData.mimeType || 'image/png'
          };
        }
      }
    }

    return { success: false, error: 'Failed to generate background image' };

  } catch (err) {
    const errorMsg = err.response?.data?.error?.message || err.message;
    console.error(`[ERROR] Background generation failed: ${errorMsg}`);
    return { success: false, error: errorMsg };
  }
}

/**
 * Step 2: Composite person (from user photo) into background
 */
async function generateComposite(apiKey, prompt, personBase64, personMimeType, backgroundBase64) {
  const GEMINI_API_URL = 'https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash-exp:generateContent';
  
  try {
    console.log(`[INFO] Compositing person into background...`);
    
    const requestBody = {
      contents: [{
        parts: [
          { text: "IMAGE 1 (Person photo):" },
          {
            inlineData: {
              mimeType: personMimeType,
              data: personBase64
            }
          },
          { text: "IMAGE 2 (Background - destination):" },
          {
            inlineData: {
              mimeType: 'image/png',
              data: backgroundBase64
            }
          },
          { text: prompt }
        ]
      }],
      generationConfig: {
        responseModalities: ['TEXT', 'IMAGE']
      }
    };

    const response = await axios.post(`${GEMINI_API_URL}?key=${apiKey}`, requestBody, {
      headers: { 'Content-Type': 'application/json' },
      timeout: 180000,
      httpsAgent: httpsAgent,
      proxy: false
    });

    const data = response.data;
    
    if (data.candidates?.[0]?.content?.parts) {
      for (const part of data.candidates[0].content.parts) {
        if (part.inlineData?.data) {
          const resultMime = part.inlineData.mimeType || 'image/png';
          console.log(`[INFO] Composite generated successfully`);
          return {
            success: true,
            image: `data:${resultMime};base64,${part.inlineData.data}`
          };
        }
      }
    }

    return { success: false, error: 'Failed to generate composite image' };

  } catch (err) {
    const errorMsg = err.response?.data?.error?.message || err.message;
    console.error(`[ERROR] Composite generation failed: ${errorMsg}`);
    return { success: false, error: errorMsg };
  }
}

/**
 * Get location name using OpenStreetMap Nominatim reverse geocoding API
 */
async function getLocationNameFromAPI(lat, lng) {
  try {
    const url = `https://nominatim.openstreetmap.org/reverse?format=json&lat=${lat}&lon=${lng}&zoom=14&addressdetails=1`;
    
    console.log(`[INFO] Reverse geocoding: ${lat}, ${lng}`);
    
    const response = await axios.get(url, {
      headers: {
        'User-Agent': 'TravelPhotoGenerator/1.0'
      },
      timeout: 30000,
      httpsAgent: httpsAgent,
      proxy: false
    });

    const data = response.data;
    
    if (data && data.display_name) {
      // Extract meaningful location name
      const address = data.address || {};
      
      // Priority: tourism > landmark > building > neighbourhood > suburb > city
      const locationName = 
        address.tourism ||
        address.landmark ||
        address.building ||
        address.attraction ||
        address.monument ||
        address.museum ||
        address.neighbourhood ||
        address.suburb ||
        address.city ||
        address.town ||
        address.village ||
        data.display_name.split(',')[0];
      
      // Add country for context
      const country = address.country || '';
      const city = address.city || address.town || address.village || '';
      
      let fullName = locationName;
      if (city && city !== locationName) {
        fullName = `${locationName}, ${city}`;
      }
      if (country && !fullName.includes(country)) {
        fullName = `${fullName}, ${country}`;
      }
      
      console.log(`[INFO] Location resolved: ${fullName}`);
      return fullName;
    }
    
    return `Location at ${lat.toFixed(4)}, ${lng.toFixed(4)}`;
    
  } catch (err) {
    console.warn(`[WARN] Reverse geocoding failed: ${err.message}`);
    return `Location at ${lat.toFixed(4)}, ${lng.toFixed(4)}`;
  }
}

/**
 * Determine season and weather based on coordinates and current date
 */
function getSeasonAndWeather(lat, lng) {
  const month = new Date().getMonth(); // 0-11
  const isNorthernHemisphere = lat >= 0;
  
  let season, weather, clothing;
  
  // Determine season
  if (isNorthernHemisphere) {
    if (month >= 2 && month <= 4) season = 'Spring';
    else if (month >= 5 && month <= 7) season = 'Summer';
    else if (month >= 8 && month <= 10) season = 'Autumn';
    else season = 'Winter';
  } else {
    if (month >= 2 && month <= 4) season = 'Autumn';
    else if (month >= 5 && month <= 7) season = 'Winter';
    else if (month >= 8 && month <= 10) season = 'Spring';
    else season = 'Summer';
  }
  
  // Determine weather and clothing based on latitude and season
  const absLat = Math.abs(lat);
  
  if (absLat < 23.5) {
    // Tropical
    weather = 'warm and humid, tropical climate';
    clothing = 'light summer clothes, t-shirt and shorts';
  } else if (absLat < 35) {
    // Subtropical
    if (season === 'Summer') {
      weather = 'hot and sunny';
      clothing = 'light summer clothes, sunglasses';
    } else if (season === 'Winter') {
      weather = 'mild and pleasant';
      clothing = 'light jacket, casual wear';
    } else {
      weather = 'pleasant and mild';
      clothing = 'casual clothes, light layers';
    }
  } else if (absLat < 55) {
    // Temperate
    if (season === 'Summer') {
      weather = 'warm and sunny';
      clothing = 'summer casual wear, t-shirt';
    } else if (season === 'Winter') {
      weather = 'cold, possibly snowy';
      clothing = 'warm winter coat, scarf, winter clothes';
    } else if (season === 'Spring') {
      weather = 'mild with occasional rain';
      clothing = 'light jacket, spring casual wear';
    } else {
      weather = 'cool with falling leaves';
      clothing = 'sweater, autumn jacket';
    }
  } else {
    // Polar/Subpolar
    if (season === 'Summer') {
      weather = 'cool and mild';
      clothing = 'jacket, layered clothing';
    } else {
      weather = 'very cold, snowy';
      clothing = 'heavy winter coat, warm hat, gloves';
    }
  }
  
  return { season, weather, clothing };
}

// Start server
app.listen(port, () => {
  console.log(`\n========================================`);
  console.log(`  Photo at Location Server`);
  console.log(`========================================`);
  console.log(`  URL: http://localhost:${port}`);
  console.log(`  Model: Google Gemini 2.0 Flash`);
  console.log(`========================================\n`);
});
