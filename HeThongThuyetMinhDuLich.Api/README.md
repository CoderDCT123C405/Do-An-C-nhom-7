# HeThongThuyetMinhDuLich.Api

## Thanh phan da co

- ASP.NET Core Web API .NET 8
- Entity Framework Core SQL Server
- DbContext va entity theo co so du lieu da chot
- CRUD cho:
  - `LoaiDiemThamQuan`
  - `DiemThamQuan`
  - `NoiDungThuyetMinh`
- CRUD cho:
  - `NgonNgu`
  - `NguoiDung`
  - `MaQR`
  - `LichSuPhat`
- Xac thuc JWT:
  - `POST /api/auth/admin/login`
  - `POST /api/auth/user/login`
  - `POST /api/auth/user/register`

## Cau hinh ket noi

Sua chuoi ket noi trong file `appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=.;Database=HeThongThuyetMinhDuLich;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

## Cau hinh edge-tts

Can cai `edge-tts` tren may chay API. Cau hinh trong `appsettings.Development.json`:

```json
"EdgeTts": {
  "Executable": "edge-tts",
  "Voice": "vi-VN-HoaiMyNeural",
  "AutoGenerateOnSave": false
}
```

Sau khi cai `edge-tts`, co the tao audio:

- Cho 1 noi dung: `POST /api/NoiDungThuyetMinh/{id}/generate-audio`
- Batch toan bo noi dung: `POST /api/NoiDungThuyetMinh/generate-audio`

## Lenh chay de xuat

```powershell
dotnet restore
dotnet build
dotnet run
```

## Cac buoc tiep theo

1. Tao migration dau tien

```powershell
dotnet ef migrations add InitialCreate
dotnet ef database update
```

2. Them controller cho:

- `HinhAnhDiemThamQuan`
- phan upload file thuc te
- phan phan quyen theo vai tro

3. Hoan thien xac thuc va phan quyen:

- Dang nhap quan tri
- Dang nhap nguoi dung
- JWT cho API
- Bao ve cac endpoint quan tri bang `[Authorize]`

4. Them nghiep vu mobile:

- API tim diem gan nhat theo GPS
- API lay noi dung theo QR
- API ghi nhan lich su phat

## Ghi chu

- Project nay duoc dung truc tiep tu mo hinh CSDL trong file SQL hien co.
- Chua co migration va chua duoc build/verify tu moi truong hien tai.
