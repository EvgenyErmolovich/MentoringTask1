using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task1
{
    class Program
    {
        static void Main(string[] args)
        {
            string rootDir = @"C:\Users\Evgeny_Ermolovich\Desktop\Training\CreateDB";

            FileSystemVisitor a = new FileSystemVisitor(rootDir);
            a.Execute();
            
            foreach (var item in a)
            {
                Console.WriteLine(item);
            }

            Console.WriteLine("Press any key");
            Console.ReadKey();
        }
    }
}
