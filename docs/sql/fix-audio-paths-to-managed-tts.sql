USE HeThongThuyetMinhDuLich;
GO

-- Script nay dong bo lai DuongDanAmThanh cho bo seed SQL hien tai
-- theo cac file mp3 dang ton tai trong HeThongThuyetMinhDuLich.Api/wwwroot/audio/tts.
-- MaNoiDung 3 va 6 hien chua co file noidung-3.mp3 / noidung-6.mp3, nen de NULL.

UPDATE NoiDungThuyetMinh
SET DuongDanAmThanh = CASE MaNoiDung
        WHEN 1 THEN '/audio/tts/noidung-1.mp3'
        WHEN 2 THEN '/audio/tts/noidung-2.mp3'
        WHEN 4 THEN '/audio/tts/noidung-4.mp3'
        WHEN 5 THEN '/audio/tts/noidung-5.mp3'
        ELSE NULL
    END,
    ThoiLuongGiay = CASE MaNoiDung
        WHEN 1 THEN 95
        WHEN 2 THEN 92
        WHEN 4 THEN 110
        WHEN 5 THEN 105
        ELSE NULL
    END
WHERE MaNoiDung IN (1, 2, 3, 4, 5, 6);
GO

SELECT
    MaNoiDung,
    TieuDe,
    MaNgonNgu,
    DuongDanAmThanh,
    ThoiLuongGiay,
    ChoPhepTTS
FROM NoiDungThuyetMinh
WHERE MaNoiDung IN (1, 2, 3, 4, 5, 6)
ORDER BY MaNoiDung;
GO