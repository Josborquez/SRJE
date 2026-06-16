namespace SRJE.Web.Models.Entities;

public class LogAcceso
{
    public long Id { get; set; }
    public string Usuario { get; set; } = string.Empty;
    public string Evento { get; set; } = string.Empty; // login_ok | login_fail | logout
    public string? Ip { get; set; }
    public DateTime Fecha { get; set; } // default SYSDATE en BD
}
