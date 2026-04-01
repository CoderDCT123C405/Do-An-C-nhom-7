# Kien truc he thong va API cot loi

## 1. Kien truc tong the

He thong theo mo hinh Client - Server, gom 3 module:
- Mobile App (.NET MAUI)
- Web API (ASP.NET Core)
- CMS (Blazor Server)

## 2. Thanh phan chuc nang

### 2.1 Mobile
- Lay danh sach diem tham quan
- Dong bo du lieu va cache offline
- Phat audio noi dung thuyet minh
- Kich hoat qua GPS/QR theo luong nghiep vu

### 2.2 API
- Xac thuc admin va nguoi dung
- CRUD danh muc du lieu (loai diem, diem, ngon ngu, tai khoan, nguoi dung)
- Quan ly noi dung thuyet minh va generate audio edge-tts
- Quan ly QR, hinh anh, lich su phat va thong ke

### 2.3 CMS
- Dang nhap quan tri
- Quan ly danh muc va noi dung
- Theo doi thong ke luot nghe
- Goi API de thao tac du lieu trung tam

## 3. Luong nghiep vu chinh

1. Admin dang nhap CMS -> tao/sua du lieu diem, noi dung, QR, hinh anh.
2. Mobile dong bo du lieu tu API -> luu cache SQLite.
3. Nguoi dung kich hoat noi dung (GPS/QR) -> app phat audio/TTS.
4. Mobile gui lich su phat ve API -> CMS tong hop thong ke.

## 4. Danh sach API theo hien trang source (01/04/2026)

Luu y: nhieu controller dung `[Route("api/[controller]")]`, vi vay route theo ten controller.

### 4.1 Auth (`api/auth`)
- `POST /api/auth`
- `POST /api/auth/admin/login`
- `POST /api/auth/user/login`
- `POST /api/auth/user/register`

### 4.2 LoaiDiemThamQuan (`api/LoaiDiemThamQuan`)
- `GET /api/LoaiDiemThamQuan`
- `GET /api/LoaiDiemThamQuan/{id}`
- `POST /api/LoaiDiemThamQuan`
- `PUT /api/LoaiDiemThamQuan/{id}`
- `DELETE /api/LoaiDiemThamQuan/{id}`

### 4.3 DiemThamQuan (`api/DiemThamQuan`)
- `GET /api/DiemThamQuan`
- `GET /api/DiemThamQuan/{id}`
- `GET /api/DiemThamQuan/gan-day?viDo={value}&kinhDo={value}`
- `POST /api/DiemThamQuan`
- `PUT /api/DiemThamQuan/{id}`
- `DELETE /api/DiemThamQuan/{id}`

### 4.4 NoiDungThuyetMinh (`api/NoiDungThuyetMinh`)
- `GET /api/NoiDungThuyetMinh/diem/{maDiem}`
- `GET /api/NoiDungThuyetMinh/diem/{maDiem}/ngonngu/{maNgonNgu}`
- `POST /api/NoiDungThuyetMinh`
- `PUT /api/NoiDungThuyetMinh/{id}`
- `POST /api/NoiDungThuyetMinh/{id}/generate-audio`
- `POST /api/NoiDungThuyetMinh/generate-audio`
- `DELETE /api/NoiDungThuyetMinh/{id}`

### 4.5 NoiDung (`api/noidung`)
- `GET /api/noidung/{maDiem}`

### 4.6 NgonNgu (`api/NgonNgu`)
- `GET /api/NgonNgu`
- `GET /api/NgonNgu/{id}`
- `POST /api/NgonNgu`
- `PUT /api/NgonNgu/{id}`
- `DELETE /api/NgonNgu/{id}`

### 4.7 NguoiDung (`api/NguoiDung`)
- `GET /api/NguoiDung`
- `GET /api/NguoiDung/{id}`
- `POST /api/NguoiDung`
- `PUT /api/NguoiDung/{id}`
- `DELETE /api/NguoiDung/{id}`

### 4.8 TaiKhoan (`api/TaiKhoan`)
- `GET /api/TaiKhoan`
- `GET /api/TaiKhoan/{id}`
- `POST /api/TaiKhoan`
- `PUT /api/TaiKhoan/{id}`
- `DELETE /api/TaiKhoan/{id}`

### 4.9 MaQr (`api/MaQr` + alias route)
- `GET /api/MaQr`
- `GET /api/MaQr/{id}`
- `GET /api/MaQr/gia-tri/{giaTriQr}`
- `GET /api/maqr/{giaTriQr}`
- `POST /api/MaQr`
- `PUT /api/MaQr/{id}`
- `DELETE /api/MaQr/{id}`

### 4.10 HinhAnhDiemThamQuan (`api/HinhAnhDiemThamQuan`)
- `GET /api/HinhAnhDiemThamQuan/diem/{maDiem}`
- `POST /api/HinhAnhDiemThamQuan/upload`
- `DELETE /api/HinhAnhDiemThamQuan/{id}`

### 4.11 LichSuPhat (`api/LichSuPhat`)
- `GET /api/LichSuPhat`
- `GET /api/LichSuPhat/{id}`
- `GET /api/LichSuPhat/nguoidung/{maNguoiDung}`
- `GET /api/LichSuPhat/thong-ke/luot-nghe-theo-diem`
- `GET /api/LichSuPhat/thong-ke/luot-nghe-theo-kich-hoat`
- `POST /api/LichSuPhat`
- `PUT /api/LichSuPhat/{id}`
- `DELETE /api/LichSuPhat/{id}`

## 5. Doi tuong du lieu chinh

Cac nhom du lieu duoc dung thuong xuyen:
- `DiemThamQuan`
- `LoaiDiemThamQuan`
- `NoiDungThuyetMinh`
- `NgonNgu`
- `MaQr`
- `LichSuPhat`
- `NguoiDung`
- `TaiKhoan`

## 6. Ghi chu trien khai

- Luong generate audio dang dung `edge-tts` local va co the auto-generate khi save noi dung.
- Route API phan lon khong phan biet hoa thuong, nhung khi viet tai lieu nen giu dung theo ten controller de de doi chieu code.
- Neu can de tich hop frontend, nen tao them OpenAPI/Postman collection tu source hien tai de tranh lech route.
