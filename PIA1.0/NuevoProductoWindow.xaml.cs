using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using PIA1._0.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace PIA1._0
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class NuevoProductoWindow : Window
    {
        private readonly DatabaseService _databaseService;
        public bool ProductoGuardado { get; private set; } = false;

        public NuevoProductoWindow(DatabaseService databaseService)
        {
            this.InitializeComponent();
            _databaseService = databaseService;

            // Poner foco en el primer campo
            txtNombre.Focus(FocusState.Programmatic);

            // Cargamos las plataformas existentes
            _ = CargarPlataformasAsync();
        }

        private async Task CargarPlataformasAsync()
        {
            try
            {
                var plataformas = await _databaseService.GetPlataformasAsync();
                cmbPlataforma.ItemsSource = plataformas;
            }
            catch (Exception ex)
            {
                // Opcional: Mostrar error si no se pueden cargar las plataformas
                System.Diagnostics.Debug.WriteLine($"Error al cargar plataformas: {ex.Message}");
            }
        }

        private async void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            await GuardarProductoAsync();
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async Task GuardarProductoAsync()
        {
            // --- INICIO DE LA VALIDACIÓN MEJORADA ---

            // 1. Ocultamos el mensaje de error anterior
            borderMensajeError.Visibility = Visibility.Collapsed;

            // 2. Creamos una lista para guardar todos los errores
            var errores = new List<string>();

            // 3. Validamos cada campo y agregamos a la lista si hay error
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                errores.Add("El nombre del producto es requerido.");
            }

            if (string.IsNullOrWhiteSpace(cmbPlataforma.Text))
            {
                errores.Add("La plataforma es requerida.");
            }

            if (numPrecio.Value <= 0)
            {
                errores.Add("El precio de venta debe ser mayor a 0.");
            }

            if (numStockMaximo.Value <= numStockMinimo.Value)
            {
                errores.Add("El stock máximo debe ser mayor que el stock mínimo.");
            }

            // 4. Si la lista tiene CUALQUIER error, los mostramos todos y detenemos
            if (errores.Count > 0)
            {
                // Unimos todos los errores en un solo mensaje, separados por saltos de línea
                MostrarError(string.Join(Environment.NewLine, errores));
                return;
            }

            // --- FIN DE LA VALIDACIÓN MEJORADA ---


            // Si llegamos aquí, no hay errores y podemos continuar.
            btnGuardar.Content = "Guardando...";
            btnGuardar.IsEnabled = false;

            try
            {
                var nuevoProducto = new NuevoProductoInventario
                {
                    Nombre = txtNombre.Text.Trim(),
                    Plataforma = cmbPlataforma.Text.Trim(),
                    PrecioVenta = (decimal)numPrecio.Value,
                    CantidadActual = (int)numStockActual.Value,
                    StockMinimo = (int)numStockMinimo.Value,
                    StockMaximo = (int)numStockMaximo.Value,
                    PuntoReorden = (int)numPuntoReorden.Value
                };

                var exito = await _databaseService.CreateProductoConInventarioAsync(nuevoProducto);

                if (exito)
                {
                    ProductoGuardado = true;
                    this.Close();
                }
                else
                {
                    MostrarError("Error al guardar el producto en la base de datos");
                }
            }
            catch (Exception ex)
            {
                MostrarError($"Error: {ex.Message}");
            }
            finally
            {
                btnGuardar.Content = "Guardar Producto";
                btnGuardar.IsEnabled = true;
            }
        }

        private void MostrarError(string mensaje)
        {
            txtMensajeError.Text = mensaje;
            borderMensajeError.Visibility = Visibility.Visible;
        }
    }
}