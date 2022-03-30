using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;

namespace Sokoban
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        string[] levelsPath = { "level1.txt", "level2.txt", "level3.txt", "level4.txt", "level5.txt" };
        int currentLevel = 0;

        char[,] map;
        List<Vector2> objectivePointsPos;
        const int tileSize = 64;
        int width, height;
        Texture2D playerTexture, wallTexture, groundTexture, boxTexture, objectiveTexture;
        KeyboardManager km;

        delegate bool Verification();
        event Verification OnObjectiveReach;

        //PLAYER
        Vector2 playerPos;
        Vector2 spawnPos;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            km = new KeyboardManager();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            playerTexture = Content.Load<Texture2D>("Character4");
            wallTexture = Content.Load<Texture2D>("Wall_Black");
            groundTexture = Content.Load<Texture2D>("GroundGravel_Grass");
            boxTexture = Content.Load<Texture2D>("Crate_Beige");
            objectiveTexture = Content.Load<Texture2D>("EndPoint_Black");

            LoadLevel();
            OnObjectiveReach += isLevelFinished;
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here
            km.Update();
            Movement();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            _spriteBatch.Begin();
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                {
                    char currentSymbol = map[x, y];

                    switch (currentSymbol)
                    {
                        case 'X':
                            _spriteBatch.Draw(wallTexture, new Vector2(x, y) * tileSize, Color.White);
                            break;
                        case ' ':
                            _spriteBatch.Draw(groundTexture, new Vector2(x, y) * tileSize, Color.White);
                            break;
                        case 'B':
                            _spriteBatch.Draw(boxTexture, new Vector2(x, y) * tileSize, Color.White);
                            break;
                        case '.':
                            _spriteBatch.Draw(objectiveTexture, new Vector2(x, y) * tileSize, Color.White);
                            break;

                        default:
                            _spriteBatch.Draw(groundTexture, new Vector2(x, y) * tileSize, Color.White);
                            break;
                    }
                }

            _spriteBatch.Draw(playerTexture, playerPos * tileSize, Color.White);
            _spriteBatch.End();

            base.Draw(gameTime);
        }

        void LoadLevel()
        {
            if (currentLevel >= levelsPath.Length) return;

            string[] lines = File.ReadAllLines("Content/" + levelsPath[currentLevel++]);
            map = new char[lines[0].Length, lines.Length];
            objectivePointsPos = new List<Vector2>();

            for (int x = 0; x < lines[0].Length; x++)
                for (int y = 0; y < lines.Length; y++)
                {
                    string currentLine = lines[y];
                    map[x, y] = currentLine[x];

                    if (currentLine[x] == '.')
                        objectivePointsPos.Add(new Vector2(x, y));

                    if (currentLine[x] == 'i')
                        playerPos = new Vector2(x, y);
                }

            height = lines.Length;
            width = lines[0].Length;
            spawnPos = playerPos;

            _graphics.PreferredBackBufferHeight = height * tileSize;
            _graphics.PreferredBackBufferWidth = width * tileSize;
            _graphics.ApplyChanges();
        }

        void Movement()
        {
            Vector2 newPos = playerPos;
            Vector2 dir = Vector2.Zero;

            if (km.IsKeyPressed(Keys.W))
            {
                newPos -= Vector2.UnitY;
                dir = -Vector2.UnitY;
            }
            if (km.IsKeyPressed(Keys.A))
            {
                newPos -= Vector2.UnitX;
                dir = -Vector2.UnitX;
            }
            if (km.IsKeyPressed(Keys.S))
            {
                newPos += Vector2.UnitY;
                dir = Vector2.UnitY;
            }
            if (km.IsKeyPressed(Keys.D))
            {
                newPos += Vector2.UnitX;
                dir = Vector2.UnitX;
            }
            if (km.IsKeyPressed(Keys.R))
            {
                newPos = spawnPos;
                currentLevel--;
                LoadLevel();
            }

            if (map[(int)newPos.X, (int)newPos.Y] == 'X')
                newPos = playerPos;

            // box behaviour
            else if (map[(int)newPos.X, (int)newPos.Y] == 'B')
            {
                if (map[(int)(newPos.X + dir.X), (int)(newPos.Y + dir.Y)] == ' ' ||
                    map[(int)(newPos.X + dir.X), (int)(newPos.Y + dir.Y)] == ' ')
                {
                    bool isObjective = map[(int)(newPos.X + dir.X), (int)(newPos.Y + dir.Y)] == '.';

                    map[(int)(newPos.X + dir.X), (int)(newPos.Y + dir.Y)] = 'B';
                    map[(int)(newPos.X), (int)(newPos.Y)] = ' ';

                    foreach (Vector2 pos in objectivePointsPos)
                    {
                        if (pos.X == newPos.X + dir.X &&
                            pos.Y == newPos.Y + dir.Y)
                        {
                            OnObjectiveReach?.Invoke();
                            break;
                        }
                    }
                }
                else if (map[(int)(newPos.X + dir.X), (int)(newPos.Y + dir.Y)] == 'X' ||
                         map[(int)(newPos.X + dir.X), (int)(newPos.Y + dir.Y)] == 'B')
                    newPos = playerPos;
            }

            map[(int)playerPos.X, (int)playerPos.Y] = ' ';
            playerPos = newPos;
            map[(int)playerPos.X, (int)playerPos.Y] = 'i';
        }

        bool isLevelFinished()
        {
            foreach (Vector2 pos in objectivePointsPos)
            {
                if (map[(int)pos.X, (int)pos.Y] != 'B') return false;
            }

            LoadLevel();

            return true;
        }
    }
}