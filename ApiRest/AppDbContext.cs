using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace ApiRest
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Envio> Envios { get; set; }
        public DbSet<HistorialEnvio> Historiales { get; set; }
    }

    public class Envio
    {
        public int Id { get; set; }
        public string CodigoRastreo { get; set; } = string.Empty;
        public string Remitente { get; set; } = string.Empty;
        public string NitRemitente { get; set; } = string.Empty;
        public string Destinatario { get; set; } = string.Empty;
        public string NitDestinatario { get; set; } = string.Empty;
        public double Peso { get; set; }
        public decimal Tarifa { get; set; }
        public string EstadoActual { get; set; } = "Registrado";
        public int IntentosEntrega { get; set; } = 0;
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public List<HistorialEnvio> Historial { get; set; } = new List<HistorialEnvio>();
    }

    public class HistorialEnvio
    {
        public int Id { get; set; }
        public int EnvioId { get; set; }
        public string NuevoEstado { get; set; } = string.Empty;
        public string UbicacionOficina { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string NotasOpcionales { get; set; } = string.Empty;
    }

    public class CrearEnvioDto
    {
        public string Remitente { get; set; } = string.Empty;
        public string NitRemitente { get; set; } = string.Empty;
        public string Destinatario { get; set; } = string.Empty;
        public string NitDestinatario { get; set; } = string.Empty;
        public double Peso { get; set; }
    }

    public class ActualizarEstadoDto
    {
        public string NuevoEstado { get; set; } = string.Empty;
        public string UbicacionOficina { get; set; } = string.Empty;
        public string NotasOpcionales { get; set; } = string.Empty;
    }

    public class RegistrarIntentoFallidoDto
    {
        public string UbicacionOficina { get; set; } = string.Empty;
        public string NotasOpcionales { get; set; } = string.Empty;
    }
}