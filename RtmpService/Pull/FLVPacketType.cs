using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RtmpService.Pull
{
    class FLVPacketType
    {
        public const int F = 0x46;
        public const int L = 0x4c;
        public const int V = 0x56;

        /// <summary>
        /// avc i帧
        /// </summary>
        public const int AVC_I = 0x17;
        /// <summary>
        /// avc p帧
        /// </summary>
        public const int AVC_P = 0x27;

        public const int AVC_SEQUENCE_Header = 0x00;
        public const int AVC_NALU = 0x01;
    }
}
