# SO DO CAU TRUC THU MUC - Do An C# (HeThongThuyetMinhDuLich)

- Ngay cap nhat: 26/03/2026
- Pham vi: cap nhat theo hien trang code sau dot nang cap API + CMS admin

## 1. Tong quan thu muc goc

```
Do An C#/
|-- Do An C#.sln
|-- PRD.md
|-- YeuCau.md
|-- TestCase.md
|-- ThuMucCauTruc.md
|-- KienTrucVaAPI.md
|-- BaoCao_Admin.md
|-- TODO.md
|-- Run-DoAn.ps1
|-- Clean-DoAn.ps1
|-- fix-full.ps1
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
  - `LichSuPhatController.cs`
  - cac controller lien quan khac
- `Models/`
  - entity + DTO auth/du lieu
- `Services/`
  - `AdminSeedService.cs`
  - service nghiep vu khac
- `Data/`
  - `DuLichDbContext.cs`
- `Program.cs`
  - cau hinh DI, auth, pipeline

Cap nhat noi bat:
- Tang validate input va conflict handling
- Bo sung/hoan thien soft delete theo nghiep vu
- Chuan hoa luong login admin

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
    - `Counter.razor`
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
- Nang cap giao dien admin + form an/hiem theo nhu cau

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

## 5. Tai lieu va script lien quan van hanh

Tai lieu:
- `PRD.md`: dinh nghia yeu cau va lich su nang cap
- `TestCase.md`: bo testcase admin
- `BaoCao_Admin.md`: tong hop ket qua trien khai admin
- `TODO.md`: backlog/cong viec tiep theo

Script:
- `Run-DoAn.ps1`: chay API + CMS
- `Clean-DoAn.ps1`: don dep build artifact
- `fix-full.ps1`: script ho tro sua loi moi truong

## 6. Ghi chu quan ly source

Nen bo qua khi commit:
- thu muc `bin/`, `obj/`
- file lock/runtime: `*.db-shm`, `*.db-wal`
- log tam: `*.log`
- key tam runtime cua CMS: `.keys/`

Muc tieu:
- Nhanh gon repo
- Tranh day file runtime khong can thiet len Git
