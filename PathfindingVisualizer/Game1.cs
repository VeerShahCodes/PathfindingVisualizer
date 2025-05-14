using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
namespace PathfindingVisualizer;

public class Game1 : Game
{
    private GraphicsDeviceManager graphics;
    private SpriteBatch spriteBatch;
    private int gridSize;
    private int screenMargin;
    private int screenWidth;
    private int screenHeight;


    public Game1()
    {
        graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here
        gridSize = 20;
        screenMargin = 125;
        screenHeight = 750;
        screenWidth = 750;
        graphics.PreferredBackBufferHeight = screenHeight;
        graphics.PreferredBackBufferWidth = screenWidth;
        graphics.ApplyChanges();

        base.Initialize();
    }

    protected override void LoadContent()
    {
        spriteBatch = new SpriteBatch(GraphicsDevice);

        // TODO: use this.Content to load your game content here
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        // TODO: Add your update logic here

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {

        GraphicsDevice.Clear(Color.White);
        spriteBatch.Begin();
        var mouseState = Mouse.GetState();
        Point mousePosition = new Point(mouseState.X, mouseState.Y);
        int hoverX = ((mousePosition.X - screenMargin) / gridSize) * gridSize + screenMargin;
        int hoverY = ((mousePosition.Y - screenMargin) / gridSize) * gridSize + screenMargin;
        Rectangle hoveredCell = new Rectangle(hoverX, hoverY, gridSize, gridSize);
        for(int rows = screenMargin; rows < screenWidth - screenMargin; rows+=gridSize) {
            for(int cols = screenMargin; cols < screenHeight - screenMargin; cols+=gridSize) {
                Rectangle rect = new Rectangle(rows, cols, gridSize, gridSize);
                Color color = Color.LightGray;
                if(rect == hoveredCell) { //if is hovered change to black
                    color = Color.LightGreen;
                    spriteBatch.FillRectangle(rect, color);
                }
                else {
                    spriteBatch.DrawRectangle(rect, color);
                }
                
            }
        }
        base.Draw(gameTime);
        spriteBatch.End();

    }
}
