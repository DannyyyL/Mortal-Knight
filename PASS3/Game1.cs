//Author: Dan Lichtin
//File Name: main.cs
//Project Name: PASS3
//Creation Date: May 19, 2022
//Modified Date: June 21, 2022
//Description: Mortal Knight is a 2D platformer with the goal of making it to the check points
//The game has several environmental hazards and enemies to challenge 
//The player in their journey of completing Mortal Knight
using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using Animation2D;
using Helper;
using Camera; 

namespace PASS3
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        //Game States
        const int MENU = 0;
        const int INSTRUCTIONS = 1;
        const int PAUSE = 2;
        const int GAMEPLAY = 3;
        const int GAMEOVER = 4;
        const int GAMEWIN = 5;

        //Current game state 
        int gameState = MENU;

        //Input States for Mouse and Keyboard
        KeyboardState prevKb;
        KeyboardState kb;
        MouseState prevMouse;
        MouseState mouse;

        /////////////////////////
        ///ALL Movement Data///// 
        /////////////////////////
        const int RIGHT = 1;
        const int LEFT = -1;
        int dir;
        int skeletonDir = LEFT;
        int batDir = LEFT;
        //Movement states
        bool isCharGrounded = true;
        int animState;
        int skeletonAnimState = 1;
        int batAnimState = 1;
        ////Forces
        const float GRAVITY = 9.8f / 30;
        const float ACCELERATION = 0.5f;
        const float ENEMY_ACCELERATION = 0.7f;
        const float FRICTION = ACCELERATION * 0.55f;
        const float TOLERANCE = FRICTION * 0.5f;
        //Force vector
        Vector2 forces;
        //Speeds
        float maxSpeed = 5f;
        float enemyMaxSpeed = 3f;
        float jumpSpeed = -8.5f;
        Vector2 characterSpeed;
        Vector2 skeletonSpeed;
        Vector2 batSpeed;

        ////////////////////////
        /////Character Data///// 
        ////////////////////////
        //Character Images and Rectangles
        Texture2D[] characterImgs = new Texture2D[6];
        Rectangle characterRec;
        Rectangle[] characterRecs = new Rectangle[4];
        GameRectangle[] characterVisibleRecs = new GameRectangle[4];
        //Character Animation Data
        Animation[] characterAnims = new Animation[6];
        Vector2 characterLoc;
        //Character stats
        int characterHealth = 10;
        const int CHARACTER_MAX_HEALTH = 10;
        //CHARACTER COLLISION states
        const int FEET = 0;
        const int HEAD = 1;
        const int LEFT_BODY = 2;
        const int RIGHT_BODY = 3;
        bool showCollisionRecs = false;
        //Character height and width
        int characterHeight;
        int characterWidth;

        ////////////////////
        /////Enemy Data///// 
        ////////////////////
        //Enemy images
        Texture2D[] skeletonImgs = new Texture2D[5];
        Texture2D[] batImgs = new Texture2D[5];
        //Enemy Animation Data
        Animation[] skeletonAnims = new Animation[5];
        Animation[] batAnims = new Animation[5];
        //Enemy recs & locs
        Vector2[] skeletonPlacements = new Vector2[3];
        Vector2[] skeletonLocs = new Vector2[2];
        Vector2[] batPlacements = new Vector2[3];
        Vector2[] batLocs = new Vector2[2];
        Rectangle skeletonRec;
        Rectangle batRec;
        GameRectangle[] enemyVisibleRecs = new GameRectangle[2];
        //Enemy Identifier
        const int SKELETON = 0;
        const int BAT = 1;
        //Enemy stats 
        bool[] deadOrAlive = new bool[2]; 
        int skeletonHealth = 3;
        int batHealth = 2;
        const int SKELETON_MAX_HEALTH = 3;
        const int BAT_MAX_HEALTH = 2;

        ///////////////////
        //Game Backgrounds
        int screenWidth;
        int screenHeight;
        Rectangle bgRec;
        Rectangle doorRec;
        Rectangle wallRec;
        //Bg images
        Texture2D mainBg;
        Texture2D level1LeftBg;
        Texture2D level1RightBg;
        Texture2D gameWinBg;
        Texture2D castleImg;
        Texture2D castleDoorImg;
        Texture2D castleWallImg;
        Texture2D currentTutorialBg;
        Texture2D tutorial1Bg;
        Texture2D tutorial2Bg;
        //One time instruction display
        //bool firstPlaythrough = true; 

        ////////////////////////
        /////Main Menu Data///// 
        ///////////////////////
        //Game fonts
        SpriteFont titleFont;
        Vector2 titleLoc;
        SpriteFont menuOptionsFont;
        //Menu btn system && UI
        Texture2D[] menuBtns = new Texture2D[5];
        Rectangle[] menuBtnRecs = new Rectangle[5];
        Vector2[] menuBtnsLocs = new Vector2[5];
        bool oneTimeTutorial = true; 
        Texture2D clockImg;
        Vector2 clockLoc;
        Vector2 timeLoc;
        //GAMEPLAY UI
        Texture2D healthBar;
        GameRectangle healthBarRec;
        Texture2D emptyBar;
        Rectangle emptyBarRec;
        Texture2D[] lockImgs = new Texture2D[2];
        Vector2 lockLoc;
        const int LOCKED = 0;
        const int UNLOCKED = 1;
        int lockState = LOCKED;
        //PAUSE UI
        Vector2 pauseLoc;
        //GAMEOVER UI
        Vector2 gameOverLoc;
        //GAMEWIN UI
        Vector2 gameWinLoc;
        Vector2 escapeTimeLoc;

        //ANIMATION states
        const int IDLE = 0;
        const int RUN = 1;
        const int ATTACK = 2;
        const int ROLL = 3;
        const int KILLED = 3;
        const int HIT = 4;
        const int JUMP = 4;
        const int DEATH = 5;
        //MENU UI states
        const int QUIT = 0;
        const int START = 1;
        const int INFO = 2;
        const int CONTINUE = 3;
        const int NEXT = 4;

        //Music & Sound effects
        Song gameplayMusic;
        SoundEffect buttonSFX;
        SoundEffect footStepsSFX;
        SoundEffect swingSFX;
        SoundEffect swingHitSFX;
        SoundEffect jumpSFX;
        SoundEffect healthLossSFX;
        SoundEffectInstance swing;
        SoundEffectInstance swingHit;
        SoundEffectInstance footSteps; 

        ///////////////////
        /////Trap Data/////
        ///////////////////
        //Trap images
        Texture2D[] trapImgs = new Texture2D[2];
        //Trap vectors
        Rectangle[] trapRecs = new Rectangle[2];
        Vector2[] trapLocs = new Vector2[2];
        //Trap anims
        Animation[] trapAnims = new Animation[2];
        //Trap types
        const int LIGHTNING = 0;
        const int SAW = 1;
        //Visible Trap recs
        GameRectangle[] trapVisibleRecs = new GameRectangle[2];

        ////////////////////
        /////Level Data/////
        ////////////////////
        //Platform imgs & recs
        Texture2D[] platformImgs = new Texture2D[5];
        Rectangle[] platformRecs = new Rectangle[5];
        //Checkpoint loc
        Vector2[] checkpointLocs = new Vector2[3];
        Vector2 currentCheckpoint;
        //Variable to track level progress
        int levelProgress = 0;

        ////////////////////
        /////ALL TIMERS/////
        ////////////////////
        Timer footStepsTimer;
        Timer rollTimer;
        Timer speedrunTimer;
        string bestTimeTxt;
        int bestTime = 1000000; 
        Timer hitTimer; 

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
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
            this.graphics.PreferredBackBufferWidth = 1200;
            this.graphics.PreferredBackBufferHeight = 640;

            //Turning off vsync
            graphics.PreferMultiSampling = true;
            graphics.SynchronizeWithVerticalRetrace = false;

            this.graphics.ApplyChanges();

            IsMouseVisible = true;

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

            // TODO: use this.Content to load your game content here
            screenWidth = graphics.GraphicsDevice.Viewport.Width;
            screenHeight = graphics.GraphicsDevice.Viewport.Height;

            ///////////////////////////
            ////Loading Backgrounds////
            ///////////////////////////
            //Loading ALL backgrounds and layers
            bgRec = new Rectangle(0, 0, screenWidth, screenHeight);
            mainBg = Content.Load<Texture2D>("Images/Backgrounds/MainBg");
            level1LeftBg = Content.Load<Texture2D>("Images/Backgrounds/LeftBg");
            level1RightBg = Content.Load<Texture2D>("Images/Backgrounds/RightBg");
            gameWinBg = Content.Load<Texture2D>("Images/Backgrounds/GameWinBg");
            castleImg = Content.Load<Texture2D>("Images/Backgrounds/BgLayer");
            tutorial1Bg = Content.Load<Texture2D>("Images/Backgrounds/TutorialBg");
            tutorial2Bg = Content.Load<Texture2D>("Images/Backgrounds/Tutorial2Bg");
            currentTutorialBg = tutorial1Bg;

            //Loading ALL fonts
            titleFont = Content.Load<SpriteFont>("Fonts/TitleFont");
            menuOptionsFont = Content.Load<SpriteFont>("Fonts/MenuOptionsFont");

            //Loading ALL buttons 
            menuBtns[0] = Content.Load<Texture2D>("Images/Sprites/UI/QuitBtn");
            menuBtns[1] = Content.Load<Texture2D>("Images/Sprites/UI/StartBtn");
            menuBtns[2] = Content.Load<Texture2D>("Images/Sprites/UI/InstructionBtn");
            menuBtns[3] = Content.Load<Texture2D>("Images/Sprites/UI/ContinueBtn");
            menuBtns[4] = Content.Load<Texture2D>("Images/Sprites/UI/NextBtn");

            /////////////////////////////
            ////Loading ALL Platforms////
            /////////////////////////////
            //Platform Imgs
            platformImgs[0] = Content.Load<Texture2D>("Images/Backgrounds/BaseFloor");
            platformImgs[1] = Content.Load<Texture2D>("Images/Backgrounds/Platform1");
            platformImgs[2] = Content.Load<Texture2D>("Images/Backgrounds/Platform2");
            platformImgs[3] = Content.Load<Texture2D>("Images/Backgrounds/Platform3");
            platformImgs[4] = Content.Load<Texture2D>("Images/Backgrounds/Platform4");
            //Platform Recs
            platformRecs[0] = new Rectangle(0, screenHeight - platformImgs[0].Height, platformImgs[0].Width, platformImgs[0].Height);
            platformRecs[1] = new Rectangle(screenWidth / 5, (int)(screenHeight / 1.5f), platformImgs[1].Width, platformImgs[1].Height);
            platformRecs[2] = new Rectangle(screenWidth / 2, (int)(screenHeight / 2f), platformImgs[2].Width, platformImgs[2].Height);
            //Background decoration
            castleDoorImg = Content.Load<Texture2D>("Images/Backgrounds/ArchDoor");
            castleWallImg = Content.Load<Texture2D>("Images/Backgrounds/WindowWall");
            doorRec = new Rectangle((int)(screenWidth - (castleDoorImg.Width / 1.15f)), (int)(screenHeight - (castleDoorImg.Height * 1.4f)), castleDoorImg.Width, castleDoorImg.Height);
            wallRec = new Rectangle(-5, (int)(screenHeight - (castleWallImg.Height * 1.525f)), castleWallImg.Width, castleWallImg.Height);

            /////////////////////////
            ////Loading ALL Traps////
            /////////////////////////
            //Trap imgs 
            trapImgs[SAW] = Content.Load<Texture2D>("Images/Sprites/Traps/HangingSaw");
            trapImgs[LIGHTNING] = Content.Load<Texture2D>("Images/Sprites/Traps/LightningTrap");
            //Trap locs & recs
            trapLocs[SAW] = new Vector2(500, screenHeight / 1.2f);
            trapRecs[SAW] = new Rectangle((int)(trapLocs[SAW].X / 0.75f), (int)(trapLocs[SAW].Y / 0.75f), trapImgs[SAW].Width, trapImgs[SAW].Height);
            trapLocs[LIGHTNING] = new Vector2(440, 378);
            trapRecs[LIGHTNING] = new Rectangle(480, 425, trapImgs[LIGHTNING].Width / 19, trapImgs[LIGHTNING].Height);
            //Trap anims
            trapAnims[LIGHTNING] = new Animation(trapImgs[SAW], 16, 1, 16, 0, 0, Animation.ANIMATE_FOREVER, 4, trapLocs[SAW], 3, true);
            trapAnims[LIGHTNING] = new Animation(trapImgs[LIGHTNING], 10, 1, 10, 0, 0, Animation.ANIMATE_FOREVER, 4, trapLocs[LIGHTNING], 1.5f, true);

            //////////////////////
            ////Loading ALL UI////
            //////////////////////
            //MENU UI
            clockImg = Content.Load<Texture2D>("Images/Sprites/UI/ClockImg");
            clockLoc = new Vector2(0, 0);
            timeLoc = new Vector2(90, -15);
            titleLoc = new Vector2(screenWidth - (titleFont.MeasureString("Mortal Knight").X * 1.1f), screenHeight / 9);
            menuBtnsLocs[QUIT] = new Vector2(screenWidth - (titleFont.MeasureString("Mortal Knight").X * 1.1f) + menuBtns[QUIT].Width * 2, screenHeight / 1.9f);
            menuBtnsLocs[START] = new Vector2(screenWidth - (titleFont.MeasureString("Mortal Knight").X * 1.1f) + menuBtns[QUIT].Width * 2, screenHeight / 2.7f);
            menuBtnsLocs[CONTINUE] = new Vector2(screenWidth - (titleFont.MeasureString("Mortal Knight").X * 1.1f) + menuBtns[CONTINUE].Width * 0.94f, screenHeight / 2.7f);
            menuBtnsLocs[INFO] = new Vector2(10, screenHeight - (menuBtns[INFO].Height * 1.2f));
            menuBtnsLocs[NEXT] = new Vector2(screenWidth - menuBtns[NEXT].Width * 1.2f, screenHeight - (menuBtns[NEXT].Height * 1.2f));
            for (int i = 0; i < 5; i++)
            {
                menuBtnRecs[i] = new Rectangle((int)menuBtnsLocs[i].X, (int)menuBtnsLocs[i].Y, menuBtns[i].Width, menuBtns[i].Height);
            }
            //GAME UI
            healthBar = Content.Load<Texture2D>("Images/Sprites/UI/_HealthBar");
            emptyBar = Content.Load<Texture2D>("Images/Sprites/UI/_EmptyBar");
            healthBarRec = new GameRectangle(GraphicsDevice, 50, (int)33.2f, healthBar.Width, healthBar.Height);
            emptyBarRec = new Rectangle(25, 25, emptyBar.Width, emptyBar.Height);
            lockImgs[LOCKED] = Content.Load<Texture2D>("Images/Sprites/UI/LockedImg");
            lockImgs[UNLOCKED] = Content.Load<Texture2D>("Images/Sprites/UI/UnlockedImg");
            lockLoc = new Vector2(screenWidth - lockImgs[LOCKED].Width, 5);
            //PAUSE UI
            pauseLoc = new Vector2(screenWidth - (titleFont.MeasureString("Pause    ").X * 1.1f), screenHeight / 9);
            //GAMEOVER UI
            gameOverLoc = new Vector2(screenWidth / 1.5f - (titleFont.MeasureString("Game Over").X * 1.1f), screenHeight / 2 - (titleFont.MeasureString("Game Over").Y / 2));
            //GAMEWIN UI
            gameWinLoc = new Vector2(screenWidth / 1.5f - (titleFont.MeasureString("You made it out of the castle!").X / 1.85f), screenHeight / 50f);
            escapeTimeLoc = new Vector2(screenWidth / 1.5f - (titleFont.MeasureString("Escape Time:").X / 1.5f), screenHeight / 6);

            //////////////////////////
            //Loading character data//
            //////////////////////////
            //Loading ALL character assets
            characterImgs[IDLE] = Content.Load<Texture2D>("Images/Sprites/_Idle");
            characterImgs[RUN] = Content.Load<Texture2D>("Images/Sprites/_Run");
            characterImgs[ATTACK] = Content.Load<Texture2D>("Images/Sprites/_Attack");
            characterImgs[ROLL] = Content.Load<Texture2D>("Images/Sprites/_Roll");
            characterImgs[JUMP] = Content.Load<Texture2D>("Images/Sprites/_Jump");
            characterImgs[DEATH] = Content.Load<Texture2D>("Images/Sprites/_Death");
            //Loading character height and width
            characterHeight = (int)(characterImgs[IDLE].Height * 2.5f);
            characterWidth = (int)(characterImgs[IDLE].Width * 2.5f);
            //Loading character Rec & Loc
            characterRec = new Rectangle(30, 400, characterWidth, characterHeight);
            characterLoc = new Vector2(characterRec.X, characterRec.Y);
            //Loading ALL character Anims
            characterAnims[IDLE] = new Animation(characterImgs[IDLE], 2, 4, 8, 0, 0, Animation.ANIMATE_FOREVER, 8, characterLoc, 2, true);
            characterAnims[RUN] = new Animation(characterImgs[RUN], 2, 4, 8, 0, 0, Animation.ANIMATE_FOREVER, 3, characterLoc, 2, false);
            characterAnims[ATTACK] = new Animation(characterImgs[ATTACK], 8, 1, 8, 0, 0, Animation.ANIMATE_FOREVER, 6, characterLoc, 2, false);
            characterAnims[ROLL] = new Animation(characterImgs[ROLL], 2, 2, 4, 0, 0, Animation.ANIMATE_FOREVER, 3, characterLoc, 2, false);
            characterAnims[JUMP] = new Animation(characterImgs[JUMP], 2, 4, 8, 0, 0, Animation.ANIMATE_ONCE, 4, characterLoc, 2, false);
            characterAnims[DEATH] = new Animation(characterImgs[DEATH], 2, 2, 4, 0, 0, Animation.ANIMATE_FOREVER, 6, characterLoc, 2, false);
            //Loading ALL character collision Recs
            SetCharacterRecs(characterRecs, characterRec);
            //Character speed
            characterSpeed = new Vector2(0, 0);
            //Game forces
            forces = new Vector2(FRICTION, GRAVITY);
            //Character checkpoint locs
            checkpointLocs[0] = new Vector2(characterRec.X, characterRec.Y);
            currentCheckpoint = checkpointLocs[0];
            checkpointLocs[1] = new Vector2(10, 260);

            //////////////////////////
            ////Loading enemy data////
            //////////////////////////
            ///BAT images
            batImgs[RUN] = Content.Load<Texture2D>("Images/Sprites/EnemySprites/_BatFlight");
            batImgs[ATTACK] = Content.Load<Texture2D>("Images/Sprites/EnemySprites/_BatAttack");
            batImgs[KILLED] = Content.Load<Texture2D>("Images/Sprites/EnemySprites/_BatDeath");
            batImgs[HIT] = Content.Load<Texture2D>("Images/Sprites/EnemySprites/_BatHit");
            //Enemy Vectors
            skeletonPlacements[0] = new Vector2(700, 363);
            skeletonPlacements[1] = new Vector2(400, 256);
            skeletonPlacements[2] = new Vector2(200, 159);
            skeletonLocs[0] = skeletonPlacements[0];
            batPlacements[0] = new Vector2(300, 200);
            batPlacements[1] = new Vector2(500, 160);
            batPlacements[2] = new Vector2(700, 100);
            batLocs[0] = batPlacements[0];
            ///SKELETON images
            skeletonImgs[IDLE] = Content.Load<Texture2D>("Images/Sprites/EnemySprites/_SkeletonIdle");
            skeletonImgs[RUN] = Content.Load<Texture2D>("Images/Sprites/EnemySprites/_SkeletonRun");
            skeletonImgs[ATTACK] = Content.Load<Texture2D>("Images/Sprites/EnemySprites/_SkeletonAttack");
            skeletonImgs[KILLED] = Content.Load<Texture2D>("Images/Sprites/EnemySprites/_SkeletonDeath");
            skeletonImgs[HIT] = Content.Load<Texture2D>("Images/Sprites/EnemySprites/_SkeletonHit");
            //Skeleton Anims
            skeletonAnims[IDLE] = new Animation(skeletonImgs[IDLE], 4, 1, 4, 0, 0, Animation.ANIMATE_FOREVER, 12, skeletonLocs[0], 1.6f, true);
            skeletonAnims[RUN] = new Animation(skeletonImgs[RUN], 4, 1, 4, 0, 0, Animation.ANIMATE_FOREVER, 5, skeletonLocs[0], 1.6f, false);
            skeletonAnims[ATTACK] = new Animation(skeletonImgs[ATTACK], 8, 1, 8, 0, 0, Animation.ANIMATE_ONCE, 3, skeletonLocs[0], 1.6f, false);
            skeletonAnims[KILLED] = new Animation(skeletonImgs[KILLED], 4, 1, 4, 0, 0, Animation.ANIMATE_ONCE, 5, skeletonLocs[0], 1.6f, false);
            skeletonAnims[HIT] = new Animation(skeletonImgs[HIT], 4, 1, 4, 0, 0, Animation.ANIMATE_FOREVER, 10, skeletonLocs[0], 1.6f, true);
            ///Bat Anims
            batAnims[RUN] = new Animation(batImgs[RUN], 8, 1, 8, 0, 0, Animation.ANIMATE_FOREVER, 5, batLocs[0], 1.9f, true);
            batAnims[ATTACK] = new Animation(batImgs[ATTACK], 8, 1, 8, 0, 0, Animation.ANIMATE_ONCE, 3, batLocs[0], 1.9f, false);
            batAnims[KILLED] = new Animation(batImgs[KILLED], 4, 1, 4, 0, 0, Animation.ANIMATE_ONCE, 5, batLocs[0], 1.9f, false);
            batAnims[HIT] = new Animation(batImgs[HIT], 4, 1, 4, 0, 0, Animation.ANIMATE_FOREVER, 10, batLocs[0], 1.9f, true);
            //Enemy Rectangles
            skeletonRec = new Rectangle(1000, 400, skeletonImgs[IDLE].Width / 9, (int)(skeletonImgs[IDLE].Height / 1.5f));
            batRec = new Rectangle(1000, 300, batImgs[RUN].Width / 15, (int)(batImgs[RUN].Height / 2.5));
            ///Enemy speed 
            skeletonSpeed = new Vector2(0, 0);
            batSpeed = new Vector2(0, 0);
            //Enemy dead/alive state; if bool is true == enemy is alive
            deadOrAlive[SKELETON] = true;
            deadOrAlive[BAT] = true;

            //////////////////////////
            ////Loading music data////
            //////////////////////////
            gameplayMusic = Content.Load<Song>("Audio/Music/GamePlayMusic");
            buttonSFX = Content.Load<SoundEffect>("Audio/Sounds/ButtonSFX");
            footStepsSFX = Content.Load<SoundEffect>("Audio/Sounds/FootStepsSFX");
            swingSFX = Content.Load<SoundEffect>("Audio/Sounds/SwingSFX");
            swingHitSFX = Content.Load<SoundEffect>("Audio/Sounds/SwordHitSFX");
            jumpSFX = Content.Load<SoundEffect>("Audio/Sounds/JumpSFX");
            healthLossSFX = Content.Load<SoundEffect>("Audio/Sounds/HealthLossSFX");
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Volume = 1f;
            SoundEffect.MasterVolume = 0.9f;

            //////////////////////////
            ////Loading ALL Timers////
            //////////////////////////
            footStepsTimer = new Timer(500, true);
            rollTimer = new Timer(3000, true);
            speedrunTimer = new Timer(Timer.INFINITE_TIMER, false);
            bestTimeTxt = speedrunTimer.GetTimePassedAsString(Timer.FORMAT_MIN_SEC_MIL);
            hitTimer = new Timer(1000, true); 
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
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
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            //Update the input 
            prevKb = kb;
            kb = Keyboard.GetState();
            prevMouse = mouse;
            mouse = Mouse.GetState();

            //Update gamestate 
            switch (gameState)
            {
                case MENU:
                    UpdateMenu();
                    break;
                case INSTRUCTIONS:
                    UpdateInstructions();
                    break;
                case GAMEPLAY:
                    UpdateGamePlay(gameTime);
                    break;
                case PAUSE:
                    UpdatePause();
                    break;
                case GAMEOVER:
                    UpdateGameOver();
                    break;
                case GAMEWIN:
                    UpdateGameWin();
                    break;
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            spriteBatch.Begin();
            switch (gameState)
            {
                case MENU:
                    DrawMenu();
                    break;
                case INSTRUCTIONS:
                    DrawInstructions();
                    break;
                case GAMEPLAY:
                    DrawGamePlay();
                    break;
                case PAUSE:
                    DrawPause();
                    break;
                case GAMEOVER:
                    DrawGameOver();
                    break;
                case GAMEWIN:
                    DrawGameWin();
                    break;
            }
            spriteBatch.End();

            base.Draw(gameTime);
        }

        ////////////////////////////////////////////////
        ////////////////////UPDATES/////////////////////
        ////////////////////////////////////////////////
        private void UpdateMenu()
        {
            if (mouse.LeftButton == ButtonState.Pressed && prevMouse.LeftButton != ButtonState.Pressed)
            {
                //Menu Button System
                //Exit btn collision detection
                if (menuBtnRecs[QUIT].Contains(mouse.Position))
                {
                    //End game
                    Exit();
                }
                //Start btn collision detection
                if (menuBtnRecs[START].Contains(mouse.Position))
                {
                    //Button sfx
                    buttonSFX.CreateInstance().Play();

                    /*
                    //IF player's first playthrough
                    //THEN send them to the instructions screen
                    //ELSE begin gameplay (and start music)
                    if (oneTimeTutorial == true)
                    {
                        gameState = INSTRUCTIONS;
                        oneTimeTutorial = false;
                    }
                    else
                    {
                        //Starting gameplay music
                        MediaPlayer.Play(gameplayMusic);
                        //Resetting & Activating speedrun timer
                        speedrunTimer.ResetTimer(true);
                        //Changing state to gameplay
                        gameState = GAMEPLAY;
                    }
                    */
                     //Starting gameplay music
                    MediaPlayer.Play(gameplayMusic);
                    //Resetting & Activating speedrun timer
                    speedrunTimer.ResetTimer(true);
                    //Changing state to gameplay
                    gameState = GAMEPLAY;
                }
                //Instruction btn collision detection
                if (menuBtnRecs[INFO].Contains(mouse.Position))
                {
                    //Button sfx
                    buttonSFX.CreateInstance().Play();
                    //Changing state to display instructions
                    gameState = INSTRUCTIONS;
                }
            }
        }

        private void UpdateInstructions()
        {
            if (mouse.LeftButton == ButtonState.Pressed && prevMouse.LeftButton != ButtonState.Pressed)
            {
                //Instruction btn collision detection
                if (menuBtnRecs[INFO].Contains(mouse.Position))
                {
                    //Button sfx
                    buttonSFX.CreateInstance().Play();
                    //Bg change
                    currentTutorialBg = tutorial1Bg;
                    //Changing state back to gameplay
                    gameState = MENU;
                }
                if (menuBtnRecs[NEXT].Contains(mouse.Position) && currentTutorialBg == tutorial1Bg)
                {
                    //Button sfx
                    buttonSFX.CreateInstance().Play();
                    //Bg change
                    currentTutorialBg = tutorial2Bg;
                }
            }
        }

        private void UpdateGamePlay(GameTime gameTime)
        {
            //Check for collision input
            if (kb.IsKeyDown(Keys.D1) && !prevKb.IsKeyDown(Keys.D1))
            {
                showCollisionRecs = !showCollisionRecs;
            }

            //Check for Pause Input
            if (kb.IsKeyDown(Keys.P) && !prevKb.IsKeyDown(Keys.P))
            {
                //Button sfx
                buttonSFX.CreateInstance().Play();
                //Changing state to pause
                gameState = PAUSE;
            }

            //Updating Timers (call from a method)
            UpdatingAllTimers(gameTime);

            //Handles all Enemy AI
            if (deadOrAlive[SKELETON] == true)
            {
                EnemyAI(skeletonLocs, skeletonSpeed, skeletonRec, skeletonAnims, SKELETON, skeletonDir, skeletonAnimState);
            }
            if (deadOrAlive[BAT] == true)
            {
                EnemyAI(batLocs, batSpeed, batRec, batAnims, BAT, batDir, batAnimState);
            }

            //Checking to see if both enemies are dead
            if (deadOrAlive[SKELETON] == false && deadOrAlive[BAT] == false)
            {
                lockState = UNLOCKED;
            }

            //Handles enemy paths
            EnemyPaths();

            //Checking for character attacks (and if they hit any enemies)
            if (characterAnims[animState].destRec.Intersects(skeletonRec) && animState == 1 && deadOrAlive[SKELETON] == true)
            {
                if (hitTimer.IsFinished())
                {
                    EnemyHealthTrigger(SKELETON);
                    hitTimer.ResetTimer(true); 
                    hitTimer.Activate();
                } 
                skeletonAnimState = 2;
            }
            if (characterAnims[animState].destRec.Intersects(batRec) && animState == 1 && deadOrAlive[BAT] == true)
            {
                if (hitTimer.IsFinished())
                {
                    EnemyHealthTrigger(BAT);
                    hitTimer.ResetTimer(true);
                    hitTimer.Activate();
                }
                batAnimState = 2;
            }

            //updating level progress
            if (characterAnims[animState].destRec.X > doorRec.X && lockState == UNLOCKED)
            {
                currentCheckpoint = checkpointLocs[1];
                characterRec.X = (int)currentCheckpoint.X;
                characterRec.Y = (int)currentCheckpoint.Y;
                characterLoc = new Vector2(characterRec.X, characterRec.Y);
                lockState = LOCKED; 
                levelProgress++;

                //Resetting enemy states, health, and recs
                deadOrAlive[SKELETON] = true;
                deadOrAlive[BAT] = true;
                skeletonHealth = SKELETON_MAX_HEALTH;
                batHealth = BAT_MAX_HEALTH;
                settingEnemyRecs(); 
            }

            //Redefining asset recs 
            settingAssetRecs();

            //Handles all movement from the user
            CharacterMovement(gameTime);

            //Update the player's collision Rectangles
            SetCharacterRecs(characterRecs, characterRec);

            //Update the enemies collision Rectangles
            skeletonRec = new Rectangle((int)(skeletonAnims[skeletonAnimState].destRec.X + 80f), (int)(skeletonAnims[skeletonAnimState].destRec.Y + 65f), skeletonImgs[IDLE].Width / 9, (int)(skeletonImgs[IDLE].Height / 1.5f));
            batRec = new Rectangle((int)(batAnims[batAnimState].destRec.X + 100f), (int)(batAnims[batAnimState].destRec.Y + 115), batImgs[RUN].Width / 15, (int)(batImgs[RUN].Height / 2.5f));

            //Checking if knight died
            //If they did; change state to game over
            if (characterHealth <= 0)
            {
                gameState = GAMEOVER;
            }

            //WIN CONDITION
            //If player reaches final level, the knight wins
            if (levelProgress == 3)
            {
                speedrunTimer.Deactivate();
                gameState = GAMEWIN;
            }
        }

        private void UpdatePause()
        {
            //Check for Pause Input
            if (kb.IsKeyDown(Keys.P) && !prevKb.IsKeyDown(Keys.P))
            {
                //Button sfx
                buttonSFX.CreateInstance().Play();
                gameState = GAMEPLAY;
            }

            if (mouse.LeftButton == ButtonState.Pressed && prevMouse.LeftButton != ButtonState.Pressed)
            {
                //Exit btn collision detection
                if (menuBtnRecs[QUIT].Contains(mouse.Position))
                {
                    //End the game
                    gameState = MENU;
                }
                //Continue btn collision detection
                if (menuBtnRecs[CONTINUE].Contains(mouse.Position))
                {
                    //Button sfx
                    buttonSFX.CreateInstance().Play();
                    //Continue game
                    gameState = GAMEPLAY;
                }
            }
        }

        private void UpdateGameOver()
        {
            if (mouse.LeftButton == ButtonState.Pressed && prevMouse.LeftButton != ButtonState.Pressed)
            {
                //Continue btn collision detection
                if (menuBtnRecs[CONTINUE].Contains(mouse.Position))
                {
                    //Button sfx
                    buttonSFX.CreateInstance().Play();
                    //Return user back to menu & reset game
                    ResetGame();
                    gameState = MENU;
                }
            }
        }

        private void UpdateGameWin()
        {
            if (mouse.LeftButton == ButtonState.Pressed && prevMouse.LeftButton != ButtonState.Pressed)
            {
                //Continue btn collision detection
                if (menuBtnRecs[CONTINUE].Contains(mouse.Position))
                {
                    //Button sfx
                    buttonSFX.CreateInstance().Play();
                    //Return user back to menu & reset game
                    if (speedrunTimer.GetTimePassed() < bestTime)
                    {
                        bestTimeTxt = speedrunTimer.GetTimePassedAsString(Timer.FORMAT_MIN_SEC_MIL);
                        bestTime = (int)speedrunTimer.GetTimePassed();
                    }
                    ResetGame();
                    gameState = MENU;
                }
            }
        }

        //////////////////////////////////////////////
        ////////////////////DRAWING///////////////////
        //////////////////////////////////////////////
        private void DrawMenu()
        {
            //Drawing background
            spriteBatch.Draw(mainBg, bgRec, Color.White);
            //Drawing title
            spriteBatch.DrawString(titleFont, "Mortal Knight", titleLoc, Color.Red);
            //Drawing ALL the menu buttons
            MenuBtnDrawer(START);
            MenuBtnDrawer(QUIT);
            MenuBtnDrawer(INFO);
            //Drawing clock
            spriteBatch.Draw(clockImg, clockLoc, Color.White);
            spriteBatch.DrawString(titleFont, bestTimeTxt, timeLoc, Color.White);
        }

        private void DrawInstructions()
        {
            //Drawing background
            spriteBatch.Draw(currentTutorialBg, bgRec, Color.White);
            //Drawing return button
            MenuBtnDrawer(INFO);
            if (currentTutorialBg == tutorial1Bg)
            {
                MenuBtnDrawer(NEXT);
            }
        }

        private void DrawGamePlay()
        {

            //Drawing the level assets depending on the progress of the level
            switch (levelProgress)
            {
                case 0:
                    //Drawing bg
                    spriteBatch.Draw(level1LeftBg, bgRec, Color.White);
                    spriteBatch.Draw(castleDoorImg, doorRec, Color.White);
                    trapAnims[LIGHTNING].Draw(spriteBatch, Color.White, Animation.FLIP_NONE);
                    //Drawing platforms
                    spriteBatch.Draw(platformImgs[0], platformRecs[0], Color.White);
                    spriteBatch.Draw(platformImgs[1], platformRecs[1], Color.White);
                    spriteBatch.Draw(platformImgs[2], platformRecs[2], Color.White);
                    break;
                case 1:
                    //Drawing bg
                    spriteBatch.Draw(level1RightBg, bgRec, Color.White);
                    //Drawing platforms
                    spriteBatch.Draw(platformImgs[0], platformRecs[0], Color.White);
                    break;
                case 2:
                    //Drawing bg
                    spriteBatch.Draw(level1LeftBg, bgRec, Color.White);
                    //Drawing platforms
                    spriteBatch.Draw(platformImgs[1], platformRecs[1], Color.White);
                    spriteBatch.Draw(platformImgs[2], platformRecs[2], Color.White);
                    spriteBatch.Draw(platformImgs[3], platformRecs[3], Color.White);
                    spriteBatch.Draw(platformImgs[4], platformRecs[4], Color.White);
                    break;

            }

            //Drawing charater animations depending on animState
            //When knight is facing right
            if (dir == RIGHT)
            {
                switch (animState)
                {
                    case 0:
                        characterAnims[IDLE].Draw(spriteBatch, Color.White, Animation.FLIP_NONE);
                        break;
                    case 1:
                        characterAnims[ATTACK].Draw(spriteBatch, Color.White, Animation.FLIP_NONE);
                        break;
                    case 2:
                        characterAnims[RUN].Draw(spriteBatch, Color.White, Animation.FLIP_NONE);
                        break;
                    case 3:
                        characterAnims[JUMP].Draw(spriteBatch, Color.White, Animation.FLIP_NONE);
                        break;
                }
            }
            //When knight is facing left
            else
            {
                switch (animState)
                {
                    case 0:
                        characterAnims[IDLE].Draw(spriteBatch, Color.White, Animation.FLIP_HORIZONTAL);
                        break;
                    case 1:
                        characterAnims[ATTACK].Draw(spriteBatch, Color.White, Animation.FLIP_HORIZONTAL);
                        break;
                    case 2:
                        characterAnims[RUN].Draw(spriteBatch, Color.White, Animation.FLIP_HORIZONTAL);
                        break;
                    case 3:
                        characterAnims[JUMP].Draw(spriteBatch, Color.White, Animation.FLIP_HORIZONTAL);
                        break;
                }
            }
            if (showCollisionRecs)
            {
                characterVisibleRecs[HEAD].Draw(spriteBatch, Color.Yellow * 0.5f, true);
                characterVisibleRecs[LEFT_BODY].Draw(spriteBatch, Color.Red * 0.5f, true);
                characterVisibleRecs[RIGHT_BODY].Draw(spriteBatch, Color.Blue * 0.5f, true);
                characterVisibleRecs[FEET].Draw(spriteBatch, Color.Green * 0.5f, true);
                enemyVisibleRecs[0].Draw(spriteBatch, Color.Red * 0.5f, true);
                enemyVisibleRecs[1].Draw(spriteBatch, Color.Green * 0.5f, true);
                trapVisibleRecs[0].Draw(spriteBatch, Color.Olive * 0.5f, true);
            }

            //Drawing ALL enemy animations (if enemy is alive)
            if (skeletonDir == LEFT && deadOrAlive[SKELETON] == true)
            {
                switch (skeletonAnimState)
                {
                    case 1:
                        skeletonAnims[RUN].Draw(spriteBatch, Color.White, Animation.FLIP_HORIZONTAL);
                        break;
                    case 2:
                        skeletonAnims[HIT].Draw(spriteBatch, Color.White, Animation.FLIP_HORIZONTAL);
                        break;
                }
            }
            else if (deadOrAlive[SKELETON] == true)
            {
                switch (skeletonAnimState)
                {
                    case 1:
                        skeletonAnims[RUN].Draw(spriteBatch, Color.White, Animation.FLIP_NONE);
                        break;
                    case 2:
                        skeletonAnims[HIT].Draw(spriteBatch, Color.White, Animation.FLIP_NONE);
                        break;
                }
            }
            if (batDir == LEFT && deadOrAlive[BAT] == true)
            {
                switch (batAnimState)
                {
                    case 1:
                        batAnims[RUN].Draw(spriteBatch, Color.White, Animation.FLIP_HORIZONTAL);
                        break;
                    case 2:
                        batAnims[HIT].Draw(spriteBatch, Color.White, Animation.FLIP_HORIZONTAL);
                        break;

                }
            }
            else if (deadOrAlive[BAT] == true)
            {
                switch (batAnimState)
                {
                    case 1:
                        batAnims[RUN].Draw(spriteBatch, Color.White, Animation.FLIP_NONE);
                        break;
                    case 2:
                        batAnims[HIT].Draw(spriteBatch, Color.White, Animation.FLIP_NONE);
                        break;
                }
            }

            //Drawing traps
            //hangingSawAnim.Draw(spriteBatch, Color.White, Animation.FLIP_NONE);

            //Drawing ALL Game UI
            //Health & empty bar
            spriteBatch.Draw(healthBar, healthBarRec.Rec, Color.White);
            spriteBatch.Draw(lockImgs[lockState], lockLoc, Color.White);
            spriteBatch.Draw(emptyBar, emptyBarRec, Color.White);
        }

        private void DrawPause()
        {
            //Drawing background
            spriteBatch.Draw(mainBg, bgRec, Color.White);
            //Drawing pause txt
            spriteBatch.DrawString(titleFont, "Pause", pauseLoc, Color.Red);
            //Drawing ALL the pause buttons
            MenuBtnDrawer(QUIT);
            MenuBtnDrawer(CONTINUE);
        }

        private void DrawGameOver()
        {
            //Drawing background
            spriteBatch.Draw(mainBg, bgRec, Color.White);
            spriteBatch.Draw(castleImg, bgRec, Color.White);
            //Drawing ui button
            MenuBtnDrawer(CONTINUE);

            //Drawing gameover txt
            spriteBatch.DrawString(titleFont, "Game Over", gameOverLoc, Color.Red);
        }

        private void DrawGameWin()
        {
            //Drawing background
            spriteBatch.Draw(gameWinBg, bgRec, Color.White);
            //Drawing ui button
            MenuBtnDrawer(CONTINUE);

            //Drawing game win text
            //Drawing ALL the gameover buttons
            spriteBatch.DrawString(titleFont, "You made it out of the castle!", gameWinLoc, Color.DarkGreen);
            spriteBatch.DrawString(titleFont, "Escape Time: " + speedrunTimer.GetTimePassedAsString(Timer.FORMAT_MIN_SEC_MIL), escapeTimeLoc, Color.White);
        }

        //Pre: Game Timer
        //Post: None
        //Desc: Handles all character movements
        private void CharacterMovement(GameTime gameTime)
        {
            //Gravity 
            characterSpeed.Y += forces.Y;

            //Check for collisions between the player and screen boundaries
            CharacterWallCollision();

            //Check for collisions between the player and ALL platforms
            PlatformCollision();

            //Check for collisions between the player and ALL traps
            TrapCollision();

            //Apply friction IF character is grounded
            if (isCharGrounded == true)
            {
                //Decelerate in the opposite direction the character is moving
                characterSpeed.X += -Math.Sign(characterSpeed.X) * forces.X;

                //IF character has decelerated enough, set horizontal speed to 0
                if (Math.Abs(characterSpeed.X) <= TOLERANCE)
                {
                    characterSpeed.X = 0f;
                }
            }

            /////////////////////////////
            //DETECTING movement inputs//
            /////////////////////////////
            //Checking to see if the player is running right
            if (kb.IsKeyDown(Keys.Right) || kb.IsKeyDown(Keys.D))
            {
                characterSpeed.X = characterSpeed.X + ACCELERATION;
                characterSpeed.X = MathHelper.Clamp(characterSpeed.X, -maxSpeed, maxSpeed);
                characterAnims[RUN].isAnimating = true;

                //Changing Direction
                dir = RIGHT;
                //Setting to running anim
                animState = 2;
            }
            //Checking to see if player is running left
            else if (kb.IsKeyDown(Keys.Left) || kb.IsKeyDown(Keys.A))
            {
                characterSpeed.X = characterSpeed.X - ACCELERATION;
                characterSpeed.X = MathHelper.Clamp(characterSpeed.X, -maxSpeed, maxSpeed);
                characterAnims[RUN].isAnimating = true;

                //Changing Direction
                dir = LEFT;

                //Setting to running anim
                animState = 2;
            }
            //Checking to see if player is attacking
            else if (kb.IsKeyDown(Keys.Up) || kb.IsKeyDown(Keys.W))
            {
                characterAnims[ATTACK].isAnimating = true;

                //Create swing instance
                if (animState != 1)
                {
                    swing = swingSFX.CreateInstance();
                }
                //Setting to attack anim
                animState = 1;
                //IF no swing sound is playing
                //THEN play swing sfx
                if (swing.State == SoundState.Stopped)
                {
                    swing.Play();
                }
            }
            //Checking to see if player is jumping
            //Only activatable if player is grounded
            else if (kb.IsKeyDown(Keys.Space) && isCharGrounded == true)
            {
                jumpSFX.CreateInstance().Play();
                characterSpeed.Y = jumpSpeed;
                characterAnims[JUMP].isAnimating = true;

                //Setting to jump anim
                animState = 3;
                //Char is off ground
                isCharGrounded = false;
            }
            //IF player is making no other actions THEN the character idles
            else
            {
                //Setting to idle anim
                animState = 0;
            }

            /////////////////////////////
            //UPDATING Speeds\Recs\Locs//
            /////////////////////////////

            //Updating character vector
            characterLoc.X += characterSpeed.X;
            characterLoc.Y += characterSpeed.Y;

            //Updating character rectangle
            characterRec.X = (int)characterLoc.X;
            characterRec.Y = (int)characterLoc.Y;

            //Updating character position to all anims
            for (int i = 0; i < 5; i++)
            {
                characterAnims[i].destRec.X = (int)characterRec.X;
                characterAnims[i].destRec.Y = (int)characterRec.Y;
            }
        }

        //Pre: Array of character body recs, and main character recs
        //Post: None
        //Desc: Updates all body character recs (Head, left/right torso, and feet)
        private void SetCharacterRecs(Rectangle[] characterRecs, Rectangle characterRec)
        {
            //HEAD rec is centred
            characterRecs[HEAD] = new Rectangle(characterAnims[IDLE].destRec.X + (int)(0.43f * characterAnims[IDLE].destRec.Width), characterAnims[IDLE].destRec.Y + (int)20f, (int)(characterAnims[IDLE].destRec.Width * 0.13f), (int)(characterAnims[IDLE].destRec.Height * 0.2f));

            //torso LEFT and RIGHT are located vertically below the HEAD
            characterRecs[LEFT_BODY] = new Rectangle((int)(characterAnims[IDLE].destRec.X + 100), characterRecs[HEAD].Y + characterRecs[HEAD].Height, (int)(characterAnims[IDLE].destRec.Width * 0.12f), (int)(characterAnims[IDLE].destRec.Height * 0.48f));
            characterRecs[RIGHT_BODY] = new Rectangle(characterRecs[LEFT_BODY].X + characterRecs[LEFT_BODY].Width, characterRecs[HEAD].Y + characterRecs[HEAD].Height, (int)(characterAnims[IDLE].destRec.Width * 0.12f), (int)(characterAnims[IDLE].destRec.Height * 0.48f));

            //FEET are located below torso rectangles
            characterRecs[FEET] = new Rectangle(characterAnims[IDLE].destRec.X + (int)(0.43f * characterAnims[IDLE].destRec.Width), characterRecs[LEFT_BODY].Y + (int)(characterRecs[LEFT_BODY].Height * 1.03), (int)(characterAnims[IDLE].destRec.Width * 0.13f), (int)(characterAnims[IDLE].destRec.Height * 0.225f));

            //Now setup the visible recs when necessary
            if (showCollisionRecs == true)
            {
                characterVisibleRecs[HEAD] = new GameRectangle(GraphicsDevice, characterRecs[HEAD]);
                characterVisibleRecs[LEFT_BODY] = new GameRectangle(GraphicsDevice, characterRecs[LEFT_BODY]);
                characterVisibleRecs[RIGHT_BODY] = new GameRectangle(GraphicsDevice, characterRecs[RIGHT_BODY]);
                characterVisibleRecs[FEET] = new GameRectangle(GraphicsDevice, characterRecs[FEET]);
                enemyVisibleRecs[0] = new GameRectangle(GraphicsDevice, skeletonRec);
                enemyVisibleRecs[1] = new GameRectangle(GraphicsDevice, batRec);
                trapVisibleRecs[0] = new GameRectangle(GraphicsDevice, trapRecs[0]);
            }
        }

        //Pre: None
        //Post: None
        //Desc: Tests the character against every platform for collision and adjust the object as necessary
        private void PlatformCollision()
        {
            bool collision = false;

            if (characterRec.Intersects(platformRecs[0]))
            {
                //Translate the character to just outside of the collision location depending on body part
                if (characterRecs[FEET].Intersects(platformRecs[0]))
                {
                    characterRec.Y = platformRecs[0].Y - characterAnims[animState].destRec.Height;
                    characterAnims[animState].destRec.Y = characterRec.Y;//platformRecs[i].Y - characterRec.Height;
                    characterLoc.Y = characterRec.Y;
                    characterSpeed.Y = 0f;
                    isCharGrounded = true;
                    collision = true;
                }
            }

                //Test collsion between the character and ALL platforms
                for (int i = 1; i < platformRecs.Length; i++)
                {
                //Check for a general collision first 
                if (characterRec.Intersects(platformRecs[i]))
                {
                    //Translate the character to just outside of the collision location depending on body part
                    if (characterRecs[FEET].Intersects(platformRecs[i]))
                    {
                        characterRec.Y = platformRecs[i].Y - characterAnims[animState].destRec.Height;
                        characterAnims[animState].destRec.Y = characterRec.Y;//platformRecs[i].Y - characterRec.Height;
                        characterLoc.Y = characterRec.Y;
                        characterSpeed.Y = 0f;
                        isCharGrounded = true;
                        collision = true;
                    }
                    else if (characterRecs[LEFT_BODY].Intersects(platformRecs[i]))
                    {
                        characterRec.X = (int)(platformRecs[i].Right / 1.3f);
                        characterAnims[animState].destRec.X = characterRec.X;// platformRecs[i].Right + 1;
                        characterLoc.X = characterRec.X;
                        characterSpeed.X = 0;
                        collision = true;
                    }
                    else if (characterRecs[RIGHT_BODY].Intersects(platformRecs[i]))
                    {
                        characterAnims[animState].destRec.X = platformRecs[i].X - characterRec.Width - 1;
                        characterLoc.X = characterRec.X;
                        characterSpeed.X = 0;
                        collision = true;
                    }
                    else if (characterRecs[HEAD].Intersects(platformRecs[i]))
                    {
                        characterAnims[animState].destRec.Y = platformRecs[i].Bottom + 1;
                        characterLoc.Y = characterRec.Y;
                        characterSpeed.Y = characterSpeed.Y * -1;
                        collision = true;
                    }

                    //If a collision occured, the character was moved.  It's collision rectangles need
                    //to be adjusted as well
                    if (collision == true)
                    {
                        SetCharacterRecs(characterRecs, characterRec);
                        collision = false;
                    }
                }
            }
        }

        //Pre: None
        //Post: None
        //Desc: Tests the character rec for colliion against all traps
        private void TrapCollision()
        {
            for (int i = 0; i < trapRecs.Length; i++)
            {
                //Trigger health function if character hits any trap
                if (characterRecs[RIGHT_BODY].Intersects(trapRecs[i]))
                {
                    CharacterHealthTrigger();
                }
                else if (characterRecs[LEFT_BODY].Intersects(trapRecs[i]))
                {
                    CharacterHealthTrigger();
                }
            }
        }

        //Pre: None
        //Post: None
        //Desc: Detect wall collision with the player and stop their movement and keep them on screen
        private void CharacterWallCollision()
        {
            if (characterRecs[RIGHT_BODY].X < 0 - (characterWidth / 10))
            {
                CharacterHealthTrigger();
            }
            if (characterRecs[RIGHT_BODY].X > 1200 - (characterWidth / 10))
            {
                CharacterHealthTrigger();  
            }
            else if (characterRecs[FEET].Y > screenHeight)
            {
                CharacterHealthTrigger();
            }
        }

        //Pre: Game Timer
        //Post: None
        //Desc: Updates all timers with the game timer
        private void UpdatingAllTimers(GameTime gameTime)
        {
            //Animation Timer updates
            for (int mainCounter = 0; mainCounter < 6; mainCounter++)
            {
                characterAnims[mainCounter].Update(gameTime);

                if (mainCounter < 5)
                {
                    skeletonAnims[mainCounter].Update(gameTime);
                }
                if (mainCounter != 0 && mainCounter < 5)
                {
                    batAnims[mainCounter].Update(gameTime);
                }
            }
            //Trap Timer updates
            trapAnims[LIGHTNING].Update(gameTime);
            rollTimer.Update(gameTime.ElapsedGameTime.Milliseconds);
            //ALL timers update
            speedrunTimer.Update(gameTime.ElapsedGameTime.Milliseconds);
            hitTimer.Update(gameTime.ElapsedGameTime.Milliseconds);
        }

        //Pre: An int that determins the button to draw
        //Post: None
        //Desc: Draws a button according to the btnState; 
        private void MenuBtnDrawer(int btnState)
        {
            if (gameState == GAMEOVER)
            {
                menuBtnsLocs[CONTINUE] = new Vector2((screenWidth / 2.5f), screenHeight / 1.7f);
                menuBtnRecs[3] = new Rectangle((int)menuBtnsLocs[3].X, (int)menuBtnsLocs[3].Y, menuBtns[3].Width, menuBtns[3].Height);
            }
            else
            {
                menuBtnsLocs[CONTINUE] = new Vector2(screenWidth - (titleFont.MeasureString("Mortal Knight").X * 1.1f) + menuBtns[CONTINUE].Width * 0.94f, screenHeight / 2.7f);
                menuBtnRecs[3] = new Rectangle((int)menuBtnsLocs[3].X, (int)menuBtnsLocs[3].Y, menuBtns[3].Width, menuBtns[3].Height);
            }
            //Draws the menu button 
            spriteBatch.Draw(menuBtns[btnState], menuBtnsLocs[btnState], Color.White);
            //If user is hovering over button change the colour to show the detection
            if (menuBtnRecs[btnState].Contains(mouse.Position))
            {
                spriteBatch.Draw(menuBtns[btnState], menuBtnsLocs[btnState], Color.SaddleBrown * 0.75f);
            }
        }

        //Pre: None
        //Post: None
        //Desc: Updates the health bar and resets player back to latest checkpoint 
        private void CharacterHealthTrigger()
        {
            characterHealth--;
            healthLossSFX.CreateInstance().Play();
            healthBarRec = new GameRectangle(GraphicsDevice, 50, (int)33.2f, (int)(healthBar.Width * (Convert.ToDouble(characterHealth) / Convert.ToDouble(CHARACTER_MAX_HEALTH))), healthBar.Height);
            characterRec.X = (int)currentCheckpoint.X;
            characterRec.Y = (int)currentCheckpoint.Y;
            characterLoc = new Vector2(characterRec.X, characterRec.Y);

            if (characterHealth <= 0)
            {
                animState = 5;
            }
        }

        //Pre: None
        //Post: None
        //Desc: Updates enemies health bar
        private void EnemyHealthTrigger(int enemyType)
        {
            if (enemyType == BAT)
            {
                batHealth--;
                if (batHealth == 0)
                {
                    deadOrAlive[BAT] = false;
                }
            }
            if (enemyType == SKELETON)
            {
                skeletonHealth--; 
                if (skeletonHealth == 0)
                {
                    deadOrAlive[SKELETON] = false; 
                }
            }

            swingHit = swingHitSFX.CreateInstance();
            if (swingHit.State == SoundState.Stopped)
            {
                swingHit.Play();
            }
        }

        //Pre: Speed, Locs, Recs, Anims, Direction, and the type of enemy is required for the AI logic
        //Post: None
        //Desc: Handles the logic of the enemy movement and updates their positions
        private void EnemyAI(Vector2[] enemyLocs, Vector2 enemySpeed, Rectangle enemyRec, Animation[] enemyAnims, int enemyType, int enemyDir, int enemyAnimState)
        {
            if (animState != 1)
            {
                skeletonAnimState = 1;
                batAnimState = 1;
            }
            enemyAnims[RUN].isAnimating = true;

            //Enemy Movement
            if (enemyDir == LEFT && enemyAnimState == 1)
            {
                enemySpeed.X = enemySpeed.X - ENEMY_ACCELERATION;
                enemySpeed.X = MathHelper.Clamp(enemySpeed.X, -enemyMaxSpeed, enemyMaxSpeed);
            }
            else if (enemyAnimState == 1)
            {
                enemySpeed.X = enemySpeed.X + ENEMY_ACCELERATION;
                enemySpeed.X = MathHelper.Clamp(enemySpeed.X, -enemyMaxSpeed, enemyMaxSpeed);
            }

            //Enemy Hit Detection
            if (enemyRec.Intersects(characterRecs[RIGHT_BODY]))
            {
                CharacterHealthTrigger(); 
            }

            //Updating skeleton vector
            enemyLocs[0].X += enemySpeed.X;
            enemyLocs[0].Y += enemySpeed.Y;

            //Updating skeleton rectangle
            enemyRec.X = (int)enemyLocs[0].X;
            enemyRec.Y = (int)enemyLocs[0].Y;

            //Updating skeleton position to all anims
            for (int i = 0 + enemyType; i < 5; i++)
            {
                enemyAnims[i].destRec.X = (int)enemyRec.X;
                enemyAnims[i].destRec.Y = (int)enemyRec.Y;
            }
        }

        //Pre: None
        //Post: None
        //Desc: Sets the recs of assets according to the level progress
        private void settingAssetRecs()
        {
            switch (levelProgress)
            {
                case 0:
                    platformRecs[0] = new Rectangle(0, screenHeight - platformImgs[0].Height, platformImgs[0].Width, platformImgs[0].Height);
                    platformRecs[1] = new Rectangle(screenWidth / 5, (int)(screenHeight / 1.5f), platformImgs[1].Width, platformImgs[1].Height);
                    platformRecs[2] = new Rectangle(screenWidth / 2, (int)(screenHeight / 2f), platformImgs[2].Width, platformImgs[2].Height);
                    for (int i = 3; i < 5; i++)
                    {
                        platformRecs[i].X = 1500;
                    }
                    doorRec = new Rectangle((int)(screenWidth - (castleDoorImg.Width / 1.15f)), (int)(screenHeight - (castleDoorImg.Height * 1.4f)), castleDoorImg.Width, castleDoorImg.Height);
                    break;
                case 1:
                    platformRecs[0] = new Rectangle(0, screenHeight - (int)(platformImgs[0].Height * 1.9f), platformImgs[0].Width, platformImgs[0].Height);
                    currentCheckpoint = checkpointLocs[1];
                    //doorRec.X = 1500;
                    for (int i = 1; i <= 4; i++)
                    {
                        platformRecs[i].X = 1500;
                    }
                    break;
                case 2:
                    platformRecs[0].X = 1500;
                    platformRecs[1] = new Rectangle(0, (int)(screenHeight / 1.5f), platformImgs[1].Width, platformImgs[1].Height);
                    platformRecs[2] = new Rectangle(screenWidth / 6, (int)(screenHeight / 2f), platformImgs[2].Width, platformImgs[2].Height);
                    platformRecs[3] = new Rectangle(screenWidth / 2, screenHeight / 2, platformImgs[3].Width, platformImgs[3].Height);
                    platformRecs[4] = new Rectangle(screenWidth - platformImgs[4].Width, screenHeight / 2, platformImgs[4].Width, platformImgs[4].Height);
                    break;
            }
        }

        //Pre: None
        //Post: None
        //Desc: Repositions enemies according to level progress
        private void settingEnemyRecs()
        {
            switch (levelProgress)
            {
                case 1:
                    skeletonLocs[0] = skeletonPlacements[1];
                    batLocs[0] = batPlacements[1];
                    //Skeleton Anims
                    skeletonAnims[IDLE] = new Animation(skeletonImgs[IDLE], 4, 1, 4, 0, 0, Animation.ANIMATE_FOREVER, 12, skeletonLocs[0], 1.6f, true);
                    skeletonAnims[RUN] = new Animation(skeletonImgs[RUN], 4, 1, 4, 0, 0, Animation.ANIMATE_FOREVER, 5, skeletonLocs[0], 1.6f, false);
                    skeletonAnims[ATTACK] = new Animation(skeletonImgs[ATTACK], 8, 1, 8, 0, 0, Animation.ANIMATE_ONCE, 3, skeletonLocs[0], 1.6f, false);
                    skeletonAnims[KILLED] = new Animation(skeletonImgs[KILLED], 4, 1, 4, 0, 0, Animation.ANIMATE_ONCE, 5, skeletonLocs[0], 1.6f, false);
                    skeletonAnims[HIT] = new Animation(skeletonImgs[HIT], 4, 1, 4, 0, 0, Animation.ANIMATE_FOREVER, 10, skeletonLocs[0], 1.6f, true);
                    ///Bat Anims
                    batAnims[RUN] = new Animation(batImgs[RUN], 8, 1, 8, 0, 0, Animation.ANIMATE_FOREVER, 5, batLocs[0], 1.9f, true);
                    batAnims[ATTACK] = new Animation(batImgs[ATTACK], 8, 1, 8, 0, 0, Animation.ANIMATE_ONCE, 3, batLocs[0], 1.9f, false);
                    batAnims[KILLED] = new Animation(batImgs[KILLED], 4, 1, 4, 0, 0, Animation.ANIMATE_ONCE, 5, batLocs[0], 1.9f, false);
                    batAnims[HIT] = new Animation(batImgs[HIT], 4, 1, 4, 0, 0, Animation.ANIMATE_FOREVER, 10, batLocs[0], 1.9f, true);
                    break;
                case 2:
                    skeletonLocs[0] = skeletonPlacements[2];
                    batLocs[0] = batPlacements[2];
                    //Skeleton Anims
                    skeletonAnims[IDLE] = new Animation(skeletonImgs[IDLE], 4, 1, 4, 0, 0, Animation.ANIMATE_FOREVER, 12, skeletonLocs[0], 1.6f, true);
                    skeletonAnims[RUN] = new Animation(skeletonImgs[RUN], 4, 1, 4, 0, 0, Animation.ANIMATE_FOREVER, 5, skeletonLocs[0], 1.6f, false);
                    skeletonAnims[ATTACK] = new Animation(skeletonImgs[ATTACK], 8, 1, 8, 0, 0, Animation.ANIMATE_ONCE, 3, skeletonLocs[0], 1.6f, false);
                    skeletonAnims[KILLED] = new Animation(skeletonImgs[KILLED], 4, 1, 4, 0, 0, Animation.ANIMATE_ONCE, 5, skeletonLocs[0], 1.6f, false);
                    skeletonAnims[HIT] = new Animation(skeletonImgs[HIT], 4, 1, 4, 0, 0, Animation.ANIMATE_FOREVER, 10, skeletonLocs[0], 1.6f, true);
                    ///Bat Anims
                    batAnims[RUN] = new Animation(batImgs[RUN], 8, 1, 8, 0, 0, Animation.ANIMATE_FOREVER, 5, batLocs[0], 1.9f, true);
                    batAnims[ATTACK] = new Animation(batImgs[ATTACK], 8, 1, 8, 0, 0, Animation.ANIMATE_ONCE, 3, batLocs[0], 1.9f, false);
                    batAnims[KILLED] = new Animation(batImgs[KILLED], 4, 1, 4, 0, 0, Animation.ANIMATE_ONCE, 5, batLocs[0], 1.9f, false);
                    batAnims[HIT] = new Animation(batImgs[HIT], 4, 1, 4, 0, 0, Animation.ANIMATE_FOREVER, 10, batLocs[0], 1.9f, true);
                    break;
            }
        }


        //Pre: None
        //Post: None
        //Desc: Checks and changes the direction that enemies are moving (according to the level)
        private void EnemyPaths()
        {
            switch (levelProgress)
            {
                case 0:
                    //Skeleton path
                    if (skeletonRec.X <= 600)
                    {
                        skeletonDir = RIGHT;
                    }
                    if (skeletonRec.X >= 900)
                    {
                        skeletonDir = LEFT;
                    }
                    //Bat path
                    if (batRec.X <= 200)
                    {
                        batDir = RIGHT; 
                    }
                    if (batRec.X >= 400)
                    {
                        batDir = LEFT; 
                    }
                    break;
                case 1:
                    //Skeleton path
                    if (skeletonRec.X <= 220)
                    {
                        skeletonDir = RIGHT;
                    }
                    if (skeletonRec.X >= 400)
                    {
                        skeletonDir = LEFT;
                    }
                    //Bat path
                    if (batRec.X <= 800)
                    {
                        batDir = RIGHT;
                    }
                    if (batRec.X >= 1100)
                    {
                        batDir = LEFT;
                    }
                    break;
                case 2:
                    //Skeleton path
                    if (skeletonRec.X <= 200)
                    {
                        skeletonDir = RIGHT;
                    }
                    if (skeletonRec.X >= 270)
                    {
                        skeletonDir = LEFT;
                    }
                    //Bat path
                    if (batRec.X <= 580)
                    {
                        batDir = RIGHT;
                    }
                    if (batRec.X >= 800)
                    {
                        batDir = LEFT;
                    }
                    break;
            }
        }

        //Pre: None
        //Post: None
        //Desc: Resets game for another playthrough
        private void ResetGame()
        {
            //Resetting all health values
            characterHealth = CHARACTER_MAX_HEALTH;
            healthBarRec = new GameRectangle(GraphicsDevice, 50, (int)33.2f, (int)(healthBar.Width * (Convert.ToDouble(characterHealth) / Convert.ToDouble(CHARACTER_MAX_HEALTH))), healthBar.Height);
            batHealth = BAT_MAX_HEALTH;
            skeletonHealth = SKELETON_MAX_HEALTH; 
            //Resetting all directions
            dir = RIGHT;
            skeletonDir = LEFT;
            batDir = LEFT;
            //Resetting level progress and lock
            levelProgress = 0;
            lockState = LOCKED;
            //Resetting character speed
            characterSpeed = new Vector2(0, 0);
            //Resetting animation state
            animState = 0;
            //Resetting character grounded state
            isCharGrounded = true;
            //Resetting enemy locs
            skeletonLocs[0] = skeletonPlacements[0];
            batLocs[0] = batPlacements[0];
            //Resetting enemy alive/dead states
            deadOrAlive[SKELETON] = true;
            deadOrAlive[BAT] = true; 
        }
    }
}
