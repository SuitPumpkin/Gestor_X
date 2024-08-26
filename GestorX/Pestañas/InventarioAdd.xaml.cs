using HandyControl.Controls;
using HandyControl.Tools.Extension;
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
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static GestorX.Pestañas.Clases;
using static System.Windows.Forms.LinkLabel;

namespace GestorX.Pestañas
{
    public partial class InventarioAdd : UserControl
    {
        private bool ImagenSeleccionada = false;
        public ItemInventario Editado { get; set; } = new ItemInventario();
        private class ItemDeproveedor
        {
            public string Nombre { get; set; }
            public string Descripción { get; set; }
        }
        private ObservableCollection<ItemDeproveedor> ItemsDeprovedor { get; set; }
        public InventarioAdd()
        {
            InitializeComponent();
            CargarInfo();
        }
        private async void CargarInfo()
        {
            Tarjeta.RenderTransform = new ScaleTransform(0.1, 0.1);
            await Task.Delay(500);
            CargarDropDown();
            MainWindow ventana = System.Windows.Window.GetWindow(this) as MainWindow;
            Principal pri = ventana.Contenido.Content as Principal;
            Editado = pri.ItemDelInventario;
            Cantidad.Value = Editado.Cantidad;
            Unidad.Text = Editado.Unidad;
            Nombre.Text = Editado.Nombre;
            Costo.Value = Editado.PrecioCompra;
            Descripción.Text = Editado.Descripción.Replace("\\n", Environment.NewLine);
            foreach (ItemDeproveedor item in Proveedor.Items)
            {
                if (item.Nombre == Editado.Vendedor)
                {
                    Proveedor.SelectedItem = item;
                    break;
                }else if (Editado.Vendedor == "" && item.Nombre == "Desconocido")
                {
                    Proveedor.SelectedItem = item;
                }
            }
            if (Editado.ID != string.Empty)
            {
                IconoPrevio.Visibility = Visibility.Collapsed;
                try
                {
                    string ImagenCargada = System.IO.Path.Combine(Ubicaciones.Imagenes, $"I-{Editado.ID}.jpg");
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
        private void CargarDropDown()
        {
            MainWindow ventana = System.Windows.Window.GetWindow(this) as MainWindow;
            Principal pri = ventana.Contenido.Content as Principal;

            var items = new ObservableCollection<ItemDeproveedor>();

            foreach(ItemAgenda p in pri.Agenda)
            {
                if (p.TipoContacto == "Proveedor")
                {
                    items.Add(new ItemDeproveedor
                    {
                        Nombre = p.Nombre,
                        Descripción = p.Descripción.Replace("\\n", Environment.NewLine)
                    });
                }
            }
            items.Add(new ItemDeproveedor
            {
                Nombre = "Desconocido",
                Descripción = "Aún no se ha agregado ese proveedor a la agenda de la empresa"
            });

            ItemsDeprovedor = items;
            Proveedor.ItemsSource = ItemsDeprovedor.OrderBy(x => x.Nombre);
        }
        private void Guardar(object sender, MouseButtonEventArgs e)
        {
            var imagenuwu = "";
            if (ImagenSeleccionada)
            {
                GC.Collect(); //fuerza a que los recursos no utilizados se borren para que al cargar denuevo se vuelvan a pedir
                imagenuwu = Preview.Source.ToString().Remove(0, 8);
            }
            if (Nombre.Text != "" && Unidad.Text != "" && Descripción.Text != "")
            {
                ItemDeproveedor proveedorselecionado = Proveedor.SelectedItem as ItemDeproveedor;
                var proveedor = "Desconocido";
                if (proveedorselecionado != null)
                {
                    proveedor = proveedorselecionado.Nombre;
                }

                ItemInventario ITEM = new ItemInventario()
                {
                    Nombre = Nombre.Text.Replace(";", ""),
                    Descripción = Descripción.Text.Replace(";", "").Replace(Environment.NewLine, "\\n"),
                    Vendedor = proveedor,
                    PrecioCompra = float.Parse(Costo.Value.ToString()),
                    Unidad = Unidad.Text.Replace(";", ""),
                    Cantidad = int.Parse(Cantidad.Value.ToString()),
                };

                bool itemrepetido = false;

                MainWindow ventana = System.Windows.Window.GetWindow(this) as MainWindow;
                Principal pri = ventana.Contenido.Content as Principal;
                if (imagenuwu != "")
                {
                    ITEM.Imagen = imagenuwu;
                }
                foreach (ItemInventario item in pri.Inventario)
                {
                    if (item.Vendedor == ITEM.Vendedor && item.Nombre == ITEM.Nombre)
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
                            ITEM.Imagen = "\\\\suitpumpkin\\Trabajo\\Bases de Datos\\Imagenes\\InventarioBase.jpg";
                        }
                        ItemInventario.Create(ITEM, pri.Inventario);
                        pri.Subpestaña.Content = new Inventario();
                    }
                    else
                    {
                        HandyControl.Controls.MessageBox.Show($"{Nombre.Text} del proveedor {proveedorselecionado.Nombre} ya existe, revisalo antes de agregarlo");
                        Nombre.Focus();
                        Nombre.SelectAll();
                    }
                }
                else
                {
                    ITEM.ID = Editado.ID;
                    ItemInventario.Update(ITEM, pri.Inventario);
                    //salir
                    pri.Subpestaña.Content = new Inventario();
                }
            }
            else
            {
                string vacios;
                List<string> camposVacios = new List<string>();
                if (string.IsNullOrWhiteSpace(Nombre.Text))
                {
                    camposVacios.Add("Nombre");
                }
                if (string.IsNullOrWhiteSpace(Unidad.Text))
                {
                    camposVacios.Add("Unidad de Medida");
                }
                if (string.IsNullOrWhiteSpace(Descripción.Text))
                {
                    camposVacios.Add("Descripción");
                }
                vacios = string.Join(", ", camposVacios);
                HandyControl.Controls.MessageBox.Show($"Falta rellenar la info de: {vacios}.","Información incompleta", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        private void Duplicar(object sender, MouseButtonEventArgs e)
        {
            if (Nombre.Text != "" && Unidad.Text != "" && Descripción.Text != "")
            {
                ItemDeproveedor proveedorselecionado = Proveedor.SelectedItem as ItemDeproveedor;
                if (Editado.Nombre != Nombre.Text || Editado.Vendedor != proveedorselecionado.Nombre)
                {
                    MessageBoxResult respuesta = HandyControl.Controls.MessageBox.Show($"¿Guardar {Nombre.Text}({proveedorselecionado.Nombre}) como copia de {Editado.Nombre}({Editado.Vendedor})?", "Confirmación", MessageBoxButton.YesNo, MessageBoxImage.Information, MessageBoxResult.No);
                    if (respuesta == MessageBoxResult.Yes)
                    {
                        MainWindow ventana = System.Windows.Window.GetWindow(this) as MainWindow;
                        Principal pri = ventana.Contenido.Content as Principal;

                        var proveedor = "Desconocido";
                        if (proveedorselecionado != null)
                        {
                            proveedor = proveedorselecionado.Nombre;
                        }

                        ItemInventario ITEM = new ItemInventario()
                        {
                            Nombre = Nombre.Text.Replace(";", ""),
                            Descripción = Descripción.Text.Replace(";", "").Replace(Environment.NewLine, "\\n"),
                            Vendedor = proveedor,
                            PrecioCompra = float.Parse(Costo.Value.ToString()),
                            Unidad = Unidad.Text.Replace(";", ""),
                            Cantidad = int.Parse(Cantidad.Value.ToString()),
                        };

                        bool itemrepetido = false;

                        foreach (ItemInventario item in pri.Inventario)
                        {
                            if (item.Vendedor == ITEM.Vendedor && item.Nombre == ITEM.Nombre)
                            {
                                itemrepetido = true;
                            }
                        }

                        if (!itemrepetido)
                        {
                            ItemInventario.Create(ITEM, pri.Inventario);
                            pri.Subpestaña.Content = new Inventario();
                        }
                        else
                        {
                            HandyControl.Controls.MessageBox.Show($"{Nombre.Text} del proveedor {proveedorselecionado.Nombre} ya existe, revisalo antes de agregarlo");
                            Nombre.Focus();
                            Nombre.SelectAll();
                        }
                        //salir
                        pri.Subpestaña.Content = new Inventario();
                    }
                }
                else
                {
                    HandyControl.Controls.MessageBox.Show("Para duplicar el producto debes cambiar el Nombre o el Proveedor");
                }
            }
            else
            {
                HandyControl.Controls.MessageBox.Show($"Rellena todos los campos", "Información incompleta", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        private void Cancelar(object sender, MouseButtonEventArgs e)
        {
            MainWindow ventana = System.Windows.Window.GetWindow(this) as MainWindow;
            Principal pri = ventana.Contenido.Content as Principal;
            if (Editado.Nombre == Nombre.Text && Editado.Cantidad == Cantidad.Value && Editado.Descripción == Descripción.Text && Editado.Unidad == Unidad.Text) //significa que aun no se edita nada
            {
                pri.Subpestaña.Content = new Inventario();
            }
            else
            {
                MessageBoxResult respuesta = HandyControl.Controls.MessageBox.Show("¿Salir sin guardar?", "Confirmación", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
                if (respuesta == MessageBoxResult.Yes)
                {
                    pri.Subpestaña.Content = new Inventario();
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
            if (e.Key == Key.Back || e.Key == Key.Delete || e.Key == Key.Tab ||
                e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.Enter || e.Key == Key.Up || e.Key == Key.Down)
            {
                //permitir teclas de control
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
        private void RemoverDeInventario(object sender, MouseButtonEventArgs e)
        {
            MessageBoxResult respuesta = HandyControl.Controls.MessageBox.Show($"¿Seguro de querer eliminar toda la información relacionada a {Editado.Nombre}?", "Confirmación", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (respuesta == MessageBoxResult.Yes)
            {
                MainWindow ventana = System.Windows.Window.GetWindow(this) as MainWindow;
                Principal pri = ventana.Contenido.Content as Principal;
                ItemInventario.Delete(Editado,pri.Inventario);
                HandyControl.Controls.MessageBox.Show($"{Editado.Nombre} Eliminado correctamente");
                pri.Subpestaña.Content = new Inventario();
            }
            else if (respuesta == MessageBoxResult.No)
            {
                //no hacer nada xd
            }
        }
    }
}
