# BO TEST CASE ADMIN CMS - HETHONG THUYET MINH DU LICH

## 1. Muc tieu
- Xac nhan CMS Admin dap ung dung yeu cau de tai MVP.
- Danh gia chat luong code admin theo tieu chuan doanh nghiep: dung chuc nang, on dinh, bao mat, kha nang mo rong.
- Lam co so cho UAT, regression test va bao cao bao ve do an.

## 2. Pham vi
- Dang nhap admin.
- Quan ly Loai Diem Tham Quan.
- Quan ly Diem Tham Quan.
- Thong ke luot nghe.
- Bao mat co ban cho API/CMS admin.

Khong nam trong pham vi tai lieu nay:
- UI/UX mobile chi tiet.
- Kiem thu hieu nang tai lon (load test > 5.000 req/phut).

## 3. Tai lieu doi chieu
- `YeuCau.md` (muc tieu he thong, chuc nang MVP).
- `PRD.md` (pham vi, API, CMS, offline-first).
- Ma nguon CMS/API hien co.

## 4. Moi truong va du lieu test
- API: `http://localhost:5000`
- CMS: `http://localhost:5256`
- DB: SQL Server (online mode)
- Tai khoan admin test:
  - Username: `admin`
  - Password: `Admin@123`

Du lieu mau toi thieu:
- 2 Loai diem dang hoat dong.
- 5 Diem tham quan thuoc nhieu loai khac nhau.
- Co du lieu lich su phat de test thong ke.

## 5. Quy uoc danh gia
- Priority:
  - `P0`: Loi nghiem trong, chan nghiep vu chinh.
  - `P1`: Loi quan trong, anh huong lon den van hanh.
  - `P2`: Loi trung binh/thap, khong chan nghiep vu.
- Ket qua test:
  - `PASS`: Dung expected result.
  - `FAIL`: Lech expected result.
  - `BLOCKED`: Khong the test do phu thuoc.

## 6. Requirement Traceability Matrix (RTM)
| Requirement | Mo ta | Test case chinh |
| --- | --- | --- |
| RQ-ADM-01 | Admin dang nhap CMS | TC-LOGIN-01..10 |
| RQ-ADM-02 | Quan ly Loai diem | TC-LOAI-01..12 |
| RQ-ADM-03 | Quan ly Diem tham quan | TC-DIEM-01..18 |
| RQ-ADM-04 | Xem thong ke luot nghe | TC-TK-01..08 |
| RQ-ADM-05 | Bao mat va phan quyen | TC-SEC-01..10 |
| RQ-ADM-06 | Xu ly loi va on dinh he thong | TC-REL-01..08 |

## 7. Bo test case chi tiet

### 7.1 Dang nhap Admin
| ID | Muc tieu | Precondition | Buoc test | Expected result | Priority |
| --- | --- | --- | --- | --- | --- |
| TC-LOGIN-01 | Dang nhap thanh cong voi tai khoan hop le | Co tai khoan admin | Nhap dung username/password, bam Login | Dang nhap thanh cong, vao trang admin, token duoc luu session | P0 |
| TC-LOGIN-02 | Bao loi khi sai password | Co tai khoan admin | Nhap username dung, password sai | Bao loi ro rang, khong vao duoc he thong | P0 |
| TC-LOGIN-03 | Bao loi khi sai username | Co tai khoan admin | Nhap username sai, password bat ky | Bao loi ro rang, khong cap token | P0 |
| TC-LOGIN-04 | Validate rong username | Trang login mo san | De trong username, nhap password | Hien message bat buoc nhap username | P1 |
| TC-LOGIN-05 | Validate rong password | Trang login mo san | Nhap username, de trong password | Hien message bat buoc nhap password | P1 |
| TC-LOGIN-06 | Chong khoang trang dau/cuoi | Co tai khoan admin | Nhap ` admin ` va ` Admin@123 ` | He thong trim du lieu hoac bao loi nhat quan | P1 |
| TC-LOGIN-07 | Kiem tra token het han | Da dang nhap va token het han | Goi API sau khi token het han | He thong yeu cau dang nhap lai, khong crash | P1 |
| TC-LOGIN-08 | Kiem tra nut back sau logout | Da dang nhap roi logout | Bam Back tren browser | Khong truy cap duoc trang protected neu chua login lai | P1 |
| TC-LOGIN-09 | Chong submit lien tiep | Trang login mo san | Bam Login lien tuc nhieu lan | Khong tao request loi, UI disable tam thoi hoac xu ly idempotent | P2 |
| TC-LOGIN-10 | Xu ly loi API down khi login | API tam ngung | Dang nhap binh thuong | Bao loi than thien, khong vo han loading | P0 |

### 7.2 Quan ly Loai Diem
| ID | Muc tieu | Precondition | Buoc test | Expected result | Priority |
| --- | --- | --- | --- | --- | --- |
| TC-LOAI-01 | Tai danh sach loai diem | Da login | Mo trang Loai diem | Hien day du danh sach, dung du lieu DB | P0 |
| TC-LOAI-02 | Tao loai diem hop le | Da login, co quyen | Nhap TenLoai hop le, Luu | Tao thanh cong, hien trong danh sach | P0 |
| TC-LOAI-03 | TenLoai bi trung | Da ton tai TenLoai A | Tao moi voi TenLoai A | He thong tu choi hoac bao loi trung du lieu | P1 |
| TC-LOAI-04 | TenLoai rong | Da login | Tao moi, de trong TenLoai | Hien validate, khong tao du lieu | P0 |
| TC-LOAI-05 | TenLoai qua do dai | Da login | Nhap TenLoai vuot max length | Hien validate hop le, khong loi server 500 | P1 |
| TC-LOAI-06 | Ky tu dac biet/XSS trong TenLoai | Da login | Nhap `<script>alert(1)</script>` | Du lieu duoc neutralize, khong thuc thi script | P0 |
| TC-LOAI-07 | Sua loai diem hop le | Da co loai diem | Cap nhat TenLoai/MoTa | Update thanh cong, list cap nhat dung | P0 |
| TC-LOAI-08 | Xoa loai diem khong co lien ket | Co loai diem chua gan diem | Xoa loai diem | Xoa thanh cong, thong bao ro rang | P1 |
| TC-LOAI-09 | Xoa loai diem dang duoc su dung | Loai diem da gan Diem | Xoa loai diem | Tu choi xoa + thong bao ly do rang buoc | P0 |
| TC-LOAI-10 | Tim kiem loai diem | Da co >= 5 loai diem | Tim theo tu khoa | Ket qua dung va nhat quan khi xoa tu khoa | P2 |
| TC-LOAI-11 | Phan trang danh sach | Du lieu >= 30 loai diem | Chuyen trang 1,2,3 | Dung so ban ghi/trang, khong mat du lieu | P2 |
| TC-LOAI-12 | Bat/tat trang thai hoat dong | Co loai diem bat ky | Toggle TrangThaiHoatDong | Trang thai luu dung, anh huong dung den nghiep vu lien quan | P1 |

### 7.3 Quan ly Diem Tham Quan
| ID | Muc tieu | Precondition | Buoc test | Expected result | Priority |
| --- | --- | --- | --- | --- | --- |
| TC-DIEM-01 | Tai danh sach diem tham quan | Da login | Mo trang Diem tham quan | Hien danh sach day du, khong loi | P0 |
| TC-DIEM-02 | Tao diem hop le | Da co it nhat 1 loai diem | Nhap day du truong bat buoc, Luu | Tao thanh cong, hien trong danh sach | P0 |
| TC-DIEM-03 | Validate MaDinhDanh rong | Da login | De trong MaDinhDanh khi tao | Bao loi validate, khong goi API tao | P0 |
| TC-DIEM-04 | Validate TenDiem rong | Da login | De trong TenDiem | Bao loi validate, khong tao | P0 |
| TC-DIEM-05 | Validate ViDo ngoai mien cho phep | Da login | Nhap ViDo > 90 hoac < -90 | Bao loi validate ro rang | P0 |
| TC-DIEM-06 | Validate KinhDo ngoai mien cho phep | Da login | Nhap KinhDo > 180 hoac < -180 | Bao loi validate ro rang | P0 |
| TC-DIEM-07 | Validate BanKinhKichHoat am/qua nho | Da login | Nhap ban kinh = 0 hoac am | Tu choi luu, bao loi hop le | P1 |
| TC-DIEM-08 | MaDinhDanh bi trung | Da ton tai MaDinhDanh | Tao moi voi ma trung | Tu choi tao, bao loi trung du lieu | P1 |
| TC-DIEM-09 | Sua thong tin diem hop le | Da co diem | Chinh sua TenDiem/DiaChi/BanKinh | Cap nhat thanh cong, reload van dung | P0 |
| TC-DIEM-10 | Xoa diem tham quan | Da co diem co the xoa | Xoa 1 diem | Xoa thanh cong, danh sach cap nhat | P1 |
| TC-DIEM-11 | Tim kiem theo TenDiem | Danh sach >= 10 diem | Nhap tu khoa tim kiem | Loc dung, khong phan biet hoa thuong | P1 |
| TC-DIEM-12 | Loc theo Loai diem | Da co nhieu loai | Chon bo loc loai | Ket qua dung theo loai da chon | P2 |
| TC-DIEM-13 | Luu thong tin dia chi unicode | Da login | Nhap dia chi co dau TV | Luu va hien thi dung, khong loi encoding | P1 |
| TC-DIEM-14 | Xu ly loi khi API timeout | Gia lap API cham/timeout | Tao hoac sua diem | Hien thong bao loi than thien, cho phep thu lai | P1 |
| TC-DIEM-15 | Kiem tra nhat quan du lieu sau refresh | Vua tao/sua diem | F5 hoac mo lai trang | Du lieu van chinh xac, khong mat cap nhat | P1 |
| TC-DIEM-16 | Chong XSS truong mo ta | Da login | Nhap script trong MoTaNgan | Du lieu duoc encode, khong execute script | P0 |
| TC-DIEM-17 | Kiem tra mapping API-CMS | Da login | Tao diem tu CMS, query truc tiep API | Du lieu CMS gui dung schema API | P1 |
| TC-DIEM-18 | Kiem tra xoa trong tinh huong canh tranh | 2 tab cung mo 1 ban ghi | Tab A xoa, Tab B xoa tiep | Tab B nhan thong bao du lieu khong ton tai, khong vo 500 | P2 |

### 7.4 Thong ke
| ID | Muc tieu | Precondition | Buoc test | Expected result | Priority |
| --- | --- | --- | --- | --- | --- |
| TC-TK-01 | Tai thong ke luot nghe theo diem | Da co du lieu lich su phat | Mo trang Thong ke | Hien dung danh sach ten diem va so luot | P0 |
| TC-TK-02 | Du lieu thong ke khi khong co lich su | DB khong co lich su phat | Mo trang Thong ke | Hien 0 hoac trang thai khong du lieu, khong crash | P1 |
| TC-TK-03 | Kiem tra sap xep giam dan theo so luot | Co nhieu diem va luot nghe | Mo thong ke | Thu tu dung theo nghiep vu (neu co yeu cau sap xep) | P2 |
| TC-TK-04 | Doi chieu tong so lieu voi DB | Co script SQL doi chieu | So sanh CMS va query SQL | Ket qua khop, sai so = 0 (hoac dung quy tac lam tron) | P0 |
| TC-TK-05 | Cap nhat thong ke sau du lieu moi | Vua phat sinh lich su phat moi | Reload trang thong ke | So lieu thay doi dung theo ban ghi moi | P1 |
| TC-TK-06 | Xu ly loi API thong ke khong kha dung | API thong ke loi | Mo thong ke | Bao loi than thien, UI khong treo | P1 |
| TC-TK-07 | Hieu nang tai trang thong ke | Co >= 1000 ban ghi lich su | Mo thong ke | Thoi gian tai trang dat muc chap nhan (<3s noi bo) | P2 |
| TC-TK-08 | Bao mat endpoint thong ke | Chua login | Goi endpoint thong ke | Bi tu choi truy cap neu endpoint yeu cau auth | P0 |

### 7.5 Bao mat va phan quyen
| ID | Muc tieu | Precondition | Buoc test | Expected result | Priority |
| --- | --- | --- | --- | --- | --- |
| TC-SEC-01 | Endpoint tao loai diem yeu cau auth | Chua login | Goi `POST /api/loaidiemthamquan` | Nhan 401/403, khong tao du lieu | P0 |
| TC-SEC-02 | Truy cap trang admin khi chua login | Chua co session | Truy cap URL trang quan tri truc tiep | Bi dieu huong login hoac chan truy cap | P0 |
| TC-SEC-03 | Token gia/khong hop le | Co token sai | Goi API admin voi token sai | Nhan 401/403 | P0 |
| TC-SEC-04 | Kiem tra logout xoa session | Da login | Logout, sau do goi API admin | Token/session bi huy, khong truy cap tiep | P1 |
| TC-SEC-05 | Kiem tra luu token an toan | Da login | Kiem tra cach luu token client | Khong lo token qua URL/log plaintext | P1 |
| TC-SEC-06 | Chong SQL Injection o truong tim kiem | Da login | Nhap payload SQLi vao search | Khong loi he thong, khong ro ri du lieu | P0 |
| TC-SEC-07 | Chong CSRF cho thao tac thay doi du lieu | Da login | Thu request tao/sua/xoa tu nguon la | Request khong hop le bi chan theo co che bao ve | P1 |
| TC-SEC-08 | Kiem tra CORS policy | Chua login | Goi API tu origin la | Tu choi theo chinh sach cau hinh | P2 |
| TC-SEC-09 | Han che thong tin loi noi bo | API gay exception | Quan sat thong bao tren CMS/API | Khong tra stack trace nhay cam cho user | P1 |
| TC-SEC-10 | Audit thao tac quan trong | Da login | Tao/Sua/Xoa du lieu | Co log theo doi toi thieu: ai, khi nao, hanh dong gi | P2 |

### 7.6 On dinh, xu ly loi, kha dung
| ID | Muc tieu | Precondition | Buoc test | Expected result | Priority |
| --- | --- | --- | --- | --- | --- |
| TC-REL-01 | Xu ly mat ket noi API luc dang su dung | Dang mo CMS | Tat API dot ngot, thao tac tren CMS | UI thong bao mat ket noi, khong crash | P1 |
| TC-REL-02 | Recover sau khi API khoi phuc | API vua khoi phuc lai | Reload/thuc hien thao tac tiep | He thong hoat dong lai binh thuong | P1 |
| TC-REL-03 | Kiem tra thong diep loi than thien | Tao tinh huong loi validate/server | Quan sat message hien thi | Message de hieu, huong dan duoc nguoi dung | P2 |
| TC-REL-04 | Kiem tra duplicate submit khi mang cham | Mang tre cao | Bam Luu nhieu lan lien tiep | Khong tao trung ban ghi | P1 |
| TC-REL-05 | Kiem tra tinh nhat quan sau loi 500 | Gia lap loi server khi luu | Thu tao/sua du lieu | Du lieu khong bi ghi nua chung, thong bao that bai | P1 |
| TC-REL-06 | Kiem tra UI tren man hinh nho | Trinh duyet width <= 768 | Mo cac trang admin chinh | Layout khong vo, thao tac duoc | P2 |
| TC-REL-07 | Kiem tra kha dung ban phim | Trang admin mo san | Dung tab/enter de thao tac form | Trinh tu focus hop ly, submit duoc bang ban phim | P2 |
| TC-REL-08 | Kiem tra unicode/tieng Viet toan man hinh | Du lieu TV co dau | Mo login/list/form/thong ke | Khong loi font/encoding, hien thi dung dau TV | P1 |

## 8. Checklist danh gia code admin (cho reviewer/giang vien)
Danh dau `Dat/Chua dat` theo tung muc:
- [ ] Co phan tach ro UI layer, service layer, model/DTO.
- [ ] API call co xu ly exception va fallback thong bao loi.
- [ ] Validate client + validate server dong bo nhau.
- [ ] Endpoint thay doi du lieu duoc bao ve auth/authorize.
- [ ] Khong hard-code secret/token/password trong code.
- [ ] Co test cho boundary values (lat/lng, ban kinh, length text).
- [ ] Co xu ly duplicate submit/idempotency co ban.
- [ ] Co log du de truy vet su co.
- [ ] Co kha nang mo rong cho vai tro khac ngoai admin.
- [ ] Code de doc, dat ten ro nghia, khong duplicate logic nhieu noi.

## 9. Tieu chi nghiem thu de xuat
- Dat 100% test case P0.
- Dat >= 90% test case P1.
- Khong con loi Critical/High mo o cac module admin chinh.
- Khong co loi bao mat muc co ban (unauthorized create/update/delete, XSS ro rang, token leakage).

## 10. Ghi chu trien khai
- Bo test case nay duoc thiet ke de danh gia ca 2 muc:
  - Muc nghiep vu (dung chuc nang theo de tai).
  - Muc ky thuat doanh nghiep (bao mat, on dinh, maintainability).
- Neu hien tai CMS chua day du tat ca form CRUD, van giu nguyen test case de lam backlog kiem thu sau khi bo sung chuc nang.
