using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using PIA1._0.Models;

namespace PIA1._0
{
    public class DatabaseService
    {
        private readonly string _connectionString = "Server=KAI\\SQLEXPRESS;Database=GameStoreDB;Trusted_Connection=True;TrustServerCertificate=True;";

        // Modelo que coincide con tu estructura de base de datos
        public async Task<List<InventarioProducto>> GetInventarioAsync()
        {
            var items = new List<InventarioProducto>();
            const string query = @"
                SELECT 
                    p.Id AS ProductoId,
                    p.Nombre AS NombreProducto,
                    p.Plataforma,
                    p.PrecioVenta,
                    SUM(i.CantidadActual) AS CantidadActual,    -- Total de stock (agrupado)
                    MIN(i.StockMinimo) AS StockMinimo,          -- El stock mínimo de esa agrupación
                    MAX(i.StockMaximo) AS StockMaximo,          -- El stock máximo de esa agrupación
                    MIN(i.PuntoReorden) AS PuntoReorden         -- El punto de reorden de esa agrupación
                FROM Productos p
                INNER JOIN Inventario i ON p.Id = i.ProductoId
                GROUP BY p.Id, p.Nombre, p.Plataforma, p.PrecioVenta  -- Agrupar por Producto
                ORDER BY p.Nombre";

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(query, connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        items.Add(new InventarioProducto
                        {
                            ProductoId = reader.GetInt32(0),
                            NombreProducto = reader.GetString(1),
                            Plataforma = reader.GetString(2),
                            PrecioVenta = reader.GetDecimal(3),
                            CantidadActual = reader.GetInt32(4),
                            StockMinimo = reader.GetInt32(5),
                            StockMaximo = reader.GetInt32(6),
                            PuntoReorden = reader.GetInt32(7)
                        });
                    }
                }
            }
            return items;
        }

        public async Task<List<InventarioProducto>> BuscarProductosAsync(string criterio)
        {
            var items = new List<InventarioProducto>();
            const string query = @"
                SELECT 
                    p.Id AS ProductoId,
                    p.Nombre AS NombreProducto,
                    p.Plataforma,
                    p.PrecioVenta,
                    SUM(i.CantidadActual) AS CantidadActual,    -- Total de stock (agrupado)
                    MIN(i.StockMinimo) AS StockMinimo,          -- El stock mínimo de esa agrupación
                    MAX(i.StockMaximo) AS StockMaximo,          -- El stock máximo de esa agrupación
                    MIN(i.PuntoReorden) AS PuntoReorden         -- El punto de reorden de esa agrupación
                    FROM Productos p
                    INNER JOIN Inventario i ON p.Id = i.ProductoId
                    GROUP BY p.Id, p.Nombre, p.Plataforma, p.PrecioVenta  -- Agrupar por Producto
                    ORDER BY p.Nombre";

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Criterio", $"%{criterio}%");

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            items.Add(new InventarioProducto
                            {
                                
                                ProductoId = reader.GetInt32(0),
                                NombreProducto = reader.GetString(1),
                                Plataforma = reader.GetString(2),
                                PrecioVenta = reader.GetDecimal(3),
                                CantidadActual = reader.GetInt32(4),
                                StockMinimo = reader.GetInt32(5),
                                StockMaximo = reader.GetInt32(6),
                                PuntoReorden = reader.GetInt32(7)
                            });
                        }
                    }
                }
            }
            return items;
        }
        public async Task<bool> CreateProductoConInventarioAsync(NuevoProductoInventario item)
        {
            const string query = @"
                BEGIN TRANSACTION;
                BEGIN TRY
                    INSERT INTO Productos (Nombre, Plataforma, PrecioVenta) 
                    VALUES (@Nombre, @Plataforma, @PrecioVenta);
            
                    DECLARE @NuevoProductoId INT = SCOPE_IDENTITY();

                    INSERT INTO Inventario (ProductoId, CantidadActual, StockMinimo, StockMaximo, PuntoReorden)
                    VALUES (@NuevoProductoId, @CantidadActual, @StockMinimo, @StockMaximo, @PuntoReorden);
                    
                    COMMIT TRANSACTION;
                    SELECT 1;
                END TRY
                BEGIN CATCH
                    ROLLBACK TRANSACTION;
                    SELECT 0;
                END CATCH";

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Nombre", item.Nombre);
                    command.Parameters.AddWithValue("@Plataforma", item.Plataforma);
                    command.Parameters.AddWithValue("@PrecioVenta", item.PrecioVenta);
                    command.Parameters.AddWithValue("@CantidadActual", item.CantidadActual);
                    command.Parameters.AddWithValue("@StockMinimo", item.StockMinimo);
                    command.Parameters.AddWithValue("@StockMaximo", item.StockMaximo);
                    command.Parameters.AddWithValue("@PuntoReorden", item.PuntoReorden);

                    var result = await command.ExecuteScalarAsync();
                    return (int)result == 1;
                }
            }
        }

        public async Task<bool> UpdateStockAsync(int inventarioId, int nuevaCantidad)
        {
            const string query = @"
                UPDATE Inventario 
                SET CantidadActual = @Cantidad 
                WHERE Id = @InventarioId";

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Cantidad", nuevaCantidad);
                    command.Parameters.AddWithValue("@InventarioId", inventarioId);

                    var rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }

        public async Task<bool> DeleteProductoCompletoAsync(int productoId)
        {
            const string query = @"
                BEGIN TRANSACTION;
                BEGIN TRY
                    DELETE FROM Inventario WHERE ProductoId = @ProductoId;
                    DELETE FROM Productos WHERE Id = @ProductoId;
                    COMMIT TRANSACTION;
                    SELECT 1;
                END TRY
                BEGIN CATCH
                    ROLLBACK TRANSACTION;
                    SELECT 0;
                END CATCH";

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ProductoId", productoId);
                    var result = await command.ExecuteScalarAsync();
                    return (int)result == 1;
                }
            }
        }

        public async Task<bool> ValidarUsuarioAsync(string nombreUsuario, string password)
        {
            const string query = @"
                SELECT 1 FROM Usuarios 
                WHERE NombreUsuario = @Usuario AND Password = @Password";

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Usuario", nombreUsuario);
                    command.Parameters.AddWithValue("@Password", password);

                    var result = await command.ExecuteScalarAsync();
                    return result != null;
                }
            }
        }

        // Nuevo método para obtener estadísticas
        public async Task<Dictionary<string, object>> GetEstadisticasAsync()
        {
            var stats = new Dictionary<string, object>();
            const string query = @"
                SELECT 
                    COUNT(*) as TotalProductos,
                    SUM(i.CantidadActual) as TotalStock,
                    SUM(i.CantidadActual * p.PrecioVenta) as ValorTotalInventario,
                    AVG(p.PrecioVenta) as PrecioPromedio,
                    COUNT(CASE WHEN i.CantidadActual <= i.StockMinimo THEN 1 END) as ProductosBajoStock
                FROM Inventario i
                INNER JOIN Productos p ON i.ProductoId = p.Id";

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(query, connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        stats.Add("TotalProductos", reader.GetInt32(0));
                        stats.Add("TotalStock", reader.GetInt32(1));
                        stats.Add("ValorTotalInventario", reader.GetDecimal(2));
                        stats.Add("PrecioPromedio", reader.GetDecimal(3));
                        stats.Add("ProductosBajoStock", reader.GetInt32(4));
                    }
                }
            }
            return stats;
        }
    }
}