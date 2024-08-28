using HandyControl.Controls;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public partial class ProyectosAdd : UserControl
    {
        private bool ImagenSeleccionada = false;
        public ItemProyecto Editado { get; set; } = new ItemProyecto();
        private class ItemDeproyecto
        {
            public string Nombre { get; set; }
            public string Descripción { get; set; }
        }
        private ObservableCollection<ItemDeproyecto> Itemsproyectos { get; set; }
        public ProyectosAdd()
        {
            InitializeComponent();
            CargarInfo();
        }
        private async void CargarInfo()
        {
            Tarjeta.RenderTransform = new ScaleTransform(0.1, 0.1);
            await Task.Delay(500);
            CargarDropDown();
            MainWindow ventana = Window.GetWindow(this) as MainWindow;
            Principal pri = ventana.Contenido.Content as Principal;

            Editado = pri.ItemDeProyectos;
            Pagado.Text = Editado.Pagado.ToString();
            Total.Text = Editado.Precio.ToString();
            Pasos.StepIndex = (Editado.Progreso / 25);
            Descripción.Text = Editado.Descripción.Replace("\\n", Environment.NewLine);
            Nombre.Text = Editado.Nombre;
            Fecha.Text = Editado.FechaCreación;
            foreach (ItemDeproyecto item in Cliente.Items)
            {
                if (item.Nombre == Editado.Cliente)
                {
                    Cliente.SelectedItem = item;
                    break;
                }
                else if (Editado.Cliente == "" && item.Nombre == "Desconocido")
                {
                    Cliente.SelectedItem = item;
                }
            }
            if (Editado.ID != string.Empty)
            {
                IconoPrevio.Visibility = Visibility.Collapsed;
                try
                {
                    Preview.Source = Editado.ImagenReal;
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
        private void CargarDropDown()
        {
            MainWindow ventana = System.Windows.Window.GetWindow(this) as MainWindow;
            Principal pri = ventana.Contenido.Content as Principal;
            var items = new ObservableCollection<ItemDeproyecto>();

            foreach (ItemAgenda p in pri.Agenda)
            {
                if (p.TipoContacto == "Cliente")
                {
                    items.Add(new ItemDeproyecto
                    {
                        Nombre = p.Nombre,
                        Descripción = p.Descripción.Replace("\\n", Environment.NewLine)
                    });
                }
                if (p.TipoContacto == "Trabajador")
                {
                    items.Add(new ItemDeproyecto
                    {
                        Nombre = p.Nombre,
                        Descripción = p.Descripción.Replace("\\n", Environment.NewLine)
                    });
                }
            }
            items.Add(new ItemDeproyecto
            {
                Nombre = "Desconocido",
                Descripción = "Aún no se ha agregado ese proveedor a la agenda de la empresa"
            });
            Itemsproyectos = items;
            Cliente.ItemsSource = Itemsproyectos.OrderBy(x => x.Nombre);
        }
        private void Guardar(object sender, MouseButtonEventArgs e)
        {
            var imagenuwu = "";
            if (ImagenSeleccionada)
            {
                GC.Collect(); //fuerza a que los recursos no utilizados se borren para que al cargar denuevo se vuelvan a pedir
                imagenuwu = Preview.Source.ToString().Remove(0, 5).Replace("/","\\");
            }
            if (Total.Text != "" && Descripción.Text != "" && Nombre.Text != "")
            {
                var cliente = "Desconocido";
                if (Cliente.SelectedItem is ItemDeproyecto clienteseleccionado)
                {
                    cliente = clienteseleccionado.Nombre;
                }

                if (Pagado.Text == "") { Pagado.Text = "0.00"; }
                if (Fecha.Text == "") { Fecha.SelectedDate = DateTime.Now; }

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

                bool itemrepetido = false;

                MainWindow ventana = System.Windows.Window.GetWindow(this) as MainWindow;
                Principal pri = ventana.Contenido.Content as Principal;
                if (imagenuwu != "")
                {
                    ITEM.Imagen = imagenuwu;
                }
                foreach (ItemProyecto item in pri.Proyectos)
                {
                    if (item.Cliente == ITEM.Cliente && item.Nombre == ITEM.Nombre)
                    {
                        itemrepetido = true;
                    }
                }

                if (Editado.ID == string.Empty)
                {
                    if (!itemrepetido)
                    {
                        if (!ImagenSeleccionada && ITEM.Imagen == "")
                        {
                            ITEM.Imagen = "\\\\suitpumpkin\\Trabajo\\Bases de Datos\\Imagenes\\ProyectoBase.jpg";
                        }
                        ITEM.ID = Editado.ID;
                        ITEM.Carpeta = System.IO.Path.Combine(Ubicaciones.CarpetaProyectos, $"P-{Editado.ID}");
                        ITEM.CarpetaEditables = System.IO.Path.Combine(Ubicaciones.CarpetaProyectos, $"P-{Editado.ID}/Editables");
                        ITEM.CarpetaMockups = System.IO.Path.Combine(Ubicaciones.CarpetaProyectos, $"P-{Editado.ID}/Mockups");
                        ITEM.CarpetaResultados = System.IO.Path.Combine(Ubicaciones.CarpetaProyectos, $"P-{Editado.ID}/Resultados");
                        ItemProyecto.Create(ITEM, pri.Proyectos);
                        pri.Subpestaña.Content = new Proyectos();
                    }
                    else
                    {
                        HandyControl.Controls.MessageBox.Show($"El cliente {cliente} ya tiene un proyecto llamado {Nombre.Text}, revisalo antes de agregarlo");
                        Nombre.Focus();
                        Nombre.SelectAll();
                    }
                }
                else
                {
                    ITEM.ID = Editado.ID;
                    ItemProyecto.Update(ITEM, pri.Proyectos);
                    pri.Subpestaña.Content = new Proyectos();
                }
            }
            else
            {
                HandyControl.Controls.MessageBox.Show("Rellena todos los campos");
            }
        }
        private void Duplicar(object sender, MouseButtonEventArgs e)
        {
            if (Total.Text != "" && Descripción.Text != "" && Nombre.Text != "")
            {
                ItemDeproyecto clienteseleccionado = Cliente.SelectedItem as ItemDeproyecto;
                if (Editado.Nombre != Nombre.Text || Editado.Cliente != clienteseleccionado.Nombre)
                {
                    MessageBoxResult respuesta = HandyControl.Controls.MessageBox.Show($"¿Guardar {Nombre.Text} como copia de {Editado.Nombre}?", "Confirmación", MessageBoxButton.YesNo, MessageBoxImage.Information, MessageBoxResult.No);
                    if (respuesta == MessageBoxResult.Yes)
                    {
                        var cliente = "Desconocido";
                        if (clienteseleccionado != null)
                        {
                            cliente = clienteseleccionado.Nombre;
                        }

                        if (Pagado.Text == "") { Pagado.Text = "0.00"; }
                        if (Fecha.Text == "") { Fecha.SelectedDate = DateTime.Now; }

                        ItemProyecto ITEM = new ItemProyecto()
                        {
                            Nombre = Nombre.Text.Replace(";", ""),
                            Descripción = Descripción.Text.Replace(";", "").Replace(Environment.NewLine, "\\n"),
                            Progreso = Pasos.StepIndex * 25,
                            FechaCreación = Fecha.Text.Replace(",", ""),
                            Precio = float.Parse(Total.Text.Replace(",", "")),
                            Pagado = float.Parse(Pagado.Text.Replace(",", "")),
                        };

                        bool itemrepetido = false;

                        MainWindow ventana = System.Windows.Window.GetWindow(this) as MainWindow;
                        Principal pri = ventana.Contenido.Content as Principal;

                        foreach (ItemProyecto item in pri.Proyectos)
                        {
                            if (item.Cliente == ITEM.Cliente && item.Nombre == ITEM.Nombre)
                            {
                                itemrepetido = true;
                            }
                        }

                        if (!itemrepetido)
                        {
                            ItemProyecto.Create(ITEM, pri.Proyectos);
                            pri.Subpestaña.Content = new Inventario();
                        }
                        else
                        {
                            HandyControl.Controls.MessageBox.Show($"El cliente {cliente} ya tiene un proyecto llamado {Nombre.Text}, revisalo antes de agregarlo");
                            Nombre.Focus();
                            Nombre.SelectAll();
                        }
                        //salir
                        pri.Subpestaña.Content = new Proyectos();
                    }
                }
                else
                {
                    HandyControl.Controls.MessageBox.Show("Para crear una copia debes cambiar el nombre o el cliente");
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
            ItemDeproyecto clienteseleccionado = Cliente.SelectedItem as ItemDeproyecto;
            if (Editado.Nombre == Nombre.Text && Editado.Precio.ToString() == Total.Text && Editado.Descripción == Descripción.Text && Editado.Pagado.ToString() == Pagado.Text && Editado.Cliente == clienteseleccionado.Nombre) //significa que aun no se edita nada
            {
                pri.Subpestaña.Content = new Proyectos();
            }
            else
            {
                MessageBoxResult respuesta = HandyControl.Controls.MessageBox.Show("¿Salir sin guardar?", "Confirmación", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
                if (respuesta == MessageBoxResult.Yes)
                {
                    pri.Subpestaña.Content = new Proyectos();
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
                if (Editado.CarpetaResultados != null) { openFileDialog.InitialDirectory = $"{Editado.CarpetaResultados}"; }
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
        private void Remover(object sender, MouseButtonEventArgs e)
        {
            MessageBoxResult respuesta = HandyControl.Controls.MessageBox.Show($"¿Seguro de querer eliminar toda la información relacionada a {Editado.Nombre}?", "Confirmación", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (respuesta == MessageBoxResult.Yes)
            {
                MainWindow ventana = Window.GetWindow(this) as MainWindow;
                Principal pri = ventana.Contenido.Content as Principal;
                ItemProyecto.Delete(Editado, pri.Proyectos);
                HandyControl.Controls.MessageBox.Show($"{Editado.Nombre} Eliminado correctamente");
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
