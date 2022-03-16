using Common;
using GameCore;
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
        private GameService gameService = new SnakeGame2.SnakeGameService();
        //private GameService gameService = new SnakeGameService(SnakeGameService.TestMap);

        private readonly HashSet<Keys> pressedKeys = new HashSet<Keys>();
        private UserPlayer userPlayer = new UserPlayer();

        private int MapWidth;
        private int MapHeight;

        IAnimationManager AnimationManager;

        private int GameTick = 300;

        public GameWindow()
        {
            var bot = ListBotPlayer.DownLeft;
            var randomBot = new RandomBotPlayer(2);
            var primitiveBot = new GameSnake.SnakeBot();
            var upLeftBot = new ListBotPlayer(FourDirMove.Up, FourDirMove.Left);
            gameService.AddPlayer(userPlayer);
            gameService.AddPlayer(randomBot);
            gameService.AddPlayer(randomBot);
            gameService.AddPlayer(randomBot);

            StartWithMapAnimate();
        }

        void StartWithSmoothAnimate()
        {
            var animationLength = 16;
            AnimationManager = new SmoothAnimationManager(animationLength: animationLength, () => {
                gameService.MakeGameTick();
                return gameService.GetCreatureTransformations();
            });
            InitSizeGame();
            InitForm(gameService.GameState.MapWidth, gameService.GameState.MapHeight);
            StartGame(LocalGameSmoothAnimateTimerTick, GameTick / animationLength);
        }

        void StartWithMapAnimate()
        {
            AnimationManager = new MapAnimationManager(() => gameService.GameState.Map);
            InitSizeGame();
            InitForm(gameService.GameState.MapWidth, gameService.GameState.MapHeight);
            StartGame(LocalGameMapAnimateTimerTick, GameTick);
        }

        void InitSizeGame()
        {
            MapWidth = gameService.GameState.MapWidth;
            MapHeight = gameService.GameState.MapHeight;
        }

        void InitForm(int MapWidth, int MapHeight) 
        {
            var elementSize = AnimationManager.SpriteSize;

            ClientSize = new Size(
                    elementSize * MapWidth,
                    elementSize * MapHeight + elementSize);

            FormBorderStyle = FormBorderStyle.FixedDialog;
        }

        void StartGame(Action<object, EventArgs> action, int interval)
        {
            var timer = new Timer();
            timer.Interval = interval;
            timer.Tick += (o, e) => action(o, e);
            timer.Start();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Text = "Snake?";
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
            var elementSize = AnimationManager.SpriteSize;

            e.Graphics.TranslateTransform(0, 0);
            e.Graphics.FillRectangle(
                Brushes.Black, 0, 0, elementSize * MapWidth, elementSize * MapHeight);

            AnimationManager.Draw(e.Graphics);

            e.Graphics.ResetTransform();
        }

        private void LocalGameSmoothAnimateTimerTick(object sender, EventArgs args)
        {
            AnimationManager.Tick(MapWidth, MapHeight);
            Invalidate();
        }

        private void LocalGameMapAnimateTimerTick(object sender, EventArgs args)
        {
            gameService.MakeGameTick();
            Invalidate();
        }
    }

    public class UserPlayer : IPlayer
    {
        public Keys PressedKey;

        public IPlayerCommand GetCommand(IGameStateForPlayer gameState)
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