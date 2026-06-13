using System;

namespace ApiRest
{
    public class EnvioService
    {
        public decimal CalcularTarifa(double peso, string nitRemitente, string nitDestinatario)
        {
            decimal tarifaBase = peso switch
            {
                <= 1.0 => 25.00m,
                <= 5.0 => 45.00m,
                <= 10.0 => 75.00m,
                _ => 100.00m
            };

            bool tieneNitValido = (!string.IsNullOrWhiteSpace(nitRemitente) && nitRemitente.ToUpper() != "CF") ||
                                  (!string.IsNullOrWhiteSpace(nitDestinatario) && nitDestinatario.ToUpper() != "CF");

            if (tieneNitValido)
            {
                tarifaBase -= tarifaBase * 0.05m; 
            }

            return tarifaBase;
        }

        public string GenerarCodigoRastreo(int totalEnviosPrevios)
        {
            string fecha = DateTime.Now.ToString("yyyyMMdd");
            int correlativo = totalEnviosPrevios + 1;
            return $"ENV-{fecha}-{correlativo.ToString().PadLeft(4, '0')}";
        }

        public bool ValidarTransicionEstado(string estadoActual, string nuevoEstado)
        {
            return (estadoActual, nuevoEstado) switch
            {
                ("Registrado", "EnTransito") => true,
                ("EnTransito", "EnReparto") => true,
                ("EnReparto", "Entregado") => true,
                ("EnReparto", "Devuelto") => true,
                ("EnReparto", "EnDevolucion") => true,
                ("EnDevolucion", "Devuelto") => true,
                _ => false
            };
        }
    }
}