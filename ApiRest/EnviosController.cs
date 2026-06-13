using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace ApiRest
{
    [ApiController]
    [Route("api/[controller]")]
    public class EnviosController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly EnvioService _service;

        public EnviosController(AppDbContext context, EnvioService service)
        {
            _context = context;
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> RegistrarEnvio([FromBody] CrearEnvioDto dto)
        {
            if (dto.Peso <= 0) return BadRequest("El peso debe ser mayor a 0.");

            int totalEnvios = await _context.Envios.CountAsync();
            string codigo = _service.GenerarCodigoRastreo(totalEnvios);
            decimal tarifaCalculada = _service.CalcularTarifa(dto.Peso, dto.NitRemitente, dto.NitDestinatario);

            var nuevoEnvio = new Envio
            {
                CodigoRastreo = codigo,
                Remitente = dto.Remitente,
                NitRemitente = dto.NitRemitente,
                Destinatario = dto.Destinatario,
                NitDestinatario = dto.NitDestinatario,
                Peso = dto.Peso,
                Tarifa = tarifaCalculada,
                EstadoActual = "Registrado"
            };

            nuevoEnvio.Historial.Add(new HistorialEnvio
            {
                NuevoEstado = "Registrado",
                UbicacionOficina = "Oficina Central de Carga",
                NotasOpcionales = "Ingreso inicial del paquete al sistema."
            });

            _context.Envios.Add(nuevoEnvio);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(ObtenerPorCodigo), new { codigo = nuevoEnvio.CodigoRastreo }, nuevoEnvio);
        }

        [HttpGet("{codigo}/rastreo")]
        public async Task<IActionResult> ObtenerPorCodigo(string codigo)
        {
            var envio = await _context.Envios
                .Include(e => e.Historial)
                .FirstOrDefaultAsync(e => e.CodigoRastreo == codigo);

            if (envio == null) return NotFound($"No se encontró el envío con código {codigo}");
            return Ok(envio);
        }

        [HttpPut("{codigo}/estado")]
        public async Task<IActionResult> ActualizarEstado(string codigo, [FromBody] ActualizarEstadoDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.UbicacionOficina))
                return BadRequest("Cada actualización debe incluir la ubicación (oficina) donde se realiza el cambio.");

            var envio = await _context.Envios.FirstOrDefaultAsync(e => e.CodigoRastreo == codigo);
            if (envio == null) return NotFound("Envío no encontrado.");

            bool transicionValida = _service.ValidarTransicionEstado(envio.EstadoActual, dto.NuevoEstado);
            if (!transicionValida)
                return BadRequest($"Transición inválida de estado: No se puede pasar de {envio.EstadoActual} a {dto.NuevoEstado}");

            envio.EstadoActual = dto.NuevoEstado;
            _context.Historiales.Add(new HistorialEnvio
            {
                EnvioId = envio.Id,
                NuevoEstado = dto.NuevoEstado,
                UbicacionOficina = dto.UbicacionOficina,
                NotasOpcionales = dto.NotasOpcionales
            });

            await _context.SaveChangesAsync();
            return Ok(new { Mensaje = "Estado actualizado con éxito.", Envio = envio });
        }

        [HttpPost("{codigo}/intento-fallido")]
        public async Task<IActionResult> RegistrarIntentoFallido(string codigo, [FromBody] RegistrarIntentoFallidoDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.UbicacionOficina))
                return BadRequest("Se requiere especificar la oficina/ubicación encargada de la ruta.");

            var envio = await _context.Envios.FirstOrDefaultAsync(e => e.CodigoRastreo == codigo);
            if (envio == null) return NotFound("Envío no encontrado.");

            if (envio.EstadoActual != "EnReparto")
                return BadRequest("Solo se pueden registrar intentos fallidos si el estado actual es 'EnReparto'.");

            envio.IntentosEntrega += 1;
            string notasLog = $"Intento fallido #{envio.IntentosEntrega}. " + dto.NotasOpcionales;

            if (envio.IntentosEntrega >= 3)
            {
                envio.EstadoActual = "EnDevolucion";
                notasLog += " - El paquete alcanzó el máximo de 3 intentos permitidos. Cambia automáticamente a 'EnDevolucion'.";
            }

            _context.Historiales.Add(new HistorialEnvio
            {
                EnvioId = envio.Id,
                NuevoEstado = envio.EstadoActual,
                UbicacionOficina = dto.UbicacionOficina,
                NotasOpcionales = notasLog
            });

            await _context.SaveChangesAsync();
            return Ok(new { Mensaje = "Intento fallido registrado.", Intentos = envio.IntentosEntrega, EstadoActual = envio.EstadoActual });
        }

        [HttpGet]
        public async Task<IActionResult> ListarTodos()
        {
            var envios = await _context.Envios.ToListAsync();
            return Ok(envios);
        }
    }
}