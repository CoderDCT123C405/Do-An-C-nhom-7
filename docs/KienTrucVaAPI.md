# Kien truc he thong va API cot loi

## 1. Kien truc tong the

He thong duoc xay dung theo mo hinh Client - Server, gom 3 thanh phan chinh:

- Mobile App: ung dung danh cho du khach tren Android/iOS
- Web API: cung cap du lieu, xu ly nghiep vu va dong bo
- CMS Web: giao dien quan tri noi dung

## 2. Thanh phan chuc nang

### 2.1 Mobile App

Mobile App dam nhan cac chuc nang:

- Dang nhap nguoi dung
- Lay vi tri GPS
- Kiem tra khoang cach den diem tham quan
- Tu dong phat thuyet minh khi vao geofence
- Quet ma QR de mo noi dung
- Hien thi ban do va thong tin diem tham quan
- Chon ngon ngu thuyet minh
- Xem lich su nghe

### 2.2 Web API

Web API dam nhan:

- Xac thuc tai khoan quan tri va nguoi dung
- Cung cap danh sach diem tham quan
- Cung cap noi dung thuyet minh theo ngon ngu
- Cung cap thong tin ma QR
- Ghi nhan lich su phat
- Quan ly tai nguyen media

### 2.3 CMS Web

CMS phuc vu quan tri vien voi cac chuc nang:

- Dang nhap he thong
- Quan ly loai diem tham quan
- Quan ly diem tham quan
- Quan ly noi dung thuyet minh
- Quan ly hinh anh
- Quan ly ma QR
- Theo doi lich su phat

## 3. Luong nghiep vu chinh

### 3.1 Luong tu dong phat theo GPS

1. Nguoi dung mo ung dung.
2. Ung dung lay danh sach diem tham quan tu API.
3. Ung dung lay vi tri hien tai cua nguoi dung.
4. He thong tinh khoang cach tu nguoi dung den cac diem tham quan.
5. Neu nguoi dung di vao vung geofence, ung dung kich hoat noi dung.
6. Ung dung phat audio hoac TTS theo ngon ngu da chon.
7. Ung dung gui lich su phat ve API.

### 3.2 Luong quet QR

1. Nguoi dung mo tinh nang quet QR.
2. Ung dung doc ma QR.
3. Ung dung gui ma QR len API hoac doi chieu du lieu da dong bo.
4. He thong tra ve thong tin diem tham quan va noi dung tuong ung.
5. Ung dung hien thi chi tiet va phat thuyet minh.
6. Ung dung ghi nhan lich su phat.

### 3.3 Luong quan tri noi dung

1. Quan tri vien dang nhap CMS.
2. Quan tri vien them hoac sua diem tham quan.
3. Quan tri vien cap nhat noi dung thuyet minh theo tung ngon ngu.
4. Quan tri vien tai len hinh anh va ma QR.
5. CMS gui du lieu qua API de luu vao co so du lieu.

## 4. API cot loi de trien khai

### 4.1 API xac thuc

- `POST /api/auth/admin/login`
- `POST /api/auth/user/login`
- `POST /api/auth/user/register`

### 4.2 API ngon ngu

- `GET /api/ngonngu`

### 4.3 API loai diem tham quan

- `GET /api/loaidiemthamquan`
- `POST /api/loaidiemthamquan`
- `PUT /api/loaidiemthamquan/{id}`
- `DELETE /api/loaidiemthamquan/{id}`

### 4.4 API diem tham quan

- `GET /api/diemthamquan`
- `GET /api/diemthamquan/{id}`
- `GET /api/diemthamquan/gan-day?vido={value}&kinhdo={value}`
- `POST /api/diemthamquan`
- `PUT /api/diemthamquan/{id}`
- `DELETE /api/diemthamquan/{id}`

### 4.5 API noi dung thuyet minh

- `GET /api/noidungthuyetminh/diem/{maDiem}`
- `GET /api/noidungthuyetminh/diem/{maDiem}/ngonngu/{maNgonNgu}`
- `POST /api/noidungthuyetminh`
- `PUT /api/noidungthuyetminh/{id}`
- `DELETE /api/noidungthuyetminh/{id}`

### 4.6 API hinh anh diem tham quan

- `GET /api/hinhanh/diem/{maDiem}`
- `POST /api/hinhanh`
- `DELETE /api/hinhanh/{id}`

### 4.7 API ma QR

- `GET /api/maqr/{giaTriQR}`
- `POST /api/maqr`
- `DELETE /api/maqr/{id}`

### 4.8 API lich su phat

- `GET /api/lichsuphat`
- `GET /api/lichsuphat/nguoidung/{maNguoiDung}`
- `POST /api/lichsuphat`
- `GET /api/thongke/luot-nghe-theo-diem`
- `GET /api/thongke/luot-nghe-theo-kich-hoat`

## 5. Doi tuong du lieu chinh cua API

### 5.1 DiemThamQuanDto

- `MaDiem`
- `MaDinhDanh`
- `TenDiem`
- `MoTaNgan`
- `ViDo`
- `KinhDo`
- `BanKinhKichHoat`
- `DiaChi`
- `MaLoai`
- `NgayCapNhat`

### 5.2 NoiDungThuyetMinhDto

- `MaNoiDung`
- `MaDiem`
- `MaNgonNgu`
- `TieuDe`
- `NoiDungVanBan`
- `DuongDanAmThanh`
- `ChoPhepTTS`
- `ThoiLuongGiay`

### 5.3 LichSuPhatDto

- `MaNguoiDung`
- `MaDiem`
- `MaNoiDung`
- `CachKichHoat`
- `ThoiGianBatDau`
- `ThoiLuongDaNghe`

## 6. Kien nghi huong trien khai

De phu hop voi do an, nen chia thanh 3 giai doan:

### Giai doan 1

- Hoan tat co so du lieu
- Dung Web API CRUD
- Test bang Postman

### Giai doan 2

- Dung CMS quan tri
- Quan ly diem tham quan, noi dung, QR, hinh anh

### Giai doan 3

- Dung Mobile App
- Tich hop GPS
- Quet QR
- Phat audio/TTS
- Ghi nhan lich su phat
