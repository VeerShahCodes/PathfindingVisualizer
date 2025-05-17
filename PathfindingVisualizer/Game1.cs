using System;
using System.Collections.Generic;
using Microsoft.VisualBasic;
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
    private int mouseX;
    private int mouseY;
    private MouseState mouseState;
    private MouseState previousState;

    private Rectangle hoveredBox;
    private int graphWidth;
    private int graphHeight;

    private Graph<Point> graph;
    private bool isFirstSelection;
    private Rectangle startingBlock;
    private Rectangle endingBlock;
    private float fontScale;

    //textures ands fonts
    SpriteFont distanceFont;
    public Game1()
    {
        graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        gridSize = 20;
        screenMargin = 125;
        screenHeight = 750;
        screenWidth = 750;
        graphics.PreferredBackBufferHeight = screenHeight;
        graphics.PreferredBackBufferWidth = screenWidth;
        graphics.ApplyChanges();

        fontScale = gridSize / 50f;

        isFirstSelection = true;

        startingBlock = new Rectangle();
        endingBlock = new Rectangle();

        graphWidth = (screenWidth - screenMargin * 2) / gridSize + 1;
        graphHeight = (screenHeight - screenMargin * 2) / gridSize + 1;
        //init graph
        graph = new Graph<Point>();

        //create vertexes
        for(int rows = 0; rows < graphWidth; rows++) {
            for (int cols = 0; cols < graphHeight; cols++)
            {
                graph.AddVertex(new Point(rows, cols));
                graph.Search(new Point(rows, cols)).Owner = graph;
            }
        }
        //create edges
        for(int i = 0; i < graphWidth; i++) {
            for(int j = 0; j < graphHeight; j++) {

                if (i == graphWidth - 1 && j == graphHeight - 1)
                {

                }
                else if (i == graphWidth - 1)
                {
                    graph.AddUndirectedEdge(new Point(i, j), new Point(i, j + 1), 1);
                }
                else if (j == graphHeight - 1)
                {
                    graph.AddUndirectedEdge(new Point(i, j), new Point(i + 1, j), 1);
                    graph.AddUndirectedEdge(new Point(i, j), new Point(i + 1, j - 1), (float)Math.Sqrt(2));
                }
                else
                {
                    graph.AddUndirectedEdge(new Point(i, j), new Point(i, j + 1), 1);
                    graph.AddUndirectedEdge(new Point(i, j), new Point(i + 1, j), 1);
                    graph.AddUndirectedEdge(new Point(i, j), new Point(i + 1, j + 1), (float)Math.Sqrt(2));
                    graph.AddUndirectedEdge(new Point(i, j), new Point(i + 1, j - 1), (float)Math.Sqrt(2));
                }

                
            }
        }


        for (int i = 0; i < graph.Edges.Count; i++)
        {
            Console.WriteLine($"Point 1:{graph.Edges[i].StartingPoint.Value}, Point 2:{graph.Edges[i].EndingPoint.Value}, Distance: {graph.Edges[i].Distance}");
        }
        base.Initialize();
    }

    protected override void LoadContent()
    {
        spriteBatch = new SpriteBatch(GraphicsDevice);
        distanceFont = Content.Load<SpriteFont>("distanceFont");

    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        mouseState = Mouse.GetState();


        mouseX = ((mouseState.X - screenMargin) / gridSize) * gridSize + screenMargin;
        mouseY = ((mouseState.Y - screenMargin) / gridSize) * gridSize + screenMargin;
        hoveredBox = new Rectangle(mouseX, mouseY, gridSize, gridSize);
        for(int rows = screenMargin; rows < screenWidth - screenMargin; rows+=gridSize) {
            for(int cols = screenMargin; cols < screenHeight - screenMargin; cols+=gridSize) {
                Rectangle rect = new Rectangle(rows, cols, gridSize, gridSize);
                if(mouseState.LeftButton == ButtonState.Pressed && previousState.LeftButton == ButtonState.Released && hoveredBox == rect) {
                    if(isFirstSelection) {
                        startingBlock = hoveredBox;
                        isFirstSelection = false;
                    }
                    else {
                        endingBlock = hoveredBox;
                        isFirstSelection = true;
                    }
                }

            
            }
        }
        base.Update(gameTime);
        previousState = mouseState;
    }

    protected override void Draw(GameTime gameTime)
    {

        GraphicsDevice.Clear(Color.White);
        spriteBatch.Begin();

        DrawGrid();

        base.Draw(gameTime);
        spriteBatch.End();

    }

    public void DrawGrid() {
        Point graphPoint = new Point(((mouseState.X - screenMargin) / gridSize * gridSize + screenMargin) / gridSize, ((mouseState.Y - screenMargin) / gridSize * gridSize + screenMargin) / gridSize);
        //draw boxes
        for(int rows = screenMargin; rows < screenWidth - screenMargin; rows+=gridSize) {
            for(int cols = screenMargin; cols < screenHeight - screenMargin; cols+=gridSize) {
                Rectangle rect = new Rectangle(rows, cols, gridSize, gridSize);
                Color color = Color.LightGray;
                if (rect == hoveredBox)
                {
                    if (isFirstSelection)
                    {
                        color = Color.LightGreen;
                        spriteBatch.FillRectangle(rect, color);
                    }
                    else
                    {
                        color = Color.Red;
                        spriteBatch.FillRectangle(rect, color);
                    }


                }
                else if (rect == startingBlock)
                {
                    spriteBatch.FillRectangle(rect, Color.LightGreen);
                }
                else if (rect == endingBlock)
                {
                    spriteBatch.FillRectangle(rect, Color.Red);
                }
                else
                {
                    spriteBatch.DrawRectangle(rect, color);
                }
                
            }
        }

        for (int i = 0; i < graphWidth; i++)
        {
            for (int j = 0; j < graphHeight; j++)
            {
                Vertex<Point> rightVert = graph.Search(new Point(i + 1, j));
                Vertex<Point> downVert = graph.Search(new Point(i, j + 1));
                Vertex<Point> currVert = graph.Search(new Point(i, j));
                Point currPoint = new Point(i, j);
                Edge<Point> rightEdge = graph.GetEdge(currVert, rightVert);
                Edge<Point> downEdge = graph.GetEdge(currVert, downVert);
                

                
                if (rightEdge != null)
                {
                    spriteBatch.DrawString(
                        distanceFont, 
                        rightEdge.Distance.ToString(), 
                        new Vector2(currPoint.X * gridSize + screenMargin + gridSize / 2, currPoint.Y * gridSize + screenMargin), 
                        Color.Black, 
                        0f,                 
                        Vector2.Zero,      
                        fontScale,           
                        SpriteEffects.None, 
                        0f                
                    );

                }
                if (downEdge != null)
                {
                    spriteBatch.DrawString(
                        distanceFont, 
                        downEdge.Distance.ToString(), 
                        new Vector2(currPoint.X * gridSize + screenMargin, currPoint.Y * gridSize + screenMargin + gridSize / 2), 
                        Color.Black, 
                        0f,                
                        Vector2.Zero,      
                        fontScale,           
                        SpriteEffects.None, 
                        0f                 
                    );

                }



            }
        }
        
        
    }
}
