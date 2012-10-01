using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Signer.Console
{
    public class SignHost
    {

        const string SN = @"C:\Program Files\Microsoft SDKs\Windows\v6.0A\Bin\sn.exe";
        const string ILDASM = @"C:\Program Files\Microsoft SDKs\Windows\v6.0A\bin\ildasm.exe";
        const string ILASM = @"C:\Windows\Microsoft.NET\Framework\v2.0.50727\ilasm.exe";

        public static void SignPath(SignerSetting setting)
        {
            if (string.IsNullOrEmpty(setting.KeyFile))
            {
                setting.KeyFile = generateKey();
            }

            ProcessContext context = new ProcessContext(setting.Path);
            ProcessContext.CurrentContext = context;

            string publicKey = getPublicKey(context);

            System.Console.WriteLine(string.Format("Public Key Generated:\r\n\r\n{0}", publicKey));
            System.Console.WriteLine();

            System.Console.WriteLine("Files to process:");

            foreach (var file in context.Files)
            {
                System.Console.WriteLine(file.ID);

                foreach (var reference in file.References)
                {
                    System.Console.WriteLine(string.Format("   >   {0}", reference));
                }

                System.Console.WriteLine("Decompiling...");
                decompile(file, publicKey);
                recompile(file);
            }

            System.Console.WriteLine("Done.");
        }

        private static void recompile(AssemblyFile file)
        {
            string path = Path.GetDirectoryName(file.FullName);
            if (!Directory.Exists(string.Format(@"{0}\out", path)))
            {
                Directory.CreateDirectory(string.Format(@"{0}\out", path));
            }
            
            string ilPath = string.Format(@"{0}\msil\{1}.il", path, file.ID);
            string resPath = string.Format(@"{0}\msil\{1}.res", path, file.ID);
            string extension = Path.GetExtension(file.FullName).Substring(1);

            Process process = new Process();
            process.StartInfo.FileName = ILASM;
            //process.StartInfo.UseShellExecute = false;
            //process.StartInfo.RedirectStandardOutput = true;

            process.StartInfo.Arguments = string.Format(@"/{0} /resource={1} /key=key.snk {2} /output={3}\out\{4}", extension, resPath, ilPath, path, Path.GetFileName(file.FullName));
            process.Start();

            process.WaitForExit();
        }

        private static void decompile(AssemblyFile file)
        {
            string path = Path.GetDirectoryName(file.FullName);

            if (!Directory.Exists(string.Format(@"{0}\msil", path)))
            {
                Directory.CreateDirectory(string.Format(@"{0}\msil", path));
            }

            Process process = new Process();
            process.StartInfo.FileName = ILDASM;
            //process.StartInfo.UseShellExecute = false;
            //process.StartInfo.RedirectStandardOutput = true;

            process.StartInfo.Arguments = string.Format(@"/all /out={0}\msil\{1}.il {2}", path, file.ID, file.FullName);
            process.Start();

            process.WaitForExit();
        }

        private static void decompile(AssemblyFile file, string publicKey)
        {
            decompile(file);

            rewrite(file, publicKey);
        }

        private static void rewrite(AssemblyFile file, string publicKey)
        {
            System.Console.WriteLine("Rewriting assembly...");

            string path = Path.GetDirectoryName(file.FullName);
            string ilPath = string.Format(@"{0}\msil\{1}.il", path, file.ID);

            string ilText = File.ReadAllText(ilPath);

            foreach (var reference in file.References)
            {
                string format = string.Format(@"\.assembly extern.*{0}\r\n{{\r\n.*", reference);

                Regex regex = new Regex(format);
                var match = regex.Match(ilText).Groups[0].Value;
                
                //remove version
                Regex verRegex = new Regex(@"\.ver .*");
                match = verRegex.Replace(match, "");

                var refFile = ProcessContext.CurrentContext.Files.FirstOrDefault(f => f.ID == reference);
                var version = refFile.Version.Replace('.', ':');

                if (!string.IsNullOrEmpty(match))
                {
                    match = match + string.Format(".publickeytoken = ({0} )\r\n  .ver {1}\r\n", publicKey, version);
                    ilText = regex.Replace(ilText, match);
                }
            }

            File.WriteAllText(ilPath, ilText);
        }

        private static string generateKey()
        {
            Process process = new Process();
            process.StartInfo.FileName = SN;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.Arguments = "-q -k key.snk";
            
            process.Start();

            process.WaitForExit();

            return "key.snk";
        }

        public static string getPublicKey(ProcessContext context)
        {
            //trial compile
            var file = context.Files[0];
            string path = Path.GetDirectoryName(file.FullName);

            decompile(file);
            recompile(file);

            Process process = new Process();
            process.StartInfo.FileName = SN;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;

            process.StartInfo.Arguments = string.Format(@"-T {0}\out\{1}", path, Path.GetFileName(file.FullName));

            process.Start();
            process.WaitForExit();

            string output = process.StandardOutput.ReadToEnd();

            string publicKey = output.Substring(output.Length - 18).Trim().ToUpper();

            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < publicKey.Length; i++)
            {
                builder.Append(publicKey[i]);

                if (i % 2 == 1)
                {
                    builder.Append(' ');
                }
            }

            publicKey = builder.ToString();

            return publicKey.Trim();

        }

    }
}
