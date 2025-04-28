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
        private MainWindow _ventana;
        /// <summary>
        /// Colección de items de proyecto
        /// </summary>
        private ObservableCollection<ItemProyecto> _originalItems;
        private ObservableCollection<ItemProyecto> _allItems;
        private ObservableCollection<ItemProyecto> _currentPageItems;

        public ObservableCollection<ItemProyecto> Items 
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
            _ventana = Window.GetWindow(this) as MainWindow;
            Items = _ventana.Proyectos;

            // Get all providers from the agenda
            var clientes = _ventana.Agenda.Where(x => x.TipoDeContacto == "Cliente").ToDictionary(x => x.ID, x => x.Nombre);

            // Create a new collection with the provider names instead of IDs
            var itemsConNombres = Items.Select(item => new
            {
                Item = item,
                NombreCliente = clientes.TryGetValue(item.Cliente, out string nombre) ? nombre : "Desconocido"
            }).ToList();

            // Set the ItemsSource with the apropiate names
            Lista.ItemsSource = itemsConNombres;
        }
        private void Agregar(object sender, MouseButtonEventArgs e)
        {
            _ventana.Pestaña.Content = new ItemAdd() { Seleccionado = Entidad.Proyecto };
        }
        private void Editar(object sender, MouseButtonEventArgs e)
        {
            var seleccionado = Lista.SelectedItem;
            if (seleccionado != null)
            {
                dynamic item = seleccionado;
                _ventana.Pestaña.Content = new ItemAdd() { Seleccionado = Entidad.Proyecto, ItemActual = item.Item };
            }
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

            _currentPageItems = new ObservableCollection<ItemProyecto>(
                _allItems.Skip(startIndex).Take(count)
            );

            Lista.ItemsSource = _currentPageItems;
        }

        private void Paginador_PageUpdated(object sender, FunctionEventArgs<int> e)
        {
            UpdateCurrentPage();
        }

        private void BusquedaUpdate(object sender, TextChangedEventArgs e)
        {
            if (_originalItems == null) return;

            if (string.IsNullOrWhiteSpace(BarraDeBusqueda.Text))
            {
                // Si la barra de búsqueda está vacía, restaurar los datos originales
                _allItems = new ObservableCollection<ItemProyecto>(_originalItems);
            }
            else
            {
                // Filtrar los datos basados en el texto de búsqueda
                _allItems = new ObservableCollection<ItemProyecto>(
                    _originalItems.Where(x => x.Nombre.ToLower().Contains(BarraDeBusqueda.Text.ToLower()))
                );
            }
            
            // Actualizar la paginación
            UpdatePagination();
            
            // Resetear a la primera página
            Paginador.PageIndex = 1;
            UpdateCurrentPage();
        }
        private void AbrirCarpeta(object sender, RoutedEventArgs e)
        {
            string carpeta = "";
            if (sender is Button button)
            {
                carpeta = button.Tag.ToString();
            }
            else if (sender is Border border)
            {
                carpeta = border.Tag.ToString();
            }

            if (Directory.Exists(carpeta))
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = carpeta,
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
                MessageBox.Show("La carpeta no existe");
            }
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            CargarItems();
        }
    }
}
