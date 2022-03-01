using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTA_5_json_editor
{
    public static class Variables
    {
        public static int version = 150;
        public static int lastestVersion = 150;

        // For per download binary fingerprinting
        public static string versionString = "aaaaaaaaaaAAAAAAAAAAaaaaaaaaaaAAAAAAAAAAaaaaaaaaaaAAAAAAAAAA";

        public static string versionCheckURL = "https://aaronrosser.xyz/projects/GTA5/PS3/GTA_5_JSON_Editor.json";
        public static string lastestDownloadLink = "";
        public static uint
            pointer = 0x1E70388,
            secondPointer = 0x1E7037C;
    }
}
