using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;

namespace raithesnitches.src.Config
{
    public class SnitchesServerConfigs
    {
        [JsonProperty]
        public SnitchesConfig[] SnitchConfigs;
    }
        
    public class SnitchesConfig
    {        

        [JsonIgnore]
        const string LEAD_COPPER_SNITCH = "raithesnitches:snitchblock-lead-copper";
        [JsonIgnore]
        const string MOLYBDOCHALKOS_COPPER_SNITCH = "raithesnitches:snitchblock-molybdochalkos-copper";
        [JsonIgnore]
        const string LEAD_TINBRONZE_SNITCH = "raithesnitches:snitchblock-lead-tinbronze";
        [JsonIgnore]
        const string MOLYBDOCHALKOS_TINBRONZE_SNITCH = "raitchesnithes:snitchblock-molybdochalkos-tinbronze";
        [JsonIgnore]
        const string LEAD_BISMUTHBRONZE_SNITCH = "raithesnitches:snitchblock-lead-bismuthbronze";
        [JsonIgnore]
        const string MOLYBDOCHALKOS_BISMUTHBRONZE_SNITCH = "raithesnitches:snitchblock-molybdochalkos-bismuthbronze";
        [JsonIgnore]
        const string LEAD_BLACKBRONZE_SNITCH = "raithesnitches:snitchblock-lead-blackbronze";
        [JsonIgnore]
        const string MOLYBDOCHALKOS_BLACKBRONZE_SNITCH = "raithesnitches:snitchblock-molybdochalkos-blackbronze";
        [JsonIgnore]
        const string LEAD_IRON_SNITCH = "raithesnitches:snitchblock-lead-iron";
        [JsonIgnore]
        const string MOLYBDOCHALKOS_IRON_SNITCH = "raithesnitches:snitchblock-molybdochalkos-iron";
        [JsonIgnore]
        const string LEAD_STEEL_SNITCH = "raithesnitches:snitchblock-lead-steel";
        [JsonIgnore]
        const string MOLYBDOCHALKOS_STEEL_SNITCH = "raithesnitches:snitchblock-molybdochalkos-steel";


        [JsonProperty]
        public string code = "";

        [JsonProperty]
        public bool snitchSneakable = false;
        [JsonProperty]
        public int snitchRadius = 16;
        [JsonProperty]
        public int snitchVerticalRange = 16;
        [JsonProperty]
        public float snitchTruesightRange = 0.5f;
        [JsonProperty]
        public int snitchMaxLog = 2000;

        [JsonProperty]
        public int maxBookLog = 200;
        [JsonProperty]
        public int maxPaperLog = 20;
        [JsonProperty]
        public float snitchDownloadTime = 2.0f;        

        public SnitchesConfig(ICoreAPI sapi, string code, bool snitchSneakable, int snitchRadius, int snitchVerticalRange, float snitchTruesightRange, int snitchMaxLog, int maxBookLog, int maxPaperLog, float snitchDownloadTime)
        {
            this.code = code;
            this.snitchSneakable = snitchSneakable;
            this.snitchRadius = snitchRadius;
            this.snitchVerticalRange = snitchVerticalRange;
            this.snitchTruesightRange = snitchTruesightRange;
            this.snitchMaxLog = snitchMaxLog;
            this.maxBookLog = maxBookLog;
            this.maxPaperLog = maxPaperLog;
            this.snitchDownloadTime = snitchDownloadTime;
        }

        public static Dictionary<string, SnitchesConfig> CreateBaseConfigs(ICoreAPI api)
        {            
            Dictionary<string, SnitchesConfig> configs = new();

            configs.Add(LEAD_COPPER_SNITCH, new SnitchesConfig(api, LEAD_COPPER_SNITCH, false, 16, 8, 0.5f, 500, 500, 20, 4.0f));
            configs.Add(MOLYBDOCHALKOS_COPPER_SNITCH, new SnitchesConfig(api, MOLYBDOCHALKOS_COPPER_SNITCH, false, 8, 16, 0.5f, 500, 500, 20, 4.0f));

            configs.Add(LEAD_TINBRONZE_SNITCH, new SnitchesConfig(api, LEAD_TINBRONZE_SNITCH, false, 32, 8, 0.5f, 750, 500, 20, 4.0f));
            configs.Add(MOLYBDOCHALKOS_TINBRONZE_SNITCH, new SnitchesConfig(api, MOLYBDOCHALKOS_TINBRONZE_SNITCH, false, 8, 32, 0.5f, 750, 500, 20, 4.0f));

            configs.Add(LEAD_BISMUTHBRONZE_SNITCH, new SnitchesConfig(api, LEAD_BISMUTHBRONZE_SNITCH, false, 32, 12, 0.5f, 750, 500, 20, 4.0f));
            configs.Add(MOLYBDOCHALKOS_BISMUTHBRONZE_SNITCH, new SnitchesConfig(api, MOLYBDOCHALKOS_BISMUTHBRONZE_SNITCH, false, 12, 32, 0.5f, 750, 500, 20, 4.0f));

            configs.Add(LEAD_BLACKBRONZE_SNITCH, new SnitchesConfig(api, LEAD_BLACKBRONZE_SNITCH, false, 32, 16, 0.5f, 750, 500, 20, 4.0f));
            configs.Add(MOLYBDOCHALKOS_BLACKBRONZE_SNITCH, new SnitchesConfig(api, MOLYBDOCHALKOS_BLACKBRONZE_SNITCH, false, 16, 32, 0.5f, 750, 500, 20, 4.0f));

            configs.Add(LEAD_IRON_SNITCH, new SnitchesConfig(api, LEAD_IRON_SNITCH, false, 40, 20, 0.5f, 1000, 500, 20, 4.0f));
            configs.Add(MOLYBDOCHALKOS_IRON_SNITCH, new SnitchesConfig(api, MOLYBDOCHALKOS_IRON_SNITCH, false, 20, 40, 0.5f, 1000, 500, 20, 4.0f));

            configs.Add(LEAD_STEEL_SNITCH, new SnitchesConfig(api, LEAD_STEEL_SNITCH, false, 48, 24, 0.5f, 1500, 500, 20, 4.0f));
            configs.Add(MOLYBDOCHALKOS_STEEL_SNITCH, new SnitchesConfig(api, MOLYBDOCHALKOS_STEEL_SNITCH, false, 24, 48, 0.5f, 1500, 500, 20, 4.0f));

            
            return configs;
        }
    }
}
