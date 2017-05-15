using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoutubeExtractor
{
    public enum AudioType
    {
        Aac,
        Mp3,
        Vorbis,


        /// <summary> 
        /// The audio type is unknown. This can occur if YoutubeExtractor is not up-to-date. 
        /// </summary> 
        Unknown

    }
}
