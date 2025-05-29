using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MMM
{
    public class Config
    {
        public bool isValid { get; set; } = false;
        public string modpacksPath { get; set; } = string.Empty;
        public string modsPath { get; set; } = string.Empty;
        public string backupPath { get; set; } = string.Empty;
    }
}
