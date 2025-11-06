using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
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
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ConfirmacionWindow : Window
    {
        public bool Confirmado { get; private set; } = false;

        public ConfirmacionWindow(string titulo, string mensaje, string textoConfirmar = "Confirmar", string textoCancelar = "Cancelar")
        {
            this.InitializeComponent();

            // Configurar la ventana
            this.Title = titulo;
            txtMensaje.Text = mensaje;
            btnConfirmar.Content = textoConfirmar;
            btnCancelar.Content = textoCancelar;
        }

        private void BtnConfirmar_Click(object sender, RoutedEventArgs e)
        {
            Confirmado = true;
            this.Close();
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            Confirmado = false;
            this.Close();
        }
    }
}
