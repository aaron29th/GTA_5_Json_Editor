using PS3Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTA_5_json_editor
{
    class JobDetails
    {
        public static PS3API PS3 = mainForm.PS3;

        public static uint firstPropOffset()
        {
            return PS3.Extension.ReadUInt32(Variables.pointer) + 0x2F16C;
        }

        public static int type
        {
            get
            {
                return PS3.Extension.ReadInt32(firstPropOffset() - 0x1BFB8);
            }
            set
            {
                PS3.Extension.WriteInt32(firstPropOffset() - 0x1BFB8, value);
            }
        }

        public static int missionSubtype
        {
            get
            {
                return PS3.Extension.ReadInt32(firstPropOffset() - 0x1BFB4);
            }
            set
            {
                PS3.Extension.WriteInt32(firstPropOffset() - 0x1BFB4, value);
            }
        }

        public static string title
        {
            get
            {
                return PS3.Extension.ReadString(firstPropOffset() + 0x17214);
            }
            set
            {
                if (value.Length <= 25) PS3.Extension.WriteString(firstPropOffset() + 0x17214, value);
                else PS3.Extension.WriteString(firstPropOffset() + 0x17214, value.Substring(0, 25));
            }
        }

        public static string description
        {
            get
            {
                return PS3.Extension.ReadString(firstPropOffset() + 0x17270);
            }
            set
            {
                if (value.Length <= 500) PS3.Extension.WriteString(firstPropOffset() + 0x17270, value);
                else PS3.Extension.WriteString(firstPropOffset() + 0x17270, value.Substring(0, 500));
            }
        }

        public static int raceType
        {
            get
            {
                return PS3.Extension.ReadInt32(firstPropOffset() - 0x2228);
            }
            set
            {
                PS3.Extension.WriteInt32(firstPropOffset() - 0x2228, value);
            }
        }

        //Deathmatch only
        public static int deathmatchType
        {
            get
            {
                return PS3.Extension.ReadInt32(firstPropOffset() - 0x1BEF9);
            }
            set
            {
                PS3.Extension.WriteInt32(firstPropOffset() - 0x1BEF9, value);
            }
        }
    }
}
