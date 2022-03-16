using GameCore;
using System.Collections.Generic;

namespace SnakeGame2
{
    public interface ITransformAble
    {
        IEnumerable<CreatureTransformation> ToTransformation();
    }
}
