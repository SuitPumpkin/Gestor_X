using HandyControl.Controls;
using Microsoft.Win32;
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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;
using Path = System.IO.Path;
using Window = System.Windows.Window;

namespace GestorX.Pestañas
{
    /// <summary>
    /// Componente para la edición o adición de items de proyectos
    /// </summary>
    public partial class ProyectosAdd : UserControl
    {
        /// <summary>
        /// Referencia a la ventana principal
        /// </summary>
        private Principal _principal;
        /// <summary>
        /// Almacena la información actual del item en caso de ser una edición y no una creación
        /// </summary>
        public ItemProyecto ItemActual { get; set; } = new ItemProyecto();
        /// <summary>
        /// Clase que representa un cliente en el sistema
        /// </summary>
        public class ItemDecliente
        {
            /// <summary>
            /// Nombre del cliente
            /// </summary>
            public string Nombre { get; set; }
            /// <summary>
            /// ID del cliente
            /// </summary>
            public string ID { get; set; }
            /// <summary>
            /// Descripción del cliente
            /// </summary>
            public string Descripción { get; set; }
        }
        /// <summary>
        /// Colección de clientes disponibles
        /// </summary>
        public ObservableCollection<ItemDecliente> ItemsDecliente { get; set; }
        /// <summary>
        /// Constructor del componente
        /// </summary>
        public ProyectosAdd()
        {
            InitializeComponent();
            CargarInfo();
        }
        /// <summary>
        /// Función para ejecutar las animaciones iniciales y la información necesaria
        /// </summary>
        private async void CargarInfo()
        {
            Tarjeta.RenderTransform = new ScaleTransform(0.1, 0.1);
            
            await Task.Delay(500);
            MainWindow ventana = Window.GetWindow(this) as MainWindow;
            _principal = ventana.Contenido.Content as Principal;
            CargarDropDown();

            ItemActual = _principal.ItemDeProyectos;
            Pagado.Text = ItemActual.Pagado.ToString();
            Total.Text = ItemActual.Precio.ToString();
            Pasos.StepIndex = (ItemActual.Progreso / 25);
            Descripción.Text = ItemActual.Descripción;
            Nombre.Text = ItemActual.Nombre;
            Fecha.Text = ItemActual.FechaCreación;
            foreach (ItemDecliente item in Cliente.Items)
            {
                if (item.ID == ItemActual.Cliente)
                {
                    Cliente.SelectedItem = item;
                    break;
                }
            }
            if (ItemActual.ID != string.Empty)
            {
                IconoPrevio.Visibility = Visibility.Collapsed;
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
            }
        }
        /// <summary>
        /// Función para cargar los clientes disponibles en el dropdown
        /// </summary>
        private void CargarDropDown()
        {
            var items = new ObservableCollection<ItemDecliente>();

            foreach (ItemAgenda p in _principal.Agenda)
            {
                items.Add(new ItemDecliente
                    {
                    Nombre = p.Nombre,
                    ID = p.ID,
                    Descripción = p.Descripción.Replace("\\n", Environment.NewLine)
                });
            }
            items.Add(new ItemDecliente
            {
                Nombre = "Desconocido",
                ID = "0",
                Descripción = "Aún no se ha agregado ese cliente a la agenda de la empresa"
            });
            ItemsDecliente = items;
            Cliente.ItemsSource = ItemsDecliente.OrderBy(x => x.Nombre);
        }
        /// <summary>
        /// Función para guardar la información del item en la base de datos
        /// </summary>
        /// <param name="sender">Objeto que desencadenó el evento</param>
        /// <param name="e">Argumentos del evento</param>
        private void Guardar(object sender, MouseButtonEventArgs e)
        {
            if (Nombre.Text != "" && Descripción.Text != "" && Cliente.SelectedItem != null)
            {
                //FASE 1: Preparar la información del objeto
                if (Pagado.Text == "") { Pagado.Text = "0.00"; }
                if (Fecha.Text == "") { Fecha.SelectedDate = DateTime.Now; }
                if (Total.Text == "") { Total.Text = "0.00"; }
                ItemDecliente clienteseleccionado = Cliente.SelectedItem as ItemDecliente;
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
                _principal.Subpestaña.Content = new Proyectos();
            }
            else
            {
                HandyControl.Controls.MessageBox.Show("Rellena todos los campos");
            }
        }
        /// <summary>
        /// Función para guardar un duplicado del item actual en la base de datos
        /// </summary>
        /// <param name="sender">Objeto que desencadenó el evento</param>
        /// <param name="e">Argumentos del evento</param>
        private void Duplicar(object sender, MouseButtonEventArgs e)
        {
            if (Nombre.Text != "" && Descripción.Text != "" && Cliente.SelectedItem != null)
            {
                if (ItemActual.Nombre != Nombre.Text)
                {
                    MessageBoxResult respuesta = HandyControl.Controls.MessageBox.Show($"¿Guardar {Nombre.Text} como copia de {ItemActual.Nombre}?", "Confirmación", MessageBoxButton.YesNo, MessageBoxImage.Information, MessageBoxResult.No);
                    if (respuesta == MessageBoxResult.Yes)
                    {
                        //FASE 1: Preparar la información del objeto
                        if (Pagado.Text == "") { Pagado.Text = "0.00"; }
                        if (Fecha.Text == "") { Fecha.SelectedDate = DateTime.Now; }
                        if (Total.Text == "") { Total.Text = "0.00"; }

                        ItemDecliente clienteseleccionado = Cliente.SelectedItem as ItemDecliente;
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
                        _principal.Subpestaña.Content = new Proyectos();
                    }
                }
                else
                {
                    HandyControl.Controls.MessageBox.Show("Para crear una copia debes cambiar el nombre");
                }
            }
            else
            {
                HandyControl.Controls.MessageBox.Show("Un proyecto necesita al menos un nombre y descripción");
            }
        }
        private void Cancelar(object sender, MouseButtonEventArgs e)
        {
            if (ItemActual.Nombre == Nombre.Text && ItemActual.Precio.ToString() == Total.Text && ItemActual.Descripción == Descripción.Text && ItemActual.Pagado.ToString() == Pagado.Text) //significa que aun no se edita nada
            {
                _principal.Subpestaña.Content = new Proyectos();
            }
            else
            {
                MessageBoxResult respuesta = HandyControl.Controls.MessageBox.Show("¿Salir sin guardar?", "Confirmación", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
                if (respuesta == MessageBoxResult.Yes)
                {
                    _principal.Subpestaña.Content = new Proyectos();
                }
            }
        }
        private void PreviewTextoNumerico(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
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
        private void Remover(object sender, MouseButtonEventArgs e)
        {
            MessageBoxResult respuesta = HandyControl.Controls.MessageBox.Show($"¿Seguro de querer eliminar toda la información relacionada a {ItemActual.Nombre}?", "Confirmación", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (respuesta == MessageBoxResult.Yes)
            {
                MainWindow ventana = Window.GetWindow(this) as MainWindow;
                Principal pri = ventana.Contenido.Content as Principal;
                ItemProyecto.Delete(ItemActual.ID);
                HandyControl.Controls.MessageBox.Show($"{ItemActual.Nombre} Eliminado correctamente");
                pri.Subpestaña.Content = new Proyectos();
            }
            else if (respuesta == MessageBoxResult.No)
            {
                //no hacer nada xd
            }
        }
        private void StepbarClick(object sender, MouseButtonEventArgs e)
        {
            var enviado = sender as StepBarItem;
            Pasos.StepIndex = enviado.Index - 1;

        }
    }
}
