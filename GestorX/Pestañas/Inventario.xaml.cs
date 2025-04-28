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
        private MainWindow _ventana;
        /// <summary>
        /// Almacenta la información de los items actuales del inventario
        /// </summary>
        private ObservableCollection<ItemInventario> _originalItems;
        private ObservableCollection<ItemInventario> _allItems;
        private ObservableCollection<dynamic> _currentPageItems;

        public ObservableCollection<ItemInventario> Items 
        { 
            get => _allItems;
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
                Lista.Items.Refresh();
            });
        }
        private void CargarItems()
        {
            _ventana = Window.GetWindow(this) as MainWindow;
            Items = _ventana.Inventario;
            
            // Get all providers from the agenda
            var proveedores = _ventana.Agenda.Where(x => x.TipoDeContacto == "Proveedor").ToDictionary(x => x.ID, x => x.Nombre);
            
            // Create a new collection with the provider names instead of IDs
            var itemsConNombres = Items.Select(item => new
            {
                Item = item,
                NombreProveedor = proveedores.TryGetValue(item.Vendedor, out string nombre) ? nombre : "Desconocido"
            }).ToList();

            // Set the ItemsSource with the provider names
            Lista.ItemsSource = itemsConNombres;
        }
        private void Agregar(object sender, MouseButtonEventArgs e)
        {
            _ventana.Pestaña.Content = new ItemAdd() { Seleccionado = Entidad.Inventario };
        }
        private void Editar(object sender, MouseButtonEventArgs e)
        {
            var seleccionado = Lista.SelectedItem;
            if (seleccionado != null)
            {
                dynamic item = seleccionado;
                _ventana.Pestaña.Content = new ItemAdd() { Seleccionado = Entidad.Inventario, ItemActual = item.Item };
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

            var proveedores = _ventana.Agenda.Where(x => x.TipoDeContacto == "Proveedor").ToDictionary(x => x.ID, x => x.Nombre);
            
            _currentPageItems = new ObservableCollection<dynamic>(
                _allItems.Skip(startIndex).Take(count).Select(item => new
                {
                    Item = item,
                    NombreProveedor = proveedores.TryGetValue(item.Vendedor, out string nombre) ? nombre : "Desconocido"
                })
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
                _allItems = new ObservableCollection<ItemInventario>(_originalItems);
            }
            else
            {
                // Filtrar los datos basados en el texto de búsqueda
                _allItems = new ObservableCollection<ItemInventario>(
                    _originalItems.Where(x => x.Nombre.ToLower().Contains(BarraDeBusqueda.Text.ToLower()))
                );
            }
            
            // Actualizar la paginación
            UpdatePagination();
            
            // Resetear a la primera página
            Paginador.PageIndex = 1;
            UpdateCurrentPage();
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            CargarItems();
        }
    }
}
