using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Text;
using ConsoleTables;

class Program
{
    #region DLLs

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook,
        LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);


    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);


    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
        IntPtr wParam, IntPtr lParam);


    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    #endregion


    #region Variables
    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYDOWN = 0x0100;
    private static LowLevelKeyboardProc _proc = HookCallback;
    private static IntPtr _hookID = IntPtr.Zero;

    private static Keys[] openKeys = { Keys.Q, Keys.W, Keys.E, Keys.R, 
        Keys.T, Keys.Y, Keys.U, Keys.I, Keys.O, Keys.P, Keys.A, Keys.S, Keys.D, Keys.F,
        Keys.G, Keys.H, Keys.J, Keys.K, Keys.L, Keys.Z, Keys.X, Keys.C,
        Keys.V, Keys.B, Keys.N, Keys.M }; 
    private static StringBuilder Word = new StringBuilder("");
    private static IList<string> words = File.ReadAllText
        (@"D:\Codes\.NET_Projects\WordSearcher\1000words.txt").Split("\n");
    #endregion

    [STAThread]
    static void Main(string[] args)
    {
        _hookID = SetHook(_proc);
        Application.Run();
        UnhookWindowsHookEx(_hookID);
    }

    #region Functions

    #region Checker

    private static void Checker()
    {
        IList<string> res = words.Where(p => p.StartsWith(Word.ToString().ToLower())).ToList();
        string word = Word.ToString().ToLower();
        Console.ResetColor();
        Console.Clear();

        if (res.Count() > 1)
        {
            ConsoleTable table = new ConsoleTable("<:::WORDS:::>");
            foreach (string str in res)
                table.AddRow(str);
            table.Write();
        }
        else if (res.Count() == 1)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("You found this word => " + res[0]);
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("No!");
        }

        Console.WriteLine("\nYour word is: " + word + "\n");
    }

    #endregion

    #region SetHook

    private static IntPtr SetHook(LowLevelKeyboardProc proc)
    {
        using (Process curProcess = Process.GetCurrentProcess())
        using (ProcessModule curModule = curProcess.MainModule)
        {
            return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                GetModuleHandle(curModule.ModuleName), 0);
        }
    }

    #endregion

    private delegate IntPtr LowLevelKeyboardProc(
        int nCode, IntPtr wParam, IntPtr lParam);

    #region HookCallback

    private static IntPtr HookCallback(
        int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
        {
            int vkCode = Marshal.ReadInt32(lParam);
            if (openKeys.Contains((Keys)vkCode))
            {
                Word.Append((Keys)vkCode);
            }
            else if ((Keys)vkCode == Keys.Back && Word.Length > 0)
            {
                Word.Length -= 1;
            }
            Checker();
        }
        return CallNextHookEx(_hookID, nCode, wParam, lParam);
    }

    #endregion

    #endregion
}