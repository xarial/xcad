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
            //WpfPropertyManagerPageTest.TestPageBuilderAll();
            WpfPropertyManagerPageTest.TestPageBuilderSimple();

            while (true) 
            {
                System.Windows.Forms.Application.DoEvents();
            }
        }
    }
}
