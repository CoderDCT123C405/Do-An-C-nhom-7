# PRODUCT REQUIREMENTS DOCUMENT (PRD)

## HeThongThuyetMinhDuLich

---

## 0. Thong tin chung
- Ngay cap nhat: 01/04/2026
- Trang thai: MVP da chay duoc end-to-end (API + CMS + Mobile)
- Phien ban: v1.3 (as-is theo code hien tai)

---

## 1. Tong quan he thong
He thong gom 3 module trong solution `Do An C#.sln`:
- `HeThongThuyetMinhDuLich.Api` (ASP.NET Core Web API, `net8.0`)
- `HeThongThuyetMinhDuLich.Cms` (Blazor Server, `net10.0`)
- `HeThongThuyetMinhDuLich.Mobile` (.NET MAUI, `net10.0-android`, them `net10.0-windows10.0.19041.0` tren Windows)

Muc tieu hien tai:
- Quan tri du lieu diem tham quan va loai diem qua CMS
- Cung cap API cho quan tri va mobile (auth, CRUD, tra cuu, thong ke)
- Mobile ho tro online/offline, GPS geofence, tra cuu QR, phat noi dung (audio/TTS)

---

## 2. Kien truc va cong nghe
- Kien truc: `CMS/Mobile <-> API <-> SQL Server hoac SQLite`
- API su dung Entity Framework Core (SqlServer + Sqlite), JWT auth, Swagger
- Mobile su dung SQLite cache (`sqlite-net-pcl`) theo huong offline-first
- CMS su dung Blazor interactive server-side, goi API qua `CmsApiClient`

Ghi chu van hanh:
- `Run-DoAn.ps1` co ho tro `-Mode offline|online`, default `offline` (Sqlite)
- Khi offline, API seed admin + du lieu demo qua `AdminSeedService`

---

## 3. Pham vi chuc nang da trien khai
### 3.1 API (da co trong code)
- Auth:
  - `POST /api/auth`
  - `POST /api/auth/admin/login`
  - `POST /api/auth/user/login`
  - `POST /api/auth/user/register`
- CRUD du lieu cot loi:
  - `LoaiDiemThamQuan`
  - `DiemThamQuan`
  - `NoiDungThuyetMinh`
  - `NgonNgu`
  - `NguoiDung`
  - `TaiKhoan`
  - `MaQr`
  - `LichSuPhat`
- API nghiep vu cho Mobile:
  - `GET /api/diemthamquan/gan-day`
  - `GET /api/noidung/{maDiem}`
  - `GET /api/maqr/{giaTriQr}` (alias QR lookup)
  - `POST /api/lichsuphat`
- API media:
  - `POST /api/hinhanhdiemthamquan/upload`
  - `GET /api/hinhanhdiemthamquan/diem/{maDiem}`
  - `DELETE /api/hinhanhdiemthamquan/{id}`
- API thong ke:
  - `GET /api/lichsuphat/thong-ke/luot-nghe-theo-diem`
  - `GET /api/lichsuphat/thong-ke/luot-nghe-theo-kich-hoat`

### 3.2 CMS Admin (da co trong code)
- Dang nhap admin bang JWT (`/login`)
- Trang tong quan (`/`)
- Quan ly Loai diem (`/loaidiem`):
  - them, sua, tim kiem, an (soft delete)
- Quan ly Diem tham quan (`/diemthamquan`):
  - them, sua, tim kiem, loc theo loai, an (soft delete)
- Thong ke (`/thongke`):
  - theo diem
  - theo cach kich hoat
- Da chuan hoa static path va script Blazor de tranh loi 404 tai nguyen

### 3.3 Mobile (da co trong code)
- Tai danh sach diem tham quan tu API
- Luu cache SQLite va doc cache khi offline
- Xac dinh vi tri GPS, tim diem gan nhat, cap nhat dinh ky
- Geofence auto-trigger va cooldown
- Tra cuu QR
- Phat noi dung qua audio URL hoac TTS
- Ghi lich su phat ve API

---

## 4. Quy tac nghiep vu va rang buoc hien tai
- `LoaiDiemThamQuan` va `DiemThamQuan`: dang uu tien soft delete (`TrangThaiHoatDong = false`)
- Khong cho an `LoaiDiemThamQuan` neu con `DiemThamQuan` dang hoat dong thuoc loai do
- API co xu ly conflict cho cac truong hop trung du lieu chinh (ten loai, ma dinh danh)
- Auth dung JWT, role chinh: `Admin`, `BienTap`

---

## 5. Yeu cau phi chuc nang hien tai
- Uu tien on dinh thao tac CMS (event click, static resources, luong form)
- Co script run tong hop de giam xung dot process/port khi chay API + CMS
- Mobile co co che fallback cache khi mat ket noi
- Du lieu demo duoc seed tu dong trong mode offline de test nhanh

---

## 6. Gioi han / no ky thuat can ghi nhan
- Phan quyen chua dong nhat tren toan bo endpoint (mot so CRUD van mo rong hon can thiet)
- Chua co test tu dong (unit/integration/UI) trong repo hien tai
- Chua co script `Run-Mobile.ps1` trong thu muc goc (tai lieu cu co de cap, code hien tai chay mobile bang lenh `dotnet run`)
- API project dang `net8.0` trong khi CMS/Mobile dang `net10.0` (co the can dong bo sau)

---

## 7. Tieu chi chap nhan MVP hien tai
- Chay duoc API + CMS bang `Run-DoAn.ps1`
- CMS dang nhap va thao tac duoc tren Loai diem + Diem tham quan
- Thong ke tren CMS lay duoc tu `LichSuPhat`
- Mobile tai duoc diem, xem noi dung, trigger GPS/QR va gui lich su phat
- Khong xoa cung Loai diem/Diem trong luong quan tri thong thuong

---

## 8. Van hanh va chay he thong (hien tai)
### 8.1 API + CMS
- Tu thu muc goc solution:
  - `powershell -ExecutionPolicy Bypass -File .\Run-DoAn.ps1 -Mode offline`
- Dich vu muc tieu:
  - API: `http://localhost:5000/swagger/index.html`
  - CMS: `http://localhost:5256` (hoac port trong dai 5256-5275 neu 5256 ban)

### 8.2 Mobile Windows (khuyen nghi khi may yeu)
- `dotnet build .\HeThongThuyetMinhDuLich.Mobile\HeThongThuyetMinhDuLich.Mobile.csproj -f net10.0-windows10.0.19041.0`
- `dotnet run --project .\HeThongThuyetMinhDuLich.Mobile\HeThongThuyetMinhDuLich.Mobile.csproj -f net10.0-windows10.0.19041.0`

### 8.3 Mobile Android
- Chay truc tiep:
  - `dotnet build .\HeThongThuyetMinhDuLich.Mobile\HeThongThuyetMinhDuLich.Mobile.csproj -f net10.0-android`
  - `dotnet run --project .\HeThongThuyetMinhDuLich.Mobile\HeThongThuyetMinhDuLich.Mobile.csproj -f net10.0-android`

---

## 9. Dinh huong tiep theo
- Dong bo phan quyen endpoint theo role nghiep vu
- Bo sung audit log cho thao tac admin
- Bo sung test tu dong (API integration + UI smoke)
- Can nhac dong bo target framework giua 3 module

## Phần cần làm
# 📘 HUONG DAN FULL PRO MAX – QR + GPS + AUDIO (HE THONG THUYET MINH DU LICH)

---

# 🎯 MUC TIEU

Xay dung module **PRO MAX** cho Mobile (.NET MAUI):

* Scan QR → load POI → play audio
* GPS tu dong kich hoat (geofence)
* Map + navigation
* Offline-first
* Audio player native
* Multi-language

---

# 🧠 KIEN TRUC TONG THE

```
QR Scan / GPS
      ↓
Xac dinh POI
      ↓
Load NoiDung (API / SQLite)
      ↓
Phat Audio
      ↓
Luu Lich Su
```

---

# 🚀 PHAN 1: QR MODULE (FULL)

## 1. Cai thu vien

```
dotnet add package ZXing.Net.Maui
```

---

## 2. MauiProgram.cs

```csharp
builder.UseBarcodeReader();
```

---

## 3. UI Scan

* Camera full screen
* Overlay khung scan
* Text huong dan

---

## 4. Xu ly scan

```csharp
bool _isScanning = true;
string _lastQr = "";

private async void OnDetected(object sender, BarcodeDetectionEventArgs e)
{
    if (!_isScanning) return;

    var qr = e.Results.FirstOrDefault()?.Value;

    if (qr == _lastQr) return;

    _lastQr = qr;
    _isScanning = false;

    await HandleQr(qr);

    await Task.Delay(2000);
    _isScanning = true;
}
```

---

## 5. Flow xu ly

```csharp
async Task HandleQr(string qr)
{
    var poi = await qrService.GetByQrAsync(qr);

    if (poi == null) return;

    await audioService.PlayAsync(poi.AudioUrl);
    await SaveHistory(poi, "qr");
}
```

---

# 🌐 PHAN 2: OFFLINE-FIRST

## Nguyen tac

* Online → goi API
* Offline → SQLite

---

## QrService

```csharp
try
{
    var data = await http.GetFromJsonAsync<Poi>("api/qr/" + qr);
    await cache.Save(data);
    return data;
}
catch
{
    return await cache.Get(qr);
}
```

---

# 📍 PHAN 3: GPS + GEOFENCE

## Lay vi tri

```csharp
var location = await Geolocation.GetLastKnownLocationAsync();
```

---

## Tinh khoang cach

```csharp
double distance = Location.CalculateDistance(
    userLocation,
    poiLocation,
    DistanceUnits.Meters);
```

---

## Trigger geofence

```csharp
if (distance <= poi.Radius)
{
    TriggerPoi(poi);
}
```

---

## Cooldown

```csharp
if (DateTime.Now - lastTrigger < TimeSpan.FromSeconds(30))
    return;
```

---

# 🗺️ PHAN 4: MAP + NAVIGATION

## Hien thi map

```csharp
Map.Pins.Add(new Pin
{
    Label = poi.Name,
    Location = new Location(poi.Lat, poi.Lng)
});
```

---

## Move den POI

```csharp
Map.MoveToRegion(MapSpan.FromCenterAndRadius(
    new Location(poi.Lat, poi.Lng),
    Distance.FromMeters(100)));
```

---

## Navigation Google Maps

```csharp
await Launcher.OpenAsync(
    $"https://www.google.com/maps/dir/?api=1&destination={lat},{lng}");
```

---

# 🔊 PHAN 5: AUDIO PLAYER NATIVE (PRO)

## Interface

```csharp
public interface IAudioService
{
    Task Play(string url);
    Task Pause();
    Task Stop();
}
```

---

## Basic implement

```csharp
public async Task Play(string url)
{
    await Launcher.Default.OpenAsync(url);
}
```

---

## Nang cap (PRO MAX)

* Play / Pause
* Seek bar
* Background play
* Fade in/out

---

## Fade audio

```csharp
await audio.Stop();
await Task.Delay(300);
await audio.Play(newUrl);
```

---

# 🧾 PHAN 6: LUU LICH SU

```csharp
await http.PostAsJsonAsync("api/lichsu", new
{
    MaDiem = poi.Id,
    CachKichHoat = type, // gps / qr
    ThoiGianBatDau = DateTime.Now
});
```

---

# 🌍 PHAN 7: MULTI LANGUAGE

## Lay theo ngon ngu

```csharp
var lang = Preferences.Get("lang", "vi");

var data = await http.GetAsync($"api/poi/{id}?lang={lang}");
```

---

## Auto switch

* Detect device language
* Fallback ve "vi"

---

# 🔄 PHAN 8: SYNC BACKGROUND

## Timer 30s

```csharp
timer.Interval = TimeSpan.FromSeconds(30);
timer.Tick += async (_, _) => await Sync();
```

---

## Sync logic

```csharp
var newData = await api.GetUpdated(lastSyncTime);
await sqlite.Upsert(newData);
```

---

# ⚠️ PHAN 9: LOI THUONG GAP

## 1. Scan khong chay

* Chua cap camera permission

## 2. API khong goi duoc

* Android dung: 10.0.2.2

## 3. Audio khong phat

* URL sai / offline

## 4. GPS sai vi tri

* Emulator loi → dung may that

---

# 🧪 PHAN 10: TEST FULL

### Test QR

* Scan QR_POI001
* Load dung POI
* Phat audio

### Test GPS

* Di vao vung POI
* Tu dong phat

### Test offline

* Tat mang
* Van load du lieu

---

# 🏆 KET QUA PRO MAX

Ban se co:

* ✅ QR scan realtime
* ✅ GPS tu dong thuyet minh
* ✅ Map + navigation
* ✅ Audio native
* ✅ Offline-first
* ✅ Sync tu dong
* ✅ Multi-language

---

# 🚀 NANG CAP TIEP (NEU CAN)

* AI voice (Text → speech xịn)
* Download audio offline
* AR scan (camera + overlay POI)
* CMS realtime dashboard

---

# 🔥 KET LUAN

He thong cua ban da dat muc:

👉 DO AN TOT NGHIEP LOAI XIN (PRO MAX)

* Kien truc dung chuan
* Offline-first xịn
* Trai nghiem nhu app du lich that

---

👉 Neu ban muon:

* UI dep hon (nhu app thuong mai)
* Dong goi APK
* Viet bao cao


