using GestorX.Componentes;
using GestorX.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using static GestorX.Pestañas.ItemAdd;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using Path = System.IO.Path;

namespace GestorX.Pestañas
{
    /// <summary>
    /// Lógica de interacción para Ajustes.xaml
    /// </summary>
    public partial class Ajustes : UserControl
    {
        public Ajustes()
        {
            InitializeComponent();
            Cargar();
        }
        private void Cargar()
        {
            UbicaciónDeProyectos.Text = Settings.Default.UbicaciónProyectos;
        }
        private void CambiarUbicación(object sender, MouseButtonEventArgs e)
        {
            //abrir un explorador de ficheros
            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.Description = "Selecciona la ubicación de los proyectos";
            dialog.ShowNewFolderButton = true;
            dialog.SelectedPath = Settings.Default.UbicaciónProyectos;
            //procesar resultado
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                UbicaciónDeProyectos.Text = dialog.SelectedPath;
            }
            else
            {
                UbicaciónDeProyectos.Text = Settings.Default.UbicaciónProyectos;
            }
        }
        private void Guardar(object sender, MouseButtonEventArgs e)
        {
            var botón = sender as Componentes.Botón1;
            switch (botón.Tag)
            {
                case "1":
                    if (!string.IsNullOrEmpty(Settings.Default.UbicaciónProyectos))
                    {
                        try
                        {
                            Debug.WriteLine("----- Mudando el contenido de: " + Settings.Default.UbicaciónProyectos + "\n----- Al destino: " + UbicaciónDeProyectos.Text + "\n----\n---");
                            int archivos = 0;
                            int carpetas = 0;
                            foreach (string item in Directory.EnumerateFileSystemEntries(Settings.Default.UbicaciónProyectos))
                            {
                                string nombreItem = Path.GetFileName(item);
                                string destino = Path.Combine(UbicaciónDeProyectos.Text, nombreItem);

                                if (Directory.Exists(item))
                                {
                                    // Es una carpeta
                                    Debug.WriteLine("--- Moviendo la carpeta: " + item + "\n--- Al destino: " + destino + "\n--\n-");
                                    Directory.Move(item, destino);
                                }
                                else if (File.Exists(item))
                                {
                                    // Es un archivo
                                    Debug.WriteLine("--- Moviendo el archivo: " + item + "\n--- Al destino: " + destino + "\n--\n-");
                                    File.Move(item, destino);
                                }
                            }
                            Debug.WriteLine("----- "+ archivos +" archivos y "+ carpetas +" carpetas movidos con exito");
                            Settings.Default.UbicaciónProyectos = UbicaciónDeProyectos.Text;
                            Settings.Default.Save();
                            // mostrar un dialogo de que la ubicación se cambio con exito
                            HandyControl.Controls.MessageBox.Show("Ubicación actualizada y "+ archivos + " archivos y " + carpetas + " carpetas movidos con exito");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Error moviendo los proyectos: " + ex.Message);
                            UbicaciónDeProyectos.Text = Settings.Default.UbicaciónProyectos;
                            HandyControl.Controls.MessageBox.Show("Error: "+ ex.Message);
                        }
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
