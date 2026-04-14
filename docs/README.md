# TAI LIEU DU AN

## 1. Diem bat dau nen doc

- `TongQuanDuAn.md`: tom tat nhanh hien trang du an, module va cach chay.
- `PRD.md`: pham vi san pham, muc tieu, acceptance va lo trinh.
- `KienTrucVaAPI.md`: kien truc tong the va danh sach API theo hien trang source.
- `ThuMucCauTruc.md`: so do thu muc va vai tro tung module.

## 2. Tai lieu nghiep vu va kiem thu

- `YeuCau.md`: yeu cau va phan tich nghiep vu tong quan.
- `TestCase.md`: bo test case cho CMS admin.
- `BaoCao_Admin.md`: bao cao tong hop qua trinh hoan thien CMS/API admin.
- `TODO.md`: backlog tai lieu va cong viec tiep theo.

## 3. Tai lieu van hanh va tham chieu

- `../HeThongThuyetMinhDuLich.Api/README.md`: huong dan rieng cho API va edge-tts.
- `sql/`: script SQL ho tro cap nhat schema/du lieu.
- `assets/submission-mainpage.png`: anh chup man hinh phuc vu nop bai/demo.
- `assets/submission-mainpage-cropped.png`: anh crop phuc vu nop bai/demo.

Tac vu QR dang van hanh:
- `sql/regenerate-poi-qr.sql`: dong bo lai gia tri QR cua tat ca POI ve mau `QR_{MaDinhDanh}` va tao ban ghi con thieu.
- `../Export-PoiQrImages.ps1`: tai/xuat lai anh QR hang loat tu gia tri dang co trong database ra `docs/assets/qr/`.

Trinh tu de chot lai QR moi:
1. Chay `sql/regenerate-poi-qr.sql` neu can lam sach hoac tao bu QR cho POI moi.
2. Chay `../Export-PoiQrImages.ps1 -Force` de xuat lai anh QR thay cho QR cu ngoai thuc te.
3. In/dan lai QR moi va test scan tren mobile voi cac POI dai dien.

## 4. Thu tu doc khuyen nghi

1. `TongQuanDuAn.md`
2. `PRD.md`
3. `KienTrucVaAPI.md`
4. `ThuMucCauTruc.md`
5. `BaoCao_Admin.md`
6. `TestCase.md`

## 5. Ghi chu

- Nhanh chinh de lam viec va clone code la `main`.
- Tai lieu trong `docs/` da duoc don lai de phan anh hien trang repo tren `main`.