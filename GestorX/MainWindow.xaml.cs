using GestorX.Pestañas;
using HandyControl.Controls;
using HandyControl.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GestorX
{
    public partial class MainWindow : System.Windows.Window
    {
        private bool _CambiandoTamaño = false;
        public MainWindow()
        {
            InitializeComponent();
            Cargar();
        }
        private void Cargar()
        {
            Contenido.Content = new Principal();
        }
        public void Notificación(GrowlInfo Info, string tipo)
        {
            switch (tipo)
            {
                case "Success":
                    Growl.Success(Info);
                    break;
                case "Info":
                    Growl.Info(Info);
                    break;
                case "Error":
                    Growl.Error(Info);
                    break;
                case "Pregunta":
                    Growl.Ask(Info);
                    break;
                default:
                    break;
            }
        }
        private void MoverVentana(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
        private void CloseButton_Click(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }
        private void CambioDeTamaño(object sender, SizeChangedEventArgs e)
        {
            if (_CambiandoTamaño)
            {
                return;
            }
            if (e.WidthChanged)
            {
                _CambiandoTamaño = true;
                this.Height = this.ActualWidth / 1.36;
                _CambiandoTamaño = false;
            }
            else if (e.HeightChanged)
            {
                _CambiandoTamaño = true;
                this.Width = this.ActualHeight * 1.36;
                _CambiandoTamaño = false;
            }
        }
    }
}
