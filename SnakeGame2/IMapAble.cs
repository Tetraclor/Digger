using GameCore;
using System;

namespace SnakeGame2
{
    public interface IMapAble
    { 
        void SetToMap(Action<Point, ICreature> set);
    }
}
