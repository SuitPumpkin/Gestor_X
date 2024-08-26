using HandyControl.Tools.Extension;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
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
    public partial class AgendaAdd : UserControl
    {
        private bool ImagenSeleccionada = false;
        public ItemAgenda Editado { get; set; } = new ItemAgenda();
        public AgendaAdd()
        {
            InitializeComponent();
            CargarInfo();
        }
        private async void CargarInfo()
        {
            Tarjeta.RenderTransform = new ScaleTransform(0.1,0.1);

            await Task.Delay(500);
            MainWindow ventana = Window.GetWindow(this) as MainWindow;
            Principal pri = ventana.Contenido.Content as Principal;

            Editado = pri.ItemDeLaAgenda;
            Correo.Text = Editado.Correo;
            Telefono.Text = Editado.Telefono;
            PaginaWeb.Text = Editado.PaginaWeb;
            Descripción.Text = Editado.Descripción.Replace("\\n", Environment.NewLine);
            Nombre.Text = Editado.Nombre;
            Ubicación.Text = Editado.Ubicación;
            foreach (ComboBoxItem item in TipoContacto.Items)
            {
                if (item.Content.ToString() == Editado.TipoContacto)
                {
                    TipoContacto.SelectedItem = item;
                    break;
                }
            }
            if (Editado.ID != string.Empty)
            {
                IconoPrevio.Visibility = Visibility.Collapsed;
                try
                {
                    string ImagenCargada = System.IO.Path.Combine(Ubicaciones.Imagenes, $"A-{Editado.ID}.jpg");
                    //usa la imagen en la carpeta temp
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad; // Cargar la imagen completamente antes de cerrar el archivo
                    bitmap.UriSource = new Uri(ImagenCargada);
                    bitmap.EndInit();
                    Preview.Source = bitmap;
                }
                catch
                {
                    // En caso de que el enlace no sea válido, asignar una imagen predeterminada
                    BitmapImage fallbackBitmap = new BitmapImage();
                    fallbackBitmap.BeginInit();
                    fallbackBitmap.UriSource = new Uri("pack://application:,,,/404.png", UriKind.Absolute);
                    fallbackBitmap.EndInit();
                    Preview.Source = fallbackBitmap;
                }
                Duplicador.Visibility = Visibility.Visible;
                Borrador.Visibility = Visibility.Visible;
            }
        }
        private void Guardar(object sender, MouseButtonEventArgs e)
        {
            var imagenuwu = "";
            if (ImagenSeleccionada)
            {
                GC.Collect(); //fuerza a que los recursos no utilizados se borren para que al cargar denuevo se vuelvan a pedir
                imagenuwu = Preview.Source.ToString().Remove(0,8);
            }
            if (Telefono.Text != "" && Correo.Text != "" && PaginaWeb.Text != "" && Nombre.Text != "")
            {
                ItemAgenda ITEM = new ItemAgenda()
                {
                    Nombre = Nombre.Text.Replace(";", ""),
                    Descripción = Descripción.Text.Replace(";", "").Replace(Environment.NewLine, "\\n"),
                    Telefono = Telefono.Text.Replace(";", ""),
                    TipoContacto = TipoContacto.Text,
                    Correo = Correo.Text.Replace(";", ""),
                    PaginaWeb = PaginaWeb.Text.Replace(";", ""),
                    Ubicación = Ubicación.Text.Replace(";", ""),
                };

                MainWindow ventana = Window.GetWindow(this) as MainWindow;
                Principal pri = ventana.Contenido.Content as Principal;
                if (imagenuwu != "")
                {
                    ITEM.Imagen = imagenuwu;
                }
                if (Editado.ID == string.Empty)
                {
                    if (!ImagenSeleccionada && ITEM.Imagen == "")
                    {
                        ITEM.Imagen = "\\\\suitpumpkin\\Trabajo\\Bases de Datos\\Imagenes\\AgendaBase.jpg";
                    }
                    ItemAgenda.Create(ITEM, pri.Agenda);
                }
                else
                {
                    ITEM.ID = Editado.ID;
                    ItemAgenda.Update(ITEM, pri.Agenda);
                }
                //salir
                pri.Subpestaña.Content = new Agenda();
            }
            else
            {
                HandyControl.Controls.MessageBox.Show("Rellena todos los campos");
            }
        }
        private void Duplicar(object sender, MouseButtonEventArgs e)
        {
            if (Telefono.Text != "" && Correo.Text != "" && PaginaWeb.Text != "" && Nombre.Text != "")
            {
                if (Editado.Nombre != Nombre.Text)
                {
                    MessageBoxResult respuesta = HandyControl.Controls.MessageBox.Show($"¿Guardar {Nombre.Text} como copia de {Editado.Nombre}?", "Confirmación", MessageBoxButton.YesNo, MessageBoxImage.Information, MessageBoxResult.No);
                    if (respuesta == MessageBoxResult.Yes)
                    {
                        ItemAgenda ITEM = new ItemAgenda()
                        {
                            Nombre = Nombre.Text.Replace(";", ""),
                            Descripción = Descripción.Text.Replace(";", "").Replace(Environment.NewLine, "\\n"),
                            Telefono = Telefono.Text.Replace(";", ""),
                            TipoContacto = TipoContacto.Text,
                            Correo = Correo.Text.Replace(";", ""),
                            PaginaWeb = PaginaWeb.Text.Replace(";", ""),
                            Ubicación = Ubicación.Text.Replace(";", ""),
                        };

                        MainWindow ventana = Window.GetWindow(this) as MainWindow;
                        Principal pri = ventana.Contenido.Content as Principal;

                        ItemAgenda.Create(ITEM,pri.Agenda);


                        //salir
                        pri.Subpestaña.Content = new Agenda();
                    }
                }
                else
                {
                    HandyControl.Controls.MessageBox.Show("Para crear una copia debes cambiar el nombre");
                }
            }
            else
            {
                HandyControl.Controls.MessageBox.Show("Rellena todos los campos");
            }
        }
        private void Cancelar(object sender, MouseButtonEventArgs e)
        {
            MainWindow ventana = Window.GetWindow(this) as MainWindow;
            Principal pri = ventana.Contenido.Content as Principal;
            if (Editado.Nombre == Nombre.Text && Editado.Correo.ToString() == Correo.Text && Editado.Descripción == Descripción.Text && Editado.PaginaWeb == PaginaWeb.Text && Editado.Telefono == Telefono.Text) //significa que aun no se edita nada
            {
                pri.Subpestaña.Content = new Agenda();
            }
            else
            {
                MessageBoxResult respuesta = HandyControl.Controls.MessageBox.Show("¿Salir sin guardar?", "Confirmación", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
                if (respuesta == MessageBoxResult.Yes)
                {
                    pri.Subpestaña.Content = new Agenda();
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
        private void Preview_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ImagenSeleccionada = true;
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Filter = "Archivos de imagen|*.png;*.jpg;*.jpeg|Todos los archivos|*.*"
                };
                if (openFileDialog.ShowDialog() == true)
                {
                    string filePath = openFileDialog.FileName;

                    // Cargar la imagen en el Image control
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
        private void RemoverDeAgenda(object sender, MouseButtonEventArgs e)
        {
            MessageBoxResult respuesta = HandyControl.Controls.MessageBox.Show($"¿Seguro de querer eliminar toda la información relacionada a {Editado.Nombre}?", "Confirmación", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (respuesta == MessageBoxResult.Yes)
            {
                MainWindow ventana = Window.GetWindow(this) as MainWindow;
                Principal pri = ventana.Contenido.Content as Principal;

                ItemAgenda.Delete(Editado, pri.Agenda);

                HandyControl.Controls.MessageBox.Show($"{Editado.Nombre} Eliminado correctamente");
                pri.Subpestaña.Content = new Agenda();
            }
        }
    }
}
