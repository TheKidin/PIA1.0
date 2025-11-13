using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.AnimatedVisuals;
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
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace PIA1._0
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoginWindow : Window
    {
        private readonly DatabaseService _databaseService;
        public LoginWindow()
        {
            this.InitializeComponent();
            _databaseService = new DatabaseService();

            txtUsuario.Focus(FocusState.Programmatic);
        }

        private async void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            await ValidarLoginAsync();
        }

        private async void txtPassword_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                await ValidarLoginAsync();
            }
        }

        private async Task ValidarLoginAsync()
        {
            var usuario = txtUsuario.Text.Trim();
            var password = txtPassword.Password.Trim();
            if (string.IsNullOrEmpty(usuario) || string.IsNullOrEmpty(password))
            {
                MostrarError("Por favor, ingrese usuario y contraseña.");
                return;
            }

            btnLogin.Content = "Validando...";
            btnLogin.IsEnabled = false;

            try
            {
                var esValido = await _databaseService.ValidarUsuarioAsync(usuario, password);
                if (esValido)
                {
                    OcultarError();
                    var mainWindow = new MainWindow();
                    mainWindow.Activate();
                    this.Close();
                }
                else
                {
                    MostrarError("Usuario o contraseña incorrectos.");
                    txtPassword.SelectAll();
                    txtPassword.Focus(FocusState.Programmatic);
                }
            }
            catch (System.Exception ex)
            {
                MostrarError($"Error al validar el usuario: {ex.Message}");
            }
            finally
            {
                btnLogin.Content = "Iniciar Sesión";
                btnLogin.IsEnabled = true;
            }
        }

        private void MostrarError(string mensaje)
        {
            txtMensajeError.Text = mensaje;
            // --- CORRECCIÓN AQUÍ ---
            borderMensajeError.Visibility = Visibility.Visible;
        }
        private void OcultarError()
        {
            // --- CORRECCIÓN AQUÍ ---
            borderMensajeError.Visibility = Visibility.Collapsed;
        }
    }
}