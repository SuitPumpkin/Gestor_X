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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GestorX.Componentes
{
    /// <summary>
    /// Lógica de interacción para TextoConBorde.xaml
    /// </summary>
    public partial class TextoConBorde : UserControl
    {
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Texto), typeof(string), typeof(TextoConBorde),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public static readonly DependencyProperty ColorProperty1 =
            DependencyProperty.Register(nameof(ColorDeRelleno), typeof(Brush), typeof(TextoConBorde),
                new FrameworkPropertyMetadata(Brushes.Black, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public static readonly DependencyProperty ColorProperty2 =
            DependencyProperty.Register(nameof(ColorDeBorde), typeof(Brush), typeof(TextoConBorde),
                new FrameworkPropertyMetadata(Brushes.White, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public static readonly DependencyProperty ThicknessProperty =
            DependencyProperty.Register(nameof(Grosor), typeof(double), typeof(TextoConBorde),
                new FrameworkPropertyMetadata(3.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public static readonly DependencyProperty DoubleProperty =
            DependencyProperty.Register(nameof(Tamaño), typeof(Double), typeof(TextoConBorde),
                new FrameworkPropertyMetadata(12.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public static readonly DependencyProperty AlignmentProperty =
            DependencyProperty.Register(nameof(PosiciónHorizontal), typeof(HorizontalAlignment), typeof(TextoConBorde),
                new FrameworkPropertyMetadata(HorizontalAlignment.Left, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public static readonly DependencyProperty FontProperty =
            DependencyProperty.Register(nameof(Fuente), typeof(FontFamily), typeof(TextoConBorde),
                new FrameworkPropertyMetadata(new FontFamily("Montserrat"), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public string Texto
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }
        public Brush ColorDeRelleno
        {
            get => (Brush)GetValue(ColorProperty1);
            set => SetValue(ColorProperty1, value);
        }
        public Brush ColorDeBorde
        {
            get => (Brush)GetValue(ColorProperty2);
            set => SetValue(ColorProperty2, value);
        }
        public double Grosor
        {
            get => (double)GetValue(ThicknessProperty);
            set => SetValue(ThicknessProperty, value);
        }
        public Double Tamaño
        {
            get => (Double)GetValue(DoubleProperty);
            set => SetValue(DoubleProperty, value);
        }
        public HorizontalAlignment PosiciónHorizontal
        {
            get => (HorizontalAlignment)GetValue(AlignmentProperty);
            set => SetValue(AlignmentProperty, value);
        }
        public FontFamily Fuente
        {
            get => (FontFamily)GetValue(FontProperty);
            set => SetValue(FontProperty, value);
        }
        public TextoConBorde()
        {
            InitializeComponent();
        }
    }
}
