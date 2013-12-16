using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace GP3Coursework
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        #region User Defined Variables
        //------------------------------------------
        // Added for use with fonts
        //------------------------------------------
        SpriteFont fontToUse;
        //--------------------------------------------------
        // Added for use with playing Audio via Media player
        //--------------------------------------------------
        private Song bkgMusic;
        private String songInfo;
        //--------------------------------------------------
        //Set the sound effects to use
        //--------------------------------------------------

        //Lap Count and half way marker
        int Player2Lap = 0;
        Boolean Player2halfLap = false;

        //Lap Count and half way marker
        int Player1Lap = 0;
        Boolean Player1halfLap = false;

        //Models
        private Model Player1;
        private Matrix[] Player1Transforms;

        private Model Test;
        private Matrix[] TestTransforms;

        private Model Player2;
        private Matrix[] Player2Transforms;

        private Model Finish;
        private Matrix[] FinishTransforms;


        //Choose Camera
        int screenstate = 0;

        // The aspect ratio determines how to scale 3d to 2d projection.
        private float aspectRatio;

        // Set the position of the model in world space, and set the rotation.
        private Vector3 TestPos = new Vector3 (150.0f, 0.0f, 0.0f);
        private Vector3 SlowPos = new Vector3(500.0f, 0.0f, 0.0f);       
        private Vector3 BoostPos = new Vector3(800.0f, 0.0f, 0.0f);
        private Vector3 FinishPos = new Vector3(150.0f, 0.0f, -50.0f);
        private Vector3 CheckPointPos = new Vector3(-120.0f, 0.0f, -120.0f);
        private Vector3 Player1Position = new Vector3(205f, 3.5f,-120.0f);
        private Vector3 Player2Position = new Vector3(185.0f, 3.0f, -120.0f);

        private float Player1Rotation = 0.0f;
        private Vector3 Player1Velocity = Vector3.Zero;
        private float Player2Rotation = 0.0f;
        private Vector3 Player2Velocity = Vector3.Zero;  

        //Random for Music
        private Random random = new Random();

        private KeyboardState lastState;

        private Boolean pause = false;

        private int Winner = 0;
        //Cameras
        private Camera MainCamera;
        private Camera Camera1;
        private Camera Camera2;
        private Camera Camera3;
        private Camera Camera4;

        //SplitScreen Variables
        Viewport MainScreen;
        Viewport Player1Screen;
        Viewport Player2Screen;
        Viewport TopDownScreen;
        Viewport PauseScreen;



        private void MoveModel()
        {
            KeyboardState keyboardState = Keyboard.GetState();
            
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // Create some velocity if the right trigger is down.
            Vector3 Player1VelocityAdd = Vector3.Zero;
            Vector3 Player2VelocityAdd = Vector3.Zero;

            // Find out what direction we should be thrusting, using rotation.
            Player1VelocityAdd.X = -(float)Math.Sin(Player1Rotation);
            Player1VelocityAdd.Z = -(float)Math.Cos(Player1Rotation);

            Player2VelocityAdd.X = -(float)Math.Sin(Player2Rotation);
            Player2VelocityAdd.Z = -(float)Math.Cos(Player2Rotation);

            if (keyboardState.IsKeyDown(Keys.Left)|| (GamePad.GetState(PlayerIndex.One).ThumbSticks.Left.X < 0))
            {
                // Rotate left.
                Player1Rotation -= -1.0f * 0.05f;
            }

            if (keyboardState.IsKeyDown(Keys.Right) || (GamePad.GetState(PlayerIndex.One).ThumbSticks.Left.X > 0))
            {
                // Rotate right.
                Player1Rotation -= 1.0f * 0.05f;
            }

            if (keyboardState.IsKeyDown(Keys.Up) || (GamePad.GetState(PlayerIndex.One).Triggers.Right > 0))
            {
                // Accelerate
                Player1VelocityAdd *= -0.08f;
                Player1Velocity += Player1VelocityAdd;
            }

            if (keyboardState.IsKeyDown(Keys.Down) || (GamePad.GetState(PlayerIndex.One).Triggers.Left > 0))
            {
                //Reverse
                Player1VelocityAdd *= 0.02f;
                Player1Velocity += Player1VelocityAdd;
            }

            if (keyboardState.IsKeyDown(Keys.A) || (GamePad.GetState(PlayerIndex.Two).ThumbSticks.Left.X < 0))
      
            {
                // Rotate left.
                Player2Rotation -= -1.0f * 0.12f;
            }

            if (keyboardState.IsKeyDown(Keys.D) || (GamePad.GetState(PlayerIndex.Two).ThumbSticks.Left.X > 0))
            {
                // Rotate right.
                Player2Rotation -= 1.0f * 0.12f;
            }

            if (keyboardState.IsKeyDown(Keys.W) || (GamePad.GetState(PlayerIndex.Two).Triggers.Right > 0))
            {
                // Accelerate.
                Player2VelocityAdd *= -0.085f;
                Player2Velocity += Player2VelocityAdd;
            }

            if (keyboardState.IsKeyDown(Keys.S) || (GamePad.GetState(PlayerIndex.Two).Triggers.Left > 0))
            {
                // Reverse
                Player2VelocityAdd *= 0.02f;
                Player2Velocity += Player2VelocityAdd;
            }

            if (keyboardState.IsKeyDown(Keys.R))
            {
                Player1Velocity = Vector3.Zero;
                Player1Position = new Vector3(205f, 3.5f, -120.0f);
                Player1Rotation = 0.0f;
                Player1Lap = 0;

                Player2Velocity = Vector3.Zero;
                Player2Position = new Vector3(185.0f, 3.0f, -120.0f);
                Player2Rotation = 0.0f;
                Player2Lap = 0;
            }

            if (keyboardState.IsKeyDown(Keys.M))
            {
                //Mute
                MediaPlayer.Pause();
            }

            if (keyboardState.IsKeyDown(Keys.N))
            {
                //Player
                MediaPlayer.Resume();
            }
            lastState = keyboardState;

        }

        private Matrix[] SetupEffectTransformDefaults(Model myModel)
        {
            Matrix[] absoluteTransforms = new Matrix[myModel.Bones.Count];
            myModel.CopyAbsoluteBoneTransformsTo(absoluteTransforms);

            foreach (ModelMesh mesh in myModel.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.Projection = MainCamera.projectionMatrix;
                    effect.View = MainCamera.camViewMatrix;
                }
           
            }
            return absoluteTransforms;
        }

        public void DrawModel(Model model, Matrix modelTransform, Matrix[] absoluteBoneTransforms)
        {
            //Draw the model, a model can have multiple meshes, so loop
            foreach (ModelMesh mesh in model.Meshes)
            {
                //This is where the mesh orientation is set
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = absoluteBoneTransforms[mesh.ParentBone.Index] * modelTransform;//MainCamera.worldMatrix;absoluteBoneTransforms[mesh.ParentBone.Index] * modelTransform;
                }
                //Draw the mesh, will use the effects set above.
                mesh.Draw();
            }
        }

        private void writeText(string msg, Vector2 msgPos, Color msgColour)
        {
            spriteBatch.Begin();
            string output = msg;
            // Find the center of the string
            Vector2 FontOrigin = fontToUse.MeasureString(output) / 2;
            Vector2 FontPos = msgPos;
            // Draw the string
            spriteBatch.DrawString(fontToUse, output, FontPos, msgColour);
            spriteBatch.End();
        }

        private void CheckCollisions()
        {
            //Player Bounding Objects
            BoundingSphere Player1Sphere = new BoundingSphere(Player1Position, Player1.Meshes[0].BoundingSphere.Radius * GameConstants.CarBoundingSphereScale);
            BoundingSphere Player2Sphere = new BoundingSphere(Player2Position, Player2.Meshes[0].BoundingSphere.Radius * GameConstants.CarBoundingSphereScale);
            //Finish Line and Check point
            BoundingBox FinishLineBox = new BoundingBox(new Vector3(135.0f, 0.0f, -58.0f), new Vector3(162.0f, 0.0f, -32.0f));
            BoundingBox CheckPointBox = new BoundingBox(new Vector3(-147.0f, 0.0f, -138.0f), new Vector3(-130.0f, 0.0f, -114.0f));
            //All the TRACK (Inside)
            BoundingBox TrackInsidePart1 = new BoundingBox(new Vector3(-160.0f, 0.0f, -182.0f), new Vector3(176.0f, 0.0f, -145.0f));
            BoundingBox TrackInsidePart2 = new BoundingBox(new Vector3(-85.0f, 0.0f, -145.0f), new Vector3(173.0f, 0.0f, -68.0f));
            BoundingBox TrackInsidePart3 = new BoundingBox(new Vector3(-180.0f, 0.0f, -32.0f), new Vector3(48.0f, 0.0f, 13.0f));
            BoundingBox TrackInsidePart4 = new BoundingBox(new Vector3(-180.0f, 0.0f, 13.0f), new Vector3(-47.0f, 0.0f, 107.0f));
            BoundingBox TrackInsidePart6 = new BoundingBox(new Vector3(176.0f, 0.0f, -182.0f), new Vector3(-160.0f, 0.0f, -145.0f));
            //All the TRACK (outside)
            BoundingBox TrackOutsidePart1 = new BoundingBox(new Vector3(-313.0f, 0.0f, -223.0f), new Vector3(-313.0f, 0.0f, -108.0f));
            BoundingBox TrackOutsidePart2 = new BoundingBox(new Vector3(-312.0f, 0.0f, -107.0f), new Vector3(-124.0f, 0.0f, -70.0f));
            BoundingBox TrackOutsidePart3 = new BoundingBox(new Vector3(-228.0f, 0.0f, -70.0f), new Vector3(-228.0f, 0.0f, 225.0f));
            BoundingBox TrackOutsidePart4 = new BoundingBox(new Vector3(-228.0f, 0.0f, 225.0f), new Vector3(54.0f, 0.0f, 225.0f));
            BoundingBox TrackOutsidePart6 = new BoundingBox(new Vector3(54.0f, 0.0f, 82.0f), new Vector3(160.0f, 0.0f, 225.0f));            
            BoundingBox TrackOutsidePart7 = new BoundingBox(new Vector3(152.0f, 0.0f, -28.0f), new Vector3(230.0f, 0.0f, 77.0f));
            BoundingBox TrackOutsidePart8 = new BoundingBox(new Vector3(226.0f, 0.0f, -214.0f), new Vector3(226.0f, 0.0f, -30.0f));
            BoundingBox TrackOutsidePart9 = new BoundingBox(new Vector3(-313.0f, 0.0f, -223.0f), new Vector3(223.0f, 0.0f, -223.0f));
            //Dirt Tracks
            BoundingBox SlowPit1 = new BoundingBox(new Vector3(-260.0f, 0.0f, -185.0f), new Vector3(-228.0f, 0.0f, -144.0f));
            BoundingBox SlowPit2 = new BoundingBox(new Vector3(-185.0f, 0.0f, -185.0f), new Vector3(-153.0f, 0.0f, -144.0f));
            BoundingBox SlowPit3 = new BoundingBox(new Vector3(-181.0f, 0.0f, 146.0f), new Vector3(-73.0f, 0.0f, -158.0f));
            BoundingBox SlowPit4 = new BoundingBox(new Vector3(-224.0f, 0.0f, 200.0f), new Vector3(47.0f, 0.0f, 215.0f));
            BoundingBox SlowPit5 = new BoundingBox(new Vector3(-41.0f, 0.0f, 20.0f), new Vector3(-12.0f, 0.0f, 103.0f));
            BoundingBox SlowPit6 = new BoundingBox(new Vector3(-28.0f, 0.0f, 106.0f), new Vector3(-5.0f, 0.0f, 157.0f));
            BoundingBox SlowPit7 = new BoundingBox(new Vector3(-41.9f, 0.0f, 20.0f), new Vector3(68f, 0.0f, 23.0f));
            BoundingBox SlowPit8 = new BoundingBox(new Vector3(53.0f, 0.0f, -67.0f), new Vector3(67.9f, 0.0f, 23.0f));
            
            //Check for collisions
            /////////////////////////////////////////////////////
            //Player 1 Inside
            /////////////////////////////////////////////////////
            if (Player1Sphere.Intersects(TrackInsidePart1))
            {
                Player1Velocity = -Player1Velocity;
            }
            
            if (Player1Sphere.Intersects(TrackInsidePart2))
            {
                Player1Velocity = -Player1Velocity;
            }
            
            if (Player1Sphere.Intersects(TrackInsidePart3))
            {
                Player1Velocity = -Player1Velocity;
            }
            
            if (Player1Sphere.Intersects(TrackInsidePart4))
            {
                Player1Velocity = -Player1Velocity;
            }
            /////////////////////////////////////////////////////
            //Player 1 Outside
            /////////////////////////////////////////////////////
            if (Player1Sphere.Intersects(TrackOutsidePart1))
            {
                Player1Velocity = -Player1Velocity;
            }

            if (Player1Sphere.Intersects(TrackOutsidePart2))
            {
                Player1Velocity = -Player1Velocity;
            }

            if (Player1Sphere.Intersects(TrackOutsidePart3))
            {
                Player1Velocity = -Player1Velocity;
            }

            if (Player1Sphere.Intersects(TrackOutsidePart4))
            {
                Player1Velocity = -Player1Velocity;
            }

            if (Player1Sphere.Intersects(TrackOutsidePart6))
            {
                Player1Velocity = -Player1Velocity;
            }

            if (Player1Sphere.Intersects(TrackOutsidePart7))
            {
                Player1Velocity = -Player1Velocity;
            }

            if (Player1Sphere.Intersects(TrackOutsidePart8))
            {
                Player1Velocity = -Player1Velocity;
            }

            if (Player1Sphere.Intersects(TrackOutsidePart9))
            {
                Player1Velocity = -Player1Velocity;
            }

            /////////////////////////////////////////////////////
            //Player 2 Inside
            /////////////////////////////////////////////////////
            if (Player2Sphere.Intersects(TrackInsidePart1))
            {
                Player2Velocity = -Player2Velocity;
            }

            if (Player2Sphere.Intersects(TrackInsidePart2))
            {
                Player2Velocity = -Player2Velocity;
            }

            if (Player2Sphere.Intersects(TrackInsidePart3))
            {
                Player2Velocity = -Player2Velocity;
            }

            if (Player2Sphere.Intersects(TrackInsidePart4))
            {
                Player2Velocity = -Player2Velocity;
            }

            /////////////////////////////////////////////////////
            //Player 2 Outside
            /////////////////////////////////////////////////////
            if (Player2Sphere.Intersects(TrackOutsidePart1))
            {
                Player2Velocity = -Player2Velocity;
            }

            if (Player2Sphere.Intersects(TrackOutsidePart2))
            {
                Player2Velocity = -Player2Velocity;
            }

            if (Player2Sphere.Intersects(TrackOutsidePart3))
            {
                Player2Velocity = -Player2Velocity;
            }

            if (Player2Sphere.Intersects(TrackOutsidePart4))
            {
                Player2Velocity = -Player2Velocity;
            }

            if (Player2Sphere.Intersects(TrackOutsidePart6))
            {
                Player2Velocity = -Player2Velocity;
            }

            if (Player2Sphere.Intersects(TrackOutsidePart7))
            {
                Player2Velocity = -Player2Velocity;
            }

            if (Player2Sphere.Intersects(TrackOutsidePart8))
            {
                Player2Velocity = -Player2Velocity;
            }

            if (Player2Sphere.Intersects(TrackOutsidePart9))
            {
                Player2Velocity = -Player2Velocity;
            }


            /////////////////////////////////////////////////////
            //Slowpits Player1
            /////////////////////////////////////////////////////
            if (Player1Sphere.Intersects(SlowPit1))
            {
                Player1Velocity *= 0.8f;
            }

            if (Player1Sphere.Intersects(SlowPit2))
            {
                Player1Velocity *= 0.8f;
            }

            if (Player1Sphere.Intersects(SlowPit3))
            {
                Player1Velocity *= 0.6f;
            }

            if (Player1Sphere.Intersects(SlowPit4))
            {
                Player1Velocity *= 0.8f;
            }

            if (Player1Sphere.Intersects(SlowPit5))
            {
                Player1Velocity *= 0.8f;
            }

            if (Player1Sphere.Intersects(SlowPit6))
            {
                Player1Velocity *= 0.8f;
            }

            if (Player1Sphere.Intersects(SlowPit7))
            {
                Player1Velocity *= 0.8f;
            }

            if (Player1Sphere.Intersects(SlowPit8))
            {
                Player1Velocity *= 0.8f;
            }


            /////////////////////////////////////////////////////
            //Slowpits Player2
            /////////////////////////////////////////////////////
            if (Player2Sphere.Intersects(SlowPit1))
            {
                Player2Velocity *= 0.8f;
            }

            if (Player2Sphere.Intersects(SlowPit2))
            {
                Player2Velocity *= 0.8f;
            }

            if (Player2Sphere.Intersects(SlowPit3))
            {
                Player2Velocity *= 0.6f;
            }

            if (Player2Sphere.Intersects(SlowPit4))
            {
                Player2Velocity *= 0.8f;
            }

            if (Player2Sphere.Intersects(SlowPit5))
            {
                Player2Velocity *= 0.8f;
            }

            if (Player2Sphere.Intersects(SlowPit6))
            {
                Player2Velocity *= 0.8f;
            }

            if (Player2Sphere.Intersects(SlowPit7))
            {
                Player2Velocity *= 0.8f;
            }

            if (Player2Sphere.Intersects(SlowPit8))
            {
                Player2Velocity *= 0.8f;
            }

            /////////////////////////////////////////////////////
            //Player Collision
            /////////////////////////////////////////////////////
            if (Player1Sphere.Intersects(Player2Sphere))
            {
                Player1Velocity = -Player1Velocity;
                Player2Velocity = -Player2Velocity;
            }

            /////////////////////////////////////////////////////
            //Player 2 Finished
            /////////////////////////////////////////////////////
            if (Player2Sphere.Intersects(FinishLineBox))
            {
                if (Player2halfLap == true)
                {
                    if (Player2Lap == 2)
                    {
                        Player1Lap = 0;
                        Winner = 2;
                        Player2halfLap = false;
                    }

                    if (Player2Lap < 2)
                    {
                        Player2Lap++;
                        Player2halfLap = false;
                    }
                }

            }

            if (Player2Sphere.Intersects(CheckPointBox))
            {
                Player2halfLap = true;
            }
            /////////////////////////////////////////////////////
            //Player 1 Finished
            /////////////////////////////////////////////////////
            if (Player1Sphere.Intersects(FinishLineBox))
            {
                if (Player1halfLap == true)
                {
                    if (Player1Lap == 2)
                    {
                        //this.Exit();
                        Winner = 1;
                        Player2Lap = 0;
                        Player1halfLap = false;
                    }

                    if (Player1Lap < 2)
                    {
                        Player1Lap++;
                        Player1halfLap = false;
                    }
                }

            }

            if (Player1Sphere.Intersects(CheckPointBox))
            {
                Player1halfLap = true;
            }
        }

        private void Menu()
        {
            // Attempt at split screen
            KeyboardState keyboardState = Keyboard.GetState();

            MainScreen = GraphicsDevice.Viewport;
            Player1Screen = MainScreen;
            Player2Screen = MainScreen;
            TopDownScreen = MainScreen;
            PauseScreen = MainScreen;

            Player1Screen.Height = Player1Screen.Height / 2;
            Player2Screen.Height = Player2Screen.Height / 2;

            Player2Screen.Y = Player1Screen.Height;

            Camera1 = new Camera(new Vector3(0.0f, 600.0f, 5.0f), Vector3.Zero);
            Camera2 = new Camera(new Vector3(Player1Position.X,(Player1Position.Y + 20.0f), (Player1Position.Z - 50.0f)), (Player1Position));
            Camera3 = new Camera(new Vector3(Player2Position.X, (Player2Position.Y + 20.0f), (Player2Position.Z - 50.0f)), (Player2Position));
            Camera4 = new Camera(new Vector3(300.0f, 600.0f, 300.0f), Vector3.Zero);

            Camera1.InitializeTransform(graphics);
            Camera2.InitializeTransform(graphics);
            Camera3.InitializeTransform(graphics);

            if (screenstate == 0)
            {
                MainCamera = Camera3;

                if (keyboardState.IsKeyDown(Keys.Z))
                {
                    screenstate = 1;
                }

                if (keyboardState.IsKeyDown(Keys.Enter))
                {
                    screenstate = 2;
                }
                
            }
            if (screenstate == 1)
            {
                MainCamera = Camera1;
                LoadContent();
            }

            if (screenstate == 2)
            {
                //MainCamera = Camera2;
                LoadContent();
            }

        }

        void DrawScene(Viewport viewport)
        {
            graphics.GraphicsDevice.Viewport = viewport;

            Matrix modelTransform = Matrix.CreateRotationY(Player1Rotation) * Matrix.CreateTranslation(Player1Position);
            DrawModel(Player1, modelTransform, Player1Transforms);

            Matrix model2Transform = Matrix.CreateRotationY(Player2Rotation) * Matrix.CreateTranslation(Player2Position);
            DrawModel(Player2, model2Transform, Player2Transforms);

            Matrix model3Transform = Matrix.CreateRotationY(0.0f) * Matrix.CreateTranslation(TestPos);
            DrawModel(Test, model3Transform, TestTransforms);

            Matrix model6Transform = Matrix.CreateRotationY(0.0f) * Matrix.CreateTranslation(FinishPos);
            DrawModel(Finish, model6Transform, FinishTransforms);
        }

       
        #endregion

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content"; //chocolate john
            this.IsMouseVisible = true;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            this.IsMouseVisible = true;
            Window.Title = "GP3 CW";
       
            Menu();
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            aspectRatio = graphics.GraphicsDevice.Viewport.AspectRatio;

            //aspectRatio = (float)GraphicsDeviceManager.DefaultBackBufferWidth / (2 * GraphicsDeviceManager.DefaultBackBufferHeight);
            //-------------------------------------------------------------
            // added to load font
            //-------------------------------------------------------------
            fontToUse = Content.Load<SpriteFont>(".\\Fonts\\Calibri");
            //-------------------------------------------------------------
            // added to load Song
            //-------------------------------------------------------------
            int theme = random.Next(1, 5);

            if (theme == 1)
            {
                bkgMusic = Content.Load<Song>(".\\Audio\\Theme1");
                MediaPlayer.Play(bkgMusic);
                MediaPlayer.IsRepeating = true;
                songInfo = "Song: " + bkgMusic.Name + " Song Duration: " + bkgMusic.Duration.Minutes + ":" + bkgMusic.Duration.Seconds;
            }

            if (theme == 2)
            {
                bkgMusic = Content.Load<Song>(".\\Audio\\Theme2");
                MediaPlayer.Play(bkgMusic);
                MediaPlayer.IsRepeating = true;
                songInfo = "Song: " + bkgMusic.Name + " Song Duration: " + bkgMusic.Duration.Minutes + ":" + bkgMusic.Duration.Seconds;
            }

            if (theme == 3)
            {
                bkgMusic = Content.Load<Song>(".\\Audio\\Theme3");
                MediaPlayer.Play(bkgMusic);
                MediaPlayer.IsRepeating = true;
                songInfo = "Song: " + bkgMusic.Name + " Song Duration: " + bkgMusic.Duration.Minutes + ":" + bkgMusic.Duration.Seconds;
            }


            if (theme == 4)
            {
                bkgMusic = Content.Load<Song>(".\\Audio\\Theme4");
                MediaPlayer.Play(bkgMusic);
                MediaPlayer.IsRepeating = true;
                songInfo = "Song: " + bkgMusic.Name + " Song Duration: " + bkgMusic.Duration.Minutes + ":" + bkgMusic.Duration.Seconds;
            }

         
            //-------------------------------------------------------------
            // added to load Models
            //-------------------------------------------------------------
            
            Player1 = Content.Load<Model>(".\\Models\\Player1");
            Player1Transforms = SetupEffectTransformDefaults(Player1);

            Player2 = Content.Load<Model>(".\\Models\\Player2");
            Player2Transforms = SetupEffectTransformDefaults(Player2);

            Test = Content.Load<Model>(".\\Models\\Track");
            TestTransforms = SetupEffectTransformDefaults(Test);

            Finish = Content.Load<Model>(".\\Models\\FinishLine");
            FinishTransforms = SetupEffectTransformDefaults(Finish);
             
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            KeyboardState keyboardState = Keyboard.GetState();

            if (screenstate == 0)
            {
                Menu();
            }
            else
            {
                //Pause state
                if (pause == true)
                {
                    if (keyboardState.IsKeyDown(Keys.Enter))
                    {
                        pause = false;
                    }
                }
                else
                {                
                    // TODO: Add your update logic here
                    MoveModel();

                    // Add velocity to the current position.
                    Player1Position += Player1Velocity;
                    Player2Position += Player2Velocity;
                    // Bleed off velocity over time.
                    Player1Velocity *= 0.95f;
                    Player2Velocity *= 0.95f;

                    float timeDelta = (float)gameTime.ElapsedGameTime.TotalSeconds;

                    CheckCollisions();

                    //Pause the game
                    if (keyboardState.IsKeyDown(Keys.Space))
                    {
                        pause = true;
                    }

                    if (Winner != 0)
                    {
                        if (keyboardState.IsKeyDown(Keys.Enter))
                        {
                            this.Exit();
                        }
                    }
                }       
                
            }
            base.Update(gameTime);
        }


        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.BlendState = BlendState.Opaque; 
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            GraphicsDevice.Clear(Color.CornflowerBlue);

            //Main Menu
            if (screenstate == 0)
            {
                GraphicsDevice.Clear(Color.Black);
                writeText("Choose :", new Vector2(200, 100), Color.White);
                writeText("Z - Top Down (Working)", new Vector2(200, 120), Color.White);
                writeText("Enter - 3rdPerson (Broken)", new Vector2(200, 140), Color.White);

                writeText("Controls - PLayer 1 : Up Down Left Right or Xbox Controller", new Vector2(100, 220), Color.LightBlue);
                writeText("Controls - PLayer 2 : W, S, A, D, or Xbox Controller", new Vector2(100, 240), Color.LightYellow);
                writeText("Oh Yeah M - Mute and N Resumes", new Vector2(100, 260), Color.Orange);
                writeText("First to 3 laps WINS!", new Vector2(200, 50), Color.Orange);
            }
            else
            {

                if (screenstate == 1)
                {
                    DrawScene(TopDownScreen);
                }

                if (screenstate == 2)
                {
                    MainCamera = Camera2;
                    //MainCamera.camPosition = new Vector3(Player1Position.X, (Player1Position.Y + 20.0f), (Player1Position.Z - 50.0f));
                    DrawScene(Player1Screen);

                    MainCamera = Camera3;
                    DrawScene(Player2Screen);
                }
             
                if (pause == true)
                {
                    //MainCamera = Camera4;
                    //DrawScene(PauseScreen);
                    writeText("Game is PAUSED: ENTER to continue", new Vector2(200, 200), Color.Wheat);
                }

                writeText("Player 1 Lap:" + Player1Lap, new Vector2(630, 10), Color.Blue);
                writeText("Player 2 Lap:" + Player2Lap, new Vector2(630, 50), Color.Yellow);
                writeText("Pause: Space", new Vector2(630, 100), Color.White);
            }

            //Winning Screens
            if (Winner == 1)
            {
                writeText("Player 1 WINS! : Press Enter to end game", new Vector2(200, 200), Color.Gold);
            }

            if (Winner == 2)
            {
                writeText("Player 2 WINS! : Press Enter to end game", new Vector2(200, 200), Color.Gold);
            }

            base.Draw(gameTime);
        }

    
    }
}
