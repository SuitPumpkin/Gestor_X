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
        /// <summary>
        /// Referencia a la ventana principal
        /// </summary>
        private MainWindow _ventana;
        /// <summary>
        /// Almacenta la información de los items actuales de la agenda
        /// </summary>
        private ObservableCollection<ItemAgenda> _originalItems;
        private ObservableCollection<ItemAgenda> _allItems;
        private ObservableCollection<ItemAgenda> _currentPageItems;

        public ObservableCollection<ItemAgenda> Items 
        { 
            get => _currentPageItems;
            set
            {
                _originalItems = value;
                _allItems = value;
                UpdatePagination();
            }
        }

        /// <summary>
        /// Constructor del componente
        /// </summary>
        public Agenda()
        {
            DataContext = this;
            BaseDeDatos.BaseDeDatosActualizada += ActualizarItems;
            InitializeComponent();
        }
        private void ActualizarItems()
        {
            try
            {
                Items = ItemAgenda.Read();
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
            _ventana = Window.GetWindow(this) as MainWindow;
            Items = _ventana.Agenda;
            Lista.ItemsSource = Items;
        }
        private void Agregar(object sender, MouseButtonEventArgs e)
        {
            _ventana.Pestaña.Content = new ItemAdd() { Seleccionado = Entidad.Agenda };
        }
        private void Editar(object sender, MouseButtonEventArgs e)
        {
            var seleccionado = Lista.SelectedItem;
            if (seleccionado != null)
            {
                _ventana.Pestaña.Content = new ItemAdd() { Seleccionado = Entidad.Agenda, ItemActual = ((ItemAgenda)seleccionado) };
            }
        }
        private void BusquedaUpdate(object sender, TextChangedEventArgs e)
        {
            if (_originalItems == null) return;

            if (string.IsNullOrWhiteSpace(BarraDeBusqueda.Text))
            {
                // Si la barra de búsqueda está vacía, restaurar los datos originales
                _allItems = new ObservableCollection<ItemAgenda>(_originalItems);
            }
            else
            {
                // Filtrar los datos basados en el texto de búsqueda
                _allItems = new ObservableCollection<ItemAgenda>(
                    _originalItems.Where(x => x.Nombre.ToLower().Contains(BarraDeBusqueda.Text.ToLower()))
                );
            }
            
            // Actualizar la paginación
            UpdatePagination();
            
            // Resetear a la primera página
            Paginador.PageIndex = 1;
            UpdateCurrentPage();
        }
        private void EnviarCorreo(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            string Correo = button.Tag.ToString();
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
        private void EnviarWhatsapp(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            string Whatsapp = button.Tag.ToString();
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
        private void AbrirWeb(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            string Web = button.Tag.ToString();
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
        private void AbrirMapa(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            string Mapa = button.Tag.ToString();
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
        public void UpdatePagination()
        {
            if (_allItems == null) return;

            int totalItems = _allItems.Count;
            int itemsPerPage = Paginador.DataCountPerPage;
            int totalPages = (int)Math.Ceiling((double)totalItems / itemsPerPage);

            Paginador.MaxPageCount = totalPages;
            UpdateCurrentPage();
        }
        private void UpdateCurrentPage()
        {
            if (_allItems == null) return;

            int startIndex = (Paginador.PageIndex - 1) * Paginador.DataCountPerPage;
            int count = Math.Min(Paginador.DataCountPerPage, _allItems.Count - startIndex);

            _currentPageItems = new ObservableCollection<ItemAgenda>(
                _allItems.Skip(startIndex).Take(count)
            );

            Lista.ItemsSource = _currentPageItems;
        }
        private void Paginador_PageUpdated(object sender, FunctionEventArgs<int> e)
        {
            UpdateCurrentPage();
        }
    }
}
