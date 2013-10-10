using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using FirstFloor.ModernUI.Windows.Controls;

namespace Ais.Internal.Dcm.ModernUI.Common
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

        public static bool? ShowMessage(string text, string title, MessageBoxButton button)
        {
            bool? dialogResult = null;
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, method: new Action(
                        () =>
                            {
                                dialogResult = ModernDialog.ShowMessage(text, title,button);
                                if (dialogResult.HasValue)
                                {
                                }
                                //var dlg = new ModernDialog
                                //{
                                //    Title = title,
                                //    Content = new BBCodeBlock { BBCode = text, Margin = new Thickness(0, 0, 0, 8) },
                                //    MinHeight = 0,
                                //    MinWidth = 0,
                                //    MaxHeight = 480,
                                //    MaxWidth = 640,
                                //};
                                //dlg.Buttons = GetButtons(dlg, button);

                                //dlg.ShowDialog();
                            }));
            return dialogResult;
        }
    }
}
