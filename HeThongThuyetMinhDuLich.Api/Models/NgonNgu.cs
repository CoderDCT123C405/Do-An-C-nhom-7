namespace HeThongThuyetMinhDuLich.Api.Models;

public class NgonNgu
{
    public int MaNgonNgu { get; set; }
    public string MaNgonNguQuocTe { get; set; } = string.Empty;
    public string TenNgonNgu { get; set; } = string.Empty;
    public bool LaMacDinh { get; set; }
    public bool TrangThaiHoatDong { get; set; } = true;

    public ICollection<NguoiDung> NguoiDungs { get; set; } = [];
    public ICollection<NoiDungThuyetMinh> NoiDungThuyetMinhs { get; set; } = [];
}
