using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SSDAP.Shared.DTOs.Reportes;

namespace SSDAP.Web.Services;

// Exporta la captura recibida de la API; no recalcula ni consulta persistencia.
public static class ReportePdf
{
    public static byte[] Generar(string localidad, ProduccionMensualDto produccion,
        CalidadMensualDto? calidad, ResultadoMensualResumenDto? revision = null)
    {
        return Document.Create(document => document.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(30);
            page.DefaultTextStyle(s => s.FontSize(9).FontColor("#24435C"));
            page.Header().PaddingBottom(14).Column(c =>
            {
                c.Item().Text("SSDAP | Reporte mensual").FontSize(20).Bold();
                c.Item().Text(localidad).FontSize(14);
                c.Item().Text($"Período: {produccion.Mes:00}/{produccion.Anio} · " +
                    (revision is null ? "Resultado calculado (sin revisión guardada)" : $"Revisión {revision.Revision} · Registro #{revision.IdResultado}"));
                c.Item().Text($"Cálculo: {Presentacion.Fecha(produccion.CalculadoEnUtc)} (Argentina)").FontColor("#526979");
            });
            page.Content().Column(c =>
            {
                c.Spacing(12);
                c.Item().Background("#EDF4F8").Padding(12).Text(
                    $"Volumen total: {Presentacion.Numero(produccion.VolumenTotalEstimadoM3)} m³   |   Promedio: {Presentacion.Numero(produccion.CaudalPromedioPeriodoM3H)} m³/h\n" +
                    $"Pozos: {Presentacion.Numero(produccion.VolumenPozosM3)} m³   |   Acueductos: {Presentacion.Numero(produccion.VolumenAcueductosM3)} m³").Bold();
                c.Item().Text("Producción programada de pozos").FontSize(12).Bold();
                Tabla(c, ["Pozo (ID)", "Horas", "Volumen (m³)"], produccion.Pozos.Select(p => new[]
                    { p.IdPozo.ToString(), Presentacion.Numero(p.HorasProgramadas), Presentacion.Numero(p.VolumenEstimadoM3) }));
                c.Item().Text("Acueductos").FontSize(12).Bold();
                Tabla(c, ["Fuente", "Caudal (m³/h)", "Volumen (m³)"], produccion.Acueductos.Select(a => new[]
                    { a.Nombre, Presentacion.Numero(a.CaudalPromedioM3H), Presentacion.Numero(a.VolumenEstimadoM3) }));
                if (calidad is not null)
                {
                    c.Item().Text($"Calidad de la mezcla · {Presentacion.Estado(calidad.Estado)}").FontSize(12).Bold();
                    Tabla(c, ["Parámetro", "Concentración", "Unidad", "Evaluación", "Rango"], calidad.Parametros.Select(p => new[]
                    {
                        Presentacion.Parametro(p.Codigo), p.Valor.HasValue ? Presentacion.Numero(p.Valor) : Presentacion.Estado(p.EstadoCalculo),
                        p.Unidad, Presentacion.Estado(p.EstadoEvaluacion), p.Limite is null ? "Sin límite" :
                            $"{Presentacion.Numero(p.Limite.ValorMinimo, "—")} a {Presentacion.Numero(p.Limite.ValorMaximo, "—")}" 
                    }));
                    c.Item().Text("Aportes y análisis utilizados").FontSize(12).Bold();
                    Tabla(c, ["Fuente", "Volumen (m³)", "Análisis"], calidad.Aportes.Select(a => new[]
                    { a.Nombre, Presentacion.Numero(a.VolumenM3), a.Analisis is null ? "Sin análisis" : $"#{a.Analisis.IdAnalisis} · {a.Analisis.Fecha:dd/MM/yyyy}" }));
                    var referencias = calidad.Parametros.Where(p => p.Limite is not null).Select(p => p.Limite!.Referencia).Distinct();
                    foreach (var referencia in referencias) c.Item().Text($"Referencia de límites: {referencia}").FontSize(8);
                }
                c.Item().Text("Cloro medido (mg/L)").FontSize(12).Bold();
                Tabla(c, ["Tipo", "Mediciones", "Mínimo", "Máximo"], produccion.Cloro.Select(m => new[]
                { m.TipoCloro, m.CantidadMediciones.ToString(), Presentacion.Numero(m.MinimoMgL), Presentacion.Numero(m.MaximoMgL) }));
                if (!string.IsNullOrWhiteSpace(revision?.Observaciones)) c.Item().Text($"Observaciones: {revision.Observaciones}");

            });
            page.Footer().Column(footer =>
            {
                footer.Item().PaddingTop(8).Text("Producción programada y calidad estimada; no certifica potabilidad.").FontSize(7).FontColor("#526979");
                footer.Item().AlignRight().Text(t => { t.Span("Página "); t.CurrentPageNumber(); t.Span(" / "); t.TotalPages(); });
            });
        })).GeneratePdf();
    }

    private static void Tabla(ColumnDescriptor columna, string[] titulos, IEnumerable<string[]> filas)
    {
        var datos = filas.ToList();
        if (datos.Count == 0)
        {
            columna.Item().Text("Sin registros en este período.").FontColor("#526979");
            return;
        }
        columna.Item().Table(t =>
        {
            t.ColumnsDefinition(cols => { foreach (var _ in titulos) cols.RelativeColumn(); });
            t.Header(h => { foreach (var titulo in titulos) h.Cell().Background("#E7EFF5").Padding(6).Text(titulo).Bold(); });
            foreach (var fila in datos)
                foreach (var valor in fila)
                {
                    var color = valor switch { "Dentro de rango" => "#21643B", "Fuera de rango" => "#A12D2B", _ => "#24435C" };
                    t.Cell().BorderBottom(0.5f).BorderColor("#DCE5ED").Padding(6).Text(valor).FontColor(color);
                }
        });
    }
}
