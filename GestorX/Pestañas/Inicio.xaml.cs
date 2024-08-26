using HandyControl.Tools.Extension;
using LiveCharts;
using LiveCharts.Helpers;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using static GestorX.Pestañas.Clases;

namespace GestorX.Pestañas
{
    public partial class Inicio : UserControl
    {
        public Inicio()
        {
            InitializeComponent();
        }
        public void CargarInfo()
        {
            MainWindow ventana = Window.GetWindow(this) as MainWindow;
            Principal pri = ventana.Contenido.Content as Principal;
            int p = 0, c = 0, t = 0;
            foreach (ItemAgenda item in pri.Agenda)
            {
                if (item.TipoContacto == "Proveedor")
                {
                    p++;
                }
                if (item.TipoContacto == "Trabajador")
                {
                    t++;
                }
                if (item.TipoContacto == "Cliente")
                {
                    c++;
                }
            }
            P2.Text = $"x{p}";
            P1.Text = $"x{p}";
            C1.Text = $"x{c}";
            C2.Text = $"x{c}";
            T1.Text = $"x{t}";
            T2.Text = $"x{t}";
            I1.Text = $"x{pri.Inventario.Count}";
            I2.Text = $"x{pri.Inventario.Count}";
            S1.Text = $"x{pri.Proyectos.Count}";
            S2.Text = $"x{pri.Proyectos.Count}";
            var proyectos = 0;
            foreach (ItemProyecto item in pri.Proyectos)
            {
                if (item.Progreso < 100)
                {
                    proyectos++;
                }
            }
            ProyectosIncompletos.Text = $"{proyectos}";
            var vi = 0.0;
            foreach (ItemInventario item in pri.Inventario)
            {
                var valor = item.PrecioCompra * item.Cantidad;
                vi += valor;
            }
            ValorInventario.Text = $"${vi}";
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            CargarInfo();
        }
    }
}
