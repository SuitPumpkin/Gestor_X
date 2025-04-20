using GestorX.Ventanas;
using HandyControl.Tools.Extension;
using LiveCharts;
using LiveCharts.Helpers;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
    public partial class Inicio : UserControl
    {
        /// <summary>
        /// Referencia a la ventana principal
        /// </summary>
        private Principal _principal;
        public Inicio()
        {
            InitializeComponent();
            Cargarpendientes();
        }
        private void Cargarpendientes()
        {
            try
            {
                // Leer todas las líneas del archivo de pendientes
                if (File.Exists(Ubicaciones.Pendientes))
                {
                    var lineas = File.ReadAllLines(Ubicaciones.Pendientes);

                    foreach (var linea in lineas)
                    {
                        // Separar contenido y fecha por el delimitador "||"
                        var partes = linea.Split(new[] { "||" }, StringSplitOptions.None);
                        if (partes.Length == 2)
                        {
                            string Nota = partes[0];
                            string fechaCreacion = partes[1];

                            // Crear el contenedor de la nota (similar al método AñadirNota)
                            Border noteContainer = new Border
                            {
                                Background = System.Windows.Media.Brushes.White,
                                BorderBrush = System.Windows.Media.Brushes.Black,
                                BorderThickness = new Thickness(0, 1, 0, 1),
                                CornerRadius = new CornerRadius(5),
                                Margin = new Thickness(2),
                                Padding = new Thickness(5),
                                HorizontalAlignment = HorizontalAlignment.Stretch,
                                ToolTip = $"{Nota}\nCreado el: {fechaCreacion}"
                            };
                            Grid noteContent = new Grid
                            {
                                Margin = new Thickness(5)
                            };
                            noteContent.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                            noteContent.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                            noteContent.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                            TextBlock noteText = new TextBlock
                            {
                                Text = Nota,
                                FontSize = 16,
                                VerticalAlignment = VerticalAlignment.Center,
                                Margin = new Thickness(2)
                            };
                            Button deleteButton = new Button
                            {
                                Content = new FontAwesome.Sharp.IconBlock() { Icon = FontAwesome.Sharp.IconChar.Trash, FontSize = 14 },
                                Style = (Style)FindResource("NoteButtonStyle")
                            };
                            deleteButton.Click += (s, args) => {
                                NotesPanel.Children.Remove(noteContainer);
                                ActualizarPendientes();
                            };
                            Button editButton = new Button
                            {
                                Content = new FontAwesome.Sharp.IconBlock() { Icon = FontAwesome.Sharp.IconChar.Pencil, FontSize = 14 },
                                Style = (Style)FindResource("NoteButtonStyle")
                            };
                            editButton.Click += (s, args) =>
                            {
                                var editDialogo = new TextInputDialog()
                                {
                                    Owner = Window.GetWindow(this),
                                    Texto = noteText.Text
                                };
                                bool? editResultado = editDialogo.ShowDialog();
                                if (editResultado == true)
                                {
                                    noteText.Text = editDialogo.Texto;
                                    noteContainer.ToolTip = $"{editDialogo.Texto}\nCreado el: {fechaCreacion}";
                                    ActualizarPendientes();
                                }
                            };
                            noteContent.Children.Add(noteText);
                            Grid.SetColumn(noteText, 0);
                            noteContent.Children.Add(deleteButton);
                            Grid.SetColumn(deleteButton, 1);
                            noteContent.Children.Add(editButton);
                            Grid.SetColumn(editButton, 2);
                            noteContainer.Child = noteContent;
                            NotesPanel.Children.Add(noteContainer);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar pendientes: {ex.Message}");
            }
        }
        private void ActualizarPendientes()
        {
            try
            {
                var notas = new List<string>();

                foreach (UIElement child in NotesPanel.Children)
                {
                    if (child is Border noteContainer && noteContainer.ToolTip is string tooltip)
                    {
                        // Extraer el contenido y la fecha de la nota desde el ToolTip
                        var tooltipLineas = tooltip.Split('\n');
                        if (tooltipLineas.Length >= 2)
                        {
                            string contenido = tooltipLineas[0];
                            string fecha = tooltipLineas[1].Replace("Creado el: ", "");
                            notas.Add($"{contenido}||{fecha}");
                        }
                    }
                }

                // Guardar las notas en el archivo de forma asincrónica
                File.WriteAllLines(Ubicaciones.Pendientes, notas);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al actualizar pendientes: {ex.Message}");
            }
        }
        private void AñadirNota(object sender, RoutedEventArgs e)
        {
            var dialogo = new TextInputDialog()
            {
                Owner = Window.GetWindow(this)
            };
            bool? resultado = dialogo.ShowDialog();
            string Nota = "";
            string fechaCreacion = "";
            if (resultado == true)
            {
                Nota = dialogo.Texto;
                fechaCreacion = DateTime.Now.ToString("hh:mm tt dd/MM/yyyy");
            }
            else { return; }
            Border noteContainer = new Border
            {
                Background = System.Windows.Media.Brushes.White,
                BorderBrush = System.Windows.Media.Brushes.Black,
                BorderThickness = new Thickness(0,1,0,1),
                Margin = new Thickness(2),
                Padding = new Thickness(5),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                ToolTip = $"{Nota}\nCreado el: {fechaCreacion}"
            };
            Grid noteContent = new Grid
            {
                Margin = new Thickness(5)
            };
            noteContent.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            noteContent.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            noteContent.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            TextBlock noteText = new TextBlock
            {
                Text = Nota,
                FontSize = 16,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(2)
            };
            Button deleteButton = new Button
            {
                Content = new FontAwesome.Sharp.IconBlock() { Icon = FontAwesome.Sharp.IconChar.Trash, FontSize = 14 },
                Style = (Style)FindResource("NoteButtonStyle")
            };
            deleteButton.Click += (s, args) => {
                NotesPanel.Children.Remove(noteContainer);
                ActualizarPendientes();
            };
            Button editButton = new Button
            {
                Content = new FontAwesome.Sharp.IconBlock() { Icon = FontAwesome.Sharp.IconChar.Pencil, FontSize = 14 },
                Style = (Style)FindResource("NoteButtonStyle")
            };
            editButton.Click += (s, args) =>
            {
                var editDialogo = new TextInputDialog()
                {
                    Owner = Window.GetWindow(this),
                    Texto = noteText.Text
                };
                bool? editResultado = editDialogo.ShowDialog();
                if (editResultado == true)
                {
                    noteText.Text = editDialogo.Texto;
                    noteContainer.ToolTip = $"{editDialogo.Texto}\nCreado el: {fechaCreacion}";
                    ActualizarPendientes();
                }
            };
            noteContent.Children.Add(noteText);
            Grid.SetColumn(noteText, 0);
            noteContent.Children.Add(deleteButton);
            Grid.SetColumn(deleteButton, 1);
            noteContent.Children.Add(editButton);
            Grid.SetColumn(editButton, 2);
            noteContainer.Child = noteContent;
            NotesPanel.Children.Add(noteContainer);
            ActualizarPendientes();
        }
        public void CargarInfo()
        {
            MainWindow ventana = Window.GetWindow(this) as MainWindow;
            _principal = ventana.Contenido.Content as Principal;
            int p = 0, c = 0, t = 0;
            foreach (ItemAgenda item in _principal.Agenda)
            {
                if (item.TipoDeContacto == "Proveedor")
                {
                    p++;
                }
                if (item.TipoDeContacto == "Trabajador")
                {
                    t++;
                }
                if (item.TipoDeContacto == "Cliente")
                {
                    c++;
                }
            }
            P2.Text = $"x{p}";
            P1.Text = $"x{p}";
            C1.Text = $"x{c}";
            C2.Text = $"x{c}";
            T1.Text = $"x{t}";
            T2.Text = $"x{t}";
            I1.Text = $"x{_principal.Inventario.Count}";
            I2.Text = $"x{_principal.Inventario.Count}";
            S1.Text = $"x{_principal.Proyectos.Count}";
            S2.Text = $"x{_principal.Proyectos.Count}";
            var proyectos = 0;
            foreach (ItemProyecto item in _principal.Proyectos)
            {
                if (item.Progreso < 100)
                {
                    proyectos++;
                }
            }
            ProyectosIncompletos.Text = $"{proyectos}";
            var vi = 0.0;
            foreach (ItemInventario item in _principal.Inventario)
            {
                var valor = item.Precio * item.Cantidad;
                vi += valor;
            }
            ValorInventario.Text = $"${vi}";
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            CargarInfo();
        }
    }
}
