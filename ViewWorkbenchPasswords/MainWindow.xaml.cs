using System;
using System.Collections.Generic;
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

namespace ViewWorkbenchPasswords
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            string filepath = string.Format("{0}{1}MySQL{1}Workbench{1}workbench_user_data.dat", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), System.IO.Path.DirectorySeparatorChar);
            bool exists = File.Exists(filepath);
            if (exists)
            {
                Output.Text = this.decrypt(filepath);
            }
            else
            {
                Output.Text = string.Format("Not found: {0}", filepath);
            }
        }

        private string decrypt(string filepath)
        {
            return DPAPILite.Decrypt(filepath);
        }
    }
}
