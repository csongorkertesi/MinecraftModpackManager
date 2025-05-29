using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMM
{
    public class Modpack(string name, string imagePath, List<string> mods)
    {
        public string name { get; set; } = name;
        public string imagePath { get; set; } = imagePath;
        public List<string> mods { get; set; } = mods;
    }
}
