using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace builder
{
    public class PathHelper
    {
        private const string LEFT_OPU_NAME = "left";
        private const string RIGHT_OPU_NAME = "right";

        private const string LEFT_OPU_FILE = "OPULeft.xml";
        private const string RIGHT_OPU_FILE = "OPURight.xml";


        public const int NO_OPU = -1;
        public const int LEFT_OPU = 0;
        public const int RIGHT_OPU = 1;

        public static string getOPUFileName( string name ) {
            if (name.ToLower().Equals( LEFT_OPU_NAME) ) {
                return LEFT_OPU_FILE;
            }
            else if (name.ToLower().Equals(RIGHT_OPU_NAME) ) {
                return RIGHT_OPU_FILE;
            }
            return LEFT_OPU_NAME;
        }
    }
}
