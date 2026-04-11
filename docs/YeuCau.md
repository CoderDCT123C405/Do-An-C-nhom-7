TAI LIEU PHAN TICH & THIET KE HE THONG
He thong thuyet minh du lich tu dong (Audio Guide App)

1. Muc tieu
- Tu dong phat noi dung thuyet minh khi nguoi dung di vao vung POI, giam thao tac thu cong trong qua trinh tham quan.
- Ho tro da ngon ngu cho ca phan hien thi va audio, cho phep nguoi dung chon ngon ngu phu hop.
- Co che do offline de van tra cuu danh sach POI, noi dung va phat audio khi thiet bi mat ket noi mang.
- Quan ly noi dung tap trung qua CMS Web, bao gom cap nhat POI, noi dung thuyet minh, QR va tai nguyen audio.

2. Thanh phan
- Mobile App: .NET MAUI.
- API: ASP.NET Core + SQL Server.
- CMS: Blazor Web.
- Offline: SQLite tren mobile.

3. Chuc nang MVP
- GPS tracking.
- Geofence trigger.
- Phat audio.
- Map + danh sach POI.
- QR kich hoat thu cong.
- Luu lich su phat.

4. Hien trang sau khoi phuc
- API da khoi phuc tu git object va build OK.
- CMS + Mobile da tao lai project khung.
- Can tiep tuc re-implement cac module da lam truoc do (UI CMS, GPS/map/geofence/TTS/offline mobile, testcase docs).

5. Dinh huong da ngon ngu cho app va POI
- App phai ho tro toi thieu 3 ngon ngu giao dien: Tieng Viet, English, 简体中文.
- Tren man hinh chinh cua mobile app phai co 1 nut `Ngon ngu` o goc tren cung ben phai.
- Khi nguoi dung bam nut nay va chon ngon ngu, toan bo app phai chuyen sang ngon ngu da chon, bao gom:
	- tieu de man hinh,
	- nhan nut bam,
	- menu,
	- thong bao,
	- nhan trang thai,
	- ten muc chuc nang,
	- noi dung POI uu tien theo ngon ngu da chon,
	- audio uu tien theo ngon ngu da chon.
- Co che nay la co che doi ngon ngu toan cuc cho app, khong chi doi rieng noi dung POI.
- Lua chon ngon ngu cua nguoi dung phai duoc luu lai de lan mo app sau khong can chon lai.
- Ban don gian nhat cua san pham nen dung 1 lua chon ngon ngu chung cho ca giao dien, noi dung POI va audio; chua can tach 2 picker rieng neu muc tieu la de moi nguoi deu de dung.
- He thong phai luu lua chon ngon ngu cua nguoi dung de lan mo app sau tu dong ap dung lai.

6. Yeu cau hien thi POI theo ngon ngu
- Khi nguoi dung chon ngon ngu Anh hoac Trung, danh sach POI, chi tiet POI, tieu de, mo ta va noi dung thuyet minh phai uu tien hien thi theo ngon ngu da chon.
- Neu POI co noi dung dung ngon ngu da chon:
	- hien thi noi dung dung ngon ngu do,
	- phat audio dung ngon ngu do.
- Neu POI khong co noi dung theo ngon ngu da chon:
	- he thong khong duoc hien trang trong hoac bao loi kho hieu,
	- phai hien thong bao than thien rang POI chua ho tro ngon ngu nay,
	- phai dua ra danh sach cac ngon ngu thay the ma POI hien co,
	- nguoi dung chon 1 ngon ngu thay the de xem va nghe ngay.
- Thu tu fallback de xuat:
	- ngon ngu nguoi dung dang chon,
	- ngon ngu mac dinh cua tai khoan/ung dung,
	- Tieng Viet,
	- ngon ngu dau tien ma POI dang co.
- Man hinh can hien thi ro nhan trang thai, vi du: "Dang xem: English | Audio: 中文 | POI nay khong co English, dang dung Tieng Viet thay the".

7. Yeu cau du lieu va CMS
- Moi POI nen co it nhat 1 noi dung thuyet minh mac dinh bang Tieng Viet.
- CMS phai cho phep bien tap noi dung POI theo tung ngon ngu rieng biet.
- CMS phai hien thi ro POI dang ho tro nhung ngon ngu nao va con thieu ngon ngu nao.
- CMS nen co canh bao khi POI chua du 3 ngon ngu muc tieu: vi, en, zh-CN.
- Audio phai gan voi tung ban noi dung theo ngon ngu, khong dung chung mot file audio cho nhieu ngon ngu.

8. Yeu cau API de ho tro fallback ngon ngu
- API nen co endpoint tra ve danh sach noi dung cua 1 POI kem danh sach ngon ngu kha dung.
- API nen ho tro truy van theo ngon ngu uu tien, vi du:
	- `maDiem`,
	- `maNgonNguUuTien`,
	- danh sach `fallbackNgonNgu`.
- Phan hoi API nen co thong tin:
	- ngon ngu nguoi dung yeu cau,
	- ngon ngu thuc te dang tra ve,
	- danh sach ngon ngu khac ma POI dang ho tro,
	- audio URL tuong ung voi ngon ngu thuc te.

9. Hanh vi UX de nguoi dung de su dung
- Dat nut chuyen ngon ngu o goc tren cung ben phai cua man hinh chinh.
- Nut nay phai de thay, bam 1 lan la thay duoc danh sach ngon ngu: `Tieng Viet`, `English`, `简体中文`.
- Sau khi chon, giao dien app phai doi ngay ma khong bat nguoi dung di tung man hinh de doi lai.
- Trang Cai dat co the hien lai lua chon nay, nhung khong phai la noi duy nhat de doi ngon ngu.
- Dung ten ngon ngu bang chinh ngon ngu do: `Tieng Viet`, `English`, `简体中文`.
- Khi fallback ngon ngu xay ra, dung hop thoai ngan gon va cho phep chon lai bang 1 cham.
- Neu khong co audio san, he thong uu tien TTS theo dung ngon ngu dang hien thi; neu TTS cung khong ho tro thi moi hien thong bao.
- Khong bat nguoi dung phai vao CMS hay biet ma ngon ngu de tu xu ly.

10. Dinh huong ky thuat trong code hien tai
- Mobile hien da co picker ngon ngu hien thi va audio, dong thoi da co co che fallback theo POI khi ngon ngu dang chon khong ton tai.
- Can nang cap tiep theo:
	- doi tu co che picker noi dung cuc bo sang co che doi ngon ngu toan app,
	- dua nut doi ngon ngu len goc tren cung ben phai,
	- dua tat ca chuoi giao dien vao resource localization (`.resx`) de doi ca nut bam va nhan giao dien,
	- luu ngon ngu da chon vao local storage/preferences,
	- bo sung thong diep fallback hien ro tren giao dien,
	- de API tra ve thong tin fallback ro rang hon thay vi de mobile tu suy doan hoan toan.
