using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WebApi.Services
{
    public class MapService
    {
        public static List<MapInfo> Maps = new();

        static MapService()
        {
            foreach (var path in Directory.GetFiles("wwwroot/SnakeMaps/"))
            {
                var filename = Path.GetFileName(path);
                var map = File.ReadAllText(path).Replace("\r", "").Trim();
                var mapInfo = new MapInfo() { Name = filename, Map = map };
                Maps.Add(mapInfo);
            }
        }

        public static MapInfo GetMap(string name)
        {
            var mapInfo = Maps
                .FirstOrDefault(v => v.Name == name);
            if (mapInfo == null)
                return new MapInfo() { Map = TestMap, Name = $"Map '{name}' - NotFound"};
            return mapInfo; 
        }

        public class MapInfo
        {
            public string Name { get; set; }
            public string Map { get; set; }
        }

        public const string TestMap = @"
WWWWW WWWWWWWWWW WWWWW
W         WW         W
S         WW         S
W         WW         W
W                    W
     W          W     
W                    W
W         WW         W
W         WW         W
W         WW         W
WWW     WWWWWW     WWW
W         WW         W
W         WW         W
W         WW         W
W                    W
     W          W     
W                    W
W         WW         W
S         WW         S
W         WW         W
WWWWW WWWWWWWWWW WWWWW
";
    }
}
