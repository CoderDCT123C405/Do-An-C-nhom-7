# PRODUCT REQUIREMENTS DOCUMENT (PRD)

## HeThongThuyetMinhDuLich

---

## 0. Thong tin chung
- Ngay cap nhat: 26/03/2026
- Trang thai: MVP + Admin CMS da nang cap theo huong doanh nghiep
- Phien ban: v1.1

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
