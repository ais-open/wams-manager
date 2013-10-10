using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Ais.Internal.Dcm.ModernUIV2.Common
{
    public static class IconHelper
    {
        public static Dictionary<string, BitmapImage> FileIconAssociation = new Dictionary<string, BitmapImage>();

        public static BitmapImage GetIconImageFromFilename(string FileName)
        {
            BitmapImage bmpImage = null;
            if (FileName == null)
            {
                bmpImage = GetDirectoryIcon();
            }
            else
            {
                string FileExtension = string.Empty;
                int IndexOfLastDot = FileName.LastIndexOf(".");
                if (IndexOfLastDot > 0)
                {
                    FileExtension = FileName.Substring(IndexOfLastDot + 1).ToLower();
                }
                if (!FileIconAssociation.TryGetValue(FileExtension, out bmpImage))
                {
                    IntPtr hImgSmall;    //the handle to the system image list
                    //IntPtr hImgLarge;    //the handle to the system image list
                    SHFILEINFO shinfo = new SHFILEINFO();
                    hImgSmall = Win32.SHGetFileInfo(FileName, 0,
                        ref shinfo, (uint)Marshal.SizeOf(shinfo),
                        Win32.SHGFI_USEFILEATTRIBUTES | Win32.SHGFI_ICON | Win32.SHGFI_SMALLICON | Win32.SHGFI_LINKOVERLAY);
                    System.Drawing.Icon myIcon = (System.Drawing.Icon)(System.Drawing.Icon.FromHandle(shinfo.hIcon).Clone());
                    Win32.DestroyIcon(shinfo.hIcon);
                    System.Drawing.Bitmap bmp = myIcon.ToBitmap();

                    MemoryStream strm = new MemoryStream();
                    bmp.Save(strm, System.Drawing.Imaging.ImageFormat.Png);

                    bmpImage = new BitmapImage();
                    bmpImage.BeginInit();
                    strm.Seek(0, SeekOrigin.Begin);
                    bmpImage.StreamSource = strm;
                    bmpImage.EndInit();

                    FileIconAssociation.Add(FileExtension, bmpImage);

                    //bmpImage.DecodePixelHeight = 40;
                    //bmpImage.DecodePixelWidth = 40;

                    int w = bmpImage.PixelWidth;
                    int h = bmpImage.PixelHeight;

                    double w1 = bmpImage.Width;
                    double h1 = bmpImage.Height;
                }
            }
            return bmpImage;
        }

        public static BitmapImage GetDirectoryIcon()
        {
            BitmapImage bmpImage = null;
            IntPtr hImgSmall;    //the handle to the system image list
            //IntPtr hImgLarge;    //the handle to the system image list
            SHFILEINFO shinfo = new SHFILEINFO();
            hImgSmall = Win32.SHGetFileInfo(null, 0,
                ref shinfo, (uint)Marshal.SizeOf(shinfo),
                Win32.SHGFI_SMALLICON | Win32.SHGFI_ICON | Win32.SHGFI_LINKOVERLAY);
            System.Drawing.Icon myIcon = (System.Drawing.Icon)(System.Drawing.Icon.FromHandle(shinfo.hIcon).Clone());
            Win32.DestroyIcon(shinfo.hIcon);
            System.Drawing.Bitmap bmp = myIcon.ToBitmap();

            MemoryStream strm = new MemoryStream();
            bmp.Save(strm, System.Drawing.Imaging.ImageFormat.Png);

            bmpImage = new BitmapImage();
            bmpImage.BeginInit(); strm.Seek(0, SeekOrigin.Begin);
            bmpImage.StreamSource = strm;
            bmpImage.EndInit();
            return bmpImage;
        }

        public struct SHFILEINFO
        {
            internal IntPtr hIcon;
            internal IntPtr iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        };
    }

    public class Win32
    {
        public const uint SHGFI_ICON = 0x000000100;
        public const uint SHGFI_DISPLAYNAME = 0x000000200;
        public const uint SHGFI_TYPENAME = 0x000000400;
        public const uint SHGFI_ATTRIBUTES = 0x000000800;
        public const uint SHGFI_ICONLOCATION = 0x000001000;
        public const uint SHGFI_EXETYPE = 0x000002000;
        public const uint SHGFI_SYSICONINDEX = 0x000004000;
        public const uint SHGFI_LINKOVERLAY = 0x000000000;// 0x000008000;
        public const uint SHGFI_SELECTED = 0x000010000;
        public const uint SHGFI_ATTR_SPECIFIED = 0x000020000;
        public const uint SHGFI_LARGEICON = 0x000000000;
        public const uint SHGFI_SMALLICON = 0x000000001;
        public const uint SHGFI_OPENICON = 0x000000002;
        public const uint SHGFI_SHELLICONSIZE = 0x000000004;
        public const uint SHGFI_PIDL = 0x000000008;
        public const uint SHGFI_USEFILEATTRIBUTES = 0x000000010;
        public const uint SHGFI_ADDOVERLAYS = 0x000000020;
        public const uint SHGFI_OVERLAYINDEX = 0x000000040;
        //  public const uint SHGFI_ICON = 0x100;
        //  public const uint SHGFI_LARGEICON = 0x0;    // 'Large icon
        //  public const uint SHGFI_SMALLICON = 0x1;    // 'Small icon
        public const uint ILD_TRANSPARENT = 0x1;

        public const int GWL_STYLE = (-16);

        public const UInt32 SWP_FRAMECHANGED = 0x0020;
        public const UInt32 SWP_NOSIZE = 0x0001;
        public const UInt32 SWP_NOMOVE = 0x0002;
        public const int WS_SYSMENU = 0x00080000;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass"), 
        DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        private static IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
        {
            if (IntPtr.Size == 8)
                return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);

            return new IntPtr(SetWindowLong32(hWnd, nIndex, dwNewLong.ToInt32()));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass"),
        DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        internal static extern IntPtr SHGetFileInfo(string pszPath,
         uint dwFileAttributes,
         ref IconHelper.SHFILEINFO psfi,
         uint cbSizeFileInfo,
         uint uFlags);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass"), 
        DllImport("user32")]
        internal static extern int DestroyIcon(IntPtr hIcon);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass"), 
        DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        private static extern int SetWindowLong32(IntPtr hWnd, int nIndex, int dwNewLong);
        
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass"), 
        System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Interoperability", "CA1400:PInvokeEntryPointsShouldExist"), 
        DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
        private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);
        
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass"), 
        DllImport("user32.dll", SetLastError = true)]
        internal static extern int SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass"), 
        DllImport("shell32.dll")]
        internal static extern int SHGetKnownFolderPath([MarshalAs(UnmanagedType.LPStruct)] Guid rfid, uint dwFlags, IntPtr hToken, out IntPtr pszPath);

    }

}
