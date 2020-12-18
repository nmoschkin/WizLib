using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Runtime.InteropServices;

namespace WiZ.Helpers
{ 
    /// <summary>
    /// Console Log Tool
    /// </summary>
    public static class ConsoleHelper
    {
        /// <summary>
        /// Returns a value indicating that the console is open.
        /// </summary>
        public static bool HasConsole { get; private set; }

        [DllImport("kernel32", EntryPoint = "AllocConsole")]
        internal static extern int InternalAllocConsole();

        /// <summary>
        /// Allocate a console for the current application instance.
        /// </summary>
        /// <returns></returns>
        public static bool AllocConsole()
        {
            if (InternalAllocConsole() != 0)
            {
                HasConsole = true;
                return true;
            }
            else
            {
                HasConsole = false;
                return false;
            }
        }

        /// <summary>
        /// Log input to console with formatting.
        /// </summary>
        /// <param name="text">Log message.</param>
        /// <param name="localip">Local IP address.</param>
        /// <param name="remoteip">Remote IP address.</param>
        public static void LogInput(string text, string localip, string remoteip)
        {
            FormatLog(text, true, localip, remoteip);
        }

        /// <summary>
        /// Log input to console with formatting.
        /// </summary>
        /// <param name="text">Log message.</param>
        /// <param name="localip">Local IP address.</param>
        /// <param name="remoteip">Remote IP address.</param>
        public static void LogInput(string text, IPAddress localip, IPAddress remoteip)
        {
            FormatLog(text, true, localip, remoteip);
        }

        /// <summary>
        /// Log output to console with formatting.
        /// </summary>
        /// <param name="text">Log message.</param>
        /// <param name="localip">Local IP address.</param>
        /// <param name="remoteip">Remote IP address.</param>
        public static void LogOutput(string text, string localip, string remoteip)
        {
            FormatLog(text, false, localip, remoteip);
        }

        /// <summary>
        /// Log output to console with formatting.
        /// </summary>
        /// <param name="text">Log message.</param>
        /// <param name="localip">Local IP address.</param>
        /// <param name="remoteip">Remote IP address.</param>
        public static void LogOutput(string text, IPAddress localip, IPAddress remoteip)
        {
            FormatLog(text, false, localip, remoteip);
        }

        /// <summary>
        /// Log to console with formatting.
        /// </summary>
        /// <param name="text">Log message.</param>
        /// <param name="inp">Is input (as opposed to output)</param>
        /// <param name="localip">Local IP address.</param>
        /// <param name="remoteip">Remote IP address.</param>
        public static void FormatLog(string text, bool inp, IPAddress localip, IPAddress remoteip)
        {
            //if (localip == null)
            //{
            //    if (NetworkHelper.LocalAddress == null) NetworkHelper.RefreshDefaultIP();
            //    localip = NetworkHelper.LocalAddress;
            //}
            FormatLog(text, inp, localip?.ToString(), remoteip?.ToString());
        }

        /// <summary>
        /// Log to console with formatting.
        /// </summary>
        /// <param name="text">Log message.</param>
        /// <param name="inp">Is input (as opposed to output)</param>
        /// <param name="localip">Local IP address.</param>
        /// <param name="remoteip">Remote IP address.</param>
        public static void FormatLog(string text, bool inp, string localip, string remoteip)
        {
            //if (localip == null)
            //{
            //    if (NetworkHelper.LocalAddress == null) NetworkHelper.RefreshDefaultIP();
            //    localip = NetworkHelper.LocalAddress?.ToString();
            //}

            if (!HasConsole) return;

            var arrow = inp ? "<=" : "=>";
            var t = DateTime.Now.ToString("G");

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(t + ": ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"LOCAL: {localip}");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($" {arrow} ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write($"REMOTE: {remoteip}\r\n");

            //Console.WriteLine($"{t}: LOCAL: {localip} {arrow} REMOTE: {remoteip}");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(t + ": ");

            if (inp)
            {
                Console.ForegroundColor = ConsoleColor.Green;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
            }

            Console.WriteLine(text);
        }
    }
}
