using System.ComponentModel.DataAnnotations;

namespace ServiceDeskNg.Server.Models.Dtos
{
    /// Prioridades admitidas por la columna prioridad_ticket (ENUM en MySQL).
    public static class PrioridadesTicket
    {
        public const string Baja = "baja";
        public const string Media = "media";
        public const string Alta = "alta";
        public const string Urgente = "urgente";

        public static readonly string[] Todas = [Baja, Media, Alta, Urgente];

        public static bool EsValida(string? prioridad) =>
            prioridad is not null && Todas.Contains(prioridad.ToLowerInvariant());

        /// Prioridades que se consideran de atención preferente.
        public static readonly string[] Preferentes = [Alta, Urgente];
    }

    /// Nombres de estado usados por la lógica de negocio, tal como los carga
    /// Database/DatabaseScript.txt. Se comparan normalizados (minúsculas y guiones).
    public static class EstadosTicket
    {
        public const string Abierto = "abierto";
        public const string EnProgreso = "en-progreso";
        public const string Pendiente = "pendiente";
        public const string PendienteUsuario = "pendiente-usuario";
        public const string Reabierto = "reabierto";
        public const string Resuelto = "resuelto";
        public const string Cerrado = "cerrado";

        /// Estados en los que el ticket sigue consumiendo capacidad del agente.
        public static readonly string[] Activos = [Abierto, EnProgreso, Pendiente, PendienteUsuario, Reabierto];

        /// Estados que cuentan como trabajo terminado.
        public static readonly string[] Finalizados = [Resuelto, Cerrado];
    }

    public class EstadoTicketDto
    {
        public int IdEstado { get; set; }
        public string NombreEstado { get; set; } = null!;
    }

    public class CategoriaTicketDto
    {
        public int IdCategoria { get; set; }
        public string NombreCategoria { get; set; } = null!;
    }

    public class TicketDto
    {
        public int IdTicket { get; set; }
        public int IdCliente { get; set; }
        public int? IdAgenteAsignado { get; set; }
        public int IdEstadoTicket { get; set; }
        public int IdCategoriaTicket { get; set; }
        public string TituloTicket { get; set; } = null!;
        public string DescripcionTicket { get; set; } = null!;
        public string? PrioridadTicket { get; set; }
        public string? UbicacionTicket { get; set; }
        public string? DepartamentoTicket { get; set; }
        public DateTime? FechaHoraCreacionTicket { get; set; }
        public DateTime? FechaHoraActualizacionTicket { get; set; }

        // Datos derivados, para que el cliente no tenga que cruzar catálogos.
        public string? NombreEstado { get; set; }
        public string? NombreCategoria { get; set; }
        public string? NombreCliente { get; set; }
        public string? NombreAgente { get; set; }
    }

    public class TicketCreateDto
    {
        /// Solo lo usan administración y supervisión para abrir un ticket a nombre de un cliente.
        /// Cuando quien crea es el propio cliente, el servidor lo toma de la sesión e ignora este valor.
        public int? IdCliente { get; set; }

        [Required(ErrorMessage = "El título del ticket es obligatorio.")]
        [StringLength(150, MinimumLength = 3)]
        public string TituloTicket { get; set; } = null!;

        [Required(ErrorMessage = "La descripción del ticket es obligatoria.")]
        [StringLength(5000, MinimumLength = 3)]
        public string DescripcionTicket { get; set; } = null!;

        [Range(1, int.MaxValue, ErrorMessage = "Debe indicar una categoría válida.")]
        public int IdCategoriaTicket { get; set; }

        /// Opcional: si no se envía, el ticket nace en estado "Abierto".
        public int? IdEstadoTicket { get; set; }

        public string? PrioridadTicket { get; set; }

        [StringLength(100)]
        public string? UbicacionTicket { get; set; }

        [StringLength(100)]
        public string? DepartamentoTicket { get; set; }
    }

    public class TicketUpdateDto
    {
        [Required(ErrorMessage = "El título del ticket es obligatorio.")]
        [StringLength(150, MinimumLength = 3)]
        public string TituloTicket { get; set; } = null!;

        [Required(ErrorMessage = "La descripción del ticket es obligatoria.")]
        [StringLength(5000, MinimumLength = 3)]
        public string DescripcionTicket { get; set; } = null!;

        [Range(1, int.MaxValue, ErrorMessage = "Debe indicar un estado válido.")]
        public int IdEstadoTicket { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Debe indicar una categoría válida.")]
        public int IdCategoriaTicket { get; set; }

        public string? PrioridadTicket { get; set; }

        [StringLength(100)]
        public string? UbicacionTicket { get; set; }

        [StringLength(100)]
        public string? DepartamentoTicket { get; set; }
    }

    public class AsignarTicketRequest
    {
        [Range(1, int.MaxValue, ErrorMessage = "Ticket inválido.")]
        public int IdTicket { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Agente inválido.")]
        public int IdAgente { get; set; }
    }

    public class EscalarTicketRequest
    {
        [Required(ErrorMessage = "Debe indicar la nueva categoría.")]
        public string NuevaCategoria { get; set; } = null!;
    }

    /// Vista del ticket que usan los paneles de supervisión (campos ya resueltos a texto).
    public class TicketPanelDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Descripcion { get; set; }
        public string User { get; set; } = "Desconocido";
        public string Agent { get; set; } = "Sin asignar";
        public int? IdAgenteAsignado { get; set; }
        public string Status { get; set; } = "";
        public string? Priority { get; set; }
        public string Category { get; set; } = "";

        /// Antigüedad legible del ticket ("45 min", "3 h", "2 d").
        public string Time { get; set; } = "-";
        public DateTime? FechaHoraCreacionTicket { get; set; }
        public DateTime? FechaHoraActualizacionTicket { get; set; }
    }

    public class EscalacionDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string EscalatedTo { get; set; } = "Sin asignar";
        public string Reason { get; set; } = "-";
        public string Time { get; set; } = "-";

        /// critical | pending | resolved
        public string Status { get; set; } = null!;
    }

    public class RendimientoSemanalDto
    {
        public string[] Labels { get; set; } = [];
        public int[] Created { get; set; } = [];
        public int[] Resolved { get; set; } = [];
    }
}
