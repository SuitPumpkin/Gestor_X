using FontAwesome.Sharp;
using HandyControl.Controls;
using HandyControl.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
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
using MessageBox = HandyControl.Controls.MessageBox;

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
        private ObservableCollection<dynamic> _displayItems;
        public ObservableCollection<dynamic> DisplayItems
        {
            get => _displayItems;
            set
            {
                _displayItems = value;
                // Actualiza el ItemsSource del DataGrid
                if (Lista != null)
                {
                    Lista.ItemsSource = _displayItems;
                }
            }
        }
        /// <summary>
        /// Constructor de la clase
        /// </summary>
        public Proyectos()
        {
            DataContext = this;
            _displayItems = new ObservableCollection<dynamic>();
            BaseDeDatos.BaseDeDatosActualizada += ActualizarItems;
            InitializeComponent();
        }
        private void ActualizarItems()
        {
            try
            {
                // Leer los proyectos de la base de datos
                _originalItems = ItemProyecto.Read();
                _allItems = new ObservableCollection<ItemProyecto>(_originalItems);

                // Actualizar la paginación
                UpdatePagination();

                // Asegurarse de estar en la primera página
                Paginador.PageIndex = 1;

                // Actualizar la página actual
                UpdateCurrentPage();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al leer la base de datos: {ex.Message}");
            }
        }
        private void CargarItems()
        {
            _ventana = System.Windows.Window.GetWindow(this) as MainWindow;

            if (_ventana != null && _ventana.Proyectos != null)
            {
                _originalItems = _ventana.Proyectos;
                _allItems = new ObservableCollection<ItemProyecto>(_originalItems);

                // Actualizar la paginación
                UpdatePagination();

                // Actualizar la página actual
                UpdateCurrentPage();
            }
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

            Paginador.MaxPageCount = Math.Max(1, totalPages); // Asegurarse de que siempre haya al menos 1 página
        }
        private void UpdateCurrentPage()
        {
            if (_allItems == null) return;

            int startIndex = (Paginador.PageIndex - 1) * Paginador.DataCountPerPage;
            int count = Math.Min(Paginador.DataCountPerPage, _allItems.Count - startIndex);

            // Obtener los items para la página actual
            var pageItems = _allItems.Skip(startIndex).Take(count).ToList();

            // Obtener los nombres de los clientes
            var clientes = _ventana?.Agenda?.Where(x => x.TipoDeContacto == "Cliente" || x.TipoDeContacto == "Trabajador")
                .ToDictionary(x => x.ID, x => x.Nombre);

            // Crear la colección con los nombres de clientes y estado de pago para mostrar
            var displayItems = new ObservableCollection<dynamic>(
                pageItems.Select(item => {
                    string estadoPagoTexto;
                    string estadoPagoColor;
                    double costoTotal = 0;
                    double costoPagado = 0;
                    try
                    {
                        var propCostoTotal = item.Precio;
                        var propCostoPagado = item.Pagado;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error al obtener propiedades de costo: {ex.Message}");
                    }

                    if (costoTotal > 0)
                    {
                        // Calcular el porcentaje pagado
                        double porcentajePagado = (costoPagado / costoTotal) * 100;

                        if (porcentajePagado >= 100)
                        {
                            estadoPagoTexto = "Pagado";
                            estadoPagoColor = "#2ECC71"; // Verde
                        }
                        else if (porcentajePagado > 0)
                        {
                            estadoPagoTexto = "Pagando";
                            estadoPagoColor = "#F39C12"; // Naranja
                        }
                        else
                        {
                            estadoPagoTexto = "Sin pagar";
                            estadoPagoColor = "#E74C3C"; // Rojo
                        }
                    }
                    else
                    {
                        estadoPagoTexto = "Sin costo";
                        estadoPagoColor = "#95A5A6"; // Gris
                    }

                    return new
                    {
                        Item = item,
                        NombreCliente = clientes.TryGetValue(item.Cliente, out string nombre) ? nombre : "Desconocido",
                        EstadoPagoTexto = estadoPagoTexto,
                        EstadoPagoColor = estadoPagoColor
                    };
                })
            );

            // Actualizar la propiedad DisplayItems (esto actualizará automáticamente el ItemsSource del DataGrid)
            DisplayItems = displayItems;
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
                string searchText = BarraDeBusqueda.Text.ToLower();
                _allItems = new ObservableCollection<ItemProyecto>(
                    _originalItems.Where(x => x.Nombre.ToLower().Contains(searchText))
                );
            }

            // Actualizar la paginación
            UpdatePagination();

            // Resetear a la primera página
            Paginador.PageIndex = 1;

            // Actualizar la página actual
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