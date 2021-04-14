using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RtmpService.Pull
{   
    public class AACParser
    {
        private ADTSContext _adts;

        private const int ADTS_HEADER_SIZE = 7;

        public AACParser()
        {
            _adts = new ADTSContext();
        }

        public int aac_set_adts_head(byte[] buf, int size)
        {
            ADTSContext acfg = _adts;
            byte type;
            if (size < ADTS_HEADER_SIZE)
            {
                return -1;
            }
            buf[0] = 0xff;
            buf[1] = 0xf1;
            type = 0;
            type |= (byte)((acfg.objecttype & 0x03) << 6);
            type |= (byte)((acfg.sample_rate_index & 0x0f) << 2);
            type |= (byte)((acfg.channel_conf & 0x07) >> 2);
            buf[2] = type;
            type = 0;
            type |= (byte)((acfg.channel_conf & 0x07) << 6);
            type |= (byte)((ADTS_HEADER_SIZE + size) >> 11);
            buf[3] = type;
            type = 0;
            type |= (byte)((ADTS_HEADER_SIZE + size) >> 3);
            buf[4] = type;
            type = 0;
            type |= (byte)(((ADTS_HEADER_SIZE + size) & 0x7) << 5);
            type |= (0x7ff >> 6) & 0x1f;
            buf[5] = type;
            type = 0;
            type |= (0x7ff & 0x3f) << 2;
            buf[6] = type;

            return 0;

        }

        public int aac_decode_extradata(byte[] pbuf, int bufsize)
        {
            int offset = 2;
            int aot, aotext, samfreindex;
            int channelconfig;
            byte[] p = pbuf;
            if (_adts == null || p == null || bufsize < 2)
            {
                return -1;
            }
            aot = (p[0 + offset] >> 3) & 0x1f;
            if (aot == 31)
            {
                aotext = (p[0 + offset] << 3 | (p[1 + offset] >> 5)) & 0x3f;
                aot = 32 + aotext;
                samfreindex = (p[1 + offset] >> 1) & 0x0f;
                if (samfreindex == 0x0f)
                {
                    channelconfig = ((p[4 + offset] << 3) | (p[5 + offset] >> 5)) & 0x0f;
                }
                else
                {
                    channelconfig = ((p[1 + offset] << 3) | (p[2 + offset] >> 5)) & 0x0f;
                }
            }
            else
            {
                samfreindex = ((p[0 + offset] << 1) | p[1 + offset] >> 7) & 0x0f;
                if (samfreindex == 0x0f)
                {
                    channelconfig = (p[4 + offset] >> 3) & 0x0f;
                }
                else
                {
                    channelconfig = (p[1 + offset] >> 3) & 0x0f;
                }
            }
            //# ifdef AOT_PROFILE_CTRL
            if (aot < 2) aot = 2;
            //#endif
            _adts.objecttype = aot - 1;
            _adts.sample_rate_index = samfreindex;
            _adts.channel_conf = channelconfig;
            return 0;
        }
    }
}
