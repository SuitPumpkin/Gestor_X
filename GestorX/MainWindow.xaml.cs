using GestorX.Pestañas;
using HandyControl.Controls;
using HandyControl.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using static GestorX.Pestañas.Clases;

namespace GestorX
{
    public partial class MainWindow : System.Windows.Window
    {
        public ObservableCollection<ItemAgenda> Agenda { get; private set; }
        public ObservableCollection<ItemInventario> Inventario { get; private set; }
        public ObservableCollection<ItemProyecto> Proyectos { get; private set; }
        public MainWindow()
        {
            VigilarCambiosEnDataBase();
            ActualizarDatosDeDataBase();
            InitializeComponent();
            Pestaña.Content = new Inicio();
        }
        private void VigilarCambiosEnDataBase()
        {
            FileSystemWatcher watcher = new FileSystemWatcher
            {
                Path = System.IO.Path.GetDirectoryName(BaseDeDatos.UbicaciónDB),
                Filter = System.IO.Path.GetFileName(BaseDeDatos.UbicaciónDB),
                NotifyFilter = NotifyFilters.LastWrite
            };
            watcher.Changed += (sender, e) => { ActualizarDatosDeDataBase(); BaseDeDatos.NotificarActualizacion(); Debug.WriteLine("La base de datos ha sido actualizada jeje"); };
            watcher.EnableRaisingEvents = true;
        }
        public void ActualizarDatosDeDataBase()
        {
            Agenda = ItemAgenda.Read();
            Inventario = ItemInventario.Read();
            Proyectos = ItemProyecto.Read();
        }
        private void CambioDePestaña(object sender, MouseButtonEventArgs e)
        {
            Border boton = sender as Border;
            string Destino = boton.Name;
            switch (Destino)
            {
                case "A":
                    Pestaña.Content = new Inicio();
                    break;
                case "B":
                    Pestaña.Content = new Inventario();
                    break;
                case "C":
                    Pestaña.Content = new Proyectos();
                    break;
                case "D":
                    Pestaña.Content = new Agenda();
                    break;
                case "E":
                    Pestaña.Content = null; //catalogo
                    break;
                case "F":
                    Pestaña.Content = new Herramientas(); //Herramientas
                    break;
                case "Z":
                    Pestaña.Content = new Ajustes(); //Herramientas
                    break;
                default:
                    Pestaña.Content = null; //error
                    break;
            }
            AjustarItemsPorPagina();
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
        private void CloseButton_Click(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            AjustarItemsPorPagina();
        }
        public void AjustarItemsPorPagina()
        {
            double Altura = this.ActualHeight;
            int itemsBase = 6;
            double alturaBase = 625;
            double alturaPorItem = 60;
            int itemsExtra = 0;
            if (Altura > alturaBase)
            {
                itemsExtra = (int)Math.Floor((Altura - alturaBase) / alturaPorItem);
            }
            int totalItems = itemsBase + itemsExtra;
            if (Pestaña.Content is Agenda a)
            {
                a.Paginador.DataCountPerPage = totalItems;
                a.UpdatePagination();
            }
            else if (Pestaña.Content is Inventario b)
            {
                b.Paginador.DataCountPerPage = totalItems;
                b.UpdatePagination();
            }
            else if (Pestaña.Content is Proyectos c)
            {
                c.Paginador.DataCountPerPage = totalItems;
                c.UpdatePagination();
            }
        }
        private async void CambioDeEstadoDeVentana(object sender, EventArgs e)
        {
            await Task.Delay(200);
            AjustarItemsPorPagina();
        }
    }
}
