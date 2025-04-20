using FontAwesome.Sharp;
using HandyControl.Controls;
using HandyControl.Data;
using HandyControl.Tools.Extension;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
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
using MessageBox = System.Windows.MessageBox;
using Window = System.Windows.Window;
using static GestorX.Pestañas.Clases;

namespace GestorX.Pestañas
{
    public partial class Inventario : UserControl
    {
        /// <summary>
        /// Referencia a la ventana principal
        /// </summary>
        private Principal _principal;
        /// <summary>
        /// Almacenta la información de los items actuales del inventario
        /// </summary>
        public ObservableCollection<ItemInventario> Items { get; set; }
        /// <summary>
        /// Constructor del componente
        /// </summary>
        public Inventario()
        {
            BaseDeDatos.BaseDeDatosActualizada += ActualizarItems;
            InitializeComponent();
        }
        private void ActualizarItems()
        {
            try
            {
                Items = ItemInventario.Read();
            }
            catch
            {
                Debug.WriteLine("Error al leer la base de datos");
            }
            Application.Current.Dispatcher.Invoke(() =>
            {
                InventarioList.Items.Refresh();
            });
        }
        private void CargarItems()
        {
            MainWindow ventana = Window.GetWindow(this) as MainWindow;
            _principal = ventana.Contenido.Content as Principal;
            Items = _principal.Inventario;
            InventarioList.ItemsSource = Items.OrderBy(x => x.Nombre);
            
            float totalValor = 0;
            try
            {
                BaseDeDatos.ComandoDeLectura("SELECT COALESCE(SUM(Precio * Cantidad), 0) AS Total FROM Inventario", reader => {
                    if (!reader.IsDBNull(reader.GetOrdinal("Total")))
                    {
                        totalValor = float.Parse($"{reader["Total"]}");
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al calcular el valor total: {ex.Message}");
            }
            ValorInventario.Text = totalValor.ToString();
        }
        private void Agregar(object sender, MouseButtonEventArgs e)
        {
            _principal.ItemDelInventario = new ItemInventario();
            _principal.Subpestaña.Content = new InventarioAdd();
        }
        private void Editar(object sender, MouseButtonEventArgs e)
        {
            var seleccionado = InventarioList.SelectedItem;
            if (seleccionado != null)
            {
                _principal.ItemDelInventario = ((ItemInventario)seleccionado);
                _principal.Subpestaña.Content = new InventarioAdd();
            }
            else
            {
                MessageBox.Show("Primero selecciona un item");
            }
        }
        private void Ordenar(object sender, MouseButtonEventArgs e)
        {
            var items = new ObservableCollection<ItemInventario>();
            MainWindow ventana = MainWindow.GetWindow(this) as MainWindow;
            switch (IconoOrdenar.Icon)
            {
                case IconChar.ArrowDownAZ:
                    //cambiar orden lista
                    items = new ObservableCollection<ItemInventario>(Items.Where(x => x.Nombre.ToLower().Contains(BarraDeBusqueda.Text.ToLower())).OrderByDescending(x => x.Nombre));
                    InventarioList.ItemsSource = items;
                    IconoOrdenar.Icon = IconChar.ArrowDownZA;
                    ventana.Notificación(new GrowlInfo
                    {
                        Message = "Nombre: Z - A",
                        ShowDateTime = false,
                        WaitTime = 3,
                        Token = "Noti"
                    },"Info");
                    break;
                case IconChar.ArrowDownZA:
                    //cambiar orden lista
                    items = new ObservableCollection<ItemInventario>(Items.Where(x => x.Nombre.ToLower().Contains(BarraDeBusqueda.Text.ToLower())).OrderBy(x => x.Cantidad));
                    InventarioList.ItemsSource = items;
                    IconoOrdenar.Icon = IconChar.ArrowDown19;
                    ventana.Notificación(new GrowlInfo
                    {
                        Message = "Cantidad: 1 - 9",
                        ShowDateTime = false,
                        WaitTime = 3,
                        Token = "Noti"
                    }, "Info");
                    break;
                case IconChar.ArrowDown19:
                    //cambiar orden lista
                    items = new ObservableCollection<ItemInventario>(Items.Where(x => x.Nombre.ToLower().Contains(BarraDeBusqueda.Text.ToLower())).OrderByDescending(x => x.Cantidad));
                    InventarioList.ItemsSource = items;
                    IconoOrdenar.Icon = IconChar.ArrowDown91;
                    ventana.Notificación(new GrowlInfo
                    {
                        Message = "Cantidad: 9 - 1",
                        ShowDateTime = false,
                        WaitTime = 3,
                        Token = "Noti"
                    }, "Info");
                    break;
                case IconChar.ArrowDown91:
                    //cambiar orden lista
                    items = new ObservableCollection<ItemInventario>(Items.Where(x => x.Nombre.ToLower().Contains(BarraDeBusqueda.Text.ToLower())).OrderBy(x => x.Nombre));
                    InventarioList.ItemsSource = items;
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
            var items = new ObservableCollection<ItemInventario>(Items.Where(x => x.Nombre.ToLower().Contains(BarraDeBusqueda.Text.ToLower())));
            InventarioList.ItemsSource = items;
        }
        private void ItemDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var seleccionado = InventarioList.SelectedItem;
            if (seleccionado != null)
            {
                MainWindow ventana = Window.GetWindow(this) as MainWindow;
                Principal pri = ventana.Contenido.Content as Principal;
                pri.ItemDelInventario = ((ItemInventario)seleccionado);
                pri.Subpestaña.Content = new InventarioAdd();
            }
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            CargarItems();
        }
    }
}
