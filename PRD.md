# PRD - HeThongThuyetMinhDuLich

## 1. Muc tieu san pham
He thong thuyet minh du lich tu dong gom:
- Mobile App cho du khach.
- Web API cho nghiep vu.
- CMS Web cho quan tri.

## 2. Pham vi MVP da hoan thien trong code
- API:
  - Auth admin/user.
  - CRUD loai diem, diem tham quan, noi dung, QR, lich su.
  - Endpoint thong ke co ban.
  - Seed admin mau.
- CMS:
  - Dang nhap.
  - Quan ly loai diem.
  - Quan ly diem tham quan.
  - Trang thong ke.
- Mobile:
  - Tai POI/noi dung tu API.
  - GPS + map + nearest POI.
  - Geofence trigger co cooldown.
  - Phat audio URL / fallback TTS.
  - QR lookup (nhap tay).
  - Cache offline SQLite cho POI/noi dung.

## 3. Tai khoan va moi truong test
- Admin:
  - `admin / Admin@123`
- API local:
  - `http://localhost:5000`
- CMS local:
  - `http://localhost:5256`

## 4. Lenh chay 1 lan
```powershell
powershell -ExecutionPolicy Bypass -File .\Run-DoAn.ps1
```

## 5. Hang muc co the nang cap tiep
1. QR camera scanner thay vi nhap tay.
2. Audio player native trong app (play/pause/seek).
3. Offline dong bo 2 chieu (khong chi cache doc).
4. Test tu dong va CI.
