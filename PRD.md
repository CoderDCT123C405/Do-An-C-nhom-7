# PRD - HeThongThuyetMinhDuLich

## 0. Thoi diem cap nhat
- Ngay cap nhat: `2026-03-20`

## 1. Muc tieu san pham
He thong thuyet minh du lich tu dong gom:
- Mobile App cho du khach.
- Web API cho nghiep vu.
- CMS Web cho quan tri.

## 2. Pham vi MVP da hoan thien trong code (hien tai)
- API:
  - Auth admin/user.
  - CRUD loai diem, diem tham quan, noi dung, QR, lich su.
  - Endpoint thong ke co ban.
  - Seed admin mau.
  - Seed du lieu demo cho mode offline (POI + noi dung + QR) neu DB rong.
- CMS:
  - Dang nhap.
  - Quan ly loai diem.
  - Quan ly diem tham quan.
  - Trang thong ke.
- Mobile:
  - Tai POI/noi dung tu API.
  - GPS + nearest POI + geofence cooldown.
  - QR lookup (nhap tay).
  - Phat audio URL / fallback TTS.
  - Cache offline SQLite cho POI/noi dung.
  - Da nang cap giao dien MainPage (theme + card layout + nhom chuc nang ro rang).
  - Windows fallback: bo map handler tren Windows de tranh crash do MAUI Maps chua ho tro.

## 3. Van hanh local va DB
- Script chay tong:
  - `powershell -ExecutionPolicy Bypass -File .\Run-DoAn.ps1`
- Mode DB:
  - `offline` (mac dinh): dung Sqlite.
  - `online`: dung SQL Server (doc tu `appsettings.json`).
- Reset DB offline de test lai du lieu mau:
  - `powershell -ExecutionPolicy Bypass -File .\Run-DoAn.ps1 -Mode offline -ResetOfflineDb`
- Script da bo sung:
  - Build retry de giam loi lock file.
  - Tu dong chon cong CMS neu cong mac dinh bi chiem.
  - Kiem tra trang thai API/CMS sau khi chay.
  - In so luong du lieu API (`DiemThamQuan`) de xac nhan DB da co du lieu.

## 4. Tai khoan va moi truong test
- Admin:
  - `admin / Admin@123`
- API local:
  - `http://localhost:5000`
- CMS local:
  - Mac dinh `http://localhost:5256`
  - Neu cong bi chiem, script se tu dong doi sang `http://localhost:5257..5275`

## 5. Tinh trang test nhanh
- API + CMS: da chay duoc local on dinh qua script.
- Mobile Windows:
  - Da mo duoc app.
  - Da test lay danh sach diem/noi dung/QR tu API.
  - Luu y: map khong bat tren Windows (gioi han MAUI Maps), nhung cac chuc nang con lai van hoat dong.

## 6. Hang muc co the nang cap tiep
1. QR camera scanner thay vi nhap tay.
2. Audio player native trong app (play/pause/seek).
3. Offline dong bo 2 chieu (khong chi cache doc).
4. Test tu dong va CI.
5. Ho tro map day du tren nen tang khong phai Windows (Android/iOS), hoac thay the map renderer phu hop cho Windows.
