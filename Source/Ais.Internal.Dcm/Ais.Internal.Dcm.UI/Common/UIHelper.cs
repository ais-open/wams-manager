using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Ais.Internal.Dcm.UI.Common
{
    class UIHelper
    {
        public static void HandlerException(Exception exp)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, method: new Action(
                                                () =>
                                                    {
                                                        var windows = new ErrorWindow(exp);
                                                        windows.ShowDialog();
                                                    }));
            //ErrorWindow windows = new ErrorWindow(exp);
            //windows.ShowDialog();
        }
    }
}
