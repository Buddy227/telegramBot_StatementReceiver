using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace BotTelegramFB
{
    public class RequestToAdmin : IDisposable
    {
        public uint Stage { get; set; } = 1;
        public StringBuilder Report { get; set; } = new StringBuilder();

       
        bool disposed = false;
        SafeHandle handle = new SafeFileHandle(IntPtr.Zero, true);

        ~RequestToAdmin()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                handle.Dispose();
                Report.Clear();
            }

            disposed = true;
        }

        public string StageText()
        {
            Stage++;
            if (Stage == 4)
            {
                return "Опишите вашу проблему:";
            }
            else
            if (Stage == 3)
            {
                return "Введите где находитесь:";
            }
            else
            {
                return "Введите ваше имя:";
            }
        }
    }
}