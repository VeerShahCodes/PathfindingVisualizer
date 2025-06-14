﻿using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.VisualBasic;
using Microsoft.Xna.Framework;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Particles.Modifiers;
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
    private Random random;
    private int currentPathIndex = 0;
    private float timeToNextSquare = 0;
    private float animationSpeed = 10;
    private bool animationInProgress = false;
    private float lastPathCost = 0;
    private bool shouldDrawPath;
    private int currentVisitedIndex = 0;
    private float visitedAnimationTimer = 0f;
    private float visitedAnimationSpeed = 5f; 
    private bool visitedAnimationInProgress = false;
    private List<Vertex<Point>> pathList;
    private bool hasClickedRandomizeButton = false;
    //textures ands fonts
    SpriteFont distanceFont;
    SpriteFont pathCostFont;

    Rectangle randomizeButton;
    Rectangle dijkstraButton;
    Rectangle aStarButton;
    Rectangle breadthFirstButton;
    Rectangle depthFirstButton;
    Rectangle obstacleButton;
    Rectangle clearButton;
    Rectangle resetButton;

    bool isRandomized = false;

    bool isOnObstacleMode = false;

    private String methodName = "A*";
    public Game1()
    {
        graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        Random random = new Random();

        shouldDrawPath = false;
        pathList = new List<Vertex<Point>>();

        gridSize = 50;
        screenMargin = 150;
        screenHeight = 1000;
        screenWidth = 1000;
        graphics.PreferredBackBufferHeight = screenHeight;
        graphics.PreferredBackBufferWidth = screenWidth;
        graphics.ApplyChanges();

        int buttonWidth = 160;
        int buttonHeight = 60;
        int buttonSpacing = 30;

        int topRowY = screenMargin / 2;
        int totalTopWidth = buttonWidth * 4 + buttonSpacing * 3;
        int topStartX = (screenWidth - totalTopWidth) / 2;
        randomizeButton = new Rectangle(topStartX, topRowY, buttonWidth, buttonHeight);
        resetButton = new Rectangle(topStartX + (buttonWidth + buttonSpacing) * 1, topRowY, buttonWidth, buttonHeight);
        clearButton = new Rectangle(topStartX + (buttonWidth + buttonSpacing) * 2, topRowY, buttonWidth, buttonHeight);
        obstacleButton = new Rectangle(topStartX + (buttonWidth + buttonSpacing) * 3, topRowY, buttonWidth, buttonHeight);

        int bottomRowY = screenHeight - screenMargin + buttonHeight / 2;
        int totalBottomWidth = buttonWidth * 4 + buttonSpacing * 3;
        int bottomStartX = (screenWidth - totalBottomWidth) / 2;
        aStarButton = new Rectangle(bottomStartX, bottomRowY, buttonWidth, buttonHeight);
        dijkstraButton = new Rectangle(bottomStartX + (buttonWidth + buttonSpacing) * 1, bottomRowY, buttonWidth, buttonHeight);
        breadthFirstButton = new Rectangle(bottomStartX + (buttonWidth + buttonSpacing) * 2, bottomRowY, buttonWidth, buttonHeight);
        depthFirstButton = new Rectangle(bottomStartX + (buttonWidth + buttonSpacing) * 3, bottomRowY, buttonWidth, buttonHeight);

        fontScale = gridSize / 50f;
        List<Vertex<Point>> obstaclePoints = new List<Vertex<Point>>();
        isFirstSelection = true;

        startingBlock = new Rectangle();
        endingBlock = new Rectangle();

        graphWidth = (screenWidth - screenMargin * 2) / gridSize;
        graphHeight = (screenHeight - screenMargin * 2) / gridSize;
        //init graph
        graph = new Graph<Point>();

        //create vertexes
        for (int rows = 0; rows < graphWidth; rows++)
        {
            for (int cols = 0; cols < graphHeight; cols++)
            {
                graph.AddVertex(new Point(rows, cols));
                graph.Search(new Point(rows, cols)).Owner = graph;
            }
        }
        //create edges
        for (int i = 0; i < graphWidth; i++)
        {
            for (int j = 0; j < graphHeight; j++)
            {
                double a = 1;
                double b = 1;
                double c = Math.Sqrt(2);

                if (i < graphWidth - 1)
                    graph.AddUndirectedEdge(new Point(i, j), new Point(i + 1, j), (float)a);

                if (j < graphHeight - 1)
                    graph.AddUndirectedEdge(new Point(i, j), new Point(i, j + 1), (float)b);


                if(i < graphWidth - 1 && j > 0)
                    graph.AddEdge(new Point(i, j), new Point(i + 1, j - 1), (float)c);
                if (i < graphWidth - 1 && j < graphHeight - 1)
                    graph.AddEdge(new Point(i, j), new Point(i + 1, j + 1), (float)c);

                if (i > 0 && j < graphHeight - 1)
                    graph.AddEdge(new Point(i, j), new Point(i - 1, j + 1), (float)c);
                if(i > 0 && j > 0)
                {
                    graph.AddEdge(new Point(i, j), new Point(i - 1, j - 1), (float)c);
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
        pathCostFont = Content.Load<SpriteFont>("pathCostFont");

    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        mouseState = Mouse.GetState();


        mouseX = ((mouseState.X - screenMargin) / gridSize) * gridSize + screenMargin;
        mouseY = ((mouseState.Y - screenMargin) / gridSize) * gridSize + screenMargin;
        hoveredBox = new Rectangle(mouseX, mouseY, gridSize, gridSize);
        if (mouseState.X >= screenMargin &&
            mouseState.X < screenWidth - screenMargin &&
            mouseState.Y >= screenMargin &&
            mouseState.Y < screenHeight - screenMargin)
        {
            mouseX = ((mouseState.X - screenMargin) / gridSize) * gridSize + screenMargin;
            mouseY = ((mouseState.Y - screenMargin) / gridSize) * gridSize + screenMargin;
            hoveredBox = new Rectangle(mouseX, mouseY, gridSize, gridSize);
        }
        else
        {
            hoveredBox = Rectangle.Empty; 
        }
        for (int rows = screenMargin; rows < screenWidth - screenMargin; rows += gridSize)
        {
            for (int cols = screenMargin; cols < screenHeight - screenMargin; cols += gridSize)
            {
                Rectangle rect = new Rectangle(rows, cols, gridSize, gridSize);
                if (mouseState.LeftButton == ButtonState.Pressed && previousState.LeftButton == ButtonState.Released && hoveredBox == rect)
                {
                    if (isOnObstacleMode)
                    {
                        Vertex<Point> vertex = graph.Search(new Point((hoveredBox.Location.X - screenMargin) / gridSize, (hoveredBox.Location.Y - screenMargin) / gridSize));
                        if (!graph.obstaclePoints.Contains(vertex.Value)) graph.obstaclePoints.Add(vertex.Value);
                        else graph.obstaclePoints.Remove(vertex.Value);
                    }
                    else if (isFirstSelection && !graph.obstaclePoints.Contains(new Point((hoveredBox.Location.X - screenMargin) / gridSize, (hoveredBox.Location.Y - screenMargin) / gridSize) ))
                    {
                        startingBlock = hoveredBox;
                        isFirstSelection = false;
                    }
                    else
                    {
                        endingBlock = hoveredBox;
                        Point firstPoint = new Point((startingBlock.Location.X - screenMargin) / gridSize, (startingBlock.Location.Y - screenMargin) / gridSize);
                        Point lastPoint = new Point((endingBlock.Location.X - screenMargin) / gridSize, (endingBlock.Location.Y - screenMargin) / gridSize);
                        if (methodName == "A*")
                            if (isRandomized)
                            {
                                pathList = graph.AStarAlgorithm(graph.Search(firstPoint), graph.Search(lastPoint), (a, b) => 0);

                            }
                            else
                            {
                                pathList = graph.AStarAlgorithm(graph.Search(firstPoint), graph.Search(lastPoint), graph.Diagonal);
                            }
                        else if (methodName == "Dijkstra")
                            pathList = graph.DijkstraAlgorithm(graph.Search(firstPoint), graph.Search(lastPoint));
                        else if (methodName == "DFS")
                            pathList = graph.PathFindDepthFirst(graph.Search(firstPoint), graph.Search(lastPoint));
                        else if (methodName == "BFS")
                            pathList = graph.PathFindBreadthFirst(graph.Search(firstPoint), graph.Search(lastPoint));
                        //pathList = graph.AStarAlgorithm(graph.Search(firstPoint), graph.Search(lastPoint), graph.Manhattan);
                        Console.WriteLine($"Path Cost: {graph.GetDistance(pathList)}, First Node: {pathList[0].Value}, Last Node: {pathList[pathList.Count - 1].Value}");
                        lastPathCost = graph.GetDistance(pathList);
                        StartPathAnimation();
                        isFirstSelection = true;
                    }
                }
                if (animationInProgress && currentPathIndex < pathList.Count)
                {
                    timeToNextSquare -= (float)gameTime.ElapsedGameTime.TotalSeconds;

                    if (timeToNextSquare <= 0)
                    {
                        currentPathIndex++;
                        timeToNextSquare = animationSpeed;

                        System.Diagnostics.Debug.WriteLine($"Current Path Index: {currentPathIndex}/{pathList.Count}");

                        if (currentPathIndex >= pathList.Count)
                        {
                            animationInProgress = false;
                            System.Diagnostics.Debug.WriteLine("Animation complete");
                        }
                    }
                }
                if (visitedAnimationInProgress && graph.VisitedNodes != null && currentVisitedIndex < graph.VisitedNodes.Count)
                {
                    visitedAnimationTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    while (visitedAnimationTimer >= visitedAnimationSpeed && currentVisitedIndex < graph.VisitedNodes.Count)
                    {
                        currentVisitedIndex++;
                        visitedAnimationTimer -= visitedAnimationSpeed;
                    }
                    if (currentVisitedIndex >= graph.VisitedNodes.Count)
                    {
                        visitedAnimationInProgress = false;
                    }
                }


            }
        }
        Rectangle buttonRect = randomizeButton;
        if (mouseState.LeftButton == ButtonState.Pressed && previousState.LeftButton == ButtonState.Released && buttonRect.Contains(mouseState.Position))
        {
            hasClickedRandomizeButton = true;
            RandomizeGraph();
            shouldDrawPath = false;
            isRandomized = true;
        }
        
        buttonRect = dijkstraButton;
        if (mouseState.LeftButton == ButtonState.Pressed && previousState.LeftButton == ButtonState.Released && buttonRect.Contains(mouseState.Position))
        {
            methodName = "Dijkstra";
        }

        buttonRect = aStarButton;
        if (mouseState.LeftButton == ButtonState.Pressed && previousState.LeftButton == ButtonState.Released && buttonRect.Contains(mouseState.Position))
        {
            methodName = "A*";
        }

        buttonRect = breadthFirstButton;
        if (mouseState.LeftButton == ButtonState.Pressed && previousState.LeftButton == ButtonState.Released && buttonRect.Contains(mouseState.Position))
        {
            methodName = "BFS";
        }

        buttonRect = depthFirstButton;
        if (mouseState.LeftButton == ButtonState.Pressed && previousState.LeftButton == ButtonState.Released && buttonRect.Contains(mouseState.Position))
        {
            methodName = "DFS";
        }

        buttonRect = obstacleButton;
        if(mouseState.LeftButton == ButtonState.Pressed && previousState.LeftButton == ButtonState.Released && buttonRect.Contains(mouseState.Position))
        {
            isOnObstacleMode = !isOnObstacleMode;
        }
        buttonRect = clearButton;
        if (mouseState.LeftButton == ButtonState.Pressed && previousState.LeftButton == ButtonState.Released && buttonRect.Contains(mouseState.Position))
        {
            isFirstSelection = true;
            startingBlock = new Rectangle();
            endingBlock = new Rectangle();
            pathList = new List<Vertex<Point>>();
            shouldDrawPath = false;
            graph.obstaclePoints.Clear();
        }
        buttonRect = resetButton;
        if (mouseState.LeftButton == ButtonState.Pressed && previousState.LeftButton == ButtonState.Released && buttonRect.Contains(mouseState.Position))
        {
            graph.ResetPathCosts();
            isRandomized = false;
            
        }
        base.Update(gameTime);
        previousState = mouseState;

    }

    protected override void Draw(GameTime gameTime)
    {

        GraphicsDevice.Clear(Color.CornflowerBlue);
        spriteBatch.Begin();

        DrawGrid();
        DrawVisitedPath(spriteBatch);
        DrawPath(spriteBatch);
        DrawRandomizeButton();
        DrawResetButton();
        DrawDijkstraButton();
        DrawAStarButton();
        DrawBreadthFirstButton();
        DrawDepthFirstButton();
        DrawObstacleButton();
        DrawClearButton();
        spriteBatch.DrawString(pathCostFont, $"Last Path Cost: {lastPathCost.ToString("0.0", CultureInfo.InvariantCulture)}", new Vector2(screenWidth / 2 - 125, screenMargin / 5), Color.White);

        base.Draw(gameTime);
        spriteBatch.End();

    }

    public void DrawGrid()
    {
        Point graphPoint = new Point(((mouseState.X - screenMargin) / gridSize * gridSize + screenMargin) / gridSize, ((mouseState.Y - screenMargin) / gridSize * gridSize + screenMargin) / gridSize);
        //draw boxes
        for (int rows = screenMargin; rows < screenWidth - screenMargin; rows += gridSize)
        {
            for (int cols = screenMargin; cols < screenHeight - screenMargin; cols += gridSize)
            {
                Rectangle rect = new Rectangle(rows, cols, gridSize, gridSize);
                Color color = Color.LightGray;
                if (rect.Intersects(hoveredBox))
                {
                    if (isOnObstacleMode)
                    {
                        color = Color.Black;
                        spriteBatch.FillRectangle(rect, color);
                    }
                    else if (isFirstSelection)
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
                else if (graph.obstaclePoints.Contains(new Point((rect.Location.X - screenMargin) / gridSize, (rect.Location.Y - screenMargin) / gridSize)))
                {
                    spriteBatch.FillRectangle(rect, Color.Gray);
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
                    spriteBatch.FillRectangle(rect, Color.White);
                    spriteBatch.DrawRectangle(rect, color);
                    
                }
                spriteBatch.DrawPoint(rect.X + gridSize / 2, rect.Y + gridSize / 2, Color.Black, 5f);




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
                    float originalValue = rightEdge.Distance;
                    int rounded = (int)originalValue;
                    spriteBatch.DrawString(
                        distanceFont,
                        rounded.ToString(),
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
                    float originalValue = downEdge.Distance;
                    int rounded = (int)Math.Round(originalValue, 1);
                    spriteBatch.DrawString(
                        distanceFont,
                        rounded.ToString(),
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

    public void DrawResetButton()
    {
        spriteBatch.FillRectangle(resetButton, Color.Gray);
        spriteBatch.DrawString(pathCostFont, "Reset", new Vector2(resetButton.X, resetButton.Y), Color.White);
    }

    public void DrawRandomizeButton()
    {
        // Draw the button
        spriteBatch.FillRectangle(randomizeButton, Color.Gray);
        spriteBatch.DrawString(pathCostFont, "Randomize", new Vector2(randomizeButton.X, randomizeButton.Y), Color.White);


    }
    public void DrawClearButton()
    {
        spriteBatch.FillRectangle(clearButton, Color.Gray);
        spriteBatch.DrawString(pathCostFont, "Clear", new Vector2(clearButton.X, clearButton.Y), Color.White);
    }
    public void DrawDijkstraButton()
    {
        if(methodName == "Dijkstra")
        {
            spriteBatch.FillRectangle(dijkstraButton, Color.Green);
        }
        else
        {
            spriteBatch.FillRectangle(dijkstraButton, Color.Red);
        }
        spriteBatch.DrawString(pathCostFont, "Dijkstra", new Vector2(dijkstraButton.X, dijkstraButton.Y), Color.White);
    }

    public void DrawAStarButton()
    {
        if (methodName == "A*")
        {
            spriteBatch.FillRectangle(aStarButton, Color.Green);
        }
        else
        {
            spriteBatch.FillRectangle(aStarButton, Color.Red);
        }
        spriteBatch.DrawString(pathCostFont, "A*", new Vector2(aStarButton.X, aStarButton.Y), Color.White);
    }

    public void DrawBreadthFirstButton()
    {
        if (methodName == "BFS")
        {
            spriteBatch.FillRectangle(breadthFirstButton, Color.Green);
        }
        else
        {
            spriteBatch.FillRectangle(breadthFirstButton, Color.Red);
        }
        spriteBatch.DrawString(pathCostFont, "BFS", new Vector2(breadthFirstButton.X, breadthFirstButton.Y), Color.White);
    }

    public void DrawObstacleButton()
    {
        if (!isOnObstacleMode)
        {
            spriteBatch.FillRectangle(obstacleButton, Color.Red);
            spriteBatch.DrawString(pathCostFont, "Obstacle", new Vector2(obstacleButton.X, obstacleButton.Y), Color.White);
        }
        else
        {
            spriteBatch.FillRectangle(obstacleButton, Color.Green);
            spriteBatch.DrawString(pathCostFont, "Obstacle", new Vector2(obstacleButton.X, obstacleButton.Y), Color.White);
        }

    }

    public void DrawDepthFirstButton()
    {
        if (methodName == "DFS")
        {
            spriteBatch.FillRectangle(depthFirstButton, Color.Green);
        }
        else
        {
            spriteBatch.FillRectangle(depthFirstButton, Color.Red);
        }
        spriteBatch.DrawString(pathCostFont, "DFS", new Vector2(depthFirstButton.X, depthFirstButton.Y), Color.White);
    }

    public void DrawVisitedPath(SpriteBatch spriteBatch)
    {
        if (shouldDrawPath && graph.VisitedNodes != null)
        {
            int limit = visitedAnimationInProgress ? currentVisitedIndex : graph.VisitedNodes.Count;

            for (int i = 0; i < limit; i++)
            {
                Rectangle drawRect = new Rectangle(
                    graph.VisitedNodes[i].Value.X * gridSize + screenMargin,
                    graph.VisitedNodes[i].Value.Y * gridSize + screenMargin,
                    gridSize, gridSize);
                spriteBatch.FillRectangle(drawRect, Color.Yellow * 0.5f);
            }
        }
    }
    public void DrawPath(SpriteBatch spriteBatch)
    {
    if (shouldDrawPath && pathList != null && pathList.Count > 0)
    {
        int visibleSquares = Math.Min(currentPathIndex, pathList.Count);

        for (int i = 0; i < visibleSquares - 1; i++)
        {
            var start = pathList[i].Value;
            var end = pathList[i + 1].Value;



            var startPos = new Vector2(
                start.X * gridSize + screenMargin + gridSize / 2f,
                start.Y * gridSize + screenMargin + gridSize / 2f
            );
            var endPos = new Vector2(
                end.X * gridSize + screenMargin + gridSize / 2f,
                end.Y * gridSize + screenMargin + gridSize / 2f
            );

            spriteBatch.DrawLine(startPos, endPos, Color.Blue, 4f);
        }
    }
    }
    public void StartPathAnimation()
    {
        currentPathIndex = 0;
        timeToNextSquare = 0;
        shouldDrawPath = true;
        animationInProgress = true;

         // Start visited animation
        currentVisitedIndex = 0;
        visitedAnimationTimer = 0f;
        visitedAnimationInProgress = true;

    }
    public void RandomizeGraph() {
        Random random = new Random();
        for (int i = 0; i < graphWidth; i++)
        {
            for (int j = 0; j < graphHeight; j++)
            {
                double a = (double)(random.NextDouble() * 20 + 1);
                double b = (double)(random.NextDouble() * 20 + 1);
                double c = Math.Sqrt(a * a + b * b);

                if (i < graphWidth - 1)
                {
                    graph.GetEdge(graph.Search(new Point(i, j)), graph.Search(new Point(i + 1, j))).Distance = (float)a;
                    graph.GetEdge(graph.Search(new Point(i + 1, j)), graph.Search(new Point(i, j))).Distance = (float)a;

                }

                if (j < graphHeight - 1)
                {
                    graph.GetEdge(graph.Search(new Point(i, j)), graph.Search(new Point(i, j + 1))).Distance = (float)b;
                    graph.GetEdge(graph.Search(new Point(i, j + 1)), graph.Search(new Point(i, j))).Distance = (float)b;

                }

                if (i < graphWidth - 1 && j < graphHeight - 1)
                {
                    graph.GetEdge(graph.Search(new Point(i, j)), graph.Search(new Point(i + 1, j + 1))).Distance = (float)c;
                    graph.GetEdge(graph.Search(new Point(i + 1, j + 1)), graph.Search(new Point(i, j))).Distance = (float)c;

                }
                if(i < graphWidth - 1 && j > 0)
                {
                    graph.GetEdge(graph.Search(new Point(i, j)), graph.Search(new Point(i + 1, j - 1))).Distance = (float)c;
                    graph.GetEdge(graph.Search(new Point(i + 1, j - 1)), graph.Search(new Point(i, j))).Distance = (float)c;

                }

                if (i > 0 && j < graphHeight - 1)
                {
                    graph.GetEdge(graph.Search(new Point(i, j)), graph.Search(new Point(i - 1, j + 1))).Distance = (float)c;
                    graph.GetEdge(graph.Search(new Point(i - 1, j + 1)), graph.Search(new Point(i, j))).Distance = (float)c;

                }
                if(i > 0 && j > 0)
                {
                    graph.GetEdge(graph.Search(new Point(i, j)), graph.Search(new Point(i - 1, j - 1))).Distance = (float)c;
                    graph.GetEdge(graph.Search(new Point(i - 1, j - 1)), graph.Search(new Point(i, j))).Distance = (float)c;

                }
            }
        }
    }

}
