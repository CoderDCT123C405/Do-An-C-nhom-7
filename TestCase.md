# Test Log - HeThongThuyetMinhDuLich

## 1) Ket qua hien tai
- Ngay cap nhat: `2026-03-19`.
- Da khoi phuc lai codebase va hoan thien lai cac module chinh:
  - API: alias route + seed admin + fix validation + auth.
  - CMS: login + quan ly loai diem + diem tham quan + thong ke.
  - Mobile: GPS/map/geofence co ban + QR lookup + audio/TTS + offline SQLite cache.

## 2) Danh sach fix chinh
1. Auth:
   - Them alias `POST /api/auth` (admin login nhanh).
2. Nearby:
   - Endpoint `api/diemthamquan/gan-day` nhan ca `vido/kinhdo` va `lat/lng`.
3. Noi dung:
   - Them endpoint `GET /api/noidung/{maDiem}`.
4. QR:
   - Them alias `GET /api/maqr/{giaTriQr}`.
   - Tra kem danh sach `NoiDung`.
5. Security:
   - Bat lai `[Authorize]` cho `POST /api/loaidiemthamquan`.
6. Validation:
   - Sua `[Range]` cua `BanKinhKichHoat` de on dinh model validation.
7. Seed:
   - Tu dong seed/reset admin mau khi API khoi dong.

## 3) Tai khoan test
- Username: `admin`
- Password: `Admin@123`

## 4) Cach chay nhanh
```powershell
powershell -ExecutionPolicy Bypass -File .\Run-DoAn.ps1
```

Sau khi chay:
- API Swagger: `http://localhost:5000/swagger/index.html`
- CMS: `http://localhost:5256`

## 5) Ghi chu
- Trong moi truong hien tai cua Codex, build co the fail do chan ket noi NuGet (`NU1301`) nen khong the xac nhan runtime 100% ngay tai sandbox.
- Tren may cua ban (co internet + workload maui day du), chay lai 3 lenh build sau de xac nhan:
  - `dotnet build HeThongThuyetMinhDuLich.Api\HeThongThuyetMinhDuLich.Api.csproj`
  - `dotnet build HeThongThuyetMinhDuLich.Cms\HeThongThuyetMinhDuLich.Cms.csproj`
  - `dotnet build HeThongThuyetMinhDuLich.Mobile\HeThongThuyetMinhDuLich.Mobile.csproj -f net10.0-windows10.0.19041.0`
