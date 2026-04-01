# TONG QUAN DU AN - CAP NHAT 01/04/2026

## 1. Muc tieu hien tai

He thong gom 3 module chinh:
- API (`HeThongThuyetMinhDuLich.Api`)
- CMS (`HeThongThuyetMinhDuLich.Cms`)
- Mobile (`HeThongThuyetMinhDuLich.Mobile`)

Trang thai tong quan:
- API/CMS da co bo trang admin va endpoint cot loi de van hanh MVP
- Luong audio thuyet minh da ho tro generate local bang `edge-tts`
- Mobile da co khung MAUI + service dong bo/cache theo huong offline-first

## 2. Nhanh ve backend API

Cac nhom controller dang co:
- Auth, TaiKhoan
- LoaiDiemThamQuan, DiemThamQuan
- NoiDung, NoiDungThuyetMinh
- NgonNgu, NguoiDung
- MaQr, HinhAnhDiemThamQuan
- LichSuPhat

Diem nhan ky thuat:
- JWT cho xac thuc
- Seed admin qua `AdminSeedService`
- TTS local qua `EdgeTtsService`
- Endpoint generate audio:
  - `POST /api/NoiDungThuyetMinh/{id}/generate-audio`
  - `POST /api/NoiDungThuyetMinh/generate-audio`

## 3. Nhanh ve CMS

Trang/feature dang co trong `Components/Pages`:
- `Login`, `Home`
- `LoaiDiem`, `DiemThamQuan`, `NoiDungThuyetMinh`
- `NgonNgu`, `NguoiDung`, `TaiKhoan`, `MaQR`
- `ThongKe`

Service chinh:
- `CmsApiClient`: goi API
- `CmsSession`: quan ly phien admin
- `ApiSettings`: cau hinh endpoint

## 4. Nhanh ve Mobile

Service chinh:
- `MobileApiClient`
- `MobileCacheStore`
- `SyncService`

Vai tro:
- Lam client cho nguoi dung cuoi
- Ho tro huong offline-first va dong bo du lieu du lich

## 5. Van hanh nhanh

Chay API + CMS:

```powershell
powershell -ExecutionPolicy Bypass -File .\Run-DoAn.ps1 -Mode online
```

Che do offline (SQLite):

```powershell
powershell -ExecutionPolicy Bypass -File .\Run-DoAn.ps1 -Mode offline
```

Reset offline DB + run:

```powershell
powershell -ExecutionPolicy Bypass -File .\Run-DoAn.ps1 -Mode offline -ResetOfflineDb
```

## 6. Tai lieu lien quan

- `docs/PRD.md`
- `docs/YeuCau.md`
- `docs/KienTrucVaAPI.md`
- `docs/TestCase.md`
- `docs/ThuMucCauTruc.md`
- `docs/TODO.md`

## 7. Ghi chu

Tai lieu nay duoc tao de tom tat nhanh hien trang source code trong workspace,
lam moc doi chieu cho cac cap nhat docs tiep theo.
