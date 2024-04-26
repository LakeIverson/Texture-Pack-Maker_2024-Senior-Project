using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;

namespace Senior_Project___Texture_Pack_Maker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class NewProject : Window
    {
        public string project = "";
        public bool quit = false;
        public NewProject()
        {
            InitializeComponent();
        }

        public void Save(object sender, RoutedEventArgs e)
        {
            project = ProjectName.Text;
            Close();
        }

        public void Close(object sender, RoutedEventArgs e)
        {
            quit = true;
            Close();
        }
    }
}
