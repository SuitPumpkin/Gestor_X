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
    public partial class Agenda : UserControl
    {
        public ObservableCollection<ItemAgenda> Items { get; set; }
        public Agenda()
        {
            InitializeComponent();
        }
        private void CargarItems()
        {
            MainWindow ventana = Window.GetWindow(this) as MainWindow;
            Principal pri = ventana.Contenido.Content as Principal;
            Items = pri.Agenda;
            AgendaList.ItemsSource = Items.OrderBy(x => x.Nombre);
        }
        private void Agregar(object sender, MouseButtonEventArgs e)
        {
            MainWindow ventana = Window.GetWindow(this) as MainWindow;
            Principal pri = ventana.Contenido.Content as Principal;
            pri.ItemDeLaAgenda = new ItemAgenda();
            pri.Subpestaña.Content = new AgendaAdd();
        }
        private void Editar(object sender, MouseButtonEventArgs e)
        {
            var seleccionado = AgendaList.SelectedItem;
            if (seleccionado != null)
            {
                MainWindow ventana = Window.GetWindow(this) as MainWindow;
                Principal pri = ventana.Contenido.Content as Principal;
                pri.ItemDeLaAgenda = ((ItemAgenda)seleccionado);
                pri.Subpestaña.Content = new AgendaAdd();
            }
            else
            {
                MessageBox.Show("Primero selecciona un item");
            }
        }
        private void Ordenar(object sender, MouseButtonEventArgs e)
        {
            var items = new ObservableCollection<ItemAgenda>();
            MainWindow ventana = MainWindow.GetWindow(this) as MainWindow;
            switch (IconoOrdenar.Icon)
            {
                case IconChar.ArrowDownAZ:
                    //cambiar orden lista
                    items = new ObservableCollection<ItemAgenda>(Items.Where(x => x.Nombre.ToLower().Contains(BarraDeBusqueda.Text.ToLower())).OrderByDescending(x => x.Nombre));
                    AgendaList.ItemsSource = items;
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
                    items = new ObservableCollection<ItemAgenda>(Items.Where(x => x.Nombre.ToLower().Contains(BarraDeBusqueda.Text.ToLower())).OrderBy(x => x.Nombre));
                    AgendaList.ItemsSource = items;
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
            var items = new ObservableCollection<ItemAgenda>(Items.Where(x => x.Nombre.ToLower().Contains(BarraDeBusqueda.Text.ToLower())));
            AgendaList.ItemsSource = items;
        }
        private void ItemDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var seleccionado = AgendaList.SelectedItem;
            if (seleccionado != null)
            {
                MainWindow ventana = Window.GetWindow(this) as MainWindow;
                Principal pri = ventana.Contenido.Content as Principal;
                pri.ItemDeLaAgenda = ((ItemAgenda)seleccionado);
                pri.Subpestaña.Content = new AgendaAdd();
            }
        }
        private void EnviarCorreo(object sender, MouseButtonEventArgs e)
        {
            Border enviado = sender as Border;
            string Correo = enviado.Tag.ToString();
            try
            {
                string asunto = "";
                string contenido = "";
                Process.Start($"mailto:{Correo}?subject={Uri.EscapeDataString(asunto)}&body={Uri.EscapeDataString(contenido)}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }
        private void EnviarWhatsapp(object sender, MouseButtonEventArgs e)
        {
            Border enviado = sender as Border;
            string Whatsapp = enviado.Tag.ToString();
            try
            {
                string mensaje = "Hola! Oye ";
                Process.Start(new ProcessStartInfo
                {
                    FileName = $"https://wa.me/{Whatsapp}?text={Uri.EscapeDataString(mensaje)}",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }
        private void AbrirWeb(object sender, MouseButtonEventArgs e)
        {
            Border enviado = sender as Border;
            string Web = enviado.Tag.ToString();
            if (Uri.TryCreate(Web, UriKind.Absolute, out Uri uri) || uri != null || Web.ToLower() != "n/a")
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = $"{Web}",
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}");
                }
            }
            else
            {

                MessageBox.Show($"No tiene pagina web");
            }
        }
        private void AbrirMapa(object sender, MouseButtonEventArgs e)
        {
            Border enviado = sender as Border;
            string Mapa = enviado.Tag.ToString();
            if (Uri.TryCreate(Mapa, UriKind.Absolute, out Uri uri) || uri != null || Mapa.ToLower() != "n/a")
            {
                if (Mapa.Length != 0)
                {
                    try
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = $"{Mapa}",
                            UseShellExecute = true
                        });
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error: {ex.Message}");
                    }
                }
            }
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            CargarItems();
        }
    }
}
