let map, marker, selectedLatLng;

function setStatus(text, type = '') {
  const status = document.getElementById('status');
  status.innerText = text;
  status.className = type; // 'success', 'error', 'loading'
}

function updateLocationInfo(locationName = null) {
  const info = document.getElementById('locationInfo');
  if (selectedLatLng) {
    if (locationName) {
      info.innerHTML = `📍 已選擇：<strong>${locationName}</strong><br><small>(${selectedLatLng.lat.toFixed(6)}, ${selectedLatLng.lng.toFixed(6)})</small>`;
    } else {
      info.innerHTML = `📍 已選擇：${selectedLatLng.lat.toFixed(6)}, ${selectedLatLng.lng.toFixed(6)}`;
    }
    info.style.color = '#155724';
  } else {
    info.innerHTML = '📍 點擊地圖選擇目的地';
    info.style.color = '#666';
  }
}

// 搜尋地點功能
async function searchLocation(query) {
  if (!query || query.trim().length === 0) {
    alert('請輸入景點名稱');
    return;
  }

  setStatus('🔍 正在搜尋地點...', 'loading');

  try {
    // 使用 OpenStreetMap Nominatim 正向地理編碼
    const url = `https://nominatim.openstreetmap.org/search?q=${encodeURIComponent(query)}&format=json&limit=1&addressdetails=1`;
    const response = await fetch(url, {
      headers: {
        'User-Agent': 'TravelPhotoGenerator/1.0'
      }
    });

    if (!response.ok) {
      throw new Error('地理編碼服務無回應');
    }

    const results = await response.json();

    if (!results || results.length === 0) {
      setStatus(`❌ 找不到「${query}」，請嘗試其他關鍵字`, 'error');
      return;
    }

    const place = results[0];
    const lat = parseFloat(place.lat);
    const lng = parseFloat(place.lon);
    const displayName = place.display_name.split(',').slice(0, 2).join(', ');

    // 確保地圖已初始化
    if (!map) {
      initMap();
    }

    // 更新地圖位置
    selectedLatLng = { lat, lng };
    map.setView([lat, lng], 15);

    if (!marker) {
      marker = L.marker([lat, lng]).addTo(map);
    } else {
      marker.setLatLng([lat, lng]);
    }

    updateLocationInfo(displayName);
    setStatus(`✅ 已定位到：${displayName}`, 'success');

  } catch (err) {
    console.error('[Search Error]', err);
    setStatus('❌ 搜尋失敗：' + err.message, 'error');
  }
}

// 搜尋按鈕事件
document.getElementById('searchLocation').addEventListener('click', () => {
  const query = document.getElementById('locationSearch').value.trim();
  searchLocation(query);
});

// Enter 鍵搜尋
document.getElementById('locationSearch').addEventListener('keypress', (e) => {
  if (e.key === 'Enter') {
    e.preventDefault();
    const query = document.getElementById('locationSearch').value.trim();
    searchLocation(query);
  }
});

document.getElementById('initMap').addEventListener('click', () => {
  if (window.L) {
    initMap();
  } else {
    setStatus('正在載入地圖...', 'loading');
    setTimeout(initMap, 500);
  }
});

function initMap() {
  if (!map) {
    const center = [25.0330, 121.5654]; // 台北 101
    map = L.map('map').setView(center, 13);
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      attribution: '© OpenStreetMap contributors',
      maxZoom: 19
    }).addTo(map);

    map.on('click', (ev) => {
      const pos = ev.latlng;
      selectedLatLng = { lat: pos.lat, lng: pos.lng };
      if (!marker) {
        marker = L.marker([pos.lat, pos.lng]).addTo(map);
      } else {
        marker.setLatLng([pos.lat, pos.lng]);
      }
      updateLocationInfo();
      setStatus('');
    });
    
    setStatus('地圖已載入，點擊選擇目的地', 'success');
  }
}

document.getElementById('generate').addEventListener('click', async () => {
  const fileInput = document.getElementById('photo');
  const apiKey = document.getElementById('apiKey').value.trim();

  // 驗證
  if (!apiKey) return alert('請輸入 Gemini API Key');
  if (!fileInput.files || fileInput.files.length === 0) return alert('請先上傳你的照片（需要清晰的正面臉部）');
  if (!selectedLatLng) return alert('請在地圖上點選一個位置');

  const fd = new FormData();
  fd.append('photo', fileInput.files[0]);
  fd.append('lat', selectedLatLng.lat);
  fd.append('lng', selectedLatLng.lng);
  fd.append('apiKey', apiKey);

  setStatus('🎨 正在使用 Gemini 2.0 Flash 生成旅遊照片...', 'loading');
  
  try {
    const resp = await fetch('/generate', { method: 'POST', body: fd });
    const data = await resp.json();
    
    if (!resp.ok) {
      const errMsg = data.details || data.error || 'Generation failed';
      throw new Error(errMsg);
    }

    if (!data.image) {
      throw new Error('未收到圖片數據');
    }

    const img = new Image();
    img.onload = () => {
      const canvas = document.getElementById('canvas');
      const ctx = canvas.getContext('2d');
      const w = canvas.width, h = canvas.height;
      ctx.clearRect(0, 0, w, h);
      
      // 保持比例縮放
      const ar = img.width / img.height;
      let dw = w, dh = Math.round(w / ar);
      if (dh > h) { dh = h; dw = Math.round(h * ar); }
      const dx = Math.round((w - dw) / 2), dy = Math.round((h - dh) / 2);
      
      ctx.drawImage(img, dx, dy, dw, dh);
      setStatus('✅ 生成完成！你的臉部已成功換上旅遊照片。右鍵點擊圖片可另存新檔', 'success');
    };
    img.onerror = () => setStatus('❌ 無法載入產生的圖片', 'error');
    img.src = data.image;
    
  } catch (err) {
    console.error('[ERROR]', err);
    setStatus('❌ 發生錯誤：' + err.message, 'error');
  }
});
