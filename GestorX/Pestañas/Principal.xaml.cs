using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    public partial class Principal : UserControl
    {
        public ObservableCollection<ItemAgenda> Agenda { get; private set; }
        public ObservableCollection<ItemInventario> Inventario { get; private set; }
        public ObservableCollection<ItemProyecto> Proyectos { get; private set; }

        public ItemInventario ItemDelInventario { get; set; } = new ItemInventario();
        public ItemAgenda ItemDeLaAgenda { get; set; } = new ItemAgenda();
        public ItemProyecto ItemDeProyectos { get; set; } = new ItemProyecto();
        public Principal()
        {
            VigilarCambios();
            ActualizarDatos();
            InitializeComponent();
            Subpestaña.Content = new Inicio();
        }
        private void VigilarCambios()
        {
            FileSystemWatcher watcher = new FileSystemWatcher
            {
                Path = System.IO.Path.GetDirectoryName(BaseDeDatos.UbicaciónDB),
                Filter = System.IO.Path.GetFileName(BaseDeDatos.UbicaciónDB),
                NotifyFilter = NotifyFilters.LastWrite
            };
            watcher.Changed += (sender, e) => {ActualizarDatos(); BaseDeDatos.NotificarActualizacion(); Debug.WriteLine("La base de datos ha sido actualizada jeje"); };
            watcher.EnableRaisingEvents = true;
        }
        public void ActualizarDatos()
        {
            Agenda = ItemAgenda.Read();
            Inventario = ItemInventario.Read();
            Proyectos = ItemProyecto.Read();
        }
        private void CambioPestaña(object sender, MouseButtonEventArgs e)
        {
            Border boton = sender as Border;
            string Pestaña = boton.Name;
            switch (Pestaña)
            {
                case "a":
                    Subpestaña.Content = new Inicio();
                    break;
                case "b":
                    Subpestaña.Content = new Inventario();
                    break;
                case "c":
                    Subpestaña.Content = new Proyectos();
                    break;
                case "d":
                    Subpestaña.Content = new Agenda();
                    break;
                case "e":
                    Subpestaña.Content = null; //catalogo
                    break;
                default:
                    Subpestaña.Content = null; //error
                    break;
            }
        }
    }
}
