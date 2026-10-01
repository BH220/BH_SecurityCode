using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH_SecurityCode.Api.Model
{
    public class FileModel
    {
        public string Extension { get; set; }
        public string NameWithoutExt { get; set; }
        public string Name { get { return $"{NameWithoutExt}{Extension}"; } }
        public string Directory { get; set; }
        public string FullPath
        {
            get
            {
                return Path.Combine(Directory, Name);
            }
        }
        public string Hash { get; set; }
        public string ClientSavePath { get; set; }

        public FileModel()
        {
        }

        public FileModel(string directory, string nameWithoutExt, string extension, string hash)
        {
            Directory = directory;
            NameWithoutExt = nameWithoutExt;
            Extension = extension;
            Hash = hash;
        }
    }
}
