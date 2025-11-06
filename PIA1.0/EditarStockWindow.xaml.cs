using Microsoft.UI.Xaml;
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
    public sealed partial class EditarStockWindow : Window
    {
        private readonly DatabaseService _databaseService;
        private readonly InventarioProducto _producto;
        public bool StockActualizado { get; private set; } = false;

        public EditarStockWindow(InventarioProducto producto, DatabaseService databaseService)
        {
            this.InitializeComponent();
            _databaseService = databaseService;
            _producto = producto;

            CargarDatosProducto();
        }

        private void CargarDatosProducto()
        {
            txtNombreProducto.Text = _producto.NombreProducto;
            txtPlataforma.Text = _producto.Plataforma;
            numNuevaCantidad.Value = _producto.CantidadActual;
            txtStockMinimo.Text = _producto.StockMinimo.ToString();
            txtStockMaximo.Text = _producto.StockMaximo.ToString();
        }

        private async void BtnActualizar_Click(object sender, RoutedEventArgs e)
        {
            await ActualizarStockAsync();
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async Task ActualizarStockAsync()
        {
            btnActualizar.Content = "Actualizando...";
            btnActualizar.IsEnabled = false;

            try
            {
                var nuevaCantidad = (int)numNuevaCantidad.Value;
                var exito = await _databaseService.UpdateStockAsync(_producto.InventarioId, nuevaCantidad);

                if (exito)
                {
                    StockActualizado = true;
                    this.Close();
                }
                else
                {
                    MostrarError("Error al actualizar el stock en la base de datos");
                }
            }
            catch (Exception ex)
            {
                MostrarError($"Error: {ex.Message}");
            }
            finally
            {
                btnActualizar.Content = "Actualizar Stock";
                btnActualizar.IsEnabled = true;
            }
        }

        private void MostrarError(string mensaje)
        {
            txtMensajeError.Text = mensaje;
            txtMensajeError.Visibility = Visibility.Visible;
        }
    }
}
