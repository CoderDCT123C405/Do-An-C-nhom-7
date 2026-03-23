THU MUC CAU TRUC DU AN (CAP NHAT)

```text
Do An C#/
|-- HeThongThuyetMinhDuLich.Api/
|   |-- Controllers/             (REST API endpoints)
|   |-- Data/                    (DbContext)
|   |-- Migrations/              (EF Core migrations)
|   |-- Models/                  (entity + dto + auth models)
|   |-- Services/                (JWT, seed du lieu)
|   |-- appsettings.json         (cau hinh API/DB/JWT)
|   |-- Program.cs               (startup pipeline)
|   `-- HeThongThuyetMinhDuLich.Api.csproj
|
|-- HeThongThuyetMinhDuLich.Cms/
|   |-- Components/
|   |   `-- Pages/               (trang CMS: login, loai diem, diem tham quan...)
|   |-- Models/                  (view models)
|   |-- Services/                (goi API)
|   |-- wwwroot/                 (asset tinh)
|   |-- appsettings.json         (cau hinh CMS)
|   |-- Program.cs               (startup Blazor)
|   `-- HeThongThuyetMinhDuLich.Cms.csproj
|
|-- HeThongThuyetMinhDuLich.Mobile/
|   |-- Models/                  (model du lieu mobile)
|   |-- Platforms/               (cau hinh theo nen tang)
|   |-- Resources/               (fonts/images/splash)
|   |-- Services/                (API client, cache SQLite)
|   |-- App.xaml(.cs)
|   |-- AppShell.xaml(.cs)
|   |-- MainPage.xaml(.cs)       (UI chinh mobile)
|   |-- MauiProgram.cs           (DI + HttpClient + startup)
|   `-- HeThongThuyetMinhDuLich.Mobile.csproj
|
|-- Do An C#.sln                 (solution tong)
|-- Run-DoAn.ps1                 (build + run API/CMS, mode offline/online)
|-- Clean-DoAn.ps1               (don file tam 1 lenh)
|-- HeThongThuyetMinhDuLich.sql  (script DB SQL Server)
|-- PRD.md                       (pham vi + hien trang)
|-- YeuCau.md                    (yeu cau nghiep vu)
|-- KienTrucVaAPI.md             (tai lieu kien truc/API)
|-- TestCase.md                  (test checklist)
`-- ThuMucCauTruc.md             (tai lieu cau truc thu muc)
```

GHI CHU FILE TAM (CO THE XOA):
- `bin/`, `obj/` trong cac project.
- `.dotnet/`, `.dotnet-home/`.
- `api.run*.log`, `cms.run*.log`, `api.build.log`, `cms.build.log`.
- `*.db-shm`, `*.db-wal`.
- `HeThongThuyetMinhDuLich.offline.db` (chi xoa khi muon reset DB offline).
