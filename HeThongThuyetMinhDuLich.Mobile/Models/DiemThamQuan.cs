using SQLite;

public class DiemThamQuan
{
    [PrimaryKey]
    public int MaDiem { get; set; }

    public string? TenDiem { get; set; }
    public string? MoTaNgan { get; set; }

    public DateTime NgayCapNhat { get; set; }
}