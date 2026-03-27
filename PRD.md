# PRODUCT REQUIREMENTS DOCUMENT (PRD)

## HeThongThuyetMinhDuLich

---

## 0. Thong tin chung
- Ngay cap nhat: 27/03/2026
- Trang thai: MVP + Admin CMS + huong dan van hanh da chuan hoa
- Phien ban: v1.2

---

## 1. Tong quan he thong
He thong gom 3 module:
- Mobile App (.NET MAUI): tra cuu diem, nghe thuyet minh, ho tro offline-first
- Web API (ASP.NET Core): cung cap API, auth, CRUD, thong ke
- CMS (Blazor Server): quan tri du lieu, dashboard, thao tac admin

Muc tieu:
- Quan tri diem tham quan/loai diem nhanh, an toan du lieu
- API ro rang, co validate, de tich hop
- UI admin hien dai, thao tac muot, dung duoc trong moi truong that

---

## 2. Kien truc tong the
- SQL Server <-> ASP.NET Core API <-> CMS / Mobile
- Mobile ho tro cache SQLite de doc offline
- CMS su dung server-side interactive rendering

Ky thuat chinh:
- RESTful API
- Dependency Injection
- Layered Architecture
- Offline-first cho Mobile

---

## 3. Pham vi chuc nang
### 3.1 API
- Auth dang nhap (admin)
- CRUD LoaiDiemThamQuan
- CRUD DiemThamQuan
- Endpoint lich su, thong ke co ban
- Validate input + xu ly conflict

### 3.2 CMS Admin
- Dang nhap/duyet phien admin
- Dashboard tong quan
- Quan ly Loai diem (them, sua, tim kiem, an)
- Quan ly Diem tham quan (them, sua, tim kiem, loc, an)
- Giao dien menu + layout admin thong nhat

### 3.3 Mobile
- Hien thi diem
- Tim diem gan
- Dong bo offline-first

---

## 4. Yeu cau phi chuc nang
- UI co tinh on dinh, khong vo style khi tai lai
- Event click phai hoat dong on dinh tren Blazor interactive
- API tra ve ma loi ro rang cho truong hop conflict/du lieu khong hop le
- Uu tien an toan du lieu: soft delete thay vi xoa cung

---

## 5. Cap nhat qua trinh thuc hien (thuc te da lam)
### Dot 1 - Hoan thien bo testcase admin
- Xay dung bo testcase cho luong admin: dang nhap, CRUD, tim kiem, loc, thong ke, bao ve du lieu
- Bo sung testcase cho tinh huong loi va tinh huong bien (du lieu trung, du lieu rong, rang buoc xoa)

### Dot 2 - Nang cap API theo huong doanh nghiep
- AuthController: bo sung validate input, chuan hoa xu ly login
- AdminSeedService: seed tai khoan admin co hash
- LoaiDiemThamQuanController:
  - validate ten loai
  - xu ly conflict khi trung ten
  - bo sung soft delete (an)
  - chan xoa/an khi con diem dang hoat dong
- DiemThamQuanController:
  - validate thong tin diem
  - xu ly conflict du lieu
  - soft delete bang TrangThaiHoatDong = false

### Dot 3 - Nang cap CMS Admin UI/UX va do on dinh
- Chuyen App.razor sang cau truc day du (head/body/base href)
- Dung duong dan static tuyet doi:
  - /app.css
  - /_framework/blazor.web.js
  - /favicon.png
- Sua tinh trang click event khong bat duoc do render mode/script
- Cai thien layout/menu/trang Home/Login/LoaiDiem/DiemThamQuan/ThongKe
- Form "Them" chi mo khi can (an/hiem theo nut)
- Tim kiem/loc/sua/xoa duoc dong bo theo luong su dung that

### Dot 4 - Xu ly su co runtime va van hanh
- Xu ly loi lock file dotnet khi build/chay
- Tach ro van de login va van de static resource 404
- Chuan hoa script restart API/CMS de giam xung dot process

---

## 6. Nguyen tac an toan du lieu
- Uu tien soft delete cho du lieu danh muc va diem
- Khong cho thao tac an/xoa neu vi pham rang buoc nghiep vu
- Ghi nhan va tra thong bao loi ro rang cho nguoi dung

---

## 7. Tieu chi chap nhan (Acceptance)
- Admin thao tac duoc them/sua/tim/loc/an tren LoaiDiem va Diem
- Cac nut tren CMS bat su kien click on dinh
- Khong con loi 404 do static path sai cau hinh
- API tra ket qua dung cho testcase chinh va testcase bien
- Du lieu khong bi mat do xoa cung ngoai y muon

---

## 8. Huong phat trien tiep
- Bo sung audit log cho thao tac admin
- Bo sung role/permission chi tiet hon
- Bo sung test tu dong (integration + UI smoke test)
- Hoan thien quy trinh CI/CD

---

## 9. Van hanh va chay he thong (chuan)
### 9.1 Chay API + CMS bang 1 lenh
- Tu thu muc goc solution:
  - `powershell -ExecutionPolicy Bypass -File .\Run-DoAn.ps1 -Mode offline`
- Muc tieu:
  - API: `http://localhost:5000`
  - CMS: tu dong gan cong trong dai 5256+

### 9.2 Chay Mobile theo huong nhe (khuyen nghi tren may yeu)
- Uu tien chay ban Windows thay vi Android emulator:
  - `dotnet build .\HeThongThuyetMinhDuLich.Mobile\HeThongThuyetMinhDuLich.Mobile.csproj -f net10.0-windows10.0.19041.0`
  - `dotnet run --project .\HeThongThuyetMinhDuLich.Mobile\HeThongThuyetMinhDuLich.Mobile.csproj -f net10.0-windows10.0.19041.0`
- Khong dung `-r win-x64` trong luong chay thuong ngay neu gap loi runtime pack.

### 9.3 Chay Mobile Android (khi can test thiet bi)
- Co emulator:
  - `powershell -ExecutionPolicy Bypass -File .\Run-Mobile.ps1`
- Co dien thoai that:
  - `powershell -ExecutionPolicy Bypass -File .\Run-Mobile.ps1 -SkipEmulator`

---

## 10. Su co thuong gap va cach xu ly nhanh
### 10.1 CMS bao loi 404 static resource
- Kiem tra API/CMS da chay dung script `Run-DoAn.ps1`
- Reload trang sau khi service len
- Neu van loi: restart lai API + CMS bang script de dong bo process/port

### 10.2 `MSB3073` / code `9009` khi run Mobile Windows
- Nguyen nhan: file `.exe` chua duoc tao nhung da run `--no-build`
- Cach xu ly:
  - build lai target windows
  - run lai khong dung `--no-build` o lan dau

### 10.3 `NU1102 Microsoft.NETCore.App.Runtime.Mono.win-x64`
- Thuong do dung them `-r win-x64` khi chay MAUI Windows
- Cach xu ly:
  - bo `-r win-x64`
  - `restore -> build -> run` voi framework windows

### 10.4 Loi Android `XARDF7024` / access denied trong `obj\...\android`
- Nguyen nhan: thu muc trung gian bi lock
- Cach xu ly:
  - tat emulator/adb
  - clean lai dung target windows neu chi can test desktop
  - neu test android, clean obj/bin roi build lai sau khi da giai phong process
