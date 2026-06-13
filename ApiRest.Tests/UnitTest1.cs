using Xunit;
using Microsoft.EntityFrameworkCore;
using ApiRest;
using System.Threading.Tasks;

namespace ApiRest.Tests
{
    public class EnvioTests
    {
        [Fact]
        public void CalcularTarifa_PesoMenorAUnKilogramo_RetornaTarifaBase()
        {
            var service = new EnvioService();
            decimal tarifa = service.CalcularTarifa(0.5, "CF", "CF");
            Assert.Equal(25.00m, tarifa);
        }

        [Fact]
        public void CalcularTarifa_ConNitValido_AplicaCincoPorCientoDescuento()
        {
            var service = new EnvioService();
            decimal tarifaEsperada = 45.00m - (45.00m * 0.05m); 
            decimal tarifaCalculada = service.CalcularTarifa(3.0, "1234567-k", "CF");
            Assert.Equal(tarifaEsperada, tarifaCalculada);
        }

        // 3. Prueba de flujo
        [Fact]
        public void ValidarTransicionEstado_DeRegistradoAEnTransito_RetornaTrue()
        {
            var service = new EnvioService();
            bool esValida = service.ValidarTransicionEstado("Registrado", "EnTransito");

            Assert.True(esValida);
        }

        //Transición de estados inválida
        [Fact]
        public void ValidarTransicionEstado_DeRegistradoAEntregado_RetornaFalse()
        {
            var service = new EnvioService();
            bool esValida = service.ValidarTransicionEstado("Registrado", "Entregado");
            Assert.False(esValida);
        }

        // 5. Prueba de persistencia en Base de Datos
        [Fact]
        public async Task RegistrarEnvio_GuardaCorrectamenteEnBaseDatos()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "DbPruebas_Envios")
                .Options;

            using (var context = new AppDbContext(options))
            {
                var nuevoEnvio = new Envio
                {
                    CodigoRastreo = "ENV-20260613-0001",
                    Remitente = "Carlos López",
                    Destinatario = "María Mercedes",
                    Peso = 2.5,
                    Tarifa = 45.00m,
                    EstadoActual = "Registrado"
                };
                context.Envios.Add(nuevoEnvio);
                await context.SaveChangesAsync();
            }

            using (var context = new AppDbContext(options))
            {
                var envioGuardado = await context.Envios.FirstOrDefaultAsync(e => e.CodigoRastreo == "ENV-20260613-0001");
                Assert.NotNull(envioGuardado);
                Assert.Equal("Carlos López", envioGuardado.Remitente);
                Assert.Equal("Registrado", envioGuardado.EstadoActual);
            }
        }
    }
}