using PS3Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTA_5_json_editor
{
    class menu
    {
        private static PS3API PS3 = mainForm.PS3;
        private static uint jobAddress(int jobNumber, bool savedJobs)
        {
            uint firstJobAddress = PS3.Extension.ReadUInt32(Variables.secondPointer) + 0x370E0;
            if (savedJobs) return firstJobAddress + 0xDA6C + ((uint)jobNumber * 0x12C);
            return firstJobAddress + ((uint)jobNumber * 0x12C);
        }

        public static string getJobId(int jobNumber, bool savedJobs)
        {
            return PS3.Extension.ReadString(jobAddress(jobNumber, savedJobs));
        }

        public static void setJobId(int jobNumber, string jobId, bool savedJobs)
        {
            PS3.Extension.WriteString(jobAddress(jobNumber, savedJobs), jobId);
            PS3.Extension.WriteString(jobAddress(jobNumber, savedJobs) + 0x30, "~y~GTA 5 JSON editor");
        }

        public static string getJobName(int jobNumber, bool savedJobs)
        {
            return PS3.Extension.ReadString(jobAddress(jobNumber, savedJobs) + 0x30);
        }
    }
}
