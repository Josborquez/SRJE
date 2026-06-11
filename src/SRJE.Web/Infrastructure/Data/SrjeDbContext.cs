using Microsoft.EntityFrameworkCore;
using SRJE.Web.Models.Entities;

namespace SRJE.Web.Infrastructure.Data;

public class SrjeDbContext : DbContext
{
    public SrjeDbContext(DbContextOptions<SrjeDbContext> options) : base(options) { }

    public DbSet<Beneficiario> Beneficiarios => Set<Beneficiario>();
    public DbSet<Funcionario> Funcionarios => Set<Funcionario>();
    public DbSet<RetenidoJudicial> RetenidosJudiciales => Set<RetenidoJudicial>();
    public DbSet<HistorialPagosTemge> HistorialPagosTemge => Set<HistorialPagosTemge>();
    public DbSet<DetallePagoTemge> DetallePagosTemge => Set<DetallePagoTemge>();
    public DbSet<Banco> Bancos => Set<Banco>();
    public DbSet<TipoRetencion> TiposRetencion => Set<TipoRetencion>();
    public DbSet<LogCarga> LogCargas => Set<LogCarga>();
    public DbSet<LogCargaDetalle> LogCargaDetalles => Set<LogCargaDetalle>();
    public DbSet<AuditoriaCambios> AuditoriaCambios => Set<AuditoriaCambios>();
    public DbSet<TipoCuenta> TiposCuenta => Set<TipoCuenta>();
    public DbSet<CuentaBeneficiario> CuentasBeneficiario => Set<CuentaBeneficiario>();
    public DbSet<UsuarioSistema> UsuariosSistema => Set<UsuarioSistema>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // BENEFICIARIOS
        modelBuilder.Entity<Beneficiario>(e =>
        {
            e.ToTable("BENEFICIARIOS");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("ID");
            e.Property(x => x.RutBeneficiario).HasColumnName("RUT_BENEFICIARIO").IsRequired();
            e.Property(x => x.DvBeneficiario).HasColumnName("DV_BENEFICIARIO").HasMaxLength(1).IsRequired();
            e.Property(x => x.NombreBeneficiario).HasColumnName("NOMBRE_BENEFICIARIO").HasMaxLength(39).IsRequired();
            e.Property(x => x.FechaNacimiento).HasColumnName("FECHA_NACIMIENTO");
            e.Property(x => x.Sexo).HasColumnName("SEXO").HasMaxLength(1);
            e.Property(x => x.EstadoCivil).HasColumnName("ESTADO_CIVIL").HasMaxLength(20);
            e.Property(x => x.Domicilio).HasColumnName("DOMICILIO").HasMaxLength(100);
            e.Property(x => x.Comuna).HasColumnName("COMUNA").HasMaxLength(50);
            e.Property(x => x.Telefono).HasColumnName("TELEFONO").HasMaxLength(20);
            e.Property(x => x.CtaOtBanco).HasColumnName("CTA_OT_BANCO").HasMaxLength(15);
            e.Property(x => x.TipoCuenta).HasColumnName("TIPO_CUENTA");
            e.Property(x => x.CodBanco).HasColumnName("COD_BANCO");
            e.Property(x => x.CtaEstado).HasColumnName("CTA_ESTADO").HasMaxLength(15);
            e.Property(x => x.Sucursal).HasColumnName("SUCURSAL").HasMaxLength(60);
            e.Property(x => x.Estado).HasColumnName("ESTADO").HasMaxLength(1).HasDefaultValue("A");
            e.Property(x => x.FechaCreacion).HasColumnName("FECHA_CREACION").HasDefaultValueSql("SYSDATE");
            e.Property(x => x.FechaModificacion).HasColumnName("FECHA_MODIFICACION");
            e.Property(x => x.UsuarioCreacion).HasColumnName("USUARIO_CREACION").HasMaxLength(50);
            e.HasIndex(x => x.RutBeneficiario).IsUnique();
            // TipoCuenta validado por tabla catalogo TIPOS_CUENTA, sin CHECK constraint
        });

        // FUNCIONARIOS
        modelBuilder.Entity<Funcionario>(e =>
        {
            e.ToTable("FUNCIONARIOS");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("ID");
            e.Property(x => x.RutFuncionario).HasColumnName("RUT_FUNCIONARIO").IsRequired();
            e.Property(x => x.DvFuncionario).HasColumnName("DV_FUNCIONARIO").HasMaxLength(1).IsRequired();
            e.Property(x => x.ApellidoPaterno).HasColumnName("APELLIDO_PATERNO").HasMaxLength(20);
            e.Property(x => x.ApellidoMaterno).HasColumnName("APELLIDO_MATERNO").HasMaxLength(20);
            e.Property(x => x.Nombres).HasColumnName("NOMBRES").HasMaxLength(30);
            e.Property(x => x.IdSistema).HasColumnName("ID_SISTEMA").HasMaxLength(8);
            e.Property(x => x.Activo).HasColumnName("ACTIVO").HasMaxLength(1).HasDefaultValue("S");
            e.Property(x => x.FechaCreacion).HasColumnName("FECHA_CREACION").HasDefaultValueSql("SYSDATE");
            e.Property(x => x.FechaModificacion).HasColumnName("FECHA_MODIFICACION");
            e.Property(x => x.UsuarioCreacion).HasColumnName("USUARIO_CREACION").HasMaxLength(50);
            e.HasIndex(x => x.RutFuncionario).IsUnique();
        });

        // RETENIDO_JUDICIAL
        modelBuilder.Entity<RetenidoJudicial>(e =>
        {
            e.ToTable("RETENIDO_JUDICIAL");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("ID");
            e.Property(x => x.IdRetencion).HasColumnName("ID_RETENCION").IsRequired();
            e.Property(x => x.RutTitular).HasColumnName("RUT_TITULAR").IsRequired();
            e.Property(x => x.DvTitular).HasColumnName("DV_TITULAR").HasMaxLength(1).IsRequired();
            e.Property(x => x.RutBeneficiario).HasColumnName("RUT_BENEFICIARIO").IsRequired();
            e.Property(x => x.DvBeneficiario).HasColumnName("DV_BENEFICIARIO").HasMaxLength(1).IsRequired();
            e.Property(x => x.Monto).HasColumnName("MONTO").HasColumnType("NUMBER(18,2)").IsRequired();
            e.Property(x => x.CodRetencion).HasColumnName("COD_RETENCION").HasMaxLength(11);
            e.Property(x => x.TipoPago).HasColumnName("TIPO_PAGO").HasMaxLength(17);
            e.Property(x => x.Estado).HasColumnName("ESTADO").HasMaxLength(1).HasDefaultValue("A");
            e.Property(x => x.FechaVigencia).HasColumnName("FECHA_VIGENCIA");
            e.Property(x => x.PeriodoProceso).HasColumnName("PERIODO_PROCESO").HasMaxLength(6).IsFixedLength();
            e.Property(x => x.CodBanco).HasColumnName("COD_BANCO");
            e.Property(x => x.TipoCuenta).HasColumnName("TIPO_CUENTA");
            e.Property(x => x.CtaEstado).HasColumnName("CTA_ESTADO").HasMaxLength(15);
            e.Property(x => x.CtaOtBanco).HasColumnName("CTA_OT_BANCO").HasMaxLength(15);
            e.HasIndex(x => new { x.RutTitular, x.RutBeneficiario, x.PeriodoProceso });
        });

        // HISTORIAL_PAGOS_TEMGE
        modelBuilder.Entity<HistorialPagosTemge>(e =>
        {
            e.ToTable("HISTORIAL_PAGOS_TEMGE");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("ID");
            e.Property(x => x.FechaProceso).HasColumnName("FECHA_PROCESO").IsRequired();
            e.Property(x => x.CodEmpresa).HasColumnName("COD_EMPRESA").HasMaxLength(21);
            e.Property(x => x.MontoTotal).HasColumnName("MONTO_TOTAL").HasColumnType("NUMBER(18,2)");
            e.Property(x => x.CantidadRegistros).HasColumnName("CANTIDAD_REGISTROS");
            e.Property(x => x.NombreArchivo).HasColumnName("NOMBRE_ARCHIVO").HasMaxLength(200);
            e.Property(x => x.Estado).HasColumnName("ESTADO").HasMaxLength(1).HasDefaultValue("G");
            e.Property(x => x.UsuarioGenera).HasColumnName("USUARIO_GENERA").HasMaxLength(50);
            e.Property(x => x.PeriodoProceso).HasColumnName("PERIODO_PROCESO").HasMaxLength(6).IsFixedLength();
            e.HasMany(x => x.Detalles).WithOne(x => x.Historial).HasForeignKey(x => x.IdHistorial);
        });

        // DETALLE_PAGO_TEMGE
        modelBuilder.Entity<DetallePagoTemge>(e =>
        {
            e.ToTable("DETALLE_PAGO_TEMGE");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("ID");
            e.Property(x => x.IdHistorial).HasColumnName("ID_HISTORIAL").IsRequired();
            e.HasIndex(x => x.IdHistorial);
            e.Property(x => x.IdRetenidoJudicial).HasColumnName("ID_RETENIDO_JUDICIAL");
            e.Property(x => x.RutBeneficiario).HasColumnName("RUT_BENEFICIARIO").IsRequired();
            e.Property(x => x.MontoPagado).HasColumnName("MONTO_PAGADO").HasColumnType("NUMBER(18,2)");
            e.Property(x => x.CodBanco).HasColumnName("COD_BANCO");
            e.Property(x => x.TipoCuenta).HasColumnName("TIPO_CUENTA");
            e.Property(x => x.EstadoLinea).HasColumnName("ESTADO_LINEA").HasMaxLength(1).HasDefaultValue("P");
            e.Property(x => x.MotivoExclusion).HasColumnName("MOTIVO_EXCLUSION").HasMaxLength(200);
        });

        // BANCOS
        modelBuilder.Entity<Banco>(e =>
        {
            e.ToTable("BANCOS");
            e.HasKey(x => x.CodBanco);
            e.Property(x => x.CodBanco).HasColumnName("COD_BANCO");
            e.Property(x => x.NombreBanco).HasColumnName("NOMBRE_BANCO").HasMaxLength(100).IsRequired();
            e.Property(x => x.UsaCtaOtBanco).HasColumnName("USA_CTA_OT_BANCO").HasMaxLength(1).HasDefaultValue("S");
            e.Property(x => x.Activo).HasColumnName("ACTIVO").HasMaxLength(1).HasDefaultValue("S");
            e.Property(x => x.CodBancoNew).HasColumnName("COD_BANCO_NEW");
        });

        // TIPOS_CUENTA
        modelBuilder.Entity<TipoCuenta>(e =>
        {
            e.ToTable("TIPOS_CUENTA");
            e.HasKey(x => x.CodTipoCuenta);
            e.Property(x => x.CodTipoCuenta).HasColumnName("COD_TIPO_CUENTA");
            e.Property(x => x.Descripcion).HasColumnName("DESCRIPCION").HasMaxLength(100).IsRequired();
            e.Property(x => x.Activo).HasColumnName("ACTIVO").HasMaxLength(1).HasDefaultValue("S");
        });

        // TIPOS_RETENCION
        modelBuilder.Entity<TipoRetencion>(e =>
        {
            e.ToTable("TIPOS_RETENCION");
            e.HasKey(x => x.CodRetencion);
            e.Property(x => x.CodRetencion).HasColumnName("COD_RETENCION").HasMaxLength(11);
            e.Property(x => x.Descripcion).HasColumnName("DESCRIPCION").HasMaxLength(100).IsRequired();
            e.Property(x => x.Moneda).HasColumnName("MONEDA").HasMaxLength(10).HasDefaultValue("CLP");
            e.Property(x => x.Activo).HasColumnName("ACTIVO").HasMaxLength(1).HasDefaultValue("S");
        });

        // LOG_CARGAS
        modelBuilder.Entity<LogCarga>(e =>
        {
            e.ToTable("LOG_CARGAS");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("ID");
            e.Property(x => x.TipoCarga).HasColumnName("TIPO_CARGA").HasMaxLength(30).IsRequired();
            e.Property(x => x.NombreArchivo).HasColumnName("NOMBRE_ARCHIVO").HasMaxLength(260);
            e.Property(x => x.HashArchivo).HasColumnName("HASH_ARCHIVO").HasMaxLength(64);
            e.Property(x => x.TamanioBytes).HasColumnName("TAMANIO_BYTES");
            e.Property(x => x.PeriodoProceso).HasColumnName("PERIODO_PROCESO").HasMaxLength(6).IsFixedLength();
            e.Property(x => x.FechaInicio).HasColumnName("FECHA_INICIO");
            e.Property(x => x.FechaFin).HasColumnName("FECHA_FIN");
            e.Property(x => x.DuracionMs).HasColumnName("DURACION_MS");
            e.Property(x => x.Estado).HasColumnName("ESTADO").HasMaxLength(1).HasDefaultValue("P");
            e.Property(x => x.TotalLineas).HasColumnName("TOTAL_LINEAS");
            e.Property(x => x.RegistrosInsertados).HasColumnName("REGISTROS_INSERTADOS");
            e.Property(x => x.RegistrosActualizados).HasColumnName("REGISTROS_ACTUALIZADOS");
            e.Property(x => x.RegistrosExcluidos).HasColumnName("REGISTROS_EXCLUIDOS");
            e.Property(x => x.RegistrosError).HasColumnName("REGISTROS_ERROR");
            e.Property(x => x.MontoTotal).HasColumnName("MONTO_TOTAL").HasColumnType("NUMBER(18,2)");
            e.Property(x => x.Usuario).HasColumnName("USUARIO").HasMaxLength(50);
            e.Property(x => x.IpUsuario).HasColumnName("IP_USUARIO").HasMaxLength(50);
            e.Property(x => x.Observaciones).HasColumnName("OBSERVACIONES").HasMaxLength(2000);
            e.Property(x => x.PuedeRevertir).HasColumnName("PUEDE_REVERTIR").HasMaxLength(1).HasDefaultValue("S");
            e.HasMany(x => x.Detalles).WithOne(x => x.Carga).HasForeignKey(x => x.IdCarga);
        });

        // LOG_CARGA_DETALLE
        modelBuilder.Entity<LogCargaDetalle>(e =>
        {
            e.ToTable("LOG_CARGA_DETALLE");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("ID");
            e.Property(x => x.IdCarga).HasColumnName("ID_CARGA").IsRequired();
            e.HasIndex(x => x.IdCarga);
            e.Property(x => x.NumeroLinea).HasColumnName("NUMERO_LINEA");
            e.Property(x => x.RutReferencia).HasColumnName("RUT_REFERENCIA").HasMaxLength(15);
            e.Property(x => x.Accion).HasColumnName("ACCION").HasMaxLength(15);
            e.Property(x => x.Estado).HasColumnName("ESTADO").HasMaxLength(2);
            e.Property(x => x.DatosOriginales).HasColumnName("DATOS_ORIGINALES");
            e.Property(x => x.DatosAnteriores).HasColumnName("DATOS_ANTERIORES");
            e.Property(x => x.DatosNuevos).HasColumnName("DATOS_NUEVOS");
            e.Property(x => x.Mensajes).HasColumnName("MENSAJES").HasMaxLength(2000);
        });

        // AUDITORIA_CAMBIOS
        modelBuilder.Entity<AuditoriaCambios>(e =>
        {
            e.ToTable("AUDITORIA_CAMBIOS");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("ID");
            e.Property(x => x.Entidad).HasColumnName("ENTIDAD").HasMaxLength(50).IsRequired();
            e.Property(x => x.IdEntidad).HasColumnName("ID_ENTIDAD").IsRequired();
            e.Property(x => x.RutAfectado).HasColumnName("RUT_AFECTADO").HasMaxLength(15);
            e.Property(x => x.Accion).HasColumnName("ACCION").HasMaxLength(20).IsRequired();
            e.Property(x => x.CampoModificado).HasColumnName("CAMPO_MODIFICADO").HasMaxLength(60);
            e.Property(x => x.ValorAnterior).HasColumnName("VALOR_ANTERIOR").HasMaxLength(500);
            e.Property(x => x.ValorNuevo).HasColumnName("VALOR_NUEVO").HasMaxLength(500);
            e.Property(x => x.Usuario).HasColumnName("USUARIO").HasMaxLength(50).IsRequired();
            e.Property(x => x.Fecha).HasColumnName("FECHA");
            e.Property(x => x.Ip).HasColumnName("IP").HasMaxLength(50);
            e.Property(x => x.Motivo).HasColumnName("MOTIVO").HasMaxLength(500);
            e.HasIndex(x => new { x.Entidad, x.IdEntidad });
            e.HasIndex(x => x.RutAfectado);
        });

        // CUENTAS_BENEFICIARIO
        modelBuilder.Entity<CuentaBeneficiario>(e =>
        {
            e.ToTable("CUENTAS_BENEFICIARIO");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("ID").ValueGeneratedOnAdd();
            e.Property(x => x.RutBeneficiario).HasColumnName("RUT_BENEFICIARIO").IsRequired();
            e.Property(x => x.CodBanco).HasColumnName("COD_BANCO").IsRequired();
            e.Property(x => x.TipoCuenta).HasColumnName("TIPO_CUENTA").IsRequired();
            e.Property(x => x.NumeroCuenta).HasColumnName("NUMERO_CUENTA").HasMaxLength(15).IsRequired();
            e.Property(x => x.Alias).HasColumnName("ALIAS").HasMaxLength(60);
            e.Property(x => x.Orden).HasColumnName("ORDEN").HasDefaultValue(1);
            e.Property(x => x.Estado).HasColumnName("ESTADO").HasMaxLength(1).HasDefaultValue("A");
            e.Property(x => x.FechaCreacion).HasColumnName("FECHA_CREACION").HasDefaultValueSql("SYSDATE");
            e.Property(x => x.UsuarioCreacion).HasColumnName("USUARIO_CREACION").HasMaxLength(50);
            e.HasIndex(x => x.RutBeneficiario);
            e.HasIndex(x => new { x.RutBeneficiario, x.CodBanco, x.TipoCuenta, x.NumeroCuenta }).IsUnique();
        });

        // USUARIOS_SISTEMA
        modelBuilder.Entity<UsuarioSistema>(e =>
        {
            e.ToTable("USUARIOS_SISTEMA");
            e.HasKey(x => x.Usuario);
            e.Property(x => x.Usuario).HasColumnName("USUARIO").HasMaxLength(50);
            e.Property(x => x.PasswordHash).HasColumnName("PASSWORD_HASH").HasMaxLength(128).IsRequired();
            e.Property(x => x.NombreCompleto).HasColumnName("NOMBRE_COMPLETO").HasMaxLength(100).IsRequired();
            e.Property(x => x.Rol).HasColumnName("ROL").HasMaxLength(20).IsRequired();
            e.Property(x => x.Estado).HasColumnName("ESTADO").HasMaxLength(1).HasDefaultValue("A");
            e.Property(x => x.FechaCreacion).HasColumnName("FECHA_CREACION").HasDefaultValueSql("SYSDATE");
        });
    }
}
