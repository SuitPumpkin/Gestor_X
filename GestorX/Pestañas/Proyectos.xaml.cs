using FontAwesome.Sharp;
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

namespace GestorX.Pestañas
{
    public partial class Proyectos : UserControl
    {
        /// <summary>
        /// Referencia a la ventana principal
        /// </summary>
        private Principal _principal;
        /// <summary>
        /// Colección de items de proyecto
        /// </summary>
        public ObservableCollection<ItemProyecto> Items { get; set; }
        /// <summary>
        /// Constructor de la clase
        /// </summary>
        public Proyectos()
        {
            BaseDeDatos.BaseDeDatosActualizada += ActualizarItems;
            InitializeComponent();
        }
        private void ActualizarItems()
        {
            try
            {
                Items = ItemProyecto.Read();
            }
            catch
            {
                Debug.WriteLine("Error al leer la base de datos");
            }
            Application.Current.Dispatcher.Invoke(() =>
            {
                Lista.Items.Refresh();
            });
        }
        private void CargarItems()
        {
            MainWindow ventana = Window.GetWindow(this) as MainWindow;
            _principal = ventana.Contenido.Content as Principal;
            Items = _principal.Proyectos;
            Lista.ItemsSource = Items.OrderBy(x => x.Nombre);
        }
        private void Agregar(object sender, MouseButtonEventArgs e)
        {
            _principal.ItemDeProyectos = new ItemProyecto();
            _principal.Subpestaña.Content = new ProyectosAdd();
        }
        private void Editar(object sender, MouseButtonEventArgs e)
        {
            var seleccionado = Lista.SelectedItem;
            if (seleccionado != null)
            {
                _principal.ItemDeProyectos = ((ItemProyecto)seleccionado);
                _principal.Subpestaña.Content = new ProyectosAdd();
            }
        }
        private void Ordenar(object sender, MouseButtonEventArgs e)
        {
            var items = new ObservableCollection<ItemProyecto>();
            MainWindow ventana = MainWindow.GetWindow(this) as MainWindow;
            switch (IconoOrdenar.Icon)
            {
                case IconChar.ArrowDownAZ:
                    //cambiar orden lista
                    items = new ObservableCollection<ItemProyecto>(Items.Where(x => x.Nombre.ToLower().Contains(BarraDeBusqueda.Text.ToLower())).OrderByDescending(x => x.Nombre));
                    Lista.ItemsSource = items;
                    IconoOrdenar.Icon = IconChar.ArrowDownZA;
                    ventana.Notificación(new GrowlInfo
                    {
                        Message = "Nombre: Z - A",
                        ShowDateTime = false,
                        WaitTime = 3,
                        Token = "Noti"
                    }, "Info");
                    break;
                case IconChar.ArrowDownZA:
                    //cambiar orden lista
                    items = new ObservableCollection<ItemProyecto>(Items.Where(x => x.Nombre.ToLower().Contains(BarraDeBusqueda.Text.ToLower())).OrderBy(x => x.Nombre));
                    Lista.ItemsSource = items;
                    IconoOrdenar.Icon = IconChar.ArrowDownAZ;
                    ventana.Notificación(new GrowlInfo
                    {
                        Message = "Nombre: A - Z",
                        ShowDateTime = false,
                        WaitTime = 3,
                        Token = "Noti"
                    }, "Info");
                    break;
                default:
                    break;
            }
        }
        private void BusquedaUpdate(object sender, TextChangedEventArgs e)
        {
            var items = new ObservableCollection<ItemProyecto>(Items.Where(x => x.Nombre.ToLower().Contains(BarraDeBusqueda.Text.ToLower())));
            Lista.ItemsSource = items;
        }
        private void AbrirCarpeta(object sender, MouseButtonEventArgs e)
        {
            Border enviado = sender as Border;
            string Carpeta = enviado.Tag.ToString();
            if (Uri.TryCreate(Carpeta, UriKind.Absolute, out Uri uri) || uri != null || Carpeta.ToLower() != "n/a")
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = $"{Carpeta}",
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}");
                }
            }
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            CargarItems();
        }
    }
}
