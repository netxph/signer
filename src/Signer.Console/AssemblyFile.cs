using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Signer.Console
{
    public class AssemblyFile
    {

        public string FullName { get; private set; }
        public string ID { get; private set; }
        public string Version { get; private set; }

        public List<string> References { get; private set; }

        public AssemblyFile(string fullName)
        {
            FullName = fullName;
            ID = Path.GetFileNameWithoutExtension(fullName);

            Version = FileVersionInfo.GetVersionInfo(fullName).FileVersion;

            Initialize();
        }

        protected virtual void Initialize()
        {
            Assembly assembly = Assembly.LoadFile(FullName);
            References = new List<string>(from reference in assembly.GetReferencedAssemblies()
                                          select reference.Name);
        }

    }
}
