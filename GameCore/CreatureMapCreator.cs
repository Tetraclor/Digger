using GameCore;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace GameCore
{
    public static class CreatureMapCreator
    {
        private static readonly ConcurrentDictionary<string, Func<ICreature>> factory = new ();
        private static readonly ConcurrentDictionary<char, string> charToClassName = new (); 

        private static string icreatureTypeName = typeof(ICreature).Name;
        public static Assembly assembly;

        public static ICreature[,] CreateMap(string map, Assembly assembly, Func<char, string> charToClassName = null, string separator = "\n")
        {
            CreatureMapCreator.assembly = assembly;
            charToClassName ??= CharToClassName;
            map = map.Replace("\r", "");
            var rows = map.Split(new[] { separator }, StringSplitOptions.RemoveEmptyEntries);
            var maxWidth = rows.Max(v => v.Length);
            
            var result = new ICreature[maxWidth, rows.Length];

            for (var x = 0; x < maxWidth; x++)
                for (var y = 0; y < rows.Length; y++)
                    result[x, y] = CreateCreatureBySymbol(rows[y].Length > x ? rows[y][x] : ' ', charToClassName);

            return result;
        }

        public static ICreature[,] CreateMap(int width, int height, Assembly assembly, Func<char, string> charToClassName = null, string separator = "\r\n")
        {
            CreatureMapCreator.assembly = assembly;
            charToClassName ??= CharToClassName;

            var result = new ICreature[width, height];
            for (var x = 0; x < width; x++)
                for (var y = 0; y < height; y++)
                    result[x, y] = CreateCreatureBySymbol(' ', charToClassName);
            return result;
        }

        public static string MapToString(this ICreature[,] map)
        {
            var w = map.GetLength(0);
            var h = map.GetLength(1);

            var stringBuilder = new StringBuilder();

            for (var y = 0; y < h; y++)
            {
                for (var x = 0; x < w; x++)
                {
                    var creature = map[x, y];
                    if (creature == null) stringBuilder.Append(' ');
                    else stringBuilder.Append(creature.ToString().Split('.').Last()[0]);
                }
                if(y != h - 1)
                    stringBuilder.Append('\n');
            }

            return stringBuilder.ToString();

        }

        public static ICreature CreateCreatureByTypeName(string name)
        {
            // Это использование механизма рефлексии. 
            // Ему посвящена одна из последних лекций второй части курса Основы программирования
            // В обычном коде можно было обойтись без нее, но нам нужно было написать такой код,
            // который работал бы, даже если вы ещё не создали класс Monster или Gold. 
            // Просто написать new Gold() мы не могли, потому что это не скомпилировалось бы до создания класса Gold.
            if (!factory.ContainsKey(name))
            {
                var type = GetICreatureTypes()
                    .FirstOrDefault(z => z.Name == name);
                if (type == null)
                    throw new Exception($"Can't find type '{name}'");
                factory[name] = () => (ICreature)Activator.CreateInstance(type);
            }

            return factory[name]();
        }

        private static ICreature CreateCreatureBySymbol(char c, Func<char, string> charToClassName)
        {
            if (c == ' ') return null;

            var className = charToClassName(c);

            if(className == null)
                throw new Exception($"wrong character for ICreature {c}");

            return CreateCreatureByTypeName(className);
        }

        private static string CharToClassName(char c)
        {
            if (charToClassName.TryGetValue(c, out string className))
                return className;

            var type = GetICreatureTypes()
                  .FirstOrDefault(z => z.Name[0] == c);

            if(type == null)

            charToClassName[c] = type?.Name;

            return type?.Name;
        }

        private static Type[] GetICreatureTypes()
        {
            return assembly
                  .GetTypes()
                  .Where(v => v.GetInterface(icreatureTypeName) != null)
                  .ToArray();
        }
    }
}
