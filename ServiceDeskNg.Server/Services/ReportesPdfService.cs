using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ServiceDeskNg.Server.Models.Dtos;

namespace ServiceDeskNg.Server.Services
{
    /// Generación de los PDF de reportes. Antes esto estaba dentro del controlador,
    /// y dos de los tres reportes devolvían una cadena de texto con extensión .pdf.
    public class ReportesPdfService
    {
        public byte[] CargaDeTrabajo(IReadOnlyList<CargaAgenteDto> carga) =>
            Documento(
                "Reporte de carga de trabajo",
                contenido => contenido.Column(columna =>
                {
                    if (carga.Count == 0)
                    {
                        columna.Item().Text("No hay agentes registrados.");
                        return;
                    }

                    columna.Item().Table(tabla =>
                    {
                        tabla.ColumnsDefinition(columnas =>
                        {
                            columnas.RelativeColumn();
                            columnas.ConstantColumn(70);
                            columnas.ConstantColumn(70);
                            columnas.ConstantColumn(70);
                        });

                        tabla.Header(cabecera =>
                        {
                            Encabezado(cabecera.Cell(), "Agente");
                            Encabezado(cabecera.Cell(), "Activos");
                            Encabezado(cabecera.Cell(), "Resueltos");
                            Encabezado(cabecera.Cell(), "Total");
                        });

                        foreach (var agente in carga)
                        {
                            Celda(tabla.Cell(), agente.Nombre);
                            Celda(tabla.Cell(), agente.TicketsActivos.ToString());
                            Celda(tabla.Cell(), agente.TicketsResueltos.ToString());
                            Celda(tabla.Cell(), agente.TicketsTotales.ToString());
                        }
                    });

                    columna.Item().PaddingTop(10).Text(
                        $"Total de tickets activos: {carga.Sum(c => c.TicketsActivos)}");
                }));

        public byte[] ResumenSemanal(
            MetricasService.ResumenSemanal resumen,
            IReadOnlyList<ComparativaAgenteDto> comparativa) =>
            Documento(
                "Reporte semanal de servicio",
                contenido => contenido.Column(columna =>
                {
                    columna.Spacing(8);

                    columna.Item().Text(
                        $"Periodo: {resumen.Desde:dd/MM/yyyy HH:mm} a {resumen.Hasta:dd/MM/yyyy HH:mm} (UTC)");

                    columna.Item().Text($"Tickets creados: {resumen.Creados}");
                    columna.Item().Text($"Tickets finalizados: {resumen.Finalizados}");
                    columna.Item().Text($"Tickets sin asignar: {resumen.SinAsignar}");
                    columna.Item().Text(
                        $"Tickets que superan el SLA de {resumen.HorasSla} h: {resumen.Vencidos}");
                    columna.Item().Text(resumen.HorasPromedioResolucion.HasValue
                        ? $"Tiempo promedio de resolución: {resumen.HorasPromedioResolucion:0.0} h"
                        : "Tiempo promedio de resolución: sin tickets finalizados en el periodo");

                    if (comparativa.Count == 0)
                        return;

                    columna.Item().PaddingTop(10).Text("Detalle por agente").FontSize(14).Bold();

                    columna.Item().Table(tabla =>
                    {
                        tabla.ColumnsDefinition(columnas =>
                        {
                            columnas.RelativeColumn();
                            columnas.ConstantColumn(70);
                            columnas.ConstantColumn(70);
                            columnas.ConstantColumn(60);
                            columnas.ConstantColumn(70);
                        });

                        tabla.Header(cabecera =>
                        {
                            Encabezado(cabecera.Cell(), "Agente");
                            Encabezado(cabecera.Cell(), "Asignados");
                            Encabezado(cabecera.Cell(), "Resueltos");
                            Encabezado(cabecera.Cell(), "Tasa");
                            Encabezado(cabecera.Cell(), "Promedio");
                        });

                        foreach (var agente in comparativa)
                        {
                            Celda(tabla.Cell(), agente.Name);
                            Celda(tabla.Cell(), agente.Asignados.ToString());
                            Celda(tabla.Cell(), agente.Resueltos.ToString());
                            Celda(tabla.Cell(), $"{agente.TasaResolucion:0.#} %");
                            Celda(tabla.Cell(), agente.TiempoPromedioHoras.HasValue
                                ? $"{agente.TiempoPromedioHoras:0.0} h"
                                : "N/D");
                        }
                    });
                }));

        public byte[] ReporteIndividual(
            string nombreAgente,
            ComparativaAgenteDto? metricas,
            IReadOnlyList<TicketDto> tickets) =>
            Documento(
                $"Reporte individual: {nombreAgente}",
                contenido => contenido.Column(columna =>
                {
                    columna.Spacing(6);

                    columna.Item().Text($"Tickets asignados: {tickets.Count}");

                    if (metricas is not null)
                    {
                        columna.Item().Text($"Resueltos: {metricas.Resueltos}");
                        columna.Item().Text($"Activos: {metricas.Activos}");
                        columna.Item().Text($"Tasa de resolución: {metricas.TasaResolucion:0.#} %");
                        columna.Item().Text(metricas.TiempoPromedioHoras.HasValue
                            ? $"Tiempo promedio de resolución: {metricas.TiempoPromedioHoras:0.0} h"
                            : "Tiempo promedio de resolución: N/D");
                    }

                    if (tickets.Count == 0)
                    {
                        columna.Item().PaddingTop(10).Text("Este agente no tiene tickets asignados.");
                        return;
                    }

                    columna.Item().PaddingTop(10).Table(tabla =>
                    {
                        tabla.ColumnsDefinition(columnas =>
                        {
                            columnas.ConstantColumn(40);
                            columnas.RelativeColumn();
                            columnas.ConstantColumn(90);
                            columnas.ConstantColumn(70);
                            columnas.ConstantColumn(90);
                        });

                        tabla.Header(cabecera =>
                        {
                            Encabezado(cabecera.Cell(), "ID");
                            Encabezado(cabecera.Cell(), "Título");
                            Encabezado(cabecera.Cell(), "Estado");
                            Encabezado(cabecera.Cell(), "Prioridad");
                            Encabezado(cabecera.Cell(), "Creado (UTC)");
                        });

                        foreach (var ticket in tickets)
                        {
                            Celda(tabla.Cell(), ticket.IdTicket.ToString());
                            Celda(tabla.Cell(), ticket.TituloTicket);
                            Celda(tabla.Cell(), ticket.NombreEstado ?? "-");
                            Celda(tabla.Cell(), ticket.PrioridadTicket ?? "-");
                            Celda(tabla.Cell(), ticket.FechaHoraCreacionTicket?.ToString("dd/MM/yyyy HH:mm") ?? "-");
                        }
                    });
                }));

        private static byte[] Documento(string titulo, Action<IContainer> contenido) =>
            Document.Create(documento =>
            {
                documento.Page(pagina =>
                {
                    pagina.Size(PageSizes.A4);
                    pagina.Margin(30);
                    pagina.DefaultTextStyle(estilo => estilo.FontSize(10));

                    pagina.Header().Column(columna =>
                    {
                        columna.Item().Text(titulo).FontSize(18).Bold();
                        columna.Item().Text("ServiceDesk TI").FontSize(10).FontColor(Colors.Grey.Darken1);
                        columna.Item().PaddingBottom(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                    });

                    pagina.Content().Element(contenido);

                    pagina.Footer().AlignCenter().Text(texto =>
                    {
                        texto.Span("Generado el ");
                        texto.Span($"{DateTime.UtcNow:dd/MM/yyyy HH:mm} UTC").SemiBold();
                        texto.Span("  ·  página ");
                        texto.CurrentPageNumber();
                        texto.Span(" de ");
                        texto.TotalPages();
                    });
                });
            }).GeneratePdf();

        private static void Encabezado(IContainer celda, string texto) =>
            celda
                .Background(Colors.Grey.Lighten3)
                .PaddingVertical(5)
                .PaddingHorizontal(4)
                .Text(texto)
                .Bold();

        private static void Celda(IContainer celda, string texto) =>
            celda
                .BorderBottom(1)
                .BorderColor(Colors.Grey.Lighten2)
                .PaddingVertical(4)
                .PaddingHorizontal(4)
                .Text(texto);
    }
}
