using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GestorX.Componentes
{
    public partial class Botón1 : UserControl
    {
        public static readonly DependencyProperty IconTypeProperty =
            DependencyProperty.Register(nameof(Icono), typeof(string), typeof(Botón1),
                new FrameworkPropertyMetadata("None", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Texto), typeof(string), typeof(Botón1),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(nameof(Comando), typeof(ICommand), typeof(Botón1),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public string Icono
        {
            get => (string)GetValue(IconTypeProperty);
            set => SetValue(IconTypeProperty, value);
        }

        public string Texto
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public ICommand Comando
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        public Botón1()
        {
            InitializeComponent();
            MainBorder.MouseLeftButtonDown += (s, e) => Comando?.Execute(null);
        }
    }
} 