# PRODUCT REQUIREMENTS DOCUMENT (PRD)

## HeThong Thuyet Minh Du Lich

---

# 0. Thong tin chung

* Ngay cap nhat: 20/03/2026
* Trang thai: MVP da hoan thien
* Phien ban: v1.0

---

# 1. Tong quan he thong

He thong bao gom:

* 📱 Mobile App
* 🌐 Web API
* 🖥️ CMS

## 🔧 Cong nghe su dung

* Mobile: .NET MAUI
* Backend: ASP.NET Core Web API
* CMS: ASP.NET Core MVC / Razor
* Database: SQL Server + SQLite

---

# 2. Kien truc tong the

```id="arch01"
SQL Server
   ↓
ASP.NET Core API
   ↓
Mobile (MAUI)
   ↓
SQLite (Local Cache)
```

## 🔧 Ky thuat ap dung

* Client-Server Architecture
* Offline-first Architecture
* Layered Architecture (API)

---

# 3. API (Backend)

## Chuc nang

* Auth (JWT)
* CRUD du lieu
* QR lookup
* Lich su phat

## 🔧 Cong nghe

* ASP.NET Core
* Entity Framework Core
* SQL Server

## ⚙️ Ky thuat

* RESTful API
* Dependency Injection
* DTO mapping
* Migration (EF Core)

---

# 4. CMS

## Chuc nang

* Quan ly du lieu
* Dang nhap admin
* Thong ke

## 🔧 Cong nghe

* ASP.NET Core MVC
* Razor View

## ⚙️ Ky thuat

* MVC Pattern
* Server-side rendering

---

# 5. Mobile App

## Chuc nang

* Hien thi POI
* GPS + Map
* Geofence
* QR lookup
* Phat audio

## 🔧 Cong nghe

* .NET MAUI
* MAUI Maps
* SQLite (sqlite-net-pcl)

## ⚙️ Ky thuat

* MVVM-lite (binding UI)
* Dependency Injection
* Async/Await

---

# 6. Offline-first (CORE)

## 🟢 ONLINE

* Goi API
* Sync du lieu

## 🔴 OFFLINE

* Doc SQLite

## 🔧 Ky thuat

* Offline-first design pattern
* Cache-first strategy
* Network detection (Connectivity)

---

# 7. Co che dong bo du lieu

## Nguyen tac

* 1 chieu (Server → Mobile)
* Khong push nguoc

## 🔧 Ky thuat

* Incremental Sync (`NgayCapNhat`)
* InsertOrReplace (SQLite)
* Background Sync

---

## Tu dong sync

* Interval: 30s

## 🔧 Cong nghe

* DispatcherTimer (MAUI)

## ⚙️ Ky thuat

* Polling mechanism
* Concurrency control (`_isSyncing`)

---

# 8. Quan ly du lieu local

## 🔧 Cong nghe

* SQLite
* sqlite-net-pcl

## ⚙️ Ky thuat

* Local caching
* Object mapping
* Transaction (RunInTransactionAsync)

---

# 9. GPS & Geofence

## 🔧 Cong nghe

* Geolocation API (MAUI)
* MAUI Maps

## ⚙️ Ky thuat

* Distance calculation
* Nearest neighbor search
* Geofencing
* Cooldown control

---

# 10. Xu ly audio

## 🔧 Cong nghe

* Launcher (mo audio URL)
* TextToSpeech (MAUI)

## ⚙️ Ky thuat

* Fallback strategy
* Async playback

---

# 11. Xu ly QR

## 🔧 Cong nghe

* HTTP API

## ⚙️ Ky thuat

* QR lookup mapping

---

# 12. Quan ly trang thai mang

## 🔧 Cong nghe

* Connectivity (MAUI)

## ⚙️ Ky thuat

* Network detection
* Failover (API → cache)

---

# 13. Testing

## 🔧 Ky thuat

* Manual testing
* Scenario-based testing

## Scenario

* Online
* Offline
* Reconnect
* Sync 1 chieu

---

# 14. Loi da xu ly

## 🔧 Ky thuat

* File lock handling
* Timer management
* Async safety

---

# 15. Gia tri dat duoc

* Offline-first
* Sync toi uu
* UX lien tuc
* Kien truc ro rang

---

# 16. Huong phat trien

* QR scan camera
* Audio player native
* CI/CD
* Map cross-platform

---

# 17. Ket luan

He thong dat:

* Offline-first architecture
* Sync incremental
* Khong conflict du lieu
* San sang trien khai

---

# 18. Cau noi bao ve

"Ung dung su dung offline-first architecture, ket hop SQLite cache va incremental sync tu SQL Server, dam bao hoat dong lien tuc va toi uu hieu nang."

---
# 19. Van hanh, Test & Troubleshooting

---

# 19.1 Chay he thong (One-command run)

## Lenh chay tong

```bash
powershell -ExecutionPolicy Bypass -File .\Run-DoAn.ps1
```

## Mode ho tro

* Offline (mac dinh): SQLite
* Online: SQL Server

```bash
powershell -ExecutionPolicy Bypass -File .\Run-DoAn.ps1 -Mode online
```

## Reset du lieu offline

```bash
powershell -ExecutionPolicy Bypass -File .\Run-DoAn.ps1 -ResetOfflineDb
```

---

# 19.2 Kiem thu he thong (Test Scenarios)

## 🧪 Test 1: Online Mode

### Buoc thuc hien

1. Chay API
2. Mo mobile app
3. Bat internet

### Ket qua mong doi

* Load danh sach POI
* Hien thi map
* GPS hoat dong
* Du lieu lay tu SQL Server

---

## 🧪 Test 2: Sync real-time

### Buoc thuc hien

1. Mo SSMS
2. Update du lieu:

```sql
UPDATE DiemThamQuan
SET TenDiem = N'Test Sync'
WHERE MaDiem = 1
```

3. Cho ~30s

### Ket qua mong doi

* Mobile tu dong cap nhat
* Khong can reload

---

## 🧪 Test 3: Offline Mode

### Buoc thuc hien

1. Tat WiFi / Airplane mode
2. Mo app

### Ket qua mong doi

* App van chay
* Du lieu tu SQLite
* Khong crash

---

## 🧪 Test 4: Reconnect

### Buoc thuc hien

1. Tat mang
2. Mo app
3. Bat mang lai

### Ket qua mong doi

* Sau ~30s tu dong sync
* UI cap nhat

---

## 🧪 Test 5: Sync 1 chieu

### Buoc thuc hien

1. Sua du lieu local (SQLite)
2. Bat mang

### Ket qua mong doi

* Server KHONG bi thay doi
* Local bi overwrite tu server

---

# 19.3 Log & Debug

## Them log

```csharp
Console.WriteLine("SYNC RUNNING");
```

## Ket qua

* Xuat log moi 30s
* Kiem tra sync hoat dong

---

# 19.4 Loi thuong gap & cach xu ly

---

## ❌ Loi 1: Access denied (Android build)

### Nguyen nhan

* File bi lock boi process (dotnet, adb, emulator)

### Cach fix

```bash
taskkill /F /IM dotnet.exe
taskkill /F /IM adb.exe
rd /s /q obj
rd /s /q bin
dotnet build
```

---

## ❌ Loi 2: Device.StartTimer obsolete

### Nguyen nhan

* MAUI deprecated API cu

### Cach fix

* Su dung:

```csharp
Dispatcher.CreateTimer()
```

---

## ❌ Loi 3: File DLL bi lock

### Nguyen nhan

* App dang chay
* Dotnet host giu file

### Cach fix

```bash
taskkill /PID <PID> /F
```

---

## ❌ Loi 4: SQLite khong co du lieu

### Nguyen nhan

* Chua seed data

### Cach fix

```bash
Run-DoAn.ps1 -ResetOfflineDb
```

---

## ❌ Loi 5: Android khong goi duoc API

### Nguyen nhan

* Sai localhost

### Cach fix

```text
Android: http://10.0.2.2:5000
PC: http://localhost:5000
```

---

## ❌ Loi 6: API build fail (permission)

### Nguyen nhan

* Thu muc .dotnet-home bi lock

### Cach fix

```bash
xoa thu muc .dotnet-home
```

---

## ❌ Loi 7: Timer chay nhieu lan

### Nguyen nhan

* Goi StartTimer nhieu lan

### Cach fix

* Dung DispatcherTimer
* Kiem tra null truoc khi start

---

## ❌ Loi 8: Sync khong cap nhat

### Nguyen nhan

* Thieu field NgayCapNhat

### Cach fix

* Them field vao DB + model + API

---

# 19.5 Checklist hoan thanh

| Hang muc     | Trang thai |
| ------------ | ---------- |
| Online load  | ✅          |
| Offline chay | ✅          |
| Sync 30s     | ✅          |
| Reconnect    | ✅          |
| Sync 1 chieu | ✅          |
| Khong crash  | ✅          |

---

# 19.6 Ket luan van hanh

He thong:

* Co the chay bang 1 lenh duy nhat
* De test, de debug
* Xu ly tot cac loi thuong gap

---

