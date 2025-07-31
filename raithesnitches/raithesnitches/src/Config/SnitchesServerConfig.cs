using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;

namespace raithesnitches.src.Config
{
    [JsonObject()]
    public class SnitchesServerConfig
    {
        [JsonIgnore]
        private ICoreAPI sapi;

        [JsonProperty]
        public bool snitchSneakable = false;
        [JsonProperty]
        public int snitchRadius = 16;
        [JsonProperty]
        public int snitchVert = 16;
        [JsonProperty]
        public float snitchTruesightRange = 0.5f;
        [JsonProperty]
        public int maxSnitchLog = 35;

        [JsonProperty]
        public int maxBookLog = 100;
        [JsonProperty]
        public int maxPaperLog = 10;

        public SnitchesServerConfig(ICoreAPI api)
        {
            sapi = api;
        }        
    }
}
