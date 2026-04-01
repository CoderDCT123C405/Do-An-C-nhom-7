# SO DO CAU TRUC THU MUC - Do An C# (HeThongThuyetMinhDuLich)

- Ngay cap nhat: 04/2025
- Pham vi: cap nhat theo hien trang code sau dot SQLite + edge-tts + tach nhanh CMS narration + to chuc /docs/

## 1. Tong quan thu muc goc (sau to chuc)

```
Do An C#/
|-- Do An C#.sln
|-- docs/                    <- Moi: To chuc tat ca tai lieu
|   |-- PRD.md
|   |-- ThuMucCauTruc.md
|   |-- TestCase.md
|   |-- YeuCau.md
|   |-- KienTrucVaAPI.md
|   |-- BaoCao_Admin.md
|   |-- TODO.md
|-- Run-DoAn.ps1
|-- Clean-DoAn.ps1
|-- fix-full.ps1
|-- Run-Mobile.ps1
|-- global.json
|-- HeThongThuyetMinhDuLich.sql
|-- HeThongThuyetMinhDuLich.Api/
|-- HeThongThuyetMinhDuLich.Cms/
`-- HeThongThuyetMinhDuLich.Mobile/
```

## 2. Chi tiet module API

Duong dan: `HeThongThuyetMinhDuLich.Api/`

Cac nhom chinh:
- `Controllers/`
  - `AuthController.cs`
  - `LoaiDiemThamQuanController.cs`
  - `DiemThamQuanController.cs`
  - `NoiDungThuyetMinhController.cs`
  - `LichSuPhatController.cs`
  - cac controller lien quan khac
- `Models/`
  - entity + DTO auth/du lieu
- `Services/`
  - `AdminSeedService.cs`
  - `EdgeTtsService.cs`
  - `EdgeTtsSettings.cs`
  - service nghiep vu khac
- `Data/`
  - `DuLichDbContext.cs`
- `Program.cs`
  - cau hinh DI, auth, pipeline
- `wwwroot/`
  - `audio/tts/`
    - noi luu file mp3 do `edge-tts` sinh ra

Cap nhat noi bat:
- Tang validate input va conflict handling
- Bo sung/hoan thien soft delete theo nghiep vu
- Chuan hoa luong login admin
- Bo sung luong sinh audio local bang `edge-tts`
- Tu dong xoa file audio cu khi noi dung bi thay the hoac xoa

## 3. Chi tiet module CMS

Duong dan: `HeThongThuyetMinhDuLich.Cms/`

Cac nhom chinh:
- `Components/`
  - `App.razor`
  - `Routes.razor`
  - `Layout/`
    - `MainLayout.razor`
    - `MainLayout.razor.css`
    - `NavMenu.razor`
    - `NavMenu.razor.css`
  - `Pages/`
    - `Home.razor`
    - `Login.razor`
    - `LoaiDiem.razor`
    - `DiemThamQuan.razor`
    - `ThongKe.razor`
    - `MaQR.razor`          <- New
    - `NgonNgu.razor`      <- New
    - `NguoiDung.razor`    <- New
    - `TaiKhoan.razor`     <- New
    - `NoiDungThuyetMinh.razor` <- New (feature/tts)
- `Services/`
  - `CmsApiClient.cs`
- `CmsSession.cs`
- `Models/`
  - `CmsModels.cs`
- `wwwroot/`
  - `app.css`
  - `favicon.png`
- `Program.cs`

Cap nhat noi bat:
- Sua static path tuyet doi de tranh 404
- Co `base href` va script Blazor dung chuan
- On dinh event click cho cac nut thao tac
- Nhanh `feature/tts` dang duoc dung de bo sung them man hinh `NoiDungThuyetMinh` va thao tac `Tao lai audio`

## 4. Chi tiet module Mobile

Duong dan: `HeThongThuyetMinhDuLich.Mobile/`

Cac nhom chinh:
- `Platforms/` (Android/iOS/Windows/MacCatalyst)
- `Resources/` (icon, splash, style, image)
- `Services/` (sync/cache/api)
- `Models/`
- `MauiProgram.cs`

Vai tro:
- Ung dung client cho nguoi dung cuoi
- Ho tro offline-first voi SQLite cache

## 5. Thu muc docs/ (Moi - 04/2025)

Chua tat ca tai lieu du an:
- `PRD.md`: Yeu cau san pham chi tiet + lich su update
- `ThuMucCauTruc.md`: So do thu muc hien tai
- `TestCase.md`: Testcase admin full
- `YeuCau.md`: Phân tích ban dau
- `KienTrucVaAPI.md`: Kiến trúc + API spec
- `BaoCao_Admin.md`: Báo cáo triển khai
- `TODO.md`: Backlog + progress tracking

Muc tieu: De quan ly, tracability cao, theo best practice.

## 6. Ghi chu .gitignore

Da cap nhat de ignore:
- `bin/`, `obj/`
- `*.db-shm`, `*.db-wal`
- `.dotnet-home/`
- `HeThongThuyetMinhDuLich.Cms/.keys/`
- `**/*.log`
- File audio dev khong can thiet

## 7. Script van hanh (ko thay doi)

- `Run-DoAn.ps1`: API + CMS
- `Clean-DoAn.ps1`: Clean build
- `Run-Mobile.ps1`: MAUI Android/Windows

