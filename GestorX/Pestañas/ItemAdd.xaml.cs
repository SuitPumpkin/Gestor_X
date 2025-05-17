using GestorX.Componentes;
using HandyControl.Controls;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Security.Principal;
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
using Path = System.IO.Path;
using Window = System.Windows.Window;

namespace GestorX.Pestañas
{
    /// <summary>
    /// Lógica de interacción para ItemAdd.xaml
    /// </summary>
    public partial class ItemAdd : UserControl
    {
        /// <summary>
        /// Referencia a la ventana principal
        /// </summary>
        private MainWindow _ventana;
        /// <summary>
        /// Entidad de Item para agregar
        /// </summary>
        public Entidad Seleccionado { get; set; } = Entidad.Agenda;
        public static readonly DependencyProperty IconTypeProperty =
            DependencyProperty.Register(nameof(Icono), typeof(string), typeof(ItemAdd),
                new FrameworkPropertyMetadata("None", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        // Clases para manejar los archivos editables y resultados
        public class ArchivoEditable
        {
            public string Ruta { get; set; }
            public string NombreArchivo { get; set; }
            public string TipoArchivo { get; set; }
            public ImageSource IconoSource { get; set; }
            public TipoEditable Tipo { get; set; }
            public string PresetIllustrator { get; set; } = "Ninguno";
        }
        public class ArchivoResultado
        {
            public string Ruta { get; set; }
            public string Nombre { get; set; }
            public string Tipo { get; set; }
            public ImageSource ImagenSource { get; set; }
        }
        public enum TipoEditable
        {
            Illustrator,
            Photoshop,
            Otro
        }

        public string Icono
        {
            get => (string)GetValue(IconTypeProperty);
            set => SetValue(IconTypeProperty, value);
        }

        /// <summary>
        /// Almacena la información actual del item en caso de ser una edición y no una creación
        /// </summary>
        public ItemBase ItemActual { get; set; }
        /// <summary>
        /// Item que se usa en las combobox
        /// </summary>
        public class ItemComboBox
        {
            public string Nombre { get; set; }
            public string ID { get; set; }
            public string Descripción { get; set; }
        }
        public ItemAdd()
        {
            InitializeComponent();
            CargarInfo();
        }
        private async void CargarInfo()
        {
            await Task.Delay(100);
            _ventana = Window.GetWindow(this) as MainWindow;
            InicializarComboBoxes();

            //cargar los datos generales
            if (ItemActual is ItemAgenda AgendaActual)
            {
                AgendaActual = ItemActual as ItemAgenda;
                Correo.Text = AgendaActual.Correo;
                Telefono.Text = AgendaActual.Telefono;
                PaginaWeb.Text = AgendaActual.PaginaWeb;
                Descripción.Text = AgendaActual.Descripción;
                Nombre.Text = AgendaActual.Nombre;
                Titulo.Text = AgendaActual.Nombre;
                Ubicación.Text = AgendaActual.UbicaciónMaps;
                foreach (ItemComboBox item in TipoDeContacto.Items)
                {
                    if (item.Nombre == AgendaActual.TipoDeContacto)
                    {
                        TipoDeContacto.SelectedItem = item;
                        break;
                    }
                }
                if (AgendaActual.ID != string.Empty)
                {
                    if (AgendaActual.Imagen == null)
                    {
                        BitmapImage fallbackBitmap = new BitmapImage();
                        fallbackBitmap.BeginInit();
                        fallbackBitmap.UriSource = new Uri("pack://application:,,,/404.png", UriKind.Absolute);
                        fallbackBitmap.EndInit();
                        Preview.Source = fallbackBitmap;
                    }
                    else
                    {
                        try
                        {
                            Preview.Source = ManejoDeImagenes.BytesAImagen(AgendaActual.Imagen);
                        }
                        catch
                        {
                            BitmapImage fallbackBitmap = new BitmapImage();
                            fallbackBitmap.BeginInit();
                            fallbackBitmap.UriSource = new Uri("pack://application:,,,/404.png", UriKind.Absolute);
                            fallbackBitmap.EndInit();
                            Preview.Source = fallbackBitmap;
                        }
                    }
                    Duplicador.Visibility = Visibility.Visible;
                    Borrador.Visibility = Visibility.Visible;
                }
            }
            else if (ItemActual is ItemInventario InventarioActual)
            {
                InventarioActual = ItemActual as ItemInventario;
                Cantidad.Text = InventarioActual.Cantidad.ToString();
                Unidad.Text = InventarioActual.Unidad;
                Nombre.Text = InventarioActual.Nombre;
                Costo.Text = InventarioActual.Precio.ToString();
                Descripción.Text = InventarioActual.Descripción;
                foreach (ItemComboBox item in Proveedor.Items)
                {
                    if (item.ID == InventarioActual.Vendedor)
                    {
                        Proveedor.SelectedItem = item;
                        break;
                    }
                }
                if (InventarioActual.ID != string.Empty)
                {
                    if (InventarioActual.Imagen == null)
                    {
                        BitmapImage fallbackBitmap = new BitmapImage();
                        fallbackBitmap.BeginInit();
                        fallbackBitmap.UriSource = new Uri("pack://application:,,,/404.png", UriKind.Absolute);
                        fallbackBitmap.EndInit();
                        Preview.Source = fallbackBitmap;
                    }
                    else
                    {
                        try
                        {
                            Preview.Source = ManejoDeImagenes.BytesAImagen(InventarioActual.Imagen);
                        }
                        catch
                        {
                            BitmapImage fallbackBitmap = new BitmapImage();
                            fallbackBitmap.BeginInit();
                            fallbackBitmap.UriSource = new Uri("pack://application:,,,/404.png", UriKind.Absolute);
                            fallbackBitmap.EndInit();
                            Preview.Source = fallbackBitmap;
                        }
                    }
                    Duplicador.Visibility = Visibility.Visible;
                    Borrador.Visibility = Visibility.Visible;
                }
            }
            else if (ItemActual is ItemProyecto ProyectoActual)
            {
                ProyectoActual = ItemActual as ItemProyecto;
                Pagado.Text = ProyectoActual.Pagado.ToString();
                Total.Text = ProyectoActual.Precio.ToString();
                Pasos.StepIndex = (ProyectoActual.Progreso / 25);
                Descripción.Text = ProyectoActual.Descripción;
                Nombre.Text = ProyectoActual.Nombre;
                Fecha.Text = ProyectoActual.FechaCreación;
                foreach (ItemComboBox item in Cliente.Items)
                {
                    if (item.ID == ProyectoActual.Cliente)
                    {
                        Cliente.SelectedItem = item;
                        break;
                    }
                }
                if (ItemActual.ID != string.Empty)
                {
                    if (ItemActual.Imagen == null)
                    {
                        BitmapImage fallbackBitmap = new BitmapImage();
                        fallbackBitmap.BeginInit();
                        fallbackBitmap.UriSource = new Uri("pack://application:,,,/404.png", UriKind.Absolute);
                        fallbackBitmap.EndInit();
                        Preview.Source = fallbackBitmap;
                    }
                    else
                    {
                        try
                        {
                            Preview.Source = ManejoDeImagenes.BytesAImagen(ItemActual.Imagen);
                        }
                        catch
                        {
                            BitmapImage fallbackBitmap = new BitmapImage();
                            fallbackBitmap.BeginInit();
                            fallbackBitmap.UriSource = new Uri("pack://application:,,,/404.png", UriKind.Absolute);
                            fallbackBitmap.EndInit();
                            Preview.Source = fallbackBitmap;
                        }
                    }
                    Duplicador.Visibility = Visibility.Visible;
                    Borrador.Visibility = Visibility.Visible;
                    try
                    {
                        CargarResultados();
                        SecciónResultados.Visibility = Visibility.Visible;
                    }
                    catch
                    {
                        Debug.WriteLine("Error al cargar los resultados");
                    }
                    try
                    {
                        CargarEditables();
                        SecciónEditables.Visibility = Visibility.Visible;
                    }
                    catch
                    {
                        Debug.WriteLine("Error al cargar los editables");
                    }
                }
            }

            //Mostrar los controles necesarios
            switch (Seleccionado)
            {
                case Entidad.Agenda:
                    if (!(ItemActual is ItemAgenda)) { ItemActual = new ItemAgenda(); }
                    if (ItemActual.ID == "0" || ItemActual.ID == null) { Icono = "User"; }
                    ControlesAGENDA.Visibility = Visibility.Visible;
                    break;
                case Entidad.Inventario:
                    if (!(ItemActual is ItemInventario)) { ItemActual = new ItemInventario(); }
                    if (ItemActual.ID == "0" || ItemActual.ID == null) { Icono = "Box"; }
                    ControlesINVENTARIO.Visibility = Visibility.Visible;
                    break;
                case Entidad.Proyecto:
                    if (!(ItemActual is ItemProyecto)) { ItemActual = new ItemProyecto(); }
                    if (ItemActual.ID == "0" || ItemActual.ID == null) { Icono = "Scroll"; }
                    ControlesPROYECTO.Visibility = Visibility.Visible;
                    break;
                default:
                    break;
            }
        }
        private void InicializarComboBoxes()
        {
            var Clientes = new ObservableCollection<ItemComboBox>();
            var Proveedores = new ObservableCollection<ItemComboBox>();
            var TiposDeContactos = new ObservableCollection<ItemComboBox>();

            foreach (ItemAgenda p in _ventana.Agenda)
            {
                if (p.TipoDeContacto == "Proveedor")
                {
                    Proveedores.Add(new ItemComboBox
                    {
                        Nombre = p.Nombre,
                        ID = p.ID,
                        Descripción = p.Descripción.Replace("\\n", Environment.NewLine)
                    });
                }
                else if (p.TipoDeContacto == "Cliente" || p.TipoDeContacto == "Trabajador")
                {
                    Clientes.Add(new ItemComboBox
                    {
                        Nombre = p.Nombre,
                        ID = p.ID,
                        Descripción = p.Descripción.Replace("\\n", Environment.NewLine)
                    });
                }
            }
            Clientes.Add(new ItemComboBox
            {
                Nombre = "Desconocido",
                ID = "0",
                Descripción = "Aún no se ha agregado a la agenda de la empresa"
            });
            Proveedores.Add(new ItemComboBox
            {
                Nombre = "Desconocido",
                ID = "0",
                Descripción = "Aún no se ha agregado a la agenda de la empresa"
            });
            TiposDeContactos.Add(new ItemComboBox
            {
                Nombre = "Cliente",
                ID = "0",
                Descripción = "Es un cliente en la empresa"
            });
            TiposDeContactos.Add(new ItemComboBox
            {
                Nombre = "Proveedor",
                ID = "0",
                Descripción = "Es un proveedor en la empresa"
            });
            TiposDeContactos.Add(new ItemComboBox
            {
                Nombre = "Trabajador",
                ID = "0",
                Descripción = "Es un trabajador de la empresa"
            });

            Cliente.ItemsSource = Clientes.OrderBy(x => x.Nombre);
            Proveedor.ItemsSource = Proveedores.OrderBy(x => x.Nombre);
            TipoDeContacto.ItemsSource = TiposDeContactos.OrderBy(x => x.Nombre);
            TipoDeContacto.SelectedIndex = 0;
        }
        private void Guardar(object sender, RoutedEventArgs e)
        {
            VerificarCamposObligatorios();
            if (ItemActual is ItemAgenda)
            {
                //FASE 1: Preparar la información del objeto
                if (Telefono.Text == "") { Telefono.Text = "0"; }
                if (Correo.Text == "") { Correo.Text = "N/A"; }
                if (PaginaWeb.Text == "") { PaginaWeb.Text = "N/A"; }
                if (Ubicación.Text == "") { Ubicación.Text = "N/A"; }
                ItemComboBox tiposelecionado = TipoDeContacto.SelectedItem as ItemComboBox;
                var TipoNombre = "Cliente";
                if (tiposelecionado != null)
                {
                    TipoNombre = tiposelecionado.Nombre;
                }
                ItemAgenda ITEM = new ItemAgenda()
                {
                    Nombre = Nombre.Text,
                    Descripción = Descripción.Text,
                    Telefono = Telefono.Text,
                    TipoDeContacto = TipoNombre,
                    Correo = Correo.Text,
                    PaginaWeb = PaginaWeb.Text,
                    UbicaciónMaps = Ubicación.Text,
                };

                //FASE 2: Preparar la imagen del objeto
                GC.Collect();
                if (Preview.Source == null)
                {
                    //analiza si el objeto ya tiene una imágen asignada y en caso de tenerla la usa, sino asigna una por defecto
                    if (ItemActual.Imagen != null)
                    {
                        ITEM.Imagen = ItemActual.Imagen;
                    }
                    else
                    {
                        BitmapImage fallbackBitmap = new BitmapImage();
                        fallbackBitmap.BeginInit();
                        fallbackBitmap.UriSource = new Uri("pack://application:,,,/AgendaBase.png", UriKind.Absolute);
                        fallbackBitmap.EndInit();
                        Preview.Source = fallbackBitmap;
                    }
                }
                if (Preview.Source.ToString() != null)
                {
                    if (Preview.Source is BitmapImage imagen)
                    {
                        using (MemoryStream stream = new MemoryStream())
                        {
                            BitmapEncoder encoder = new PngBitmapEncoder();
                            encoder.Frames.Add(BitmapFrame.Create(imagen));
                            encoder.Save(stream);
                            ITEM.Imagen = stream.ToArray();
                        }
                    }
                    else
                    {
                        ITEM.Imagen = ManejoDeImagenes.ImagenABytes(Preview.Source.ToString().Replace("file:", "").Replace("///C:", "C:").Replace("///D:", "D:").Replace("///E:", "E:"));
                    }
                }


                //FASE 3: Guardar o actualizar el objeto
                if (ItemActual.ID == string.Empty) { ItemAgenda.Create(ITEM); }
                else
                {
                    ITEM.ID = ItemActual.ID;
                    ItemAgenda.Update(ITEM);
                }

                //FASE 4: salir
                _ventana.Pestaña.Content = new Agenda();
            }
            else if(ItemActual is ItemInventario)
            {
                //FASE 1: Preparar la información del objeto
                if (Unidad.Text == "") { Unidad.Text = "Unidad"; }
                if (int.Parse(Costo.Text) == 0) { Costo.Text = "0"; }
                if (int.Parse(Cantidad.Text) == 0) { Cantidad.Text = "0"; }
                ItemComboBox proveedorselecionado = Proveedor.SelectedItem as ItemComboBox;
                var proveedorID = "0";
                if (proveedorselecionado != null)
                {
                    proveedorID = proveedorselecionado.ID;
                }
                ItemInventario ITEM = new ItemInventario()
                {
                    Nombre = Nombre.Text,
                    Descripción = Descripción.Text,
                    Vendedor = proveedorID,
                    Precio = float.Parse(Costo.Text),
                    Unidad = Unidad.Text,
                    Cantidad = int.Parse(Cantidad.Text),
                };

                //FASE 2: Preparar la imagen del objeto
                GC.Collect();
                if (Preview.Source == null)
                {
                    //analiza si el objeto ya tiene una imágen asignada y en caso de tenerla la usa, sino asigna una por defecto
                    if (ItemActual.Imagen != null)
                    {
                        ITEM.Imagen = ItemActual.Imagen;
                    }
                    else
                    {
                        BitmapImage fallbackBitmap = new BitmapImage();
                        fallbackBitmap.BeginInit();
                        fallbackBitmap.UriSource = new Uri("pack://application:,,,/InventarioBase.png", UriKind.Absolute);
                        fallbackBitmap.EndInit();
                        Preview.Source = fallbackBitmap;
                    }
                }
                if (Preview.Source.ToString() != null)
                {
                    if (Preview.Source is BitmapImage imagen)
                    {
                        using (MemoryStream stream = new MemoryStream())
                        {
                            BitmapEncoder encoder = new PngBitmapEncoder();
                            encoder.Frames.Add(BitmapFrame.Create(imagen));
                            encoder.Save(stream);
                            ITEM.Imagen = stream.ToArray();
                        }
                    }
                    else
                    {
                        ITEM.Imagen = ManejoDeImagenes.ImagenABytes(Preview.Source.ToString().Replace("file:", "").Replace("///C:", "C:").Replace("///D:", "D:").Replace("///E:", "E:"));
                    }
                }

                //FASE 3: Guardar o actualizar el objeto
                if (ItemActual.ID == string.Empty) { ItemInventario.Create(ITEM); }
                else
                {
                    ITEM.ID = ItemActual.ID;
                    ItemInventario.Update(ITEM);
                }

                //FASE 4: salir
                _ventana.Pestaña.Content = new Inventario();
            }
            else if(ItemActual is ItemProyecto)
            {
                //FASE 1: Preparar la información del objeto
                if (Pagado.Text == "") { Pagado.Text = "0.00"; }
                if (Fecha.Text == "") { Fecha.SelectedDate = DateTime.Now; }
                if (Total.Text == "") { Total.Text = "0.00"; }
                ItemComboBox clienteseleccionado = Cliente.SelectedItem as ItemComboBox;
                var cliente = "0";
                if (clienteseleccionado != null)
                {
                    cliente = clienteseleccionado.ID;
                }
                ItemProyecto ITEM = new ItemProyecto()
                {
                    Nombre = Nombre.Text.Replace(";", ""),
                    Descripción = Descripción.Text.Replace(";", "").Replace(Environment.NewLine, "\\n"),
                    Progreso = Pasos.StepIndex * 25,
                    FechaCreación = Fecha.Text.Replace(",", ""),
                    Precio = float.Parse(Total.Text.Replace(",", "")),
                    Cliente = cliente,
                    Pagado = float.Parse(Pagado.Text.Replace(",", "")),
                };

                //FASE 2: Preparar la imagen del objeto
                GC.Collect();
                if (Preview.Source == null)
                {
                    //analiza si el objeto ya tiene una imágen asignada y en caso de tenerla la usa, sino asigna una por defecto
                    if (ItemActual.Imagen != null)
                    {
                        ITEM.Imagen = ItemActual.Imagen;
                    }
                    else
                    {
                        BitmapImage fallbackBitmap = new BitmapImage();
                        fallbackBitmap.BeginInit();
                        fallbackBitmap.UriSource = new Uri("pack://application:,,,/ProyectoBase.png", UriKind.Absolute);
                        fallbackBitmap.EndInit();
                        Preview.Source = fallbackBitmap;
                    }
                }
                if (Preview.Source.ToString() != null)
                {
                    if (Preview.Source is BitmapImage imagen)
                    {
                        using (MemoryStream stream = new MemoryStream())
                        {
                            BitmapEncoder encoder = new PngBitmapEncoder();
                            encoder.Frames.Add(BitmapFrame.Create(imagen));
                            encoder.Save(stream);
                            ITEM.Imagen = stream.ToArray();
                        }
                    }
                    else
                    {
                        ITEM.Imagen = ManejoDeImagenes.ImagenABytes(Preview.Source.ToString().Replace("file:", "").Replace("///C:", "C:").Replace("///D:", "D:").Replace("///E:", "E:"));
                    }
                }

                //FASE 3: Guardar o actualizar el objeto
                if (ItemActual.ID == string.Empty) { ItemProyecto.Create(ITEM); }
                else
                {
                    ITEM.ID = ItemActual.ID;
                    ItemProyecto.Update(ITEM);
                }
                //FASE 4: salir
                _ventana.Pestaña.Content = new Proyectos();
            }
        }
        private void Duplicar(object sender, RoutedEventArgs e)
        {
            VerificarCamposObligatorios();
            if (ItemActual is ItemAgenda)
            {
                MessageBoxResult respuesta = HandyControl.Controls.MessageBox.Show($"¿Guardar {Nombre.Text} como copia de {ItemActual.Nombre}?", "Confirmación", MessageBoxButton.YesNo, MessageBoxImage.Information, MessageBoxResult.No);
                if (respuesta == MessageBoxResult.Yes)
                {
                    //FASE 1: Preparar la información del objeto
                    if (Telefono.Text == "") { Telefono.Text = "0"; }
                    if (Correo.Text == "") { Correo.Text = "N/A"; }
                    if (PaginaWeb.Text == "") { PaginaWeb.Text = "N/A"; }
                    if (Ubicación.Text == "") { Ubicación.Text = "N/A"; }
                    ItemAgenda ITEM = new ItemAgenda()
                    {
                        Nombre = Nombre.Text,
                        Descripción = Descripción.Text,
                        Telefono = Telefono.Text,
                        TipoDeContacto = TipoDeContacto.Text,
                        Correo = Correo.Text,
                        PaginaWeb = PaginaWeb.Text,
                        UbicaciónMaps = Ubicación.Text,
                    };


                    //FASE 2: Preparar la imagen del objeto
                    GC.Collect();
                    if (Preview.Source == null)
                    {
                        //analiza si el objeto ya tiene una imágen asignada y en caso de tenerla la usa, sino asigna una por defecto
                        if (ItemActual.Imagen != null)
                        {
                            ITEM.Imagen = ItemActual.Imagen;
                        }
                        else
                        {
                            BitmapImage fallbackBitmap = new BitmapImage();
                            fallbackBitmap.BeginInit();
                            fallbackBitmap.UriSource = new Uri("pack://application:,,,/InventarioBase.png", UriKind.Absolute);
                            fallbackBitmap.EndInit();
                            Preview.Source = fallbackBitmap;
                        }
                    }
                    if (Preview.Source.ToString() != null)
                    {
                        if (Preview.Source is BitmapImage imagen)
                        {
                            using (MemoryStream stream = new MemoryStream())
                            {
                                BitmapEncoder encoder = new PngBitmapEncoder();
                                encoder.Frames.Add(BitmapFrame.Create(imagen));
                                encoder.Save(stream);
                                ITEM.Imagen = stream.ToArray();
                            }
                        }
                        else
                        {
                            ITEM.Imagen = ManejoDeImagenes.ImagenABytes(Preview.Source.ToString().Replace("file:", "").Replace("///C:", "C:").Replace("///D:", "D:").Replace("///E:", "E:"));
                        }
                    }


                    //FASE 3: Guardar el objeto duplicado
                    ItemAgenda.Create(ITEM);

                    //FASE 4: salir
                    _ventana.Pestaña.Content = new Agenda();
                }
            }
            else if (ItemActual is ItemInventario)
            {
                MessageBoxResult respuesta = HandyControl.Controls.MessageBox.Show($"¿Guardar {Nombre.Text} como copia de {ItemActual.Nombre}?", "Confirmación", MessageBoxButton.YesNo, MessageBoxImage.Information, MessageBoxResult.No);
                if (respuesta == MessageBoxResult.Yes)
                {
                    //FASE 1: Preparar la información del objeto
                    if (Unidad.Text == "") { Unidad.Text = "Unidad"; }
                    if (int.Parse(Costo.Text) == 0) { Costo.Text = "0"; }
                    if (int.Parse(Cantidad.Text) == 0) { Cantidad.Text = "0"; }

                    ItemComboBox proveedorselecionado = Proveedor.SelectedItem as ItemComboBox;
                    var proveedorID = "0";
                    if (proveedorselecionado != null)
                    {
                        proveedorID = proveedorselecionado.ID;
                    }

                    ItemInventario ITEM = new ItemInventario()
                    {
                        Nombre = Nombre.Text,
                        Descripción = Descripción.Text,
                        Vendedor = proveedorID,
                        Precio = float.Parse(Costo.Text),
                        Unidad = Unidad.Text,
                        Cantidad = int.Parse(Cantidad.Text),
                    };

                    //FASE 2: Preparar la imagen del objeto
                    GC.Collect();
                    if (Preview.Source == null)
                    {
                        //analiza si el objeto ya tiene una imágen asignada y en caso de tenerla la usa, sino asigna una por defecto
                        if (ItemActual.Imagen != null)
                        {
                            ITEM.Imagen = ItemActual.Imagen;
                        }
                        else
                        {
                            BitmapImage fallbackBitmap = new BitmapImage();
                            fallbackBitmap.BeginInit();
                            fallbackBitmap.UriSource = new Uri("pack://application:,,,/InventarioBase.png", UriKind.Absolute);
                            fallbackBitmap.EndInit();
                            Preview.Source = fallbackBitmap;
                        }
                    }
                    if (Preview.Source.ToString() != null)
                    {
                        if (Preview.Source is BitmapImage imagen)
                        {
                            using (MemoryStream stream = new MemoryStream())
                            {
                                BitmapEncoder encoder = new PngBitmapEncoder();
                                encoder.Frames.Add(BitmapFrame.Create(imagen));
                                encoder.Save(stream);
                                ITEM.Imagen = stream.ToArray();
                            }
                        }
                        else
                        {
                            ITEM.Imagen = ManejoDeImagenes.ImagenABytes(Preview.Source.ToString().Replace("file:", "").Replace("///C:", "C:").Replace("///D:", "D:").Replace("///E:", "E:"));
                        }
                    }

                    //FASE 3: Guardar el objeto duplicado
                    ItemInventario.Create(ITEM);

                    //FASE 4: salir
                    _ventana.Pestaña.Content = new Inventario();
                }
            }
            else if (ItemActual is ItemProyecto)
            {
                MessageBoxResult respuesta = HandyControl.Controls.MessageBox.Show($"¿Guardar {Nombre.Text} como copia de {ItemActual.Nombre}?", "Confirmación", MessageBoxButton.YesNo, MessageBoxImage.Information, MessageBoxResult.No);
                if (respuesta == MessageBoxResult.Yes)
                {
                    //FASE 1: Preparar la información del objeto
                    if (Pagado.Text == "") { Pagado.Text = "0.00"; }
                    if (Fecha.Text == "") { Fecha.SelectedDate = DateTime.Now; }
                    if (Total.Text == "") { Total.Text = "0.00"; }

                    ItemComboBox clienteseleccionado = Cliente.SelectedItem as ItemComboBox;
                    var cliente = "0";
                    if (clienteseleccionado != null)
                    {
                        cliente = clienteseleccionado.ID;
                    }

                    ItemProyecto ITEM = new ItemProyecto()
                    {
                        Nombre = Nombre.Text,
                        Descripción = Descripción.Text,
                        Progreso = Pasos.StepIndex * 25,
                        FechaCreación = Fecha.Text,
                        Precio = float.Parse(Total.Text),
                        Cliente = cliente,
                        Pagado = float.Parse(Pagado.Text),
                    };

                    //FASE 2: Preparar la imagen del objeto
                    GC.Collect();
                    if (Preview.Source == null)
                    {
                        //analiza si el objeto ya tiene una imágen asignada y en caso de tenerla la usa, sino asigna una por defecto
                        if (ItemActual.Imagen != null)
                        {
                            ITEM.Imagen = ItemActual.Imagen;
                        }
                        else
                        {
                            BitmapImage fallbackBitmap = new BitmapImage();
                            fallbackBitmap.BeginInit();
                            fallbackBitmap.UriSource = new Uri("pack://application:,,,/InventarioBase.png", UriKind.Absolute);
                            fallbackBitmap.EndInit();
                            Preview.Source = fallbackBitmap;
                        }
                    }
                    if (Preview.Source.ToString() != null)
                    {
                        if (Preview.Source is BitmapImage imagen)
                        {
                            using (MemoryStream stream = new MemoryStream())
                            {
                                BitmapEncoder encoder = new PngBitmapEncoder();
                                encoder.Frames.Add(BitmapFrame.Create(imagen));
                                encoder.Save(stream);
                                ITEM.Imagen = stream.ToArray();
                            }
                        }
                        else
                        {
                            ITEM.Imagen = ManejoDeImagenes.ImagenABytes(Preview.Source.ToString().Replace("file:", "").Replace("///C:", "C:").Replace("///D:", "D:").Replace("///E:", "E:"));
                        }
                    }

                    //FASE 3: Guardar el objeto duplicado
                    ItemProyecto.Create(ITEM);

                    //FASE 4: salir
                    _ventana.Pestaña.Content = new Proyectos();
                }
            }
        }
        private void Cancelar(object sender, RoutedEventArgs e)
        {
            //TODO: Verificar si no se han hecho cambios y en caso de no haber cambios simplemente salir
            if (HayCambios())
            {
                MessageBoxResult respuesta = HandyControl.Controls.MessageBox.Show("¿Salir sin guardar?", "Confirmación", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
                if (respuesta == MessageBoxResult.No)
                {
                    return;
                }
            }
            switch (Seleccionado)
            {
                case Entidad.Agenda:
                    _ventana.Pestaña.Content = new Agenda();
                    break;
                case Entidad.Inventario:
                    _ventana.Pestaña.Content = new Inventario();
                    break;
                case Entidad.Proyecto:
                    _ventana.Pestaña.Content = new Proyectos();
                    break;
                default:
                    _ventana.Pestaña.Content = new Inicio();
                    break;
            }
        }
        private void Remover(object sender, RoutedEventArgs e)
        {
            MessageBoxResult respuesta = HandyControl.Controls.MessageBox.Show($"¿Seguro de querer eliminar toda la información relacionada a {ItemActual.Nombre}?", "Confirmación", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (respuesta == MessageBoxResult.Yes)
            {
                switch (Seleccionado)
                {
                    case Entidad.Agenda:
                        ItemAgenda.Delete(ItemActual.ID);
                        _ventana.Pestaña.Content = new Agenda();
                        break;
                    case Entidad.Inventario:
                        ItemInventario.Delete(ItemActual.ID);
                        _ventana.Pestaña.Content = new Inventario();
                        break;
                    case Entidad.Proyecto:
                        ItemProyecto.Delete(ItemActual.ID);
                        _ventana.Pestaña.Content = new Proyectos();
                        break;
                    default:
                        _ventana.Pestaña.Content = new Inicio();
                        break;
                }
                HandyControl.Controls.MessageBox.Show($"{ItemActual.Nombre} Eliminado correctamente");
            }
        }
        /// <summary>
        /// Función para cargar una imagen desde el dispositivo
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SeleccionandoImagen(object sender, MouseButtonEventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Filter = "Archivos de imagen|*.png;*.jpg;*.jpeg|Todos los archivos|*.*"
                };
                if (openFileDialog.ShowDialog() == true)
                {
                    string filePath = openFileDialog.FileName;
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(filePath);
                    bitmap.EndInit();
                    Preview.Source = bitmap;
                }
            }
            catch (Exception ex)
            {
                HandyControl.Controls.MessageBox.Show($"Error al cargar la imagen: {ex.Message}");
            }
        }
        /// <summary>
        /// Previene la inserción de texto no numérico por otros metodos
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PreviewTextoNumerico(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
        /// <summary>
        /// Previene que se usen teclas para insertar texto no numérico
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PreviewKeyNumerico(object sender, KeyEventArgs e)
        {
            //permitir teclas de control
            if (e.Key == Key.Back || e.Key == Key.Delete || e.Key == Key.Tab ||
                e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.Enter)
            {
                e.Handled = false;
            }
            else
            {
                // Evitar que se escriban caracteres no numéricos
                e.Handled = !(e.Key >= Key.D0 && e.Key <= Key.D9 || e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9);
            }
        }
        /// <summary>
        /// Función para cambiar el paso de la barra de progreso
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StepbarClick(object sender, MouseButtonEventArgs e)
        {
            var enviado = sender as StepBarItem;
            Pasos.StepIndex = enviado.Index - 1;

        }
        private bool HayCambios()
        {
            if (ItemActual.ID == string.Empty) { return false; }
            if (ItemActual is ItemAgenda agenda)
            {
                return agenda.Nombre != Nombre.Text ||
                       agenda.Descripción != Descripción.Text ||
                       agenda.Telefono != Telefono.Text ||
                       agenda.Correo != Correo.Text ||
                       agenda.PaginaWeb != PaginaWeb.Text ||
                       agenda.UbicaciónMaps != Ubicación.Text ||
                       agenda.TipoDeContacto != (TipoDeContacto.SelectedItem as ItemComboBox)?.Nombre;
            }
            else if (ItemActual is ItemInventario inventario)
            {
                return inventario.Nombre != Nombre.Text ||
                       inventario.Descripción != Descripción.Text ||
                       inventario.Cantidad.ToString() != Cantidad.Text ||
                       inventario.Unidad != Unidad.Text ||
                       inventario.Precio.ToString() != Costo.Text ||
                       inventario.Vendedor != (Proveedor.SelectedItem as ItemComboBox)?.ID;
            }
            else if (ItemActual is ItemProyecto proyecto)
            {
                return proyecto.Nombre != Nombre.Text ||
                       proyecto.Descripción != Descripción.Text ||
                       proyecto.Precio.ToString() != Total.Text ||
                       proyecto.Pagado.ToString() != Pagado.Text ||
                       proyecto.FechaCreación != Fecha.Text ||
                       proyecto.Cliente != (Cliente.SelectedItem as ItemComboBox)?.ID ||
                       proyecto.Progreso != Pasos.StepIndex * 25;
            }
            return false;
        }
        private void CargarEditables()
        {
            try
            {
                var carpetaProyecto = Environment.CurrentDirectory;
                var listaEditables = new ObservableCollection<ArchivoEditable>();

                if (ItemActual is ItemProyecto proyecto)
                {
                    // Crear carpeta del proyecto si no existe
                    var carpetaEditables = proyecto.CarpetaEditables;

                    if (!Directory.Exists(carpetaEditables))
                    {
                        Directory.CreateDirectory(carpetaEditables);
                    }

                    // Obtener archivos en la carpeta
                    var archivos = Directory.GetFiles(carpetaEditables);

                    foreach (var archivo in archivos)
                    {
                        var info = new FileInfo(archivo);

                        // Excluir directorios
                        if (info.Attributes.HasFlag(FileAttributes.Directory))
                            continue;

                        var editable = new ArchivoEditable
                        {
                            Ruta = archivo,
                            NombreArchivo = info.Name,
                            TipoArchivo = info.Extension.ToLower()
                        };

                        // Determinar el tipo de archivo y el icono
                        switch (info.Extension.ToLower())
                        {
                            case ".ai":
                                editable.Tipo = TipoEditable.Illustrator;
                                editable.IconoSource = CargarIcono("illustrator.png");
                                break;
                            case ".psd":
                                editable.Tipo = TipoEditable.Photoshop;
                                editable.IconoSource = CargarIcono("photoshop.png");
                                break;
                            default:
                                editable.Tipo = TipoEditable.Otro;
                                editable.IconoSource = CargarIcono("generic.png");
                                break;
                        }

                        listaEditables.Add(editable);
                    }

                    EditablesLista.ItemsSource = listaEditables;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al cargar los editables: {ex.Message}");
            }
        }
        private void CargarResultados()
        {
            try
            {
                var carpetaProyecto = Environment.CurrentDirectory;
                var listaResultados = new ObservableCollection<ArchivoResultado>();

                if (ItemActual is ItemProyecto proyecto)
                {
                    var archivos = Directory.GetFiles(proyecto.CarpetaMockups);
                    foreach (var archivo in archivos)
                    {
                        var info = new FileInfo(archivo);
                        // Solo considerar archivos de imagen
                        if (!esArchivoDeImagen(info.Extension))
                            continue;
                        var resultado = new ArchivoResultado
                        {
                            Ruta = archivo,
                            Nombre = info.Name,
                            Tipo = "Mockup: "
                        };
                        // Cargar imagen
                        try
                        {
                            BitmapImage bitmap = new BitmapImage();
                            bitmap.BeginInit();
                            bitmap.UriSource = new Uri(archivo);
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.EndInit();
                            resultado.ImagenSource = bitmap;
                        }
                        catch
                        {
                            // Si falla la carga de la imagen, usar imagen por defecto
                            BitmapImage fallbackBitmap = new BitmapImage();
                            fallbackBitmap.BeginInit();
                            fallbackBitmap.UriSource = new Uri("pack://application:,,,/404.png", UriKind.Absolute);
                            fallbackBitmap.EndInit();
                            resultado.ImagenSource = fallbackBitmap;
                        }
                        listaResultados.Add(resultado);
                    }

                    archivos = Directory.GetFiles(proyecto.CarpetaResultados);
                    foreach (var archivo in archivos)
                    {
                        var info = new FileInfo(archivo);
                        // Solo considerar archivos de imagen
                        if (!esArchivoDeImagen(info.Extension))
                            continue;
                        var resultado = new ArchivoResultado
                        {
                            Ruta = archivo,
                            Nombre = info.Name,
                            Tipo = "Resultado: "
                        };
                        // Cargar imagen
                        try
                        {
                            BitmapImage bitmap = new BitmapImage();
                            bitmap.BeginInit();
                            bitmap.UriSource = new Uri(archivo);
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.EndInit();
                            resultado.ImagenSource = bitmap;
                        }
                        catch
                        {
                            // Si falla la carga de la imagen, usar imagen por defecto
                            BitmapImage fallbackBitmap = new BitmapImage();
                            fallbackBitmap.BeginInit();
                            fallbackBitmap.UriSource = new Uri("pack://application:,,,/404.png", UriKind.Absolute);
                            fallbackBitmap.EndInit();
                            resultado.ImagenSource = fallbackBitmap;
                        }
                        listaResultados.Add(resultado);
                    }

                    ResultadosCarrusel.ItemsSource = listaResultados;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al cargar los resultados: {ex.Message}");
            }
        }
        private bool esArchivoDeImagen(string extension)
        {
            string[] formatosImagen = { ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".tiff" };
            return formatosImagen.Contains(extension.ToLower());
        }
        private ImageSource CargarIcono(string nombreArchivo)
        {
            try
            {
                var uri = new Uri($"pack://application:,,,/{nombreArchivo}", UriKind.Absolute);
                BitmapImage icon = new BitmapImage();
                icon.BeginInit();
                icon.UriSource = uri;
                icon.CacheOption = BitmapCacheOption.OnLoad;
                icon.EndInit();
                return icon;
            }
            catch
            {
                // Si falla, usar un icono genérico
                BitmapImage fallbackIcon = new BitmapImage();
                fallbackIcon.BeginInit();
                fallbackIcon.UriSource = new Uri("pack://application:,,,/404.png", UriKind.Absolute);
                fallbackIcon.EndInit();
                return fallbackIcon;
            }
        }
        private void AgregarEditable(object sender, RoutedEventArgs e)
        {
            //TODO: abrir selección de presets

        }
        private bool VerificarCamposObligatorios()
        {
            string mensaje = "";
            bool camposValidos = true;

            // Verificar campos generales que siempre son requeridos
            if (string.IsNullOrWhiteSpace(Nombre.Text))
            {
                mensaje += "- El nombre es obligatorio.\n";
                camposValidos = false;
            }

            // Verificar campos específicos según el tipo de entidad
            if (ItemActual is ItemAgenda)
            {
                // Verificar que el teléfono sea numérico
                if (!string.IsNullOrEmpty(Telefono.Text) && !decimal.TryParse(Telefono.Text, out _))
                {
                    mensaje += "- El teléfono debe ser un número válido.\n";
                    camposValidos = false;
                }

                // Verificar formato correcto de correo electrónico si no está vacío
                if (!string.IsNullOrEmpty(Correo.Text) && Correo.Text != "N/A")
                {
                    try
                    {
                        var addr = new System.Net.Mail.MailAddress(Correo.Text);
                        if (addr.Address != Correo.Text)
                        {
                            mensaje += "- El correo electrónico no tiene un formato válido.\n";
                            camposValidos = false;
                        }
                    }
                    catch
                    {
                        mensaje += "- El correo electrónico no tiene un formato válido.\n";
                        camposValidos = false;
                    }
                }

                // Verificar que se ha seleccionado un tipo de contacto
                if (TipoDeContacto.SelectedItem == null)
                {
                    mensaje += "- Debe seleccionar un tipo de contacto.\n";
                    camposValidos = false;
                }
            }
            else if (ItemActual is ItemInventario)
            {
                // Verificar campos numéricos
                if (!decimal.TryParse(Cantidad.Text, out _))
                {
                    mensaje += "- La cantidad debe ser un número válido.\n";
                    camposValidos = false;
                }

                if (!decimal.TryParse(Costo.Text, out _))
                {
                    mensaje += "- El costo debe ser un número válido.\n";
                    camposValidos = false;
                }

                // Verificar unidad
                if (string.IsNullOrWhiteSpace(Unidad.Text))
                {
                    mensaje += "- La unidad es obligatoria.\n";
                    camposValidos = false;
                }

                // Verificar que se ha seleccionado un proveedor
                if (Proveedor.SelectedItem == null)
                {
                    mensaje += "- Debe seleccionar un proveedor.\n";
                    camposValidos = false;
                }
            }
            else if (ItemActual is ItemProyecto)
            {
                // Verificar campos numéricos
                if (!decimal.TryParse(Total.Text, out _))
                {
                    mensaje += "- El precio total debe ser un número válido.\n";
                    camposValidos = false;
                }

                if (!decimal.TryParse(Pagado.Text, out _))
                {
                    mensaje += "- El monto pagado debe ser un número válido.\n";
                    camposValidos = false;
                }

                // Verificar fecha
                if (string.IsNullOrEmpty(Fecha.Text))
                {
                    mensaje += "- La fecha es obligatoria.\n";
                    camposValidos = false;
                }

                // Verificar que se ha seleccionado un cliente
                if (Cliente.SelectedItem == null)
                {
                    mensaje += "- Debe seleccionar un cliente.\n";
                    camposValidos = false;
                }
            }

            // Mostrar mensaje de error si hay campos inválidos
            if (!camposValidos)
            {
                HandyControl.Controls.MessageBox.Show(
                    $"Por favor corrija los siguientes errores:\n\n{mensaje}",
                    "Campos inválidos",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }

            return camposValidos;
        }
    }
}