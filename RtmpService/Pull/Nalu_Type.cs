using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RtmpService.Pull
{
    public class Nalu_Type
    {
        public const int NALU_TYPE_SLICE = 1;

        public const int NALU_TYPE_DPA = 2;

        public const int NALU_TYPE_DPB = 3;

        public const int NALU_TYPE_DPC = 4;

        public const int NALU_TYPE_IDR = 5;

        public const int NALU_TYPE_SEI = 6;

        public const int NALU_TYPE_SPS = 7;

        public const int NALU_TYPE_PPS = 8;

        public const int NALU_TYPE_AUD = 9;//访问分隔符

        public const int NALU_TYPE_EOSEQ = 10;

        public const int NALU_TYPE_EOSTREAM = 11;

        public const int NALU_TYPE_FILL = 12;
    }
}
