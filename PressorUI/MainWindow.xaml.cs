using Pressor.Logic;
using System.Windows;

namespace PressorUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var pp = new PressorParameters();
            Resources.Add("pp", pp);

        }
    }
}
