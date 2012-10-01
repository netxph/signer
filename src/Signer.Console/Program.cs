using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Signer.Console
{
    class Program
    {
        static void Main(string[] args)
        {

            OptionParser parser = new OptionParser();
            var setting = parser.Parse(args);

            SignHost.SignPath(setting);

            System.Console.ReadLine();
        }
    }
}
