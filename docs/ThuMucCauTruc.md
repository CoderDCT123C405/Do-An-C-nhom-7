# SO DO CAU TRUC THU MUC - Do An C#

- Ngay cap nhat: 13/04/2026
- Pham vi: hien trang source code trong workspace
- Muc tieu: mo ta cau truc thu muc va chuc nang cua tung phan de de onboarding, bao tri va nop bao cao

## 1. Tong quan thu muc goc

```
Do An C#/
|-- .github/
|-- .tools/
|-- .vscode/
|-- docs/
|-- HeThongThuyetMinhDuLich.Api/
|-- HeThongThuyetMinhDuLich.Cms/
|-- HeThongThuyetMinhDuLich.Mobile/
|-- logs/
|-- Do An C#.sln
|-- Run-DoAn.ps1
|-- Clean-DoAn.ps1
|-- fix-full.ps1
|-- Capture-MobileLogcat.ps1
|-- Install-MobileToEmulator.ps1
|-- Rebuild-AllAudio.ps1
|-- Export-PoiQrImages.ps1
|-- HeThongThuyetMinhDuLich.sql
`-- HeThongThuyetMinhDuLich.offline.db
```

Chuc nang tung phan:
- `.github/`: cau hinh workflow, quy tac ho tro quy trinh source control/CI neu du an su dung GitHub.
- `.tools/`: script/cong cu phu tro noi bo cho viec build, fix loi hoac van hanh du an.
- `.vscode/`: task, launch, setting phuc vu lam viec trong VS Code.
- `docs/`: bo tai lieu phan tich, kien truc, testcase, tong quan du an va script SQL ho tro.
- `HeThongThuyetMinhDuLich.Api/`: backend ASP.NET Core cung cap API, xac thuc, truy cap CSDL va sinh audio thuyet minh.
- `HeThongThuyetMinhDuLich.Cms/`: giao dien quan tri de quan ly diem tham quan, noi dung, tai khoan va thong ke.
- `HeThongThuyetMinhDuLich.Mobile/`: ung dung MAUI danh cho nguoi dung cuoi, doc QR, xem diem tham quan, dong bo/offline va phat audio.
- `logs/`: log thu duoc tu logcat/mobile de debug thiet bi va luong chay thuc te.
- `Do An C#.sln`: solution tong hop 3 module de build/chay trong Visual Studio hoac VS Code.
- `HeThongThuyetMinhDuLich.sql`: script khoi tao/co so du lieu goc cho SQL Server.
- `HeThongThuyetMinhDuLich.offline.db`: CSDL SQLite phuc vu che do offline cua mobile hoac kich ban demo offline.

## 2. Thu muc docs

Duong dan: `docs/`

- `README.md`: file dieu huong nhanh, giup tim tai lieu can doc theo muc dich.
- `TongQuanDuAn.md`: tom tat hien trang 3 module va cach van hanh nhanh.
- `PRD.md`: yeu cau san pham, pham vi chuc nang va tien do trien khai.
- `YeuCau.md`: phan tich/thiet ke tong quan ban dau, mo ta bai toan va dinh huong giai phap.
- `KienTrucVaAPI.md`: mo ta kien truc he thong, cac luong chinh va danh sach API cot loi.
- `TestCase.md`: bo testcase de kiem thu CMS/admin va cac nghiep vu lien quan.
- `ThuMucCauTruc.md`: tai lieu cau truc thu muc va chuc nang tung phan trong workspace.
- `assets/`: anh chup/manh hinh phuc vu minh hoa giao dien, demo va nop bai.
- `sql/`: script SQL bo sung/doi chieu du lieu, migration tay va script dong bo lich su schema.

Chi tiet them:
- `docs/assets/submission-mainpage*.png`: anh minh hoa giao dien trang chinh phuc vu bao cao.
- `docs/sql/baseline-efhistory-and-sync-timestamps.sql`: script dong bo lich su migration va cot timestamp.
- `docs/sql/fix-audio-paths-to-managed-tts.sql`: script sua duong dan audio TTS da quan ly.
- `docs/sql/require-updater-and-chinese-content.sql`: script cap nhat rang buoc audit/updater va noi dung tieng Trung.

## 3. Module API

Duong dan: `HeThongThuyetMinhDuLich.Api/`

Vai tro:
- Dong vai tro backend trung tam cho CMS va Mobile.
- Quan ly nghiep vu diem tham quan, ngon ngu, noi dung thuyet minh, tai khoan va lich su phat.
- Sinh token JWT cho dang nhap admin.
- Tao file audio TTS va phuc vu file tinh qua `wwwroot`.

Cau truc chinh:
- `Program.cs`: diem khoi dong, cau hinh DI, middleware, CORS, EF Core, auth va static files.
- `appsettings.json`, `appsettings.Development.json`: cau hinh ket noi CSDL, JWT, TTS, logging va tham so moi truong.
- `Controllers/`: dinh nghia HTTP endpoint cho tung nghiep vu.
- `Services/`: chua service xu ly chuc nang dung chung hoac nghiep vu phuc tap.
- `Data/`: DbContext va cau hinh truy cap du lieu.
- `Models/`: entity database, DTO request/response va model phu tro.
- `Migrations/`: lich su migration EF Core de tao/cap nhat schema.
- `wwwroot/`: static files, dac biet la audio TTS de client tai/phat.
- `Properties/`: metadata runtime/debug cua du an ASP.NET Core.
- `bin/`, `obj/`: output build tam thoi, khong thuoc source nghiep vu.

Y nghia cac nhom controller:
- `AuthController`: dang nhap/xac thuc cho khu vuc quan tri.
- `TaiKhoanController`: quan ly tai khoan dang nhap he thong.
- `NguoiDungController`: quan ly thong tin nguoi dung/nguoi quan tri.
- `LoaiDiemThamQuanController`: CRUD nhom/loai diem tham quan.
- `DiemThamQuanController`: CRUD diem tham quan va thong tin vi tri/thuoc tinh chinh.
- `NoiDungController`: quan ly noi dung tong quat lien quan diem tham quan.
- `NoiDungThuyetMinhController`: quan ly bai thuyet minh da ngon ngu va luong sinh audio.
- `NgonNguController`: quan ly danh muc ngon ngu ho tro trong he thong.
- `MaQrController`: quan ly ma QR dung de mo nhanh noi dung tren mobile.
- `HinhAnhDiemThamQuanController`: quan ly anh cua diem tham quan.
- `LichSuPhatController`: ghi nhan/tra cuu lich su phat audio hoac lich su tuong tac lien quan.

Y nghia cac service:
- `JwtTokenService`: tao va ky JWT cho phien dang nhap.
- `AdminSeedService`: khoi tao tai khoan admin mac dinh hoac du lieu nen luc startup.
- `EdgeTtsService`: goi edge-tts de sinh file mp3 cho noi dung thuyet minh.
- `EdgeTtsSettings`: model cau hinh cho che do TTS.
- `AudioPathResolver`: chuan hoa/quan ly duong dan luu audio da generate.
- `LanguageCatalog`: danh muc ngon ngu phuc vu mapping ma ngon ngu va hien thi.

Ghi chu nghiep vu:
- File audio sinh ra duoc luu duoi `wwwroot/audio/tts/` de CMS/Mobile phat lai.
- He thong co endpoint generate audio thu cong cho tung ban ghi hoac theo lo.
- Tu dong generate audio co the bat/tat qua cau hinh `EdgeTts:AutoGenerateOnSave`.

## 4. Module CMS

Duong dan: `HeThongThuyetMinhDuLich.Cms/`

Vai tro:
- Cung cap giao dien quan tri web de nhan su van hanh cap nhat du lieu.
- Tieu thu API backend, quan ly phien dang nhap admin va hien thi thong ke/co so du lieu.

Cau truc chinh:
- `Program.cs`: diem khoi dong ung dung Blazor/ASP.NET Core, cau hinh service va route.
- `Components/`: to chuc layout, router va cac trang nghiep vu.
- `Services/`: lop goi API va quan ly session/cau hinh phia CMS.
- `Models/`: model binding, DTO view model cho giao dien quan tri.
- `wwwroot/`: CSS, icon, JS va tai nguyen tinh cho giao dien.
- `.keys/`: du lieu khoa/noi dung noi bo phuc vu cau hinh cuc bo cua CMS.
- `Properties/`, `bin/`, `obj/`: metadata va artifact build/runtime.

Chi tiet `Components/`:
- `App.razor`: shell goc cua ung dung.
- `Routes.razor`: khai bao route cho cac trang.
- `_Imports.razor`: import namespace dung chung cho component.
- `Layout/`: bo cuc tong the, menu dieu huong va modal reconnect.
- `Pages/`: tung man hinh nghiep vu cua admin.

Y nghia `Components/Layout/`:
- `MainLayout.razor`: khung trang chinh cua CMS.
- `NavMenu.razor`: menu dieu huong den cac man hinh quan tri.
- `ReconnectModal.razor`: thong bao mat ket noi/reconnect khi CMS giao tiep voi API.

Y nghia `Components/Pages/`:
- `Login.razor`, `Logout.razor`: dang nhap/dang xuat admin.
- `Home.razor`: trang tong quan sau khi vao he thong.
- `LoaiDiem.razor`: quan ly loai diem tham quan.
- `DiemThamQuan.razor`: quan ly danh sach va thong tin chi tiet diem tham quan.
- `NoiDungThuyetMinh.razor`: quan ly bai thuyet minh, ngon ngu va audio.
- `NgonNgu.razor`: quan ly danh muc ngon ngu.
- `NguoiDung.razor`: quan ly nguoi dung he thong.
- `TaiKhoan.razor`: quan ly tai khoan dang nhap/admin.
- `MaQR.razor`: quan ly ma QR lien ket diem tham quan.
- `ThongKe.razor`: thong ke va tong hop du lieu van hanh.
- `Error.razor`, `NotFound.razor`: xu ly loi va route khong ton tai.
- `Counter.razor`, `Weather.razor`: page mau/template Blazor, co the giu lai cho tham khao hoac can don dep sau.

Y nghia `Services/`:
- `CmsApiClient`: lop trung gian goi API va dong nhat xu ly request/response.
- `ApiSettings`: doc cau hinh endpoint backend.
- `CmsSession`: luu/truy xuat phien dang nhap cua admin.

## 5. Module Mobile

Duong dan: `HeThongThuyetMinhDuLich.Mobile/`

Vai tro:
- Ứng dung MAUI cho khach tham quan/nguoi dung cuoi.
- Ho tro quet QR, xem danh sach diem, dinh vi, dong bo du lieu, fallback offline va phat audio thuyet minh.

Cau truc chinh:
- `App.xaml`, `App.xaml.cs`: cau hinh app level, resource toan cuc va khoi tao lifecycle.
- `AppShell.xaml`, `AppShell.xaml.cs`: shell dieu huong chinh cua ung dung.
- `MainPage.xaml`, `MainPage.xaml.cs`: man hinh chinh, ket hop map/danh sach diem va luong tuong tac chinh.
- `AuthPage.xaml`, `AuthPage.xaml.cs`: man hinh xac thuc neu app can dang nhap/nhan phien.
- `QrScannerPage.cs`: trang quet QR de mo nhanh diem tham quan hoac noi dung.
- `MauiProgram.cs`: cau hinh DI, font, service va thu vien can thiet cho MAUI.
- `Services/`: logic goi API, session ngon ngu, cache va dong bo.
- `Models/`: DTO va model hien thi du lieu mobile.
- `Resources/`: localization, asset raw va style giao dien.
- `Platforms/`: code dac thu cho Android, iOS, Windows, MacCatalyst.
- `Helpers/`: cho helper dung chung; hien tai dang trong va co the dung de tach logic ho tro ve sau.



- `obj_clean/`: output tam trong qua trinh don dep/build lai, khong phai source nghiep vu.

Y nghia `Services/`:
- `MobileApiClient`: goi backend API tu mobile.
- `MobileCacheStore`: cache du lieu cuc bo de ho tro offline-first.
- `SyncService`: dong bo du lieu giua API va bo nho cuc bo.
- `LanguageService`: quan ly ngon ngu giao dien, luu Preferences va doc chuoi tu resource.
- `AuthSession`: giu trang thai phien dang nhap/nhan dien nguoi dung tren mobile.

Y nghia `Models/`:
- `DiemThamQuan.cs`: model diem tham quan dung trong UI va xu ly nghiep vu mobile.
- `MobileDtos.cs`: tap hop DTO trao doi du lieu giua mobile va API.

Y nghia `Resources/`:
- `Localization/`: file `AppStrings*.resx` va lop phuc vu da ngon ngu giao dien.
- `Styles/`: mau giao dien, mau sac, style control.
- `Raw/`: file nhung truc tiep vao app, du lieu tinh hoac tai nguyen khong can bien doi.

Y nghia `Platforms/`:
- `Android/`: manifest, permission, cau hinh Google Maps, camera, location va platform service Android.
- `iOS/`, `MacCatalyst/`, `Windows/`: entrypoint/cau hinh dac thu cho tung he dieu hanh duoc MAUI ho tro.

## 6. Log va script van hanh

Thu muc/file nay khong phai module nghiep vu, nhung rat quan trong cho qua trinh chay thu va debug:

- `Run-DoAn.ps1`: build + run API/CMS, ho tro online/offline va tu dong xu ly port.
- `Clean-DoAn.ps1`: don `bin/`, `obj/`, log va artifact build de tra workspace ve trang thai sach.
- `fix-full.ps1`: script tong hop de xu ly cac loi moi truong, build hoac runtime da gap.
- `Capture-MobileLogcat.ps1`: thu logcat tu Android de debug mobile.
- `Install-MobileToEmulator.ps1`: cai ban mobile len emulator/deployment nhanh.
- `Rebuild-AllAudio.ps1`: generate lai toan bo audio TTS tu noi dung hien co.
- `Export-PoiQrImages.ps1`: xuat lai anh QR hang loat tu CSDL theo gia tri QR hien tai cua tung POI.
- `api.build.log`, `api.run.log`, `api.run.err.log`: log build/chay backend.
- `cms.build.log`, `cms.run.log`, `cms.run.err.log`: log build/chay CMS.
- `logs/mobile-logcat-*.txt`: log chay thuc te tu mobile, huu ich khi tra loi crash, permission va sync.

## 7. Thu muc/runtime files can luu y

Nhung nhom thu muc/file sau chu yeu la artifact runtime, khong nen xem la source nghiep vu:

- `bin/`, `obj/`, `obj_clean/`: output build tam thoi.
- `.vs/`: cache/thiet lap cuc bo cua Visual Studio.
- `*.log`: log khi build, chay va debug.
- `*.db-shm`, `*.db-wal`: file journal/phu tro cua SQLite.

Khuyen nghi quan ly source:
- Khong commit artifact build, log va file lock/runtime tam.
- Neu can nop bai hoac backup, uu tien giu source, tai lieu va script van hanh; bo qua output sinh tu build.
