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
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MostrarError("El nombre del producto es requerido");
                return;
            }

            btnGuardar.Content = "Guardando...";
            btnGuardar.IsEnabled = false;

            try
            {
                var nuevoProducto = new NuevoProductoInventario
                {
                    Nombre = txtNombre.Text.Trim(),
                    Plataforma = string.IsNullOrWhiteSpace(txtPlataforma.Text) ? "N/A" : txtPlataforma.Text.Trim(),
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
            txtMensajeError.Visibility = Visibility.Visible;
        }
    }
}
