# PRD - Há»‡ thá»‘ng thuyáº¿t minh du lá»‹ch báº±ng GPS

## 1. ThÃ´ng tin chung

- TÃªn Ä‘á» tÃ i: Há»‡ thá»‘ng thuyáº¿t minh du lá»‹ch báº±ng GPS
- PhiÃªn báº£n tÃ i liá»‡u: v1.5
- NgÃ y cáº­p nháº­t: 13/04/2026
- Tráº¡ng thÃ¡i: MVP Ä‘Ã£ cÃ³ API, CMS, Mobile, offline SQLite, GPS/QR trigger, Ä‘a ngÃ´n ngá»¯ vÃ  TTS local

## 2. TÃ³m táº¯t sáº£n pháº©m

Há»‡ thá»‘ng cho phÃ©p khÃ¡ch du lá»‹ch sá»­ dá»¥ng á»©ng dá»¥ng mobile Ä‘á»ƒ nháº­n ná»™i dung thuyáº¿t minh táº¡i Ä‘iá»ƒm tham quan. Ná»™i dung cÃ³ thá»ƒ Ä‘Æ°á»£c kÃ­ch hoáº¡t tá»± Ä‘á»™ng theo GPS, quÃ©t QR hoáº·c chá»n thá»§ cÃ´ng trong á»©ng dá»¥ng. ToÃ n bá»™ dá»¯ liá»‡u trung tÃ¢m Ä‘Æ°á»£c quáº£n lÃ½ qua CMS vÃ  API, trong khi mobile há»— trá»£ cache offline Ä‘á»ƒ váº«n sá»­ dá»¥ng Ä‘Æ°á»£c khi máº¥t káº¿t ná»‘i.

Sáº£n pháº©m gá»“m 3 module chÃ­nh:
- Mobile app cho ngÆ°á»i dÃ¹ng cuá»‘i, xÃ¢y dá»±ng báº±ng .NET MAUI
- Web API cho xá»­ lÃ½ nghiá»‡p vá»¥ vÃ  dá»¯ liá»‡u trung tÃ¢m, xÃ¢y dá»±ng báº±ng ASP.NET Core
- CMS cho quáº£n trá»‹ viÃªn, xÃ¢y dá»±ng báº±ng Blazor Server

## 3. Má»¥c tiÃªu Ä‘á» tÃ i

- Tá»± Ä‘á»™ng hÃ³a tráº£i nghiá»‡m nghe thuyáº¿t minh táº¡i Ä‘iá»ƒm tham quan
- Giáº£m phá»¥ thuá»™c vÃ o hÆ°á»›ng dáº«n viÃªn táº¡i chá»—
- Há»— trá»£ Ä‘á»“ng bá»™ dá»¯ liá»‡u táº­p trung vÃ  váº­n hÃ nh Ä‘Æ°á»£c trong Ä‘iá»u kiá»‡n máº¥t máº¡ng
- Cho phÃ©p quáº£n trá»‹ viÃªn cáº­p nháº­t ná»™i dung, QR, hÃ¬nh áº£nh vÃ  audio tá»« má»™t há»‡ thá»‘ng chung
- Há»— trá»£ Ä‘a ngÃ´n ngá»¯ á»Ÿ cáº£ giao diá»‡n mobile vÃ  ná»™i dung thuyáº¿t minh

## 4. Pháº¡m vi chá»©c nÄƒng hiá»‡n táº¡i

### 4.1 Mobile

- ÄÄƒng nháº­p, Ä‘Äƒng kÃ½ ngÆ°á»i dÃ¹ng
- Äá»“ng bá»™ danh sÃ¡ch Ä‘iá»ƒm tham quan, ná»™i dung, ngÃ´n ngá»¯ vÃ  mÃ£ QR tá»« API
- Cache dá»¯ liá»‡u xuá»‘ng SQLite Ä‘á»ƒ sá»­ dá»¥ng offline
- KÃ­ch hoáº¡t ná»™i dung báº±ng 3 cÃ¡ch: GPS, QR vÃ  chá»n thá»§ cÃ´ng
- PhÃ¡t audio tá»« file cÃ³ sáºµn náº¿u CMS/API Ä‘Ã£ sinh trÆ°á»›c
- Fallback sang ná»™i dung/ngÃ´n ngá»¯ kháº£ dá»¥ng khi khÃ´ng cÃ³ Ä‘Ãºng ngÃ´n ngá»¯ Æ°u tiÃªn
- Há»— trá»£ chá»n ngÃ´n ngá»¯ giao diá»‡n: `vi`, `en`, `zh-CN`
- Há»— trá»£ route test GPS/poi sweep Ä‘á»ƒ demo vÃ  kiá»ƒm thá»­ geofence
- Gá»­i lá»‹ch sá»­ phÃ¡t vá» API khi cÃ³ káº¿t ná»‘i

### 4.2 API

- XÃ¡c thá»±c admin vÃ  ngÆ°á»i dÃ¹ng báº±ng JWT
- Quáº£n lÃ½ cÃ¡c danh má»¥c chÃ­nh:
	- Loáº¡i Ä‘iá»ƒm tham quan
	- Äiá»ƒm tham quan
	- Ná»™i dung thuyáº¿t minh
	- NgÃ´n ngá»¯
	- NgÆ°á»i dÃ¹ng
	- TÃ i khoáº£n
	- MÃ£ QR
	- HÃ¬nh áº£nh Ä‘iá»ƒm tham quan
	- Lá»‹ch sá»­ phÃ¡t
- Cung cáº¥p endpoint láº¥y ná»™i dung theo Ä‘iá»ƒm, theo ngÃ´n ngá»¯ vÃ  fallback ná»™i dung
- Sinh audio thuyáº¿t minh local báº±ng `edge-tts`
- LÆ°u vÃ  phá»¥c vá»¥ Ä‘Æ°á»ng dáº«n audio cho mobile
- Tá»•ng há»£p dá»¯ liá»‡u thá»‘ng kÃª lÆ°á»£t nghe theo Ä‘iá»ƒm vÃ  theo kiá»ƒu kÃ­ch hoáº¡t

### 4.3 CMS

- ÄÄƒng nháº­p há»‡ thá»‘ng quáº£n trá»‹
- Quáº£n lÃ½ loáº¡i Ä‘iá»ƒm vÃ  Ä‘iá»ƒm tham quan
- Quáº£n lÃ½ ná»™i dung thuyáº¿t minh theo ngÃ´n ngá»¯
- Quáº£n lÃ½ mÃ£ QR, tÃ i khoáº£n, ngÆ°á»i dÃ¹ng, ngÃ´n ngá»¯
- Theo dÃµi thá»‘ng kÃª lÆ°á»£t nghe
- Gá»i API Ä‘á»ƒ thÃªm, sá»­a, xÃ³a vÃ  sinh láº¡i audio cho ná»™i dung

## 5. Äá»‘i tÆ°á»£ng sá»­ dá»¥ng

- Quáº£n trá»‹ viÃªn:
	- Quáº£n lÃ½ dá»¯ liá»‡u Ä‘iá»ƒm tham quan vÃ  ná»™i dung thuyáº¿t minh
	- Sinh audio TTS vÃ  theo dÃµi tÃ¬nh tráº¡ng váº­n hÃ nh
- NgÆ°á»i dÃ¹ng cuá»‘i:
	- ÄÄƒng nháº­p hoáº·c Ä‘Äƒng kÃ½ trÃªn mobile
	- Di chuyá»ƒn Ä‘áº¿n Ä‘iá»ƒm tham quan Ä‘á»ƒ nghe thuyáº¿t minh
	- QuÃ©t QR hoáº·c chá»n Ä‘iá»ƒm Ä‘á»ƒ nghe ná»™i dung thá»§ cÃ´ng

## 6. Luá»“ng nghiá»‡p vá»¥ chÃ­nh

### 6.1 Luá»“ng quáº£n trá»‹ ná»™i dung

1. Quáº£n trá»‹ viÃªn Ä‘Äƒng nháº­p CMS.
2. Táº¡o hoáº·c cáº­p nháº­t Ä‘iá»ƒm tham quan.
3. ThÃªm ná»™i dung thuyáº¿t minh theo ngÃ´n ngá»¯.
4. Sinh audio báº±ng `edge-tts` náº¿u cáº§n.
5. Cáº­p nháº­t QR, hÃ¬nh áº£nh vÃ  cÃ¡c thÃ´ng tin liÃªn quan.
6. Dá»¯ liá»‡u Ä‘Æ°á»£c lÆ°u táº¡i SQL Server vÃ  phá»¥c vá»¥ qua API.

HÃ¬nh minh há»a luá»“ng Ä‘Äƒng nháº­p CMS:

![Sequence dang nhap CMS](./assets/login.jpg)

HÃ¬nh minh há»a luá»“ng Ä‘Äƒng xuáº¥t CMS:

![Sequence dang xuat CMS](./assets/logout.jpg)

### 6.2 Luá»“ng sá»­ dá»¥ng trÃªn mobile

1. NgÆ°á»i dÃ¹ng Ä‘Äƒng nháº­p hoáº·c Ä‘Äƒng kÃ½ trÃªn mobile.
2. á»¨ng dá»¥ng Ä‘á»“ng bá»™ dá»¯ liá»‡u tá»« API vÃ  lÆ°u vÃ o SQLite local.
3. Khi ngÆ°á»i dÃ¹ng Ä‘áº¿n gáº§n Ä‘iá»ƒm tham quan, app kiá»ƒm tra vá»‹ trÃ­ vÃ  bÃ¡n kÃ­nh kÃ­ch hoáº¡t.
4. Náº¿u Ä‘Ãºng Ä‘iá»u kiá»‡n, app láº¥y ná»™i dung phÃ¹ há»£p theo ngÃ´n ngá»¯ Æ°u tiÃªn.
5. Náº¿u khÃ´ng cÃ³ Ä‘Ãºng ngÃ´n ngá»¯, há»‡ thá»‘ng fallback sang ná»™i dung kháº£ dá»¥ng.
6. App phÃ¡t audio cÃ³ sáºµn hoáº·c audio TTS Ä‘Ã£ Ä‘Æ°á»£c sinh trÆ°á»›c.
7. á»¨ng dá»¥ng gá»­i lá»‹ch sá»­ phÃ¡t vá» API Ä‘á»ƒ tá»•ng há»£p thá»‘ng kÃª.

HÃ¬nh minh há»a kÃ­ch hoáº¡t theo GPS/geofence:

![Sequence GPS trigger](./assets/gps%20trigger.jpg)

### 6.3 Luá»“ng QR

1. NgÆ°á»i dÃ¹ng má»Ÿ mÃ n quÃ©t QR.
2. App Ä‘á»c giÃ¡ trá»‹ QR vÃ  Ä‘á»‘i chiáº¿u vá»›i dá»¯ liá»‡u Ä‘Ã£ Ä‘á»“ng bá»™.
3. TÃ¬m Ä‘iá»ƒm tham quan tÆ°Æ¡ng á»©ng.
4. Láº¥y ná»™i dung theo ngÃ´n ngá»¯ Æ°u tiÃªn vÃ  phÃ¡t audio.

HÃ¬nh minh há»a luá»“ng quÃ©t QR:

![Sequence QR trigger](./assets/hinhqr.jpg)

## 7. YÃªu cáº§u chá»©c nÄƒng chi tiáº¿t

### 7.1 Äá»“ng bá»™ vÃ  offline

- Mobile pháº£i Ä‘á»c Ä‘Æ°á»£c dá»¯ liá»‡u Ä‘iá»ƒm tham quan vÃ  ná»™i dung khi máº¥t máº¡ng.
- Dá»¯ liá»‡u Ä‘Æ°á»£c cache trong SQLite trÃªn thiáº¿t bá»‹.
- Khi cÃ³ máº¡ng trá»Ÿ láº¡i, mobile Æ°u tiÃªn Ä‘á»“ng bá»™ láº¡i dá»¯ liá»‡u má»›i nháº¥t tá»« API.
- Audio cÃ³ thá»ƒ phÃ¡t tá»« Ä‘Æ°á»ng dáº«n HTTP hoáº·c tá»‡p ná»™i bá»™ Ä‘Ã£ cache/sinh trÆ°á»›c.

HÃ¬nh minh há»a luá»“ng Ä‘á»“ng bá»™ vÃ  offline cache:

![Sequence mobile sync offline](./assets/sync.jpg)

### 7.2 GPS vÃ  geofence

- Há»‡ thá»‘ng xÃ¡c Ä‘á»‹nh vá»‹ trÃ­ ngÆ°á»i dÃ¹ng tá»« GPS trÃªn thiáº¿t bá»‹.
- Kiá»ƒm tra khoáº£ng cÃ¡ch Ä‘áº¿n cÃ¡c Ä‘iá»ƒm tham quan gáº§n Ä‘Ã¢y.
- KÃ­ch hoáº¡t ná»™i dung khi náº±m trong bÃ¡n kÃ­nh cá»§a Ä‘iá»ƒm.
- KhÃ´ng phÃ¡t láº·p láº¡i liÃªn tá»¥c khi ngÆ°á»i dÃ¹ng váº«n Ä‘ang á»Ÿ trong cÃ¹ng má»™t vÃ¹ng.
- Há»— trá»£ route test Ä‘á»ƒ demo logic GPS mÃ  khÃ´ng cáº§n di chuyá»ƒn thá»±c táº¿.

### 7.3 Äa ngÃ´n ngá»¯ vÃ  fallback

- Mobile pháº£i cho phÃ©p chá»n ngÃ´n ngá»¯ giao diá»‡n.
- Ná»™i dung thuyáº¿t minh Ä‘Æ°á»£c Æ°u tiÃªn theo ngÃ´n ngá»¯ ngÆ°á»i dÃ¹ng chá»n.
- Náº¿u Ä‘iá»ƒm tham quan khÃ´ng cÃ³ ná»™i dung Ä‘Ãºng ngÃ´n ngá»¯, há»‡ thá»‘ng pháº£i fallback sang ngÃ´n ngá»¯ kháº£ dá»¥ng.
- ThÃ´ng tin vá» ngÃ´n ngá»¯ Ä‘ang phÃ¡t cáº§n Ä‘Æ°á»£c hiá»ƒn thá»‹ rÃµ trÃªn mobile.

### 7.4 Audio vÃ  TTS

- Ná»™i dung cÃ³ thá»ƒ sá»­ dá»¥ng file audio cÃ³ sáºµn hoáº·c audio sinh báº±ng `edge-tts`.
- API cung cáº¥p chá»©c nÄƒng sinh audio cho tá»«ng ná»™i dung hoáº·c theo yÃªu cáº§u.
- Mobile Æ°u tiÃªn phÃ¡t Ä‘Æ°á»ng dáº«n audio há»£p lá»‡ náº¿u API/CMS Ä‘Ã£ sinh file trÆ°á»›c.
- Há»‡ thá»‘ng pháº£i xá»­ lÃ½ Ä‘Æ°á»£c trÆ°á»ng há»£p Ä‘Æ°á»ng dáº«n audio lÃ  URL HTTP hoáº·c tá»‡p local.

### 7.5 Thá»‘ng kÃª vÃ  lá»‹ch sá»­

- Má»—i láº§n phÃ¡t ná»™i dung cáº§n cÃ³ kháº£ nÄƒng ghi nháº­n lá»‹ch sá»­.
- API tá»•ng há»£p thá»‘ng kÃª lÆ°á»£t nghe theo Ä‘iá»ƒm vÃ  theo kiá»ƒu kÃ­ch hoáº¡t.
- CMS hiá»ƒn thá»‹ thá»‘ng kÃª Ä‘á»ƒ quáº£n trá»‹ viÃªn theo dÃµi má»©c Ä‘á»™ sá»­ dá»¥ng.

## 8. YÃªu cáº§u phi chá»©c nÄƒng

- Kiáº¿n trÃºc dá»¯ liá»‡u chuáº©n: `SQL Server -> API -> Mobile SQLite cache`
- CMS vÃ  API pháº£i hoáº¡t Ä‘á»™ng Ä‘Æ°á»£c trong mÃ´i trÆ°á»ng local Ä‘á»ƒ demo Ä‘á»“ Ã¡n
- á»¨ng dá»¥ng mobile Æ°u tiÃªn tráº£i nghiá»‡m offline-first
- TÃ i khoáº£n vÃ  phiÃªn Ä‘Äƒng nháº­p Ä‘Æ°á»£c quáº£n lÃ½ cÃ³ xÃ¡c thá»±c
- TÃ i liá»‡u vÃ  mÃ£ nguá»“n pháº£i dá»… bÃ n giao Ä‘á»ƒ thÃ nh viÃªn má»›i clone vá» cÃ³ thá»ƒ cháº¡y Ä‘Æ°á»£c

## 9. ThÃ nh pháº§n dá»¯ liá»‡u chÃ­nh

Há»‡ thá»‘ng Ä‘ang xoay quanh cÃ¡c nhÃ³m Ä‘á»‘i tÆ°á»£ng sau:
- `LoaiDiemThamQuan`
- `DiemThamQuan`
- `NoiDungThuyetMinh`
- `NgonNgu`
- `MaQr`
- `HinhAnhDiemThamQuan`
- `LichSuPhat`
- `NguoiDung`
- `TaiKhoan`

SQL Server lÃ  nguá»“n dá»¯ liá»‡u trung tÃ¢m. SQLite trÃªn mobile chá»‰ Ä‘Ã³ng vai trÃ² cache offline vÃ  lÆ°u dá»¯ liá»‡u Ä‘á»“ng bá»™ Ä‘á»ƒ phá»¥c vá»¥ ngÆ°á»i dÃ¹ng cuá»‘i.

## 10. API chÃ­nh Ä‘ang cÃ³ trong source

### 10.1 XÃ¡c thá»±c

- `POST /api/auth/admin/login`
- `POST /api/auth/user/login`
- `POST /api/auth/user/register`

### 10.2 Danh má»¥c vÃ  Ä‘iá»ƒm tham quan

- `GET/POST/PUT/DELETE /api/LoaiDiemThamQuan`
- `GET/POST/PUT/DELETE /api/DiemThamQuan`
- `GET /api/DiemThamQuan/gan-day`

### 10.3 Ná»™i dung vÃ  audio

- `GET /api/NoiDungThuyetMinh/diem/{maDiem}`
- `GET /api/NoiDungThuyetMinh/diem/{maDiem}/ngonngu/{maNgonNgu}`
- `POST /api/NoiDungThuyetMinh/{id}/generate-audio`
- `POST /api/NoiDungThuyetMinh/generate-audio`
- `GET /api/noidung/{maDiem}`
- `GET /api/noidung/{maDiem}/fallback?maNgonNguUuTien={id}`

### 10.4 Äá»‘i tÆ°á»£ng há»— trá»£ khÃ¡c

- `GET/POST/PUT/DELETE /api/NgonNgu`
- `GET/POST/PUT/DELETE /api/NguoiDung`
- `GET/POST/PUT/DELETE /api/TaiKhoan`
- `GET/POST/PUT/DELETE /api/MaQr`
- `GET /api/MaQr/gia-tri/{giaTriQr}`
- `GET /api/HinhAnhDiemThamQuan/diem/{maDiem}`
- `POST /api/HinhAnhDiemThamQuan/upload`
- `GET/POST/PUT/DELETE /api/LichSuPhat`
- `GET /api/LichSuPhat/thong-ke/luot-nghe-theo-diem`
- `GET /api/LichSuPhat/thong-ke/luot-nghe-theo-kich-hoat`

## 11. CÃ´ng nghá»‡ sá»­ dá»¥ng

- Mobile: .NET MAUI
- API: ASP.NET Core
- CMS: Blazor Server
- CÆ¡ sá»Ÿ dá»¯ liá»‡u trung tÃ¢m: SQL Server
- Cache offline mobile: SQLite
- XÃ¡c thá»±c: JWT
- Audio TTS: `edge-tts`

## 12. TiÃªu chÃ­ nghiá»‡m thu MVP hiá»‡n táº¡i

- Quáº£n trá»‹ viÃªn Ä‘Äƒng nháº­p CMS vÃ  quáº£n lÃ½ Ä‘Æ°á»£c dá»¯ liá»‡u cÆ¡ báº£n
- Táº¡o/sá»­a Ä‘Æ°á»£c ná»™i dung thuyáº¿t minh theo ngÃ´n ngá»¯
- Sinh Ä‘Æ°á»£c audio TTS tá»« ná»™i dung
- Mobile Ä‘Äƒng nháº­p, Ä‘á»“ng bá»™ vÃ  Ä‘á»c dá»¯ liá»‡u offline Ä‘Æ°á»£c
- Mobile kÃ­ch hoáº¡t ná»™i dung báº±ng GPS hoáº·c QR Ä‘Æ°á»£c
- Mobile phÃ¡t Ä‘Æ°á»£c audio vÃ  cÃ³ fallback ngÃ´n ngá»¯/ná»™i dung
- API ghi nháº­n Ä‘Æ°á»£c lá»‹ch sá»­ phÃ¡t vÃ  CMS xem Ä‘Æ°á»£c thá»‘ng kÃª cÆ¡ báº£n

## 13. Giá»›i háº¡n vÃ  hÆ°á»›ng phÃ¡t triá»ƒn tiáº¿p

### 13.1 Giá»›i háº¡n hiá»‡n táº¡i

- Há»‡ thá»‘ng hiá»‡n táº¡i táº­p trung vÃ o MVP vÃ  demo Ä‘á»“ Ã¡n, chÆ°a cÃ³ CI/CD hoÃ n chá»‰nh
- Kiá»ƒm thá»­ tá»± Ä‘á»™ng chÆ°a Ä‘áº§y Ä‘á»§
- TTS Ä‘ang phá»¥ thuá»™c vÃ o mÃ´i trÆ°á»ng cÃ³ cÃ i Ä‘áº·t `edge-tts`

### 13.2 HÆ°á»›ng phÃ¡t triá»ƒn tiáº¿p

- Bá»• sung checklist smoke test vÃ  tÃ i liá»‡u váº­n hÃ nh chi tiáº¿t hÆ¡n
- Má»Ÿ rá»™ng kiá»ƒm thá»­ tá»± Ä‘á»™ng cho API, CMS vÃ  mobile
- HoÃ n thiá»‡n bá»™ tÃ i liá»‡u onboarding cho thÃ nh viÃªn má»›i
- Tá»‘i Æ°u thÃªm tráº£i nghiá»‡m báº£n Ä‘á»“, Ä‘á»‹nh tuyáº¿n vÃ  quáº£n lÃ½ media

## 15. Thuáº­t toÃ¡n vÃ  logic

### 15.1 Haversine

Há»‡ thá»‘ng sá»­ dá»¥ng cÃ´ng thá»©c Haversine Ä‘á»ƒ tÃ­nh khoáº£ng cÃ¡ch giá»¯a vá»‹ trÃ­ hiá»‡n táº¡i cá»§a ngÆ°á»i dÃ¹ng vÃ  tá»a Ä‘á»™ Ä‘iá»ƒm tham quan.

CÃ´ng thá»©c tá»•ng quÃ¡t:

$$
d = 2R \cdot \arcsin\left(\sqrt{\sin^2\left(\frac{\Delta \varphi}{2}\right) + \cos(\varphi_1)\cos(\varphi_2)\sin^2\left(\frac{\Delta \lambda}{2}\right)}\right)
$$

Trong Ä‘Ã³:
- $R$ lÃ  bÃ¡n kÃ­nh TrÃ¡i Äáº¥t
- $\varphi$ lÃ  vÄ© Ä‘á»™
- $\lambda$ lÃ  kinh Ä‘á»™

Má»¥c Ä‘Ã­ch:
- XÃ¡c Ä‘á»‹nh Ä‘iá»ƒm tham quan nÃ o Ä‘ang á»Ÿ gáº§n ngÆ°á»i dÃ¹ng nháº¥t
- Há»— trá»£ logic geofence Ä‘á»ƒ kÃ­ch hoáº¡t ná»™i dung Ä‘Ãºng vá»‹ trÃ­

### 15.2 Geofence

Má»—i Ä‘iá»ƒm tham quan cÃ³ bÃ¡n kÃ­nh kÃ­ch hoáº¡t riÃªng. Sau khi tÃ­nh khoáº£ng cÃ¡ch, há»‡ thá»‘ng so sÃ¡nh vá»›i bÃ¡n kÃ­nh cá»§a Ä‘iá»ƒm:

- Náº¿u `distance <= triggerRadius` thÃ¬ Ä‘iá»ƒm Ä‘á»§ Ä‘iá»u kiá»‡n kÃ­ch hoáº¡t
- Náº¿u ngÆ°á»i dÃ¹ng Ä‘Ã£ tá»«ng Ä‘Æ°á»£c kÃ­ch hoáº¡t gáº§n Ä‘Ã¢y, há»‡ thá»‘ng Ã¡p dá»¥ng cooldown Ä‘á»ƒ trÃ¡nh phÃ¡t láº·p tá»©c liÃªn tiáº¿p
- Náº¿u ngÆ°á»i dÃ¹ng ra khá»i vÃ¹ng kÃ­ch hoáº¡t, state cá»§a Ä‘iá»ƒm sáº½ Ä‘Æ°á»£c Ä‘áº·t láº¡i Ä‘á»ƒ láº§n sau cÃ³ thá»ƒ phÃ¡t láº¡i

### 15.3 Nearest POI

Khi cÃ³ nhiá»u Ä‘iá»ƒm tham quan á»Ÿ gáº§n nhau, mobile Æ°u tiÃªn chá»n Ä‘iá»ƒm gáº§n nháº¥t hoáº·c Ä‘iá»ƒm phÃ¹ há»£p nháº¥t theo Ä‘iá»u kiá»‡n:

- Lá»c cÃ¡c Ä‘iá»ƒm Ä‘ang náº±m trong táº§m Ä‘á»“ng bá»™/quan sÃ¡t
- TÃ­nh khoáº£ng cÃ¡ch Ä‘áº¿n tá»«ng Ä‘iá»ƒm
- Sáº¯p xáº¿p theo khoáº£ng cÃ¡ch tÄƒng dáº§n
- Chá»n Ä‘iá»ƒm gáº§n nháº¥t vÃ  Ä‘á»§ Ä‘iá»u kiá»‡n geofence Ä‘á»ƒ xá»­ lÃ½ tiáº¿p

Logic nÃ y Ä‘Æ°á»£c dÃ¹ng cho:
- Hiá»‡n Ä‘iá»ƒm gáº§n nháº¥t trÃªn mobile
- Tá»± Ä‘á»™ng kÃ­ch hoáº¡t ná»™i dung khi demo GPS
- Giáº£m trÆ°á»ng há»£p phÃ¡t sai Ä‘iá»ƒm khi cÃ¡c POI náº±m gáº§n nhau

### 15.4 State flag

Äá»ƒ trÃ¡nh phÃ¡t ná»™i dung láº·p láº¡i khÃ´ng cáº§n thiáº¿t, mobile duy trÃ¬ cÃ¡c cá» tráº¡ng thÃ¡i trong quÃ¡ trÃ¬nh cháº¡y:

- ÄÃ¡nh dáº¥u Ä‘iá»ƒm vá»«a Ä‘Æ°á»£c auto-trigger
- LÆ°u má»‘c thá»i gian kÃ­ch hoáº¡t gáº§n nháº¥t theo tá»«ng POI
- Theo dÃµi tráº¡ng thÃ¡i fallback ngÃ´n ngá»¯ Ä‘ang Ä‘Æ°á»£c Ã¡p dá»¥ng
- Theo dÃµi route GPS test vÃ  cÃ¡c Ä‘iá»ƒm Ä‘Ã£ trigger trong phiÃªn demo

State flag giÃºp:
- Háº¡n cháº¿ spam audio
- TrÃ¡nh gá»­i lá»‹ch sá»­ láº·p láº¡i quÃ¡ nhiá»u
- GiÃºp luá»“ng GPS/QR/manual hoáº¡t Ä‘á»™ng nháº¥t quÃ¡n

## 16. Flow vÃ  sequence

### 16.1 Flow nghiá»‡p vá»¥ tá»•ng quÃ¡t

```text
Admin dang nhap CMS
-> Tao/Sua diem tham quan va noi dung
-> API luu du lieu vao SQL Server
-> Mobile dang nhap va dong bo du lieu
-> Nguoi dung di chuyen hoac quet QR
-> He thong tim diem phu hop
-> Lay noi dung theo ngon ngu uu tien
-> Fallback neu can
-> Phat audio
-> Gui lich su phat ve API
-> CMS xem thong ke
```

### 16.2 Flow GPS trÃªn mobile

```text
Start
-> Xin quyen GPS
-> Lay vi tri hien tai
-> Tinh khoang cach den danh sach diem
-> Tim nearest POI
-> Kiem tra geofence + cooldown
-> Lay noi dung theo ngon ngu duoc chon
-> Fallback neu khong co noi dung phu hop
-> Resolve audio URL/local file
-> Phat audio
-> Ghi lich su phat
```

### 16.3 Sequence diagram quáº£n trá»‹ ná»™i dung

```mermaid
sequenceDiagram
	autonumber
	actor Admin
	participant CMS
	participant API
	participant SQL as SQL Server
	participant TTS as edge-tts

	Admin->>CMS: Dang nhap
	CMS->>API: Gui thong tin xac thuc
	API->>SQL: Kiem tra tai khoan
	SQL-->>API: Ket qua xac thuc
	API-->>CMS: Tra JWT/token
	CMS-->>Admin: Dang nhap thanh cong

	Admin->>CMS: Tao hoac sua diem/noi dung
	CMS->>API: Goi endpoint luu du lieu
	API->>SQL: Insert/Update du lieu
	SQL-->>API: Xac nhan luu
	API-->>CMS: Tra ket qua thanh cong

	Admin->>CMS: Yeu cau generate audio
	CMS->>API: POST generate-audio
	API->>TTS: Sinh file mp3 tu noi dung
	TTS-->>API: Tra file audio/duong dan
	API->>SQL: Cap nhat audio URL va metadata
	SQL-->>API: Xac nhan cap nhat
	API-->>CMS: Tra ket qua generate audio
```

HÃ¬nh minh há»a test thá»±c táº¿ cho luá»“ng CMS Ä‘Äƒng nháº­p:

![Anh test dang nhap CMS](./assets/login.jpg)

### 16.4 Sequence diagram mobile GPS/QR

```mermaid
sequenceDiagram
	autonumber
	actor User
	participant Mobile
	participant API
	participant SQL as SQL Server
	participant Cache as SQLite Cache
	participant Audio as Audio/TTS Player

	User->>Mobile: Dang nhap hoac mo ung dung
	Mobile->>API: Dong bo POI, noi dung, ngon ngu, QR
	API->>SQL: Doc du lieu trung tam
	SQL-->>API: Tra du lieu
	API-->>Mobile: Tra payload dong bo
	Mobile->>Cache: Luu cache offline

	alt Kich hoat bang GPS
		User->>Mobile: Di vao vung geofence
		Mobile->>Mobile: Lay vi tri hien tai
		Mobile->>Mobile: Tinh khoang cach, chon nearest POI
		Mobile->>Mobile: Kiem tra geofence va cooldown
	else Kich hoat bang QR
		User->>Mobile: Quet QR
		Mobile->>Cache: Doi chieu QR trong cache
		Cache-->>Mobile: Tra POI tuong ung
	else Chon thu cong
		User->>Mobile: Chon POI/noi dung trong app
	end

	alt Co du lieu phu hop trong cache
		Mobile->>Cache: Lay noi dung theo ngon ngu uu tien
		Cache-->>Mobile: Tra noi dung/audio URL
	else Can lay du lieu moi tu API
		Mobile->>API: Lay noi dung/fallback theo diem
		API->>SQL: Doc noi dung va audio
		SQL-->>API: Tra noi dung phu hop
		API-->>Mobile: Tra noi dung va fallback
		Mobile->>Cache: Cap nhat cache local
	end

	Mobile->>Mobile: Resolve fallback ngon ngu va nguon audio
	Mobile->>Audio: Phat audio/TTS
	Audio-->>User: Nghe thuyet minh
	Mobile->>API: Gui lich su phat
	API->>SQL: Luu lich su va thong ke
	SQL-->>API: Xac nhan luu
	API-->>Mobile: Tra ket qua ghi nhan
```

HÃ¬nh minh há»a test thá»±c táº¿ cho GPS trigger:

![Anh test GPS trigger](./assets/gps%20trigger.jpg)

HÃ¬nh minh há»a test thá»±c táº¿ cho QR trigger:

![Anh test QR trigger](./assets/hinhqr.jpg)

## 17. Test case

Báº£ng test tÃ³m táº¯t cho MVP hiá»‡n táº¡i:

| ID | NhÃ³m test | TÃ¬nh huá»‘ng | Káº¿t quáº£ mong Ä‘á»£i |
| --- | --- | --- | --- |
| TC-MVP-01 | CMS Auth | Admin Ä‘Äƒng nháº­p Ä‘Ãºng tÃ i khoáº£n há»£p lá»‡ | ÄÄƒng nháº­p thÃ nh cÃ´ng vÃ  vÃ o Ä‘Æ°á»£c trang quáº£n trá»‹ |
| TC-MVP-02 | CMS Ná»™i dung | Táº¡o má»›i ná»™i dung thuyáº¿t minh theo ngÃ´n ngá»¯ | LÆ°u thÃ nh cÃ´ng, hiá»‡n láº¡i Ä‘Ãºng trong danh sÃ¡ch |
| TC-MVP-03 | CMS Audio | Generate audio cho ná»™i dung | API sinh file mp3 vÃ  cáº­p nháº­t Ä‘Æ°á»ng dáº«n audio |
| TC-MVP-04 | API | Láº¥y ná»™i dung theo Ä‘iá»ƒm tham quan | Tráº£ Ä‘Ãºng danh sÃ¡ch ná»™i dung theo Ä‘iá»ƒm |
| TC-MVP-05 | API Fallback | Gá»i endpoint fallback vá»›i ngÃ´n ngá»¯ Æ°u tiÃªn khÃ´ng tá»“n táº¡i | Tráº£ ná»™i dung fallback kháº£ dá»¥ng, khÃ´ng lá»—i |
| TC-MVP-06 | Mobile Sync | Mobile Ä‘á»“ng bá»™ dá»¯ liá»‡u khi cÃ³ máº¡ng | Danh sÃ¡ch Ä‘iá»ƒm, ná»™i dung, ngÃ´n ngá»¯ vÃ  QR Ä‘Æ°á»£c lÆ°u vÃ o SQLite |
| TC-MVP-07 | Mobile Offline | Táº¯t máº¡ng sau khi Ä‘Ã£ Ä‘á»“ng bá»™ | Mobile váº«n Ä‘á»c Ä‘Æ°á»£c dá»¯ liá»‡u vÃ  phÃ¡t ná»™i dung tá»« cache |
| TC-MVP-08 | GPS Trigger | NgÆ°á»i dÃ¹ng Ä‘i vÃ o bÃ¡n kÃ­nh kÃ­ch hoáº¡t | Ná»™i dung tá»± Ä‘á»™ng Ä‘Æ°á»£c chá»n vÃ  phÃ¡t audio |
| TC-MVP-09 | QR Trigger | NgÆ°á»i dÃ¹ng quÃ©t QR há»£p lá»‡ | Má»Ÿ Ä‘Ãºng Ä‘iá»ƒm tham quan vÃ  phÃ¡t ná»™i dung tÆ°Æ¡ng á»©ng |
| TC-MVP-10 | Language | Chá»n ngÃ´n ngá»¯ `en` hoáº·c `zh-CN` | Giao diá»‡n vÃ  ná»™i dung Æ°u tiÃªn theo ngÃ´n ngá»¯ Ä‘Ã£ chá»n |
| TC-MVP-11 | Audio Source | Ná»™i dung cÃ³ sáºµn file audio | Mobile Æ°u tiÃªn phÃ¡t audio Ä‘Ã£ sinh sáºµn |
| TC-MVP-12 | Logging | PhÃ¡t ná»™i dung thÃ nh cÃ´ng | API ghi nháº­n lá»‹ch sá»­ phÃ¡t vÃ  CMS xem Ä‘Æ°á»£c thá»‘ng kÃª cÆ¡ báº£n |

TÃ i liá»‡u test chi tiáº¿t cho pháº§n CMS admin Ä‘Æ°á»£c má»Ÿ rá»™ng thÃªm trong `docs/TestCase.md`.

### 17.1 Sá»‘ liá»‡u test thá»±c táº¿

Báº£ng dÆ°á»›i Ä‘Ã¢y tá»•ng há»£p cÃ¡c thÃ´ng sá»‘ Ä‘Ã£ Ä‘Æ°á»£c Ä‘á»‘i chiáº¿u tá»« source vÃ  mÃ´i trÆ°á»ng demo hiá»‡n táº¡i:

| Háº¡ng má»¥c | GiÃ¡ trá»‹ thá»±c táº¿ | Nguá»“n Ä‘á»‘i chiáº¿u |
| --- | --- | --- |
| Build API local | `dotnet build` thÃ nh cÃ´ng, exit code `0` | Terminal build ngÃ y 12/04/2026 |
| Cá»•ng API demo | `http://localhost:5000` | `Run-DoAn.ps1` |
| Cá»•ng CMS demo | `http://localhost:5256` hoáº·c cá»•ng fallback trong khoáº£ng `5257-5275` | `Run-DoAn.ps1` |
| Sá»‘ ngÃ´n ngá»¯ giao diá»‡n mobile Ä‘ang há»— trá»£ | `3` ngÃ´n ngá»¯: `vi`, `en`, `zh-CN` | `LanguageService.cs` |
| Chu ká»³ refresh GPS | `5` giÃ¢y | `MainPage.xaml.cs` |
| Chu ká»³ GPS test step | `6` giÃ¢y | `MainPage.xaml.cs` |
| Cooldown geofence | `2` phÃºt | `MainPage.xaml.cs` |
| BÃ¡n kÃ­nh kÃ­ch hoáº¡t tá»‘i thiá»ƒu há»¯u hiá»‡u | `45` mÃ©t | `MainPage.xaml.cs` |
| BÃ¡n kÃ­nh POI máº·c Ä‘á»‹nh | `0.6` km | `MainPage.xaml.cs` |
| BÃ¡n kÃ­nh ngÆ°á»i dÃ¹ng máº·c Ä‘á»‹nh | `1` km | `MainPage.xaml.cs` |
| Tá»‡p route GPS test | `vinh-khanh-food-tour.gpx` | `MainPage.xaml.cs` |

Nháº­n xÃ©t tá»« Ä‘á»£t test ná»™i bá»™:
- Luá»“ng demo hiá»‡n táº¡i Ä‘Ã£ cÃ³ Ä‘á»§ dá»¯ liá»‡u Ä‘á»ƒ kiá»ƒm thá»­ GPS, QR, offline cache vÃ  Ä‘a ngÃ´n ngá»¯.
- Pháº§n test mobile Æ°u tiÃªn xÃ¡c nháº­n Ä‘Æ°á»£c logic geofence, fallback ngÃ´n ngá»¯ vÃ  phÃ¡t audio tá»« nguá»“n Ä‘Ã£ sinh sáºµn.
- Há»‡ thá»‘ng phÃ¹ há»£p Ä‘á»ƒ trÃ¬nh bÃ y theo kiá»ƒu demo local, chÆ°a Ä‘áº¡t má»©c deployment production.

## 18. Edge case

Cáº§n lÆ°u Ã½ cÃ¡c trÆ°á»ng há»£p biÃªn sau:

- GPS dao Ä‘á»™ng lÃ m ngÆ°á»i dÃ¹ng Ä‘á»©ng sÃ¡t biÃªn geofence, dá»… gÃ¢y trigger láº·p láº¡i
- Nhiá»u Ä‘iá»ƒm tham quan náº±m gáº§n nhau, khÃ³ chá»n Ä‘Ãºng nearest POI
- NgÃ´n ngá»¯ ngÆ°á»i dÃ¹ng chá»n khÃ´ng cÃ³ ná»™i dung tÆ°Æ¡ng á»©ng cho Ä‘iá»ƒm hiá»‡n táº¡i
- Audio URL tráº£ vá» dáº¡ng `file://` hoáº·c `localhost` khi cháº¡y trÃªn Android emulator
- Mobile Ä‘ang offline trong lÃºc ngÆ°á»i dÃ¹ng chÆ°a ká»‹p Ä‘á»“ng bá»™ dá»¯ liá»‡u má»›i nháº¥t
- QR khÃ´ng há»£p lá»‡ hoáº·c QR khÃ´ng tá»“n táº¡i trong cache
- File audio Ä‘Ã£ Ä‘Æ°á»£c táº¡o nhÆ°ng rá»—ng hoáº·c khÃ´ng phÃ¡t Ä‘Æ°á»£c
- Token háº¿t háº¡n trong quÃ¡ trÃ¬nh Ä‘ang sá»­ dá»¥ng mobile hoáº·c CMS
- API máº¥t káº¿t ná»‘i trong lÃºc Ä‘ang Ä‘á»“ng bá»™ hoáº·c gá»­i lá»‹ch sá»­ phÃ¡t
- Quyá»n GPS bá»‹ tá»« chá»‘i hoáº·c thiáº¿t bá»‹ khÃ´ng há»— trá»£ map/GPS Ä‘áº§y Ä‘á»§

HÆ°á»›ng xá»­ lÃ½ mong muá»‘n:
- CÃ³ fallback sang cache local náº¿u online tháº¥t báº¡i
- CÃ³ thÃ´ng bÃ¡o lá»—i thÃ¢n thiá»‡n cho ngÆ°á»i dÃ¹ng
- KhÃ´ng crash app khi GPS, audio hoáº·c API gáº·p lá»—i

## 19. UI vÃ  wireframe mÃ´ táº£

### 19.1 Mobile

MÃ n hÃ¬nh chÃ­nh mobile hiá»‡n táº¡i gá»“m cÃ¡c khá»‘i chá»©c nÄƒng:
- Thanh tiÃªu Ä‘á» vÃ  cÃ¡c hÃ nh Ä‘á»™ng Ä‘Äƒng xuáº¥t/Ä‘á»“ng bá»™
- Khu vá»±c map hoáº·c tráº¡ng thÃ¡i GPS
- Bá»™ chá»n ngÃ´n ngá»¯ hiá»ƒn thá»‹/ná»™i dung
- Danh sÃ¡ch Ä‘iá»ƒm tham quan
- Danh sÃ¡ch ná»™i dung cá»§a Ä‘iá»ƒm Ä‘ang chá»n
- Khu vá»±c GPS test log Ä‘á»ƒ demo route kÃ­ch hoáº¡t

Wireframe mÃ´ táº£:

```text
+--------------------------------------------------+
| He thong thuyet minh du lich                     |
| [Sync] [Logout]                                 |
+--------------------------------------------------+
| GPS status / Map view                            |
| Nearest POI / Current location                   |
+--------------------------------------------------+
| Language: [vi | en | zh-CN]                      |
| Display language: [dropdown]                     |
+--------------------------------------------------+
| Danh sach diem tham quan                         |
| - POI A                                          |
| - POI B                                          |
| - POI C                                          |
+--------------------------------------------------+
| Noi dung thuyet minh cua diem dang chon          |
| [Play] [Stop]                                    |
| Tieu de / mo ta / fallback status                |
+--------------------------------------------------+
| GPS test log / route demo                        |
+--------------------------------------------------+
```

### 19.2 CMS

Wireframe mÃ´ táº£ cho CMS admin:

```text
+--------------------------------------------------+
| Sidebar                                          |
| - Dashboard                                      |
| - Loai diem                                      |
| - Diem tham quan                                 |
| - Noi dung thuyet minh                           |
| - QR                                             |
| - Ngon ngu                                       |
| - Tai khoan / Nguoi dung                         |
+--------------------------------------------------+
| Header + user session                            |
+--------------------------------------------------+
| Bang du lieu / form tao sua                      |
| Nut Luu / Xoa / Generate audio                   |
+--------------------------------------------------+
```

áº¢nh minh há»a thao tÃ¡c thÃªm/sá»­a loáº¡i Ä‘iá»ƒm trÃªn CMS:

![Them sua loai diem](./assets/thÃªm sá»­a loáº¡i Ä‘iá»ƒm.jpg)

áº¢nh minh há»a thao tÃ¡c áº©n loáº¡i Ä‘iá»ƒm trÃªn CMS:

![An loai diem](./assets/áº©n loáº¡i Ä‘iá»ƒm.jpg)

### 19.3 Ghi chÃº UI

- Mobile Æ°u tiÃªn rÃµ tráº¡ng thÃ¡i GPS, ngÃ´n ngá»¯ Ä‘ang chá»n vÃ  ná»™i dung Ä‘ang phÃ¡t
- CMS Æ°u tiÃªn luá»“ng CRUD nhanh, dá»… thao tÃ¡c khi demo Ä‘á»“ Ã¡n
- Wireframe trong PRD lÃ  mÃ´ táº£ logic giao diá»‡n, khÃ´ng pháº£i mockup pixel-perfect

### 19.4 HÃ¬nh áº£nh minh há»a test thá»±c táº¿

áº¢nh chá»¥p mÃ n hÃ¬nh dÆ°á»›i Ä‘Ã¢y lÃ  tÃ i sáº£n test/demo Ä‘Ã£ lÆ°u trong `docs/assets/`.

LÆ°u Ã½:
- PRD nÃ y chá»‰ gáº¯n áº£nh tá»•ng quan, sequence vÃ  mÃ n hÃ¬nh test á»Ÿ thÆ° má»¥c `docs/assets/`.
- KhÃ´ng sá»­ dá»¥ng bá»™ áº£nh trong `docs/assets/qr/` Ä‘á»ƒ trÃ¡nh lÃ m PRD bá»‹ dÃ i vÃ  láº·p láº¡i dá»¯ liá»‡u mÃ£ QR.

HÃ¬nh 1 - MÃ n hÃ¬nh mobile khi test giao diá»‡n Audio Guide:

[Xem file gá»‘c: submission-mainpage.png](./assets/submission-mainpage.png)

<img src="./assets/submission-mainpage.png" alt="Man hinh mobile Audio Guide" width="900" />

HÃ¬nh 2 - áº¢nh crop táº­p trung vÃ o pháº§n header vÃ  tháº» chá»©c nÄƒng chÃ­nh:

[Xem file gá»‘c: submission-mainpage-cropped.png](./assets/submission-mainpage-cropped.png)

<img src="./assets/submission-mainpage-cropped.png" alt="Anh crop giao dien Audio Guide" width="900" />

Ã nghÄ©a minh há»a:
- XÃ¡c nháº­n giao diá»‡n mobile Ä‘Ã£ Ä‘Æ°á»£c render thá»±c táº¿ trong quÃ¡ trÃ¬nh test
- Cho tháº¥y mÃ n hÃ¬nh chÃ­nh Ä‘ang cÃ³ tiÃªu Ä‘á», tháº» thÃ´ng tin vÃ  bá»‘ cá»¥c phá»¥c vá»¥ demo
- CÃ³ thá»ƒ dÃ¹ng trá»±c tiáº¿p trong bÃ¡o cÃ¡o, slide hoáº·c lÃºc báº£o vá»‡ Ä‘á»“ Ã¡n

HÃ¬nh 3 - Sequence Ä‘Äƒng nháº­p CMS:

[Xem file gá»‘c: login.jpg](./assets/login.jpg)

<img src="./assets/login.jpg" alt="Sequence dang nhap CMS" width="900" />

HÃ¬nh 4 - Sequence Ä‘Äƒng xuáº¥t CMS:

[Xem file gá»‘c: logout.jpg](./assets/logout.jpg)

<img src="./assets/logout.jpg" alt="Sequence dang xuat CMS" width="900" />

HÃ¬nh 5 - Sequence mobile GPS trigger:

[Xem file gá»‘c: gps trigger.jpg](./assets/gps%20trigger.jpg)

<img src="./assets/gps%20trigger.jpg" alt="Sequence GPS trigger" width="900" />

HÃ¬nh 6 - Sequence mobile QR trigger:

[Xem file gá»‘c: hinhqr.jpg](./assets/hinhqr.jpg)

<img src="./assets/hinhqr.jpg" alt="Sequence QR trigger" width="900" />

HÃ¬nh 7 - Sequence mobile sync/offline:

[Xem file gá»‘c: sync.jpg](./assets/sync.jpg)

<img src="./assets/sync.jpg" alt="Sequence mobile sync" width="900" />

HÃ¬nh 8 - Sequence thÃªm/sá»­a loáº¡i Ä‘iá»ƒm trÃªn CMS:

[Xem file gá»‘c: thÃªm sá»­a loáº¡i Ä‘iá»ƒm.jpg](./assets/thÃªm sá»­a loáº¡i Ä‘iá»ƒm.jpg)

<img src="./assets/thÃªm sá»­a loáº¡i Ä‘iá»ƒm.jpg" alt="Sequence them sua loai diem" width="900" />

HÃ¬nh 9 - Sequence áº©n loáº¡i Ä‘iá»ƒm trÃªn CMS:

[Xem file gá»‘c: áº©n loáº¡i Ä‘iá»ƒm.jpg](./assets/áº©n loáº¡i Ä‘iá»ƒm.jpg)

<img src="./assets/áº©n loáº¡i Ä‘iá»ƒm.jpg" alt="Sequence an loai diem" width="900" />

## 20. KPI

Äá»ƒ Ä‘Ã¡nh giÃ¡ má»©c Ä‘á»™ Ä‘áº¡t Ä‘Æ°á»£c cá»§a MVP, bá»™ KPI Ä‘á» xuáº¥t cho Ä‘á»“ Ã¡n gá»“m:

| NhÃ³m KPI | Chá»‰ sá»‘ | Má»¥c tiÃªu hiá»‡n táº¡i |
| --- | --- | --- |
| Äá»“ng bá»™ dá»¯ liá»‡u | Tá»· lá»‡ sync thÃ nh cÃ´ng khi cÃ³ máº¡ng | >= 95% trong mÃ´i trÆ°á»ng demo |
| Offline | Mobile Ä‘á»c Ä‘Æ°á»£c ná»™i dung Ä‘Ã£ cache khi máº¥t máº¡ng | 100% vá»›i dá»¯ liá»‡u Ä‘Ã£ Ä‘á»“ng bá»™ |
| GPS Trigger | Tá»· lá»‡ kÃ­ch hoáº¡t Ä‘Ãºng Ä‘iá»ƒm trong route demo | >= 90% |
| QR Trigger | Tá»· lá»‡ quÃ©t QR há»£p lá»‡ vÃ  má»Ÿ Ä‘Ãºng Ä‘iá»ƒm | >= 95% |
| Audio | Tá»· lá»‡ phÃ¡t Ä‘Æ°á»£c audio khi cÃ³ file há»£p lá»‡ | >= 95% |
| Fallback ngÃ´n ngá»¯ | Tá»· lá»‡ tráº£ Ä‘Æ°á»£c ná»™i dung thay tháº¿ khi thiáº¿u ngÃ´n ngá»¯ Æ°u tiÃªn | 100% náº¿u cÃ³ ná»™i dung kháº£ dá»¥ng |
| CMS | Táº¡o/sá»­a ná»™i dung vÃ  generate audio thÃ nh cÃ´ng | >= 95% trÃªn dá»¯ liá»‡u demo |
| Thá»‘ng kÃª | Lá»‹ch sá»­ phÃ¡t Ä‘Æ°á»£c ghi nháº­n vÃ  hiá»ƒn thá»‹ trÃªn CMS | >= 95% |

Ã nghÄ©a:
- KPI dÃ¹ng Ä‘á»ƒ báº£o vá»‡ tÃ­nh kháº£ thi cá»§a MVP
- KhÃ´ng pháº£i SLA production, mÃ  lÃ  má»©c Ä‘Ã¡nh giÃ¡ trong bá»‘i cáº£nh Ä‘á»“ Ã¡n vÃ  demo

## 21. Security

Há»‡ thá»‘ng hiá»‡n táº¡i Ã¡p dá»¥ng cÃ¡c nguyÃªn táº¯c báº£o máº­t cÆ¡ báº£n phÃ¹ há»£p vá»›i pháº¡m vi MVP:

- API sá»­ dá»¥ng JWT cho xÃ¡c thá»±c admin vÃ  ngÆ°á»i dÃ¹ng
- CÃ¡c thao tÃ¡c quáº£n trá»‹ dá»¯ liá»‡u Ä‘i qua CMS vÃ  API thay vÃ¬ truy cáº­p trá»±c tiáº¿p DB
- API cÃ³ `Authentication` vÃ  `Authorization` trong cáº¥u hÃ¬nh khá»Ÿi Ä‘á»™ng
- KhÃ´ng sá»­ dá»¥ng SQLite lÃ m nguá»“n dá»¯ liá»‡u trung tÃ¢m trong luá»“ng váº­n hÃ nh chuáº©n
- Mobile chá»‰ lÆ°u cache offline phá»¥c vá»¥ truy cáº­p ná»™i dung, khÃ´ng thay tháº¿ há»‡ thá»‘ng trung tÃ¢m
- Há»‡ thá»‘ng cáº§n xin quyá»n GPS trÃªn thiáº¿t bá»‹ vÃ  chá»‰ sá»­ dá»¥ng vá»‹ trÃ­ cho luá»“ng kÃ­ch hoáº¡t ná»™i dung

Rá»§i ro vÃ  lÆ°u Ã½ hiá»‡n táº¡i:
- JWT key vÃ  cáº¥u hÃ¬nh váº­n hÃ nh cáº§n Ä‘Æ°á»£c quáº£n lÃ½ báº±ng file cáº¥u hÃ¬nh/phiÃªn báº£n triá»ƒn khai phÃ¹ há»£p, khÃ´ng Ä‘á»ƒ lá»™ trong mÃ´i trÆ°á»ng cÃ´ng khai
- TTS phá»¥ thuá»™c vÃ o cÃ´ng cá»¥ `edge-tts` cÃ i trÃªn mÃ¡y cháº¡y API
- Cáº§n tiáº¿p tá»¥c rÃ  soÃ¡t validate input, phÃ¢n quyá»n endpoint vÃ  log audit náº¿u má»Ÿ rá»™ng há»‡ thá»‘ng

## 22. Error handling

Há»‡ thá»‘ng hiá»‡n táº¡i Æ°u tiÃªn xá»­ lÃ½ lá»—i theo hÆ°á»›ng thÃ¢n thiá»‡n vá»›i ngÆ°á»i dÃ¹ng vÃ  fallback khi cÃ³ thá»ƒ:

### 22.1 TrÃªn mobile

- Náº¿u gá»i API tháº¥t báº¡i khi cÃ³ máº¡ng, mobile sáº½ Æ°u tiÃªn Ä‘á»c dá»¯ liá»‡u Ä‘Ã£ cache trong SQLite
- Náº¿u audio URL khÃ´ng há»£p lá»‡, mobile cÃ³ cÆ¡ cháº¿ resolve láº¡i tá»« base URL phÃ¹ há»£p
- Náº¿u ngÃ´n ngá»¯ Æ°u tiÃªn khÃ´ng cÃ³ ná»™i dung, mobile gá»i fallback vÃ  dÃ¹ng ngÃ´n ngá»¯ kháº£ dá»¥ng
- Náº¿u Ä‘á»“ng bá»™ tháº¥t báº¡i do máº¥t máº¡ng, app khÃ´ng Ä‘Æ°á»£c crash vÃ  sáº½ thá»­ Ä‘á»“ng bá»™ láº¡i khi cÃ³ káº¿t ná»‘i

### 22.2 TrÃªn API vÃ  CMS

- API tráº£ response JSON vÃ  bá» qua trÆ°á»ng null Ä‘á»ƒ payload gá»n hÆ¡n
- CMS vÃ  mobile cáº§n hiá»‡n thÃ´ng bÃ¡o lá»—i dá»… hiá»ƒu khi login, Ä‘á»“ng bá»™, generate audio hoáº·c lÆ°u dá»¯ liá»‡u tháº¥t báº¡i
- Script `Run-DoAn.ps1` cÃ³ xá»­ lÃ½ build láº¡i, retry vÃ  thÃ´ng bÃ¡o log khi API/CMS khá»Ÿi Ä‘á»™ng tháº¥t báº¡i

Má»¥c tiÃªu xá»­ lÃ½ lá»—i:
- KhÃ´ng crash há»‡ thá»‘ng trong cÃ¡c tÃ¬nh huá»‘ng lá»—i phá»• biáº¿n
- CÃ³ thÃ´ng bÃ¡o rÃµ nghÄ©a Ä‘á»ƒ ngÆ°á»i dÃ¹ng vÃ  nhÃ³m demo xá»­ lÃ½ nhanh
- Æ¯u tiÃªn fallback thay vÃ¬ dá»«ng há»‡ thá»‘ng hoÃ n toÃ n

## 23. API response sample

DÆ°á»›i Ä‘Ã¢y lÃ  má»™t sá»‘ máº«u response Ä‘áº¡i diá»‡n Ä‘á»ƒ Ä‘Æ°a vÃ o PRD vÃ  thuyáº¿t trÃ¬nh.

### 23.1 Login ngÆ°á»i dÃ¹ng thÃ nh cÃ´ng

```json
{
	"token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
	"taiKhoan": {
		"maTaiKhoan": 12,
		"tenDangNhap": "user01",
		"vaiTro": "User"
	}
}
```

### 23.2 Láº¥y danh sÃ¡ch Ä‘iá»ƒm tham quan

```json
[
	{
		"maDiem": 1,
		"tenDiem": "Pho am thuc Vinh Khanh",
		"viDo": 10.7552,
		"kinhDo": 106.7018,
		"banKinhKichHoat": 60,
		"ngayCapNhat": "2026-04-12T10:00:00"
	}
]
```

### 23.3 Fallback ná»™i dung theo ngÃ´n ngá»¯

```json
{
	"maDiem": 1,
	"maNgonNguYeuCau": 3,
	"maNgonNguDaDung": 1,
	"coFallback": true,
	"noiDung": {
		"maNoiDung": 15,
		"tieuDe": "Gioi thieu diem tham quan",
		"moTa": "Noi dung thuyet minh bang tieng Viet",
		"duongDanAmThanh": "/audio/tts/noidung-15.mp3"
	}
	}
```

### 23.4 Ghi nháº­n lá»‹ch sá»­ phÃ¡t

```json
{
	"thanhCong": true,
	"thongDiep": "Da luu lich su phat",
	"duLieu": {
		"maLichSu": 101,
		"maDiem": 1,
		"kieuKichHoat": "gps"
	}
}
```

LÆ°u Ã½:
- Máº«u response trong PRD mang tÃ­nh mÃ´ táº£ nghiá»‡p vá»¥ vÃ  cáº¥u trÃºc payload
- Payload thá»±c táº¿ cÃ³ thá»ƒ thÃªm bá»›t má»™t sá»‘ trÆ°á»ng tÃ¹y theo DTO Ä‘ang sá»­ dá»¥ng trong source

## 24. Deployment

### 24.1 MÃ´i trÆ°á»ng demo hiá»‡n táº¡i

- API cháº¡y local qua `dotnet` trÃªn cá»•ng `5000`
- CMS cháº¡y local qua `dotnet` trÃªn cá»•ng `5256` hoáº·c cá»•ng trong khoáº£ng fallback náº¿u bá»‹ trÃ¹ng
- Mobile káº¿t ná»‘i Ä‘áº¿n API local khi test trÃªn Windows hoáº·c Android emulator
- SQL Server lÃ  nguá»“n dá»¯ liá»‡u chuáº©n cho cháº¿ Ä‘á»™ online
- SQLite Ä‘Æ°á»£c dÃ¹ng cho mobile cache vÃ  cÃ³ thá»ƒ dÃ¹ng SQLite mode trong API Ä‘á»ƒ dev/test cá»¥c bá»™

### 24.2 CÃ¡ch cháº¡y nhanh

Cháº¿ Ä‘á»™ online:

```powershell
powershell -ExecutionPolicy Bypass -File .\Run-DoAn.ps1 -Mode online
```

Cháº¿ Ä‘á»™ offline dev/test:

```powershell
powershell -ExecutionPolicy Bypass -File .\Run-DoAn.ps1 -Mode offline
```

Reset offline DB rá»“i cháº¡y láº¡i:

```powershell
powershell -ExecutionPolicy Bypass -File .\Run-DoAn.ps1 -Mode offline -ResetOfflineDb
```

### 24.3 Ghi chÃº triá»ƒn khai

- Swagger Ä‘Æ°á»£c báº­t Ä‘á»ƒ test API nhanh trong mÃ´i trÆ°á»ng local/demo
- `UseHttpsRedirection` chá»‰ Ã¡p dá»¥ng khi khÃ´ng á»Ÿ mÃ´i trÆ°á»ng Development
- Cáº§n cáº¥u hÃ¬nh Ä‘Ãºng chuá»—i káº¿t ná»‘i SQL Server vÃ  JWT trÆ°á»›c khi demo
- Náº¿u test Android emulator, cáº§n Ä‘áº£m báº£o mobile sá»­ dá»¥ng Ä‘Ãºng base URL nhÆ° `10.0.2.2`
- Deployment hiá»‡n táº¡i phÃ¹ há»£p cho demo Ä‘á»“ Ã¡n vÃ  mÃ´i trÆ°á»ng ná»™i bá»™, chÆ°a hÆ°á»›ng tá»›i production scale

## 25. Káº¿t luáº­n

PRD nÃ y pháº£n Ã¡nh hiá»‡n tráº¡ng thá»±c táº¿ cá»§a Ä‘á»“ Ã¡n trÃªn nhÃ¡nh `main`: há»‡ thá»‘ng Ä‘Ã£ cÃ³ Ä‘áº§y Ä‘á»§ 3 module API, CMS vÃ  Mobile; há»— trá»£ GPS, QR, offline SQLite, Ä‘a ngÃ´n ngá»¯, fallback ná»™i dung vÃ  generate audio báº±ng `edge-tts`. TÃ i liá»‡u nÃ y Ä‘Æ°á»£c dÃ¹ng lÃ m má»‘c Ä‘á»ƒ tiáº¿p tá»¥c hoÃ n thiá»‡n bá»™ docs vÃ  nghiá»‡m thu sáº£n pháº©m.


## 26. Phu luc - Sequence Diagram CMS

### 26.1 Quan ly nguoi dung

![Sequence them nguoi dung](./assets/TH%C3%8AM%20NG%C6%AF%E1%BB%9CI%20D%C3%99NG.jpg)

![Sequence sua nguoi dung](./assets/S%E1%BB%ACA%20NG%C6%AF%E1%BB%9CI%20D%C3%99NG.jpg)

![Sequence an hien nguoi dung](./assets/%E1%BA%A8N%20HI%E1%BB%86N%20NG%C6%AF%E1%BB%9CI%20D%C3%99NG.jpg)

![Sequence tim kiem va phan trang nguoi dung](./assets/T%C3%8CM%20KI%E1%BA%BEM-PH%C3%82N%20TRANG.jpg)

### 26.2 Quan ly ngon ngu

![Sequence load trang ngon ngu](./assets/LOAD%20TRANG%20NG%C3%94N%20NG%E1%BB%AE.jpg)

![Sequence them ngon ngu](./assets/TH%C3%8AM%20NG%C3%94N%20NG%E1%BB%AE.jpg)

![Sequence sua ngon ngu](./assets/S%E1%BB%ACA%20NG%C3%94N%20NG%E1%BB%AE.jpg)

![Sequence an hien ngon ngu](./assets/%E1%BA%A8N%20HI%E1%BB%86N%20NG%C3%94N%20NG%E1%BB%AE.jpg)

![Sequence tim kiem ngon ngu](./assets/T%C3%8CM%20KI%E1%BA%BEM.jpg)

### 26.3 Quan ly loai diem tham quan

![Sequence load trang loai diem](./assets/SEQUENCE%20LOAD%20TRANG%20%C4%90I%E1%BB%82M%20THAM%20QUAN.jpg)

![Sequence them loai diem](./assets/TH%C3%8AM%20LO%E1%BA%A0I%20%C4%90I%E1%BB%82M.jpg)

![Sequence sua loai diem](./assets/S%E1%BB%ACA%20LO%E1%BA%A0I%20%C4%90I%E1%BB%82M.jpg)

![Sequence an hien loai diem](./assets/%E1%BA%A8N-HI%E1%BB%86N%20LO%E1%BA%A0I%20%C4%90I%E1%BB%82M.jpg)

### 26.4 Quan ly diem tham quan va hinh anh

![Sequence them diem tham quan](./assets/SEQUENCE%20TH%C3%8AM%20%C4%90I%E1%BB%82M%20THAM%20QUAN.jpg)

![Sequence sua diem tham quan](./assets/SEQUENCE%20S%E1%BB%ACA%20%C4%90I%E1%BB%82M%20THAM%20QUAN.jpg)

![Sequence upload anh](./assets/SEQUENCE%20UPLOAD%20%E1%BA%A2NH.jpg)

![Sequence xoa anh](./assets/SEQUENCE%20XO%C3%81%20ANHE.jpg)
## 27. Dashboard - Flow & Sequence

### 27.1 Tổng quan Dashboard

Dashboard hiển thị các thông tin thống kê hệ thống:
- KPI tổng quan (tổng POI, user online, lượt nghe…)
- Biểu đồ xu hướng
- Thống kê QR / GPS
- Top POI, Top User, Top Nội dung
- Lịch sử chi tiết

Dữ liệu được load từ API và xử lý tại Dashboard Component.

---

### 27.2 Activity Diagram - Load Dashboard

<img src="./assets/ACTIVITY-DASHBOARD.jpg" width="1000"/>

**Mô tả:**
- Khi mở Dashboard → gọi `OnInitializedAsync()`
- Thực hiện `LoadAsync()`
- Gọi nhiều API song song:
  - GetDiemAsync
  - GetLoaiDiemAsync
  - GetThongKeTheoDiemAsync
  - GetThongKeTheoKichHoatAsync
  - GetNguoiDungDangHoatDongAsync
  - GetLichSuPhatAsync
  - GetMaQRAsync
- Dùng `Task.WhenAll()` để chờ tất cả hoàn thành
- Nếu lỗi → hiển thị thông báo
- Nếu thành công:
  - Bind dữ liệu
  - Tính KPI + Top + biểu đồ
  - Render UI

---

### 27.3 Sequence Diagram - Load Dashboard

<img src="./assets/SEQUENCE LOAD TRANG DASHBOARH.jpg" width="1000"/>

**Mô tả:**
- Dashboard gọi `LoadAsync()`
- CmsApiClient gọi API Server
- Các API chạy song song (parallel):
  - GetDiemAsync()
  - GetLoaiDiemAsync()
  - GetThongKeTheoDiemAsync()
  - GetThongKeTheoKichHoatAsync()
  - GetNguoiDungDangHoatDongAsync()
  - GetLichSuPhatAsync()
  - GetMaQRAsync()
- Sau đó:
  - `Task.WhenAll()`
  - Bind dữ liệu
  - Render UI

---

### 27.4 Sequence Diagram - Auto Refresh Dashboard

<img src="./assets/AUTO REFRESH.jpg" width="1000"/>

**Mô tả:**
- Dashboard gọi `StartAutoRefresh()`
- Timer chạy định kỳ
- Mỗi lần tick:
  - Gọi lại `LoadAsync()`
  - Fetch dữ liệu mới
  - Update UI (`StateHasChanged`)
- Giúp dashboard luôn realtime

---

### 27.5 Sequence Diagram - Filter Dashboard Data

<img src="./assets/FILER DATA.jpg" width="1000"/>

**Mô tả:**
- User nhập filter (search, ngày, POI…)
- Dashboard gọi `GetFilteredLichSu()`
- Xử lý:
  - Where (lọc)
  - Contains (tìm kiếm)
  - OrderByDescending (sắp xếp)
- Sau đó:
  - Tính toán:
    - KPI
    - Top User
    - Top POI
    - Biểu đồ
- Render lại UI

---

### 27.6 Sequence Diagram - Auto Refresh (Chi tiết Timer)

<img src="./assets/AUTO REFRESH.jpg" width="1000"/>

**Mô tả bổ sung:**
- `PeriodicTimer` chạy vòng lặp
- `WaitForNextTickAsync()`
- Gọi `LoadAsync()` liên tục
- Dừng khi component dispose

---

### 27.7 Kỹ thuật sử dụng

Dashboard áp dụng các kỹ thuật:
- Gọi API song song (`Task.WhenAll`)
- Timer tự động refresh
- Xử lý dữ liệu phía client (LINQ)
- Render UI động (Blazor)

---

### 27.8 Ý nghĩa hệ thống

- Tối ưu tốc độ load dashboard
- Hiển thị dữ liệu realtime
- Phân tích hành vi người dùng
- Hỗ trợ quản trị ra quyết định

---