using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using PIA1._0;
using PIA1._0.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;

namespace PIA1._0
{
    public class CurrencyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is decimal decimalValue)
            {
                return $"${decimalValue:F2}";
            }
            if (value is double doubleValue)
            {
                return $"${doubleValue:F2}";
            }
            if (value is int intValue)
            {
                return $"${intValue:F2}";
            }
            return "$0.00";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public sealed partial class MainWindow : Window
    {
        private readonly DatabaseService _databaseService;
        private List<InventarioProducto> _inventarioCompleto;

        public MainWindow()
        {
            this.InitializeComponent();
            _databaseService = new DatabaseService();

            _ = CargarDatosAsync();
        }

        public MainWindow(string usuario) : this()
        {
            if (!string.IsNullOrEmpty(usuario))
            {
                txtUsuarioActual.Text = usuario;
            }
        }

        private async Task CargarDatosAsync()
        {
            try
            {
                var dbInventario = await _databaseService.GetInventarioAsync();

                _inventarioCompleto = dbInventario.Select(item => new PIA1._0.Models.InventarioProducto
                {
                    InventarioId = item.InventarioId,
                    ProductoId = item.ProductoId,
                    NombreProducto = item.NombreProducto,
                    Plataforma = item.Plataforma,
                    PrecioVenta = item.PrecioVenta,
                    CantidadActual = item.CantidadActual,
                    StockMinimo = item.StockMinimo,
                    StockMaximo = item.StockMaximo,
                    PuntoReorden = item.PuntoReorden
                }).ToList();

                lvInventario.ItemsSource = _inventarioCompleto;

                // Cargar comboboxes
                CargarFiltros();
                await ActualizarEstadisticasAsync();
            }
            catch (Exception ex)
            {
                MostrarMensaje($"Error al cargar datos: {ex.Message}");
            }
        }

        private void CargarFiltros()
        {
            // Cargar plataformas únicas
            var plataformas = _inventarioCompleto
                .Select(p => p.Plataforma)
                .Distinct()
                .OrderBy(p => p)
                .ToList();

            cmbPlataforma.Items.Clear();
            cmbPlataforma.Items.Add(new ComboBoxItem { Content = "🎮 Todas las plataformas", Tag = "todas" });

            foreach (var plataforma in plataformas)
            {
                cmbPlataforma.Items.Add(new ComboBoxItem { Content = plataforma, Tag = plataforma });
            }
            cmbPlataforma.SelectedIndex = 0;
        }

        private async void BtnBuscar_Click(object sender, RoutedEventArgs e)
        {
            await RealizarBusquedaAvanzada();
        }

        private async Task RealizarBusquedaAvanzada()
        {
            try
            {
                var plataforma = (cmbPlataforma.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "todas";
                var filtroExtra = (cmbFiltroExtra.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "Todos";

                List<InventarioProducto> resultados = _inventarioCompleto;

                // Aplicar filtro de plataforma
                if (plataforma != "todas")
                {
                    resultados = resultados.Where(p => p.Plataforma == plataforma).ToList();
                }

                // Aplicar filtros extra
                resultados = AplicarFiltrosExtra(resultados, filtroExtra);

                var resultadosConvertidos = resultados.Select(item => new PIA1._0.Models.InventarioProducto
                {
                    InventarioId = item.InventarioId,
                    ProductoId = item.ProductoId,
                    NombreProducto = item.NombreProducto,
                    Plataforma = item.Plataforma,
                    PrecioVenta = item.PrecioVenta,
                    CantidadActual = item.CantidadActual,
                    StockMinimo = item.StockMinimo,
                    StockMaximo = item.StockMaximo,
                    PuntoReorden = item.PuntoReorden
                }).ToList();

                lvInventario.ItemsSource = resultadosConvertidos;

                MostrarResultadoBusqueda(resultadosConvertidos.Count, plataforma, filtroExtra);
            }
            catch (Exception ex)
            {
                MostrarMensaje($"Error en la búsqueda: {ex.Message}");
            }
        }

        private List<InventarioProducto> AplicarFiltrosExtra(List<InventarioProducto> productos, string filtro)
        {
            return filtro switch
            {
                "bajo_stock" => productos.Where(p => p.CantidadActual <= p.StockMinimo).ToList(),
                "sin_stock" => productos.Where(p => p.CantidadActual == 0).ToList(),
                "stock_alto" => productos.Where(p => p.CantidadActual >= p.StockMaximo * 0.8).ToList(),
                "precio_alto" => productos.Where(p => p.PrecioVenta > 500).ToList(),
                "precio_bajo" => productos.Where(p => p.PrecioVenta < 100).ToList(),
                _ => productos
            };
        }

        private void MostrarResultadoBusqueda(int cantidad, string plataforma, string filtro)
        {
            string mensaje = $"📊 Se encontraron {cantidad} productos";

            if (plataforma != "todas")
            {
                mensaje += $" en {plataforma}";
            }

            if (filtro != "Todos")
            {
                var nombreFiltro = ObtenerNombreFiltro(filtro);
                mensaje += $" con filtro {nombreFiltro}";
            }

            MostrarMensajeTemporal(mensaje);
        }

        private string ObtenerNombreFiltro(string filtroTag)
        {
            return filtroTag switch
            {
                "bajo_stock" => "⚠️ Bajo Stock",
                "sin_stock" => "❌ Sin Stock",
                "stock_alto" => "📈 Stock Alto",
                "precio_alto" => "💰 Precio Alto",
                "precio_bajo" => "💸 Precio Bajo",
                _ => "Todos"
            };
        }

        private async void CmbPlataforma_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await RealizarBusquedaAvanzada();
        }

        private async void CmbFiltroExtra_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await RealizarBusquedaAvanzada();
        }

        // Limpiar búsqueda
        private void BtnLimpiar_Click(object sender, RoutedEventArgs e)
        {
            cmbPlataforma.SelectedIndex = 0;
            cmbFiltroExtra.SelectedIndex = 0;
            lvInventario.ItemsSource = _inventarioCompleto;
            MostrarMensajeTemporal("🔍 Filtros limpiados - Mostrando todos los productos");
        }

        private async void BtnNuevoProducto_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var nuevoProductoWindow = new NuevoProductoWindow(_databaseService);
                nuevoProductoWindow.Activate();

                var tcs = new TaskCompletionSource<bool>();
                nuevoProductoWindow.Closed += (s, args) => tcs.SetResult(nuevoProductoWindow.ProductoGuardado);

                var productoGuardado = await tcs.Task;

                if (productoGuardado)
                {
                    MostrarMensaje("✅ Producto agregado correctamente");
                    await CargarDatosAsync();
                }
            }
            catch (Exception ex)
            {
                MostrarMensaje($"Error al agregar producto: {ex.Message}");
            }
        }

        private async void BtnEditar_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int productoId)
            {
                try
                {
                    var producto = _inventarioCompleto.FirstOrDefault(x => x.ProductoId == productoId);
                    if (producto != null)
                    {
                        var editarWindow = new EditarProductoWindow(_databaseService, producto);
                        editarWindow.Activate();

                        var tcs = new TaskCompletionSource<bool>();
                        editarWindow.Closed += (s, args) => tcs.SetResult(editarWindow.ProductoEditado);

                        var productoEditado = await tcs.Task;

                        if (productoEditado)
                        {
                            MostrarMensaje("✅ Producto editado correctamente");
                            await CargarDatosAsync();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MostrarMensaje($"Error al editar producto: {ex.Message}");
                }
            }
        }

        private async void BtnEliminar_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int productoId)
            {
                try
                {
                    var producto = _inventarioCompleto.FirstOrDefault(x => x.ProductoId == productoId);
                    if (producto != null)
                    {
                        var confirmarWindow = new ConfirmacionWindow(
                            "Confirmación Eliminación",
                            $"¿Estás seguro de que quieres eliminar '{producto.NombreProducto}'?\n\nEsta acción no se puede deshacer.",
                            "Eliminar",
                            "Cancelar");

                        confirmarWindow.Activate();

                        var tcs = new TaskCompletionSource<bool>();
                        confirmarWindow.Closed += (s, args) => tcs.SetResult(confirmarWindow.Confirmado);

                        var confirmado = await tcs.Task;

                        if (confirmado)
                        {
                            var exito = await _databaseService.DeleteProductoCompletoAsync(productoId);
                            if (exito)
                            {
                                MostrarMensaje("✅ Producto eliminado correctamente");
                                await CargarDatosAsync();
                            }
                            else
                            {
                                MostrarMensaje("❌ Error al eliminar el producto.");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MostrarMensaje($"Error al eliminar producto: {ex.Message}");
                }
            }
        }

        private async void BtnActualizarStats_Click(object sender, RoutedEventArgs e)
        {
            await ActualizarEstadisticasAsync();
        }

        private async Task ActualizarEstadisticasAsync()
        {
            try
            {
                var stats = await _databaseService.GetEstadisticasAsync();

                var totalProductos = stats["TotalProductos"] == DBNull.Value
                    ? 0
                    : Convert.ToInt32(stats["TotalProductos"]);

                var totalStock = stats["TotalStock"] == DBNull.Value
                    ? 0
                    : Convert.ToInt32(stats["TotalStock"]);

                var valorInventario = stats["ValorTotalInventario"] == DBNull.Value
                    ? 0m
                    : Convert.ToDecimal(stats["ValorTotalInventario"]);

                var precioPromedio = stats["PrecioPromedio"] == DBNull.Value
                    ? 0m
                    : Convert.ToDecimal(stats["PrecioPromedio"]);

                var bajoStock = stats["ProductosBajoStock"] == DBNull.Value
                    ? 0
                    : Convert.ToInt32(stats["ProductosBajoStock"]);

                txtTotalProductos.Text = totalProductos.ToString();
                txtTotalStock.Text = totalStock.ToString();
                txtValorInventario.Text = $"${valorInventario:F2}";
                txtPrecioPromedio.Text = $"${precioPromedio:F2}";
                txtBajoStock.Text = bajoStock.ToString();
            }
            catch (Exception ex)
            {
                MostrarMensaje($"Error al cargar estadísticas: {ex.Message}");
            }
        }

        private void BtnCerrarSesion_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new LoginWindow();
            loginWindow.Activate();
            this.Close();
        }

        private void MostrarMensaje(string mensaje)
        {
            MostrarMensajeTemporal(mensaje, 5000);
        }

        private void MostrarMensajeTemporal(string mensaje, int duracionMs = 3000)
        {
            try
            {
                var teachingTip = new TeachingTip
                {
                    Title = "GameStore",
                    Subtitle = mensaje,
                    IsOpen = true,
                    PreferredPlacement = TeachingTipPlacementMode.Bottom,
                    CloseButtonContent = "OK",
                };

                if (this.Content is Grid grid)
                {
                    grid.Children.Add(teachingTip);

                    teachingTip.Closed += (s, e) =>
                    {
                        grid.Children.Remove(teachingTip);
                    };

                    if (duracionMs > 0)
                    {
                        _ = CerrarTeachingTipDespuesDe(teachingTip, duracionMs);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error mostrando TeachingTip: {ex.Message}");
            }
        }

        private async Task CerrarTeachingTipDespuesDe(TeachingTip teachingTip, int milisegundos)
        {
            await Task.Delay(milisegundos);
            if (teachingTip.IsOpen)
            {
                teachingTip.IsOpen = false;
            }
        }
    }
}