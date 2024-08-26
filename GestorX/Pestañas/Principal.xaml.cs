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

namespace GestorX.Pestañas
{
    public partial class Principal : UserControl
    {
        public ObservableCollection<ItemAgenda> Agenda { get; } = ItemAgenda.Archivo.Leer();
        public ObservableCollection<ItemInventario> Inventario { get; } = ItemInventario.Archivo.Leer();
        public ObservableCollection<ItemProyecto> Proyectos { get; } = ItemProyecto.Archivo.Leer();

        public ItemInventario ItemDelInventario { get; set; } = new ItemInventario();
        public ItemAgenda ItemDeLaAgenda { get; set; } = new ItemAgenda();
        public ItemProyecto ItemDeProyectos { get; set; } = new ItemProyecto();
        public Principal()
        {
            InitializeComponent();
            Subpestaña.Content = new Inicio();
        }
        private void CambioPestaña(object sender, MouseButtonEventArgs e)
        {
            Border boton = sender as Border;
            string Pestaña = boton.Name;
            switch (Pestaña)
            {
                case "a":
                    Subpestaña.Content = new Inicio();
                    break;
                case "b":
                    Subpestaña.Content = new Inventario();
                    break;
                case "c":
                    Subpestaña.Content = new Proyectos();
                    break;
                case "d":
                    Subpestaña.Content = new Agenda();
                    break;
                case "e":
                    Subpestaña.Content = null; //catalogo
                    break;
                default:
                    Subpestaña.Content = null; //error
                    break;
            }
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Regex Agenda_ = new Regex(@"A-([^\\]+)\.jpg");
            Regex Inventario_ = new Regex(@"I-([^\\]+)\.jpg");
            Regex Proyectos_ = new Regex(@"P-([^\\]+)\.jpg");
            foreach (string imagen in ItemBase.ImagenesEnDesUso())
            {
                Match matcha = Agenda_.Match(imagen);
                Match matchi = Inventario_.Match(imagen);
                Match matchp = Proyectos_.Match(imagen);
                if (matcha.Success)
                {
                    var id = matcha.Groups[1].Value;
                    ItemAgenda p = ItemAgenda.Read(id, Agenda);
                    if (p == null)
                    {
                        File.Delete(imagen);
                        Console.WriteLine($"Id agenda: {id}");
                    }
                }
                if (matchi.Success)
                {
                    var id = matchi.Groups[1].Value;
                    ItemInventario p = ItemInventario.Read(id, Inventario);
                    if (p == null)
                    {
                        File.Delete(imagen);
                        Console.WriteLine($"Id inventario: {id}");
                    }
                }
                if (matchp.Success)
                {
                    var id = matchp.Groups[1].Value;
                    ItemProyecto p = ItemProyecto.Read(id, Proyectos);
                    if (p == null)
                    {
                        File.Delete(imagen);
                        Console.WriteLine($"Id proyectos: {id}");
                    }
                }
            }
        }
    }
}
