using Common;
using GameCore;
using GameDigger;
using GameSnake;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace WinFormsGameView
{
    public class GameWindow : Form
    {
       // private GameService gameService = new DiggerGameService(DiggerGameService.mapWithPlayerTerrain);
        private GameService gameService = new SnakeGameService(SnakeGameService.TestMap);

        private readonly HashSet<Keys> pressedKeys = new HashSet<Keys>();
        private UserPlayer userPlayer = new UserPlayer();

        private int MapWidth;
        private int MapHeight;

        public GameWindow()
        {
            CreatureAnimation.SpriteSize = 32;
            CreatureAnimation.AnimationTickLength = 32;
            InitLocalGame();
            InitForm(MapWidth, MapHeight);
            StartGame(LocalGameTimerTick);
        }

        void InitForm(int MapWidth, int MapHeight) 
        {
            var elementSize = CreatureAnimation.SpriteSize;

            ClientSize = new Size(
                    elementSize * MapWidth,
                    elementSize * MapHeight + elementSize);

            FormBorderStyle = FormBorderStyle.FixedDialog;
        }

        void InitLocalGame()
        {
            MapWidth = gameService.GameState.MapWidth;
            MapHeight = gameService.GameState.MapHeight; 

            var bot = ListBotPlayer.DownLeft;
            var randomBot = new RandomBotPlayer(2);
            gameService.AddPlayer(userPlayer);
        }

        void StartGame(Action<object, EventArgs> action)
        {
            var timer = new Timer();
            timer.Interval = 15;
            timer.Tick += (o, e) => action(o, e);
            timer.Start();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Text = "Digger";
            DoubleBuffered = true;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            pressedKeys.Add(e.KeyCode);
            userPlayer.PressedKey = e.KeyCode;
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            pressedKeys.Remove(e.KeyCode);
            userPlayer.PressedKey = pressedKeys.Any() ? pressedKeys.Min() : Keys.None;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var elementSize = CreatureAnimation.SpriteSize;

            e.Graphics.TranslateTransform(0, elementSize);
            e.Graphics.FillRectangle(
                Brushes.Black, 0, 0, elementSize * MapWidth, elementSize * MapHeight);

            for (int i = 0; i < CreatureAnimation.Animations.Count; i++)
            {
                var a = CreatureAnimation.Animations[i];
                e.Graphics.DrawImage(a.Sprite, a.DrawLocation);
            }

            e.Graphics.ResetTransform();
           // e.Graphics.DrawString(GameState.Scores.ToString(), new Font("Arial", 16), Brushes.Green, 0, 0);
        }


        private void LocalGameTimerTick(object sender, EventArgs args)
        {
            if (CreatureAnimation.AnimationTickCount == 0)
            {
                gameService.MakeGameTick();

                CreatureAnimation.Animations = gameService.Game.Animations
                    .Select(v => new CreatureAnimation(v))
                    .ToList();
            }
               
            CreatureAnimation.Tick(MapWidth, MapHeight);

            Invalidate();
        }
    }

    static class CreatureImageNameGetter
    {
        public static string Get(Terrain _) => "Terrain.png";
        public static string Get(Digger _) => "Digger.png";
        public static string Get(HeadSnake _) => "HeadSnake.png";
        public static string Get(BodySnake _) => "BodySnake.png";
        public static string Get(Apple _) => "Apple.png";
        public static string Get(Wall _) => "Terrain.png";
        public static string Get(Spawn _) => "Spawn.png";
    }

    public class UserPlayer : IPlayer
    {
        public Keys PressedKey;

        public IPlayerCommand GetCommand(IReadOnlyGameState gameState)
        {
            return new PlayerCommand() { Move = KeyToMoveCommand(PressedKey) };
        }

        FourDirMove KeyToMoveCommand(Keys key)
        {
            return key switch
            {
                Keys.Left => FourDirMove.Left,
                Keys.Right => FourDirMove.Right,
                Keys.Up => FourDirMove.Up,
                Keys.Down => FourDirMove.Down,
                _ => FourDirMove.None,
            };
        }
    }
}