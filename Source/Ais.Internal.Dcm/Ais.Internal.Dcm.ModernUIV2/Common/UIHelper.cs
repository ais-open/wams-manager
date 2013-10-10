using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using FirstFloor.ModernUI.Windows.Controls;

namespace Ais.Internal.Dcm.ModernUIV2.Common
{
    /// <summary>
    /// This class helps to show error diaglogs and message
    /// </summary>
    class UIHelper
    {
        public static void HandlerException(Exception exp)
        {
            //Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, method: new Action(
            //                                    () =>
            //                                    {
            //                                        var windows = new ErrorWindow(exp);
            //                                        windows.ShowDialog();
            //                                    }));
        }

        public static bool? ShowMessage(string text, string title, MessageBoxButton button)
        {
            bool? dialogResult = null;
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, method: new Action(
                        () =>
                            {
                                MessageBoxResult result = ModernDialog.ShowMessage(text, title,button);
                                if (result == MessageBoxResult.OK)
                                {
                                    dialogResult = true;
                                }
                            }));
            return dialogResult;
        }
    }
}
