using FontAwesome.Sharp;
using HandyControl.Controls;
using HandyControl.Data;
using HandyControl.Tools.Extension;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public ObservableCollection<ItemInventario> Items { get; set; }
        public float ValorInventario { get; set; }
        public Inventario()
        {
            InitializeComponent();
            DataContext = this;
        }
        private void CargarItems()
        {
            MainWindow ventana = Window.GetWindow(this) as MainWindow;
            Principal pri = ventana.Contenido.Content as Principal;
            Items = pri.Inventario;
            foreach (ItemInventario item in Items)
            {
                var valor = item.PrecioCompra * item.Cantidad;
                ValorInventario += valor;
            }
            valor.Text = ValorInventario.ToString();
            InventarioList.ItemsSource = Items.OrderBy(x => x.Nombre);
        }
        private void AgregarAInventario(object sender, MouseButtonEventArgs e)
        {
            MainWindow ventana = Window.GetWindow(this) as MainWindow;
            Principal pri = ventana.Contenido.Content as Principal;
            pri.ItemDelInventario = new ItemInventario();
            pri.Subpestaña.Content = new InventarioAdd();
        }
        private void EditarDeInventario(object sender, MouseButtonEventArgs e)
        {
            var seleccionado = InventarioList.SelectedItem;
            if (seleccionado != null)
            {
                MainWindow ventana = Window.GetWindow(this) as MainWindow;
                Principal pri = ventana.Contenido.Content as Principal;
                pri.ItemDelInventario = ((ItemInventario)seleccionado);
                pri.Subpestaña.Content = new InventarioAdd();
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
