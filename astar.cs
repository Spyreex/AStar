using System;
using System.Drawing;
using System.IO;
using System.Collections.Generic;

public class Program
{
    private static Point startPoint;
    private static Point endPoint;
    private static AStarQueue astar;
    private static char[][] map;
    private static int maxRowLength;
    private static int maxColumnLength;

    public static void Main(string[] args)
    {
        foreach (string arg in args)
        {
            if (File.Exists(arg))
            {
                Initialize(arg);
                Console.WriteLine(Environment.NewLine + "=============================");
            }
            else
                Console.WriteLine("File does not exist!");
        }
    }

    private static void Initialize(string file)
    {
        // read map
 	    string mapText = File.ReadAllText(file);

        // split read text into strings that end at newlines
        string[] mapTextArray = mapText.Split(new [] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);

        maxRowLength = 0;

        foreach (string line in mapTextArray)
        {
            if (line.Length > maxRowLength)
                maxRowLength = line.Length;
        }

        // biggest row
        // Console.WriteLine(maxRowLength);

        // columns
        // Console.WriteLine(mapTextArray.Length);
        maxColumnLength = mapTextArray.Length;

        map = new char[maxColumnLength][];
        for (int x = 0; x < maxColumnLength; x++)
        {
            map[x] = new char[maxRowLength];
            for (int y = 0; y < maxRowLength; y++)
            {
                if (y < mapTextArray[x].Length)
                {
                    if (Array.Exists(new char[] {'#','.','0','1'}, element => element == mapTextArray[x][y]))
                        map[x][y] = mapTextArray[x][y];
                    else
                        map[x][y] = '#';
                } else {
                    map[x][y] = '#';
                }
                // Console.Write(mapTextArray[x][y]);
            }
            // Console.WriteLine("");
        }

        startPoint = new Point(-1, -1);
        endPoint = new Point(-1, -1);

        // go through map to find start and end point, detects multiple instances
        for (int x = 0; x < map.Length; x++)
        {
            for (int y = 0; y < map[x].Length; y++)
            {
                if (map[x][y] == '0')
                {
                    if (!(startPoint == new Point(-1, -1)))
                        Console.WriteLine("Multiple startpoints");
                    else
                        startPoint = new Point(x, y);
                }
                if (map[x][y] == '1')
                {
                    if (!(endPoint == new Point(-1, -1)))
                        Console.WriteLine("Multiple endpoints");
                    else
                        endPoint = new Point(x, y);
                }
            }
        }

        astar = new AStarQueue(startPoint, endPoint);
        Console.WriteLine("Map of " + file);
        printArray(map);
        // Console.WriteLine(astar.toString());
        Pathing();
    }

    // starts going through the map
    private static void Pathing()
    {
        // Console.WriteLine(astar.queue[0].toString());
        // check if top of queue is end goal here
        while (astar.queue[0].coordinate != endPoint)
        {
            AddQueue();
            // Console.WriteLine(astar.toString());
            if (astar.queue.Count == 0)
                break;
            // astar.PrintList();
            // Console.WriteLine("");
        }

        if (astar.queue.Count == 0)
            Console.WriteLine("No Solution Found");
        else {
            AStarQueueItem tempItem = astar.queue[0];
            List<Point> solutionPath = new List<Point>() {astar.queue[0].coordinate};

            while (tempItem.prevCoordinate != new Point(-1,-1))
            {
                solutionPath.Add(tempItem.coordinate);
                tempItem = astar.FindItemStash(tempItem.prevCoordinate);
                // Console.WriteLine(tempItem.coordinate);
            }
            char[][] newMap = map;
            foreach (Point coord in solutionPath)
            {
                if (coord != endPoint && coord != startPoint)
                {
                    newMap[coord.X][coord.Y] = '*';
                }
            }
            Console.WriteLine(Environment.NewLine + "Solution:");
            printArray(newMap);
        }
    }

    // from one Point, add all surrounding points to queue (if applicable)
    private static void AddQueue()
    {
        AStarQueueItem currItem = astar.queue[0];
        double newStep;

        // loops through each Point arround current coordinate and checks whether this is a valid point to go to next, if so, adds to queue
        for (int x = -1; x < 2; x++)
        {
            for (int y = -1; y < 2; y++)
            {
                if (currItem.coordinate.X + x >= 0 
                    && currItem.coordinate.Y + y >= 0 
                    && currItem.coordinate.X + x < maxColumnLength 
                    && currItem.coordinate.Y + y < maxRowLength)
                {
                    if ((map[currItem.coordinate.X + x][currItem.coordinate.Y + y] == '.' || map[currItem.coordinate.X + x][currItem.coordinate.Y + y] == '1') 
                        && !(x == 0 && y == 0) 
                        && !(new Point(x,y) == currItem.prevCoordinate))
                    {
                        newStep = Math.Sqrt(Math.Abs(x) + Math.Abs(y));
                        AStarQueueItem newItem = new AStarQueueItem(new Point(currItem.coordinate.X + x, currItem.coordinate.Y + y), currItem.coordinate, currItem.step + newStep, endPoint);
                        AStarQueueItem oldItem = astar.FindItem(newItem.coordinate);
                        if (astar.FindItemStash(newItem.coordinate).coordinate != newItem.coordinate)
                        {
                            if (oldItem.coordinate == newItem.coordinate)
                            {
                                if (oldItem.cost > newItem.cost)
                                    astar.InsertAStar(newItem);
                            } else {
                                astar.InsertAStar(newItem);
                            }
                        }
                    }
                }
            }
        }
        // stash current Point
        astar.stash.Add(currItem);
        astar.queue.Remove(currItem);
    }

    private static void printArray(char[][] array)
    {
        foreach (char[] line in array)
        {
            foreach (char thing in line)
            {
                Console.Write(thing);
            }
            Console.WriteLine("");
        }
    }
}

public class AStarQueue
{
    private Point startingPoint;
    private Point endPoint;
    public List<AStarQueueItem> queue;
    public List<AStarQueueItem> stash;

    public AStarQueue(Point startingPoint, Point endPoint)
    {
        this.startingPoint = startingPoint;
        this.endPoint = endPoint;
        this.queue = new List<AStarQueueItem>() {new AStarQueueItem(startingPoint, new Point(-1, -1), 0, endPoint)};
        this.stash = new List<AStarQueueItem>();
    }

    // inserts Point into queue, according to cost
    public void InsertAStar(AStarQueueItem item)
    {
        int index = 0;

        if (queue.Count != 0)
        {
            while (index < queue.Count)
            {
                if (item.cost < queue[index].cost)
                    break;
                index++;
            }
        }
        queue.Insert(index, item);
    }
    
    public AStarQueueItem FindItem(Point coordinate)
    {
        foreach (AStarQueueItem item in queue)
        {
            if (item.coordinate == coordinate)
                return item;
        }
        return new AStarQueueItem(new Point(-1, -1), new Point(-1, -1), -1, new Point(-1, -1));
    }

        public AStarQueueItem FindItemStash(Point coordinate)
    {
        foreach (AStarQueueItem item in stash)
        {
            if (item.coordinate == coordinate)
                return item;
        }
        return new AStarQueueItem(new Point(-1, -1), new Point(-1, -1), -1, new Point(-1, -1));
    }

    public void PrintList()
    {
        foreach (AStarQueueItem item in queue)
        {
            Console.WriteLine(item.toString());
        }
    }

    public void PrintStash()
    {
        foreach (AStarQueueItem item in stash)
        {
            Console.WriteLine(item.toString());
        }
    }

    public string toString()
    {
        return "The AStar Queue has: " + queue.Count + " item(s). The map's startpoint and endpoint are: " + startingPoint + " and " + endPoint + ".";
    }
}

public class AStarQueueItem
{
    public Point coordinate;
    public Point prevCoordinate;
    public double step;
    public double distance;
    public double cost;

    public AStarQueueItem(Point coordinate, Point prevCoordinate, double step, Point endPoint)
    {
        this.coordinate = coordinate;
        this.prevCoordinate = prevCoordinate;
        this.step = step;
        // pythagorean thing here to determine distance
        this.distance = Math.Sqrt(Math.Pow(Math.Abs(coordinate.X - endPoint.X),2) + Math.Pow(Math.Abs(coordinate.Y - endPoint.Y),2));
        this.cost = this.step + this.distance;
    }

    public string toString()
    {
        return "Point: {" + coordinate.X + "," + coordinate.Y + "} from parent point: {" + prevCoordinate.X + "," + prevCoordinate.Y + "} is step " + step + ", " + distance + " distance from the end goal and has a cost of: " + cost + ".";
    }
}