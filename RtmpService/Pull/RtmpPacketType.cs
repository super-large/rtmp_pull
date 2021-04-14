using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RtmpService.Pull
{
    public class RtmpPacketType
    {
        public const byte RTMP_PACKET_TYPE_CHUNK_SIZE = 0x01;

        public const byte RTMP_PACKET_TYPE_BYTES_READ_REPORT = 0x03;
        public const byte RTMP_PACKET_TYPE_CONTROL = 0x04;
        public const byte RTMP_PACKET_TYPE_SERVER_BW = 0x05;
        public const byte RTMP_PACKET_TYPE_CLIENT_BW = 0x06;
        /*      RTMP_PACKET_TYPE_...                0x07 */
        public const byte RTMP_PACKET_TYPE_AUDIO = 0x08;
        public const byte RTMP_PACKET_TYPE_VIDEO = 0x09;
        /*      RTMP_PACKET_TYPE_...                0x0A */
        /*      RTMP_PACKET_TYPE_...                0x0B */
        /*      RTMP_PACKET_TYPE_...                0x0C */
        /*      RTMP_PACKET_TYPE_...                0x0D */
        /*      RTMP_PACKET_TYPE_...                0x0E */
        public const byte RTMP_PACKET_TYPE_FLEX_STREAM_SEND = 0x0F;
        public const byte RTMP_PACKET_TYPE_FLEX_SHARED_OBJECT = 0x10;
        public const byte RTMP_PACKET_TYPE_FLEX_MESSAGE = 0x11;
        public const byte RTMP_PACKET_TYPE_INFO = 0x12;
        public const byte RTMP_PACKET_TYPE_SHARED_OBJECT = 0x13;
        public const byte RTMP_PACKET_TYPE_INVOKE = 0x14;
        /*      RTMP_PACKET_TYPE_...                0x15 */
        public const byte RTMP_PACKET_TYPE_FLASH_VIDEO = 0x16;
    }
}
