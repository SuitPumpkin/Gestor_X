using HandyControl.Tools.Extension;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
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
using static System.Net.Mime.MediaTypeNames;
namespace GestorX.Pestañas
{
    /// <summary>
    /// Componente para la edición o adición de items a la agenda
    /// </summary>
    public partial class AgendaAdd : UserControl
    {
        /// <summary>
        /// Referencia a la ventana principal
        /// </summary>
        private Principal _principal;
        /// <summary>
        /// Almacena la información actual del item en caso de ser una edición y no una creación
        /// </summary>
        public ItemAgenda ItemActual { get; set; } = new ItemAgenda();
        /// <summary>
        /// Constructor del componente
        /// </summary>
        public AgendaAdd()
        {
            InitializeComponent();
            CargarInfo();
        }
        /// <summary>
        /// Función para ejecutar las animaciones iniciales y la información necesaria
        /// </summary>
        private async void CargarInfo()
        {
            Tarjeta.RenderTransform = new ScaleTransform(0.1,0.1);

            await Task.Delay(500);
            MainWindow ventana = Window.GetWindow(this) as MainWindow;
            _principal = ventana.Contenido.Content as Principal;

            ItemActual = _principal.ItemDeLaAgenda;
            Correo.Text = ItemActual.Correo;
            Telefono.Text = ItemActual.Telefono;
            PaginaWeb.Text = ItemActual.PaginaWeb;
            Descripción.Text = ItemActual.Descripción;
            Nombre.Text = ItemActual.Nombre;
            Ubicación.Text = ItemActual.UbicaciónMaps;
            foreach (ComboBoxItem item in TipoContacto.Items)
            {
                if (item.Content.ToString() == ItemActual.TipoDeContacto)
                {
                    TipoContacto.SelectedItem = item;
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
        /// Función para guardar la información de un item en la base de datos
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Guardar(object sender, MouseButtonEventArgs e)
        {
            if (Descripción.Text != "" && Nombre.Text != "")
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
                    TipoDeContacto = TipoContacto.Text,
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
                _principal.Subpestaña.Content = new Agenda();
            }
            else
            {
                HandyControl.Controls.MessageBox.Show("Un contacto necesita al menos un nombre y descripción");
            }
        }
        /// <summary>
        /// Función para guardar un duplicado del item actual en la base de datos
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Duplicar(object sender, MouseButtonEventArgs e)
        {
            if (Descripción.Text != "" && Nombre.Text != "")
            {
                if (ItemActual.Nombre != Nombre.Text)
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
                            TipoDeContacto = TipoContacto.Text,
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
                        _principal.Subpestaña.Content = new Agenda();
                    }
                }
                else
                {
                    HandyControl.Controls.MessageBox.Show("Para crear una copia debes cambiar el nombre");
                }
            }
            else
            {
                HandyControl.Controls.MessageBox.Show("Un contacto necesita al menos un nombre y descripción");
            }
        }
        /// <summary>
        /// Función para salir de la interfáz actual sin guardar los cambios
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Cancelar(object sender, MouseButtonEventArgs e)
        {
            if (ItemActual.Nombre == Nombre.Text && ItemActual.Correo.ToString() == Correo.Text && ItemActual.Descripción == Descripción.Text && ItemActual.PaginaWeb == PaginaWeb.Text && ItemActual.Telefono == Telefono.Text) //significa que aun no se edita nada
            {
                _principal.Subpestaña.Content = new Agenda();
            }
            else
            {
                MessageBoxResult respuesta = HandyControl.Controls.MessageBox.Show("¿Salir sin guardar?", "Confirmación", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
                if (respuesta == MessageBoxResult.Yes)
                {
                    _principal.Subpestaña.Content = new Agenda();
                }
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
        /// Función para remover de la bsae de datos el item actual.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RemoverDeAgenda(object sender, MouseButtonEventArgs e)
        {
            MessageBoxResult respuesta = HandyControl.Controls.MessageBox.Show($"¿Seguro de querer eliminar toda la información relacionada a {ItemActual.Nombre}?", "Confirmación", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (respuesta == MessageBoxResult.Yes)
            {
                ItemAgenda.Delete(ItemActual.ID);

                HandyControl.Controls.MessageBox.Show($"{ItemActual.Nombre} Eliminado correctamente");
                _principal.Subpestaña.Content = new Agenda();
            }
        }
        /// <summary>
        /// Previene que se usen teclas para insertar texto no numérico
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PreviewKeyNumerico(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Back || e.Key == Key.Delete || e.Key == Key.Tab || e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.Enter)
            {
                e.Handled = false;
            }
            else
            {
                e.Handled = !(e.Key >= Key.D0 && e.Key <= Key.D9 || e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9);
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
    }
}
