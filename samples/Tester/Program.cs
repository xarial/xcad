using System;
using System.Threading.Tasks;
using System.Windows;

namespace Tester
{
    internal class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            WpfPropertyManagerPageTest.TestPageBuilder();

            while (true) 
            {
                System.Windows.Forms.Application.DoEvents();
            }
        }
    }
}
