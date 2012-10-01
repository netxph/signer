using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Signer.Console
{
    public class ProcessContext
    {

        public ProcessContext(string path)
        {
            Initialize(path);
        }

        public static ProcessContext CurrentContext { get; set; }

        public List<AssemblyFile> Files { get; set; }

        public string WorkPath { get; set; }

        protected virtual void Initialize(string path)
        {
            WorkPath = Path.GetFullPath(path);

            var files = new List<string>(Directory.GetFiles(path, "*.dll"));
            files.AddRange(Directory.GetFiles(path, "*.exe"));

            Files = new List<AssemblyFile>();

            foreach (var file in files)
            {
                Files.Add(new AssemblyFile(file));
            }

            cleanUp();
        }

        private void cleanUp()
        {
            foreach (var file in Files)
            {

                //reverse search so that we can modify items in the collection
                for (int i = file.References.Count() - 1; i >= 0; i--)
                {
                    if(!Files.Exists(assembly => assembly.ID == file.References[i]))
                    {
                        file.References.RemoveAt(i);
                    }
                }
            }
        }

    }
}
