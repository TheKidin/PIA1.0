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
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace PIA1._0
{
    public sealed partial class EditarProductoWindow : Window
    {
        private readonly DatabaseService _databaseService;
        private readonly InventarioProducto _producto;

        public bool ProductoEditado { get; private set; } = false;

        public EditarProductoWindow(DatabaseService databaseService, InventarioProducto producto)
        {
            this.InitializeComponent();
            _databaseService = databaseService;
            _producto = producto;

            CargarDatosProducto();
        }

        private void CargarDatosProducto()
        {
            // Cargar los datos del producto en los controles
            txtNombre.Text = _producto.NombreProducto;
            txtPlataforma.Text = _producto.Plataforma;
            numPrecio.Value = (double)_producto.PrecioVenta;
            numStockActual.Value = _producto.CantidadActual;
            numStockMinimo.Value = _producto.StockMinimo;
            numStockMaximo.Value = _producto.StockMaximo;
            numPuntoReorden.Value = _producto.PuntoReorden;
        }

        private async void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validar campos
                if (string.IsNullOrWhiteSpace(txtNombre.Text))
                {
                    MostrarError("El nombre del producto es requerido.");
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtPlataforma.Text))
                {
                    MostrarError("La plataforma es requerida.");
                    return;
                }

                if (numPrecio.Value <= 0)
                {
                    MostrarError("El precio debe ser mayor a 0.");
                    return;
                }

                // Actualizar el producto
                _producto.NombreProducto = txtNombre.Text.Trim();
                _producto.Plataforma = txtPlataforma.Text.Trim();
                _producto.PrecioVenta = (decimal)numPrecio.Value;
                _producto.CantidadActual = (int)numStockActual.Value;
                _producto.StockMinimo = (int)numStockMinimo.Value;
                _producto.StockMaximo = (int)numStockMaximo.Value;
                _producto.PuntoReorden = (int)numPuntoReorden.Value;

                // Actualizar en la base de datos
                var exito = await _databaseService.ActualizarProductoAsync(_producto);

                if (exito)
                {
                    ProductoEditado = true;
                    this.Close();
                }
                else
                {
                    MostrarError("Error al actualizar el producto en la base de datos.");
                }
            }
            catch (Exception ex)
            {
                MostrarError($"Error al guardar cambios: {ex.Message}");
            }
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MostrarError(string mensaje)
        {
            txtMensajeError.Text = mensaje;
            borderMensajeError.Visibility = Visibility.Visible;
        }
    }
}
