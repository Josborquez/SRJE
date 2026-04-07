# SRJE - Diagrama de Arquitectura y Flujos

## Arquitectura General

```mermaid
graph TB
    subgraph Frontend["Frontend (Vue 3 + Vite - :5173)"]
        Router[Vue Router]
        subgraph Views["Vistas"]
            Login[Login]
            Dashboard[Dashboard]
            ListaBenef[Lista Beneficiarios]
            DetalleBenef[Detalle Beneficiario]
            FichaBenef[Ficha Beneficiario]
            ImpRemun[Importar Remuneraciones]
            ImpTemge[Importar TEMGE]
            ImpCuentas[Importar Nuevas Cuentas]
            GenTemge[Generar TEMGE]
            Auditoria[Auditoria Beneficiarios]
            MantBancos[Mantenedor Bancos]
            MantTiposCta[Mantenedor Tipos Cuenta]
        end
        subgraph Stores["Pinia Stores"]
            AuthStore[Auth Store]
            BenefStore[Beneficiarios Store]
        end
        ApiClient[Axios Client /api]
    end

    subgraph Backend[".NET 8 Backend - :5000"]
        subgraph Middleware["Middleware Pipeline"]
            ExcMiddleware[GlobalExceptionMiddleware]
            CookieAuth[Cookie Authentication]
        end
        subgraph Controllers["Controllers /api/..."]
            AuthCtrl[AuthController]
            BenefCtrl[BeneficiariosController]
            ArchCtrl[ArchivosController]
            CatCtrl[CatalogosController]
            MantCtrl[MantenedoresController]
        end
        subgraph Services["Services (Scoped DI)"]
            AuthSvc[IAuthService / DevAuthService]
            BenefSvc[IBeneficiarioService]
            RemunSvc[IRemuneracionesService]
            TemgeSvc[ITemgeService]
            NuevasSvc[INuevasCuentasService]
            AuditSvc[IAuditoriaBeneficiariosService]
        end
        subgraph Parsers["Parsers"]
            RemunParser[RemuneracionesParser<br/>Fixed-width 126 chars Latin-1]
            TemgeParser[TemgeParser<br/>Fixed-width 129/130 chars Latin-1]
            TemgeBuilder[TemgeBuilder<br/>Generador archivo TEMGE]
            NuevasParser[NuevasCuentasParser<br/>Excel EPPlus]
        end
        subgraph Helpers["Helpers"]
            RutHelper[RutHelper<br/>Validacion modulo 11]
            FWHelper[FixedWidthHelper]
        end
    end

    subgraph Database["Oracle Database"]
        Beneficiario[(BENEFICIARIOS)]
        Retencion[(RETENIDOS_JUDICIALES)]
        Funcionario[(FUNCIONARIOS)]
        Banco[(BANCOS)]
        TipoCuenta[(TIPOS_CUENTA)]
        LogCarga[(LOG_CARGA)]
        LogDetalle[(LOG_CARGA_DETALLE)]
        AuditCambios[(AUDITORIA_CAMBIOS)]
        HistPagos[(HISTORIAL_PAGOS_TEMGE)]
        DetPagos[(DETALLE_PAGO_TEMGE)]
    end

    Router --> Views
    Views --> Stores
    Stores --> ApiClient
    Views --> ApiClient
    ApiClient -->|HTTP /api| Middleware
    Middleware --> Controllers
    AuthCtrl --> AuthSvc
    BenefCtrl --> BenefSvc
    ArchCtrl --> RemunSvc
    ArchCtrl --> TemgeSvc
    ArchCtrl --> NuevasSvc
    ArchCtrl --> AuditSvc
    CatCtrl --> Database
    MantCtrl --> Database
    RemunSvc --> RemunParser
    TemgeSvc --> TemgeParser
    TemgeSvc --> TemgeBuilder
    NuevasSvc --> NuevasParser
    AuditSvc --> RemunParser
    Services --> Database
    Parsers --> Helpers
```

## Flujo de Importacion (Patron Preview-then-Commit)

```mermaid
sequenceDiagram
    actor U as Usuario
    participant V as Vista Vue
    participant API as API /api/archivos
    participant S as Service
    participant P as Parser
    participant DB as Oracle DB

    U->>V: Sube archivo (TXT/Excel)
    V->>API: POST /.../preview (Stream)
    API->>S: PreviewAsync(stream)
    S->>P: Parsear(stream)
    P-->>S: List<PreviewLineaDto>
    S->>DB: Consulta beneficiarios existentes
    S-->>API: ArchivoPreviewDto (lineas con estado)
    API-->>V: JSON Preview

    Note over V: Muestra tabla con estados:<br/>OK / NUEVO / ADVERTENCIA / ERROR<br/>Usuario selecciona lineas

    U->>V: Confirma importacion
    V->>API: POST /.../confirmar (lineas seleccionadas)
    API->>S: ConfirmarAsync(request, usuario, ip)

    rect rgb(230, 245, 255)
        Note over S,DB: Transaccion DB
        S->>DB: Upsert Beneficiarios
        S->>DB: Upsert Retenciones/Cuentas
        S->>DB: Insert LogCarga
        S->>DB: Insert LogCargaDetalle (por linea)
    end

    S-->>API: ResultadoImportacionDto
    API-->>V: Resultado (insertados, actualizados, errores)
    V-->>U: Muestra resumen
```

## Flujo de Remuneraciones (Detallado)

```mermaid
flowchart TD
    A[Archivo TXT Remuneraciones<br/>126 chars/linea, Latin-1] --> B[RemuneracionesParser]
    B --> C{Validar RUT<br/>modulo 11}
    C -->|Valido| D[Estado: OK]
    C -->|Invalido| E[Estado: ERROR]
    D --> F[Verificar si existe<br/>en BD]
    F -->|No existe| G[Estado: NUEVO]
    F -->|Existe| H[Estado: OK]

    G --> I[Preview al Usuario]
    H --> I
    E --> I

    I -->|Confirmar| J[Batch Transaction]

    J --> K{Beneficiario<br/>existe?}
    K -->|No| L[Crear Beneficiario]
    K -->|Si| M[Mantener existente]
    L --> N[Upsert Retencion]
    M --> N
    N --> O{Retencion existe<br/>en periodo?}
    O -->|Si| P[Actualizar monto]
    O -->|No| Q[Insertar nueva]

    P --> R[LogCarga + LogCargaDetalle]
    Q --> R

    subgraph Campos["Campos del Archivo (126 chars)"]
        direction LR
        F1["1-9: RUT Func"]
        F2["11-19: RUT Benef"]
        F3["21-90: Nombre"]
        F4["91-98: Monto"]
        F5["99-109: Cod Ret"]
        F6["110-126: Tipo Pago"]
    end
```

## Flujo TEMGE (Importacion y Generacion)

```mermaid
flowchart LR
    subgraph Importar["Importar TEMGE (entrada)"]
        A1[Archivo TEMGE<br/>del banco] --> A2[TemgeParser]
        A2 --> A3[Header tipo 1<br/>129 chars]
        A2 --> A4[Detalles tipo 2<br/>130 chars c/u]
        A2 --> A5[Footer tipo 3<br/>129 chars]
        A5 --> A6{Validar integridad<br/>monto total + count}
        A6 -->|OK| A7[Preview]
        A6 -->|Error| A8[Advertencia global]
        A7 --> A9[Confirmar]
        A9 --> A10[HistorialPagosTemge<br/>+ DetallePagoTemge]
        A9 --> A11[Actualizar banco/cuenta<br/>en Beneficiarios]
    end

    subgraph Generar["Generar TEMGE (salida)"]
        B1[Seleccionar periodo] --> B2[Cargar retenciones<br/>activas]
        B2 --> B3{Resolver cuenta<br/>bancaria}
        B3 -->|1ro| B4[Datos retencion]
        B3 -->|2do| B5[Datos beneficiario]
        B3 -->|3ro| B6[Default BancoEstado]
        B4 --> B7[TemgeBuilder]
        B5 --> B7
        B6 --> B7
        B7 --> B8[Archivo .txt<br/>Latin-1, CRLF]
        B7 --> B9[HistorialPagosTemge<br/>+ DetallePagoTemge]
        B8 --> B10[Descarga usuario]
    end
```

## Modelo de Datos (Entidades Principales)

```mermaid
erDiagram
    BENEFICIARIOS {
        long Id PK
        long RutBeneficiario UK
        string DvBeneficiario
        string NombreBeneficiario
        string CtaOtBanco
        long TipoCuenta FK
        long CodBanco FK
        string CtaEstado
        long RutFuncionario
        string Estado "A=Activo I=Inactivo"
    }

    RETENIDOS_JUDICIALES {
        long Id PK
        long RutBeneficiario FK
        long RutTitular
        decimal Monto
        string CodRetencion
        string Estado "A=Activo P=Pagado C=Cancelado"
        string PeriodoProceso "YYYYMM"
        long CodBanco FK
        long TipoCuenta FK
    }

    FUNCIONARIOS {
        long Id PK
        long RutFuncionario UK
        string ApellidoPaterno
        string ApellidoMaterno
        string Nombres
        string Activo "S/N"
    }

    BANCOS {
        long CodBanco PK
        string NombreBanco
        string UsaCtaOtBanco "S/N"
        string Activo "S/N"
    }

    TIPOS_CUENTA {
        long CodTipoCuenta PK
        string Descripcion
        string Activo "S/N"
    }

    LOG_CARGA {
        long Id PK
        string TipoCarga "REMUN/TEMGE/CUENTAS"
        string NombreArchivo
        string PeriodoProceso
        string Estado "P/C/E"
        int RegistrosInsertados
        int RegistrosActualizados
        decimal MontoTotal
        string Usuario
    }

    LOG_CARGA_DETALLE {
        long Id PK
        long IdCarga FK
        int NumeroLinea
        string Accion "INSERT/ACTUALIZAR/EXCLUIR"
        string Estado "OK/E"
    }

    AUDITORIA_CAMBIOS {
        long Id PK
        string Entidad
        long IdEntidad
        string Accion "INSERT/ACTUALIZAR/INACTIVAR"
        string CampoModificado
        string ValorAnterior
        string ValorNuevo
        string Usuario
    }

    HISTORIAL_PAGOS_TEMGE {
        long Id PK
        datetime FechaProceso
        decimal MontoTotal
        int CantidadRegistros
        string Estado
    }

    DETALLE_PAGO_TEMGE {
        long Id PK
        long IdHistorial FK
        long RutBeneficiario
        decimal MontoPagado
    }

    BENEFICIARIOS ||--o{ RETENIDOS_JUDICIALES : "tiene retenciones"
    BENEFICIARIOS }o--|| BANCOS : "banco"
    BENEFICIARIOS }o--|| TIPOS_CUENTA : "tipo cuenta"
    BENEFICIARIOS }o--o| FUNCIONARIOS : "funcionario titular"
    LOG_CARGA ||--o{ LOG_CARGA_DETALLE : "detalle lineas"
    HISTORIAL_PAGOS_TEMGE ||--o{ DETALLE_PAGO_TEMGE : "detalle pagos"
```

## Navegacion Frontend

```mermaid
flowchart TD
    Login[/Login/] -->|Auth cookie| Dashboard[Dashboard]

    Dashboard --> BenefModule[Beneficiarios]
    Dashboard --> ArchModule[Archivos]
    Dashboard --> MantModule[Mantenedores]

    subgraph BenefModule["Modulo Beneficiarios"]
        Lista[Lista Beneficiarios<br/>Paginada + Filtros] --> Detalle[Detalle Beneficiario<br/>+ Retenciones]
        Lista --> Nuevo[Crear Beneficiario]
        Detalle --> Editar[Editar Beneficiario]
        Detalle --> Inactivar[Inactivar]
        Lista --> ExportExcel[Exportar Excel]
        Lista --> ExportCSV[Exportar CSV]
    end

    subgraph ArchModule["Modulo Archivos"]
        ImpRemun[Importar Remuneraciones<br/>TXT fixed-width]
        ImpTemge[Importar TEMGE<br/>TXT fixed-width]
        ImpCuentas[Importar Nuevas Cuentas<br/>Excel]
        GenTemge[Generar TEMGE<br/>Descarga TXT]
        AuditBenef[Auditoria Beneficiarios<br/>Comparar archivo vs BD]
    end

    subgraph MantModule["Modulo Mantenedores"]
        MBancos[CRUD Bancos]
        MTipos[CRUD Tipos Cuenta]
    end
```

## Flujo de Autenticacion

```mermaid
sequenceDiagram
    actor U as Usuario
    participant V as Vue App
    participant R as Vue Router
    participant S as Auth Store
    participant API as /api/auth

    U->>V: Accede a ruta protegida
    R->>S: verificarSesion()
    S->>API: GET /auth/me
    API-->>S: 401 No autenticado
    R->>V: Redirect /login

    U->>V: Ingresa credenciales
    V->>S: login(user, pass)
    S->>API: POST /auth/login
    API-->>S: Set-Cookie (8h, HttpOnly, SameSite)
    API-->>S: UsuarioInfo {nombre, rol}
    S->>R: router.push("/")
    R->>V: Dashboard

    Note over API: Roles: admin, operador, consulta
```

## Flujo de Auditoria (Comparacion)

```mermaid
flowchart TD
    A[Subir archivo remuneraciones] --> B[RemuneracionesParser]
    B --> C[Deduplicar por RUT<br/>toma primera ocurrencia]
    C --> D[Cargar beneficiarios<br/>activos de BD]

    D --> E{Comparar}

    E --> F["Solo en Archivo<br/>(nuevos, no en BD)"]
    E --> G["Solo en Sistema<br/>(no en archivo)"]
    E --> H["Con Diferencias<br/>(nombre o RUT func distinto)"]
    E --> I["Coincidentes<br/>(match exacto)"]

    F --> J[Vista comparacion]
    G --> J
    H --> J
    I --> J

    J --> K[Exportar Excel<br/>4 hojas]
    J --> L[Exportar CSV<br/>secciones]
```
