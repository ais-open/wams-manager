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
using Ais.Internal.Dcm.ModernUIV2.ViewModels;
using Ais.Internal.Dcm.ModernUIV2.Common;

namespace Ais.Internal.Dcm.ModernUIV2
{
    /// <summary>
    /// Interaction logic for ErrorWindow.xaml
    /// </summary>
    public partial class ErrorWindow : Window
    {
        bool status = false;

        public ErrorWindow(Exception exp)
        {
            InitializeComponent();
            // this.DataContext = ;
            if (exp is WAMSException)
            {
                var exception = (WAMSException)exp;
                var v = new { Message = exception.WAMSMessage, Detail = exception.Detail };
                this.DataContext = v;
            }
            else
            {
                var v = new { Message = exp.Message, Detail = exp.ToString() };
                this.DataContext = v;
            }
            this.txtErrorDetail.Visibility = System.Windows.Visibility.Hidden;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            status = !status;
            if (status)
            {
                this.btnShowHideError.Content = Literals.MESSAGE_INFO_HIDE_DETAIL;// "Hide Detail";
                this.txtErrorDetail.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                this.btnShowHideError.Content = Literals.MESSAGE_INFO_SHOW_DETAIL;
                this.txtErrorDetail.Visibility = System.Windows.Visibility.Hidden;
            }
        }
    }
}
