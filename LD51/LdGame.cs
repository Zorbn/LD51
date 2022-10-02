using System;
using System.Collections.Generic;
using LD51.Disasters;
using LD51.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace LD51;

/*
 * TODO IDEAS:
 * Power-ups?
 * Networking?
 */
public class LdGame : Game
{
    public const int UiPadding = 2;
    public const float DisasterTime = 10f;
    public const int DisasterLevelThreshold = 3;
    public const int BaseDisasterLevel = 1;
    public const int MaxDisasterLevel = 4;
    public const int VirtualScreenWidth = 320;
    public const int VirtualScreenHeight = 180;
    public const int WindowDefaultSizeMultiplier = 2;
    public const string PlayButtonText = "[ Play ]";
    public const string RespawnButtonText = "[ Respawn ]";
    public const string QuitButtonText = "[ Quit ]";

    private readonly GraphicsDeviceManager graphics;
    private readonly Random random = new();

    private TextureAtlas atlas;
    private Camera camera;
    private string currentDisasterName;

    private int disasterLevel;
    private Queue<Disaster> disasters;
    private float disasterTimer;
    private GameStates gameState;
    private Input input;
    private ParticleSystem particleSystem;
    private Player player;
    private SpriteBatch spriteBatch;
    private TextRenderer textRenderer;
    private TileMap tileMap;
    private Camera uiCamera;
    private int wavesCompleted;

    public LdGame()
    {
        graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        Window.AllowUserResizing = true;
        graphics.PreferredBackBufferWidth = VirtualScreenWidth * WindowDefaultSizeMultiplier;
        graphics.PreferredBackBufferHeight = VirtualScreenHeight * WindowDefaultSizeMultiplier;
        graphics.ApplyChanges();
    }

    protected override void Initialize()
    {
        gameState = GameStates.MainMenu;

        atlas = new TextureAtlas(GraphicsDevice, "Content/atlas.png", 8);
        textRenderer = new TextRenderer(0, 6, 32);

        camera = new Camera(VirtualScreenWidth, VirtualScreenHeight);
        uiCamera = new Camera(VirtualScreenWidth, VirtualScreenHeight, false);
        input = new Input();

        base.Initialize();
    }

    protected override void LoadContent()
    {
        spriteBatch = new SpriteBatch(GraphicsDevice);
    }

    protected override void Update(GameTime gameTime)
    {
        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape)) Exit();

        input.Update();

        switch (gameState)
        {
            case GameStates.MainMenu:
                UpdateMainMenu(deltaTime);
                break;
            case GameStates.InGame:
                UpdateInGame(deltaTime);
                break;
            case GameStates.Dead:
                UpdateDead(deltaTime);
                break;
        }

        input.ResetButtons();

        base.Update(gameTime);
    }

    private void UpdateMainMenu(float deltaTime)
    {
        if (input.WasButtonPressed(camera, "Play"))
        {
            SetupGame();
            gameState = GameStates.InGame;
        }

        if (input.WasButtonPressed(camera, "Quit")) Exit();
    }

    private void UpdateInGame(float deltaTime)
    {
        int dirX = 0, dirY = 0;

        if (input.IsKeyDown(Keys.A)) dirX -= 1;

        if (input.IsKeyDown(Keys.D)) dirX += 1;

        if (input.IsKeyDown(Keys.W)) dirY -= 1;

        if (input.IsKeyDown(Keys.S)) dirY += 1;

        player.Update(tileMap, deltaTime);
        player.Move(tileMap, dirX, dirY, deltaTime);
        camera.GoTo(player.Sprite.Center, tileMap);

        player.Sprite.LookAt(camera.GetMousePosition());

        var blockingSprites = new List<Sprite>();

        foreach (Disaster disaster in disasters) blockingSprites.AddRange(disaster.GetBlockingSprites());

        foreach (Disaster disaster in disasters) disaster.Update(random, player, tileMap, blockingSprites, deltaTime);

        disasterTimer -= deltaTime;

        if (disasterTimer < 0f)
        {
            disasterTimer = DisasterTime;

            if (disasters.Count > 0)
            {
                if (disasters.Count > disasterLevel) disasters.Dequeue().Vanish(atlas, tileMap, particleSystem);

                wavesCompleted++;

                disasterLevel = BaseDisasterLevel + wavesCompleted / DisasterLevelThreshold;
                if (disasterLevel > MaxDisasterLevel) disasterLevel = MaxDisasterLevel;
            }

            Disaster newDisaster = CreateDisaster();
            currentDisasterName = newDisaster.Name;
            disasters.Enqueue(newDisaster);
        }

        particleSystem.Update(deltaTime);

        if (player.Health <= 0) gameState = GameStates.Dead;
    }

    private Disaster CreateDisaster()
    {
        const int disasterCount = 4;
        int disasterI = random.Next(disasterCount);

        return disasterI switch
        {
            0 => new ZombieDisaster(random, atlas, tileMap, particleSystem),
            1 => new TornadoDisaster(random, atlas, tileMap, particleSystem),
            2 => new AnvilDisaster(random, atlas, tileMap, particleSystem),
            3 => new LavaDisaster(random, atlas, tileMap, particleSystem),
            _ => throw new ArgumentOutOfRangeException($"No disaster corresponds to an index of {disasterI}")
        };
    }

    private void UpdateDead(float deltaTime)
    {
        if (input.WasButtonPressed(camera, "Respawn"))
        {
            SetupGame();
            gameState = GameStates.InGame;
        }

        if (input.WasButtonPressed(camera, "Quit")) Exit();
    }

    private void SetupGame()
    {
        particleSystem = new ParticleSystem();

        tileMap = new TileMap(GraphicsDevice, 40, 40, 16);
        tileMap.Generate(random);

        float mapCenterX = tileMap.Width * tileMap.TileSize * 0.5f;
        float mapCenterY = tileMap.Height * tileMap.TileSize * 0.5f;
        player = new Player(atlas, new Vector2(mapCenterX, mapCenterY), 0, 16, 16);
        camera.GoTo(player.Sprite.Center, tileMap);

        disasters = new Queue<Disaster>();
        disasterTimer = DisasterTime;
        disasterLevel = BaseDisasterLevel;
        wavesCompleted = 0;
        currentDisasterName = "";
    }

    protected override void Draw(GameTime gameTime)
    {
        camera.ScaleToScreen(Window.ClientBounds.Width, Window.ClientBounds.Height);
        uiCamera.ScaleToScreen(Window.ClientBounds.Width, Window.ClientBounds.Height);

        switch (gameState)
        {
            case GameStates.MainMenu:
                DrawMainMenu();
                break;
            case GameStates.InGame:
                DrawInGame();
                break;
            case GameStates.Dead:
                DrawDead();
                break;
        }

        base.Draw(gameTime);
    }

    private void DrawMainMenu()
    {
        GraphicsDevice.Clear(GameColors.Brown);
        spriteBatch.Begin(samplerState: SamplerState.PointClamp);

        var titleTextPos = new Vector2(camera.ViewWidth / 2f, camera.ViewHeight / 2f - atlas.TileSize * 2f);
        textRenderer.DrawTextWithShadow(spriteBatch, uiCamera, atlas, titleTextPos, "Disastrous", GameColors.White,
            GameColors.Black, true, true);

        var playTextPos = new Vector2(camera.ViewWidth / 2f, camera.ViewHeight / 2f + atlas.TileSize * 2f);
        DrawButton(playTextPos, PlayButtonText, "Play");
        Vector2 quitTextPos = playTextPos + new Vector2(0f, atlas.TileSize * 2f);
        DrawButton(quitTextPos, QuitButtonText, "Quit");

        spriteBatch.End();
    }

    private void DrawInGame()
    {
        tileMap.PreDraw(GraphicsDevice, spriteBatch, atlas);

        GraphicsDevice.Clear(GameColors.Blue);
        spriteBatch.Begin(samplerState: SamplerState.PointClamp);

        tileMap.Draw(spriteBatch, camera);
        player.Draw(spriteBatch, atlas, camera);

        foreach (Disaster disaster in disasters) disaster.Draw(spriteBatch, atlas, camera);

        particleSystem.Draw(spriteBatch, camera, atlas);

        // Draw health bar.
        for (var i = 0; i < player.Health; i += 2)
        {
            var texX = 0;

            if (i == player.Health - 1) texX = 1;

            atlas.Draw(spriteBatch, uiCamera, new Vector2(i * atlas.TileSize * 0.5f + UiPadding, UiPadding), texX, 27,
                1,
                1, GameColors.White);
        }

        if (player.Health < 1)
            atlas.Draw(spriteBatch, uiCamera, new Vector2(UiPadding, UiPadding), 2, 27, 1,
                1, GameColors.White);

        // Draw timer.
        float timerY = UiPadding * 2 + atlas.TileSize;
        atlas.Draw(spriteBatch, uiCamera, new Vector2(UiPadding, timerY), disasterLevel - BaseDisasterLevel, 26, 1,
            1, GameColors.White);
        textRenderer.DrawTextWithShadow(spriteBatch, uiCamera, atlas,
            new Vector2(UiPadding * 2 + atlas.TileSize, timerY),
            $"{(int)MathF.Ceiling(disasterTimer)}", GameColors.White, GameColors.Black);

        // Draw wave counter.
        float wavesY = UiPadding * 3 + atlas.TileSize * 2;
        atlas.Draw(spriteBatch, uiCamera, new Vector2(UiPadding, wavesY), 0, 25, 1,
            1, GameColors.White);
        textRenderer.DrawTextWithShadow(spriteBatch, uiCamera, atlas,
            new Vector2(UiPadding * 2 + atlas.TileSize, wavesY),
            $"{wavesCompleted}", GameColors.White, GameColors.Black);

        // Draw wave title.
        if (disasters.Count > 0)
            textRenderer.DrawTextWithShadow(spriteBatch, uiCamera, atlas, new Vector2(camera.ViewWidth / 2f, UiPadding),
                currentDisasterName, GameColors.White, GameColors.Black, true);

        spriteBatch.End();
    }

    private void DrawDead()
    {
        GraphicsDevice.Clear(GameColors.Blue);

        DrawInGame();

        spriteBatch.Begin(samplerState: SamplerState.PointClamp);

        var youDiedTextPos = new Vector2(camera.ViewWidth / 2f, camera.ViewHeight / 2f - atlas.TileSize * 2f);
        textRenderer.DrawTextWithShadow(spriteBatch, uiCamera, atlas, youDiedTextPos, "You died!", GameColors.White,
            GameColors.Black, true, true);

        var respawnTextPos = new Vector2(camera.ViewWidth / 2f, camera.ViewHeight / 2f + atlas.TileSize * 2f);
        DrawButton(respawnTextPos, RespawnButtonText, "Respawn");
        Vector2 quitTextPos = respawnTextPos + new Vector2(0f, atlas.TileSize * 2f);
        DrawButton(quitTextPos, QuitButtonText, "Quit");

        spriteBatch.End();
    }

    private void DrawButton(Vector2 position, string text, string name)
    {
        Rectangle rect = textRenderer.GetTextRect(atlas, position, text, true, true);
        Color color = rect.IsHovered(camera) ? GameColors.Yellow : GameColors.White;
        textRenderer.DrawTextWithShadow(spriteBatch, uiCamera, atlas, position, text, color,
            GameColors.Black, true, true);

        input.AddButtonRect(name, rect);
    }
}