using Common;
using GameCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameSnake
{
    public class Snake
    {
        public HeadSnake Head { get; private set; }
        public List<BodySnake> Body { get; private set; }

        public Snake(HeadSnake head, params BodySnake[] body)
        {
            Head = head;
            Head.Snake = this;
            Body = body.ToList();
        }

        public void Tick(GameState game)
        {
            var creatureCommand = Head.PrevCreatureCommand;

            foreach (var item in Body)
            {
                var temp = item.CreatureCommand;
                item.CreatureCommand = creatureCommand;
                creatureCommand = temp;
            }
        }
    }

    public class HeadSnake : ICreature
    {
        public CreatureCommand PrevCreatureCommand;
        public Snake Snake;

        public CreatureCommand Act(GameState game, int x, int y)
        {
            var creatureCommand = CreatureCommandHelper.FromPlayerCommandNoCheckBound(game, this);

            if (creatureCommand == CreatureCommandHelper.NoneCommand)
                creatureCommand = PrevCreatureCommand;

            var movePoint = creatureCommand.Move(x, y);
            var moveToCreature = game.GetCreatureOrNull(movePoint);

            var isMoveBack = Snake.Body.Contains(moveToCreature);

            if (isMoveBack)
                creatureCommand = PrevCreatureCommand;

            PrevCreatureCommand = creatureCommand.Clone();

            return creatureCommand.TorSpace(game, x, y);
        }

        public bool DeadInConflict(ICreature conflictedObject)
        {
            return false;
        }

        public int TransformPriority()
        {
            return 1;
        }
    }

    public class BodySnake : ICreature
    {
        public CreatureCommand CreatureCommand;

        public CreatureCommand Act(GameState game, int x, int y)
        {
            return CreatureCommand.TorSpace(game, x, y);
        }

        public bool DeadInConflict(ICreature conflictedObject)
        {
            return false;
        }

        public int TransformPriority()
        {
            return 1;
        }
    }

    public class Apple : ICreature
    {
        public CreatureCommand Act(GameState game, int x, int y)
        {
            return new CreatureCommand();
        }

        public bool DeadInConflict(ICreature conflictedObject)
        {
            return conflictedObject is HeadSnake;
        }

        public int TransformPriority()
        {
            return 0;
        }
    }
}
