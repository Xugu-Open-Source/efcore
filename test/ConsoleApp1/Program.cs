using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XuguClient;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            var test = new XGConnection();
            test.ConnectionString = "IP=127.0.0.1;DB=SYSTEM;User=SYSDBA;PWD=SYSDBA;Port=5138;AUTO_COMMIT=on;CHAR_SET=UTF8";
            test.Open();
            Console.WriteLine("123");
            Console.ReadLine();
        }
    }
}
