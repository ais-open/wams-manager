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
using FirstFloor.ModernUI.Windows;
using FirstFloor.ModernUI.Windows.Controls;
using System.IO;

namespace Ais.Internal.Dcm.ModernUI
{
    /// <summary>
    /// Interaction logic for PreviewWindow.xaml
    /// </summary>
    public partial class PreviewWindow : ModernDialog
    {
        public PreviewWindow()
        {
            InitializeComponent();
            webBrowser.Loaded += webBrowser_Loaded;
            this.Closing += PreviewWindow_Closing;
        }

        void PreviewWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Uri uri = new Uri("about:blank");
            webBrowser.Navigate(uri);
        }

        void webBrowser_Loaded(object sender, RoutedEventArgs e)
        {
            string curDir = Directory.GetCurrentDirectory();
            string path = curDir + "\\preview.html";
            Uri uri = new Uri(path);
            webBrowser.Navigate(uri);
        }
    }
}
