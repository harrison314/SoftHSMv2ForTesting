using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftHSMv2ForTesting
{
    internal class SoftHsmFolderStructure
    {
        public string ConfigFilePath
        {
            get;
            set;
        }

        public string UtilPath
        {
            get;
            set;
        }

        public string TokenPath
        {
            get;
            set;
        }

        public string LibFilderPath
        {
            get;
            set;
        }

        public string BasePath
        {
            get;
            set;
        }

        public SoftHsmFolderStructure()
        {

        }
    }
}
