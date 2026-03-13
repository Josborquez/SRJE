namespace SRJE.Web.Models;

public class SrjeSettings
{
    public const string SectionName = "Srje";

    public string CodEmpresa { get; set; } = "06110104519640100572";
    public long CodBancoEstado { get; set; } = 12;
    public int LargoCtaEstado { get; set; } = 15;
    public int LargoCtaOtBanco { get; set; } = 15;
    public int LargoNombreTemge { get; set; } = 39;
}
