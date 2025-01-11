using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace GestorX.Ventanas
{
    public partial class TextInputDialog : Window
    {
        public string Texto { get; set; }
        public TextInputDialog()
        {
            InitializeComponent();
            DataContext = this;
        }
        private void CerrarVentana(object sender, MouseButtonEventArgs e)
        {
            DialogResult = false;
            Close();
        }
        private void Guardar(object sender, MouseButtonEventArgs e)
        {
            Texto = Contenido.Text;
            DialogResult = true;
            Close();
        }
    }
}
