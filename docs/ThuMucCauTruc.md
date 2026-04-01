# SO DO CAU TRUC THU MUC - Do An C#

- Ngay cap nhat: 01/04/2026
- Pham vi: hien trang source code trong workspace

## 1. Thu muc goc

```
Do An C#/
|-- docs/
|-- HeThongThuyetMinhDuLich.Api/
|-- HeThongThuyetMinhDuLich.Cms/
|-- HeThongThuyetMinhDuLich.Mobile/
|-- Do An C#.sln
|-- Run-DoAn.ps1
|-- Clean-DoAn.ps1
|-- fix-full.ps1
|-- HeThongThuyetMinhDuLich.sql
|-- HeThongThuyetMinhDuLich.offline.db
`-- *.log / *.db-shm / *.db-wal (runtime files)
```

## 2. Thu muc docs

Duong dan: `docs/`

- `PRD.md`: tai lieu yeu cau san pham va cap nhat qua trinh trien khai
- `YeuCau.md`: phan tich/thiet ke tong quan ban dau
- `KienTrucVaAPI.md`: mo ta kien truc va danh sach API cot loi
- `TestCase.md`: bo test case cho CMS admin
- `ThuMucCauTruc.md`: tai lieu cau truc thu muc (file nay)
- `TODO.md`: backlog cong viec tiep theo

## 3. Module API

Duong dan: `HeThongThuyetMinhDuLich.Api/`

Thanh phan chinh:
- `Controllers/`: Auth, LoaiDiem, Diem, NoiDungThuyetMinh, MaQR, NgonNgu, NguoiDung, LichSuPhat, ...
- `Services/`: `JwtTokenService`, `AdminSeedService`, `EdgeTtsService`, `EdgeTtsSettings`
- `Data/`: `DuLichDbContext`
- `Models/`: entity + DTO
- `wwwroot/audio/tts/`: luu file mp3 generate boi edge-tts

Ghi chu:
- Co endpoint generate audio thu cong:
  - `POST /api/NoiDungThuyetMinh/{id}/generate-audio`
  - `POST /api/NoiDungThuyetMinh/generate-audio`
- Auto generate audio co the bat/tat qua `EdgeTts:AutoGenerateOnSave`

## 4. Module CMS

Duong dan: `HeThongThuyetMinhDuLich.Cms/`

Thanh phan chinh:
- `Components/Layout/`: layout + nav menu
- `Components/Pages/`: `Login`, `Home`, `LoaiDiem`, `DiemThamQuan`, `NoiDungThuyetMinh`, `ThongKe`, ...
- `Services/`: `CmsApiClient`, `ApiSettings`, `CmsSession`
- `Models/`: model cho giao dien quan tri
- `wwwroot/`: static files (`app.css`, `favicon.png`)

## 5. Module Mobile

Duong dan: `HeThongThuyetMinhDuLich.Mobile/`

Thanh phan chinh:
- `Platforms/`: Android, iOS, Windows, MacCatalyst
- `Services/`: `MobileApiClient`, `MobileCacheStore`, `SyncService`
- `Models/`: model du lieu app
- `Resources/`: icon, splash, style, image
- `AppShell.xaml`, `MainPage.xaml`, `MauiProgram.cs`

## 6. Script van hanh

- `Run-DoAn.ps1`: build + run API/CMS, tu dong xu ly port 5000/5256
- `Clean-DoAn.ps1`: don dep artifact build va log
- `fix-full.ps1`: script xu ly loi moi truong runtime/build

## 7. Ghi chu quan ly source

Nhom file runtime nen bo qua khi commit:
- `bin/`, `obj/`
- `*.log`
- `*.db-shm`, `*.db-wal`
- file output tam hoac lock file trong qua trinh build/run
