using UnityEngine;
using System.Collections.Generic;
using Delaunay.Geo;
using Delaunay;
using CSharpCity.Tools.RandomNumberGenerators;

/// <summary>
/// This class uses a method used by TinyKeep's developer and described in https://www.reddit.com/r/gamedev/comments/1dlwc4/procedural_dungeon_generation_algorithm_explained/
/// also, the method is detailed in https://github.com/adonaac/blog/issues/7
/// This is nothing more but an Unity's script implementation of the described method.
/// </summary>

public class DungeonGenerator : MonoBehaviour
{
    #region Attributes
    /// <summary>
    /// Defines the horizontal area the random rooms may occupy before separation
    /// </summary>
    [SerializeField]
    private int roomGenerationHorizontalSpace;
    /// <summary>
    /// Defines the vertical area the random rooms may occupy before separation
    /// </summary>
    [SerializeField]
    private int roomGenerationVerticalSpace;
    /// <summary>
    /// The minimum number of random rooms that will be generated in rooms addition step that matches the minMainRoomWidth and minMainRoomHeight criteria
    /// </summary>
    [SerializeField]
    private int minNumberOfMainRooms;
    /// <summary>
    /// The maximum number of random rooms that will be considered main rooms (if more than this number of rooms are found, the rest of them will be treated as secundary)
    /// </summary>
    [SerializeField]
    private int maxNumberOfMainRooms;
    /// <summary>
    /// The minimum width of a room.
    /// </summary>
    [SerializeField]
    private float minRoomWidth;
    /// <summary>
    /// The maximum width of a room
    /// </summary>
    [SerializeField]
    private float maxRoomWidth;
    /// <summary>
    /// The minimum height of a room
    /// </summary>
    [SerializeField]
    private float minRoomHeight;
    /// <summary>
    /// The maximum width of a room
    /// </summary>
    [SerializeField]
    private float maxRoomHeight;
    /// <summary>
    /// The minimum width of a room to be considered a main room
    /// </summary>
    [SerializeField]
    private float minMainRoomWidth;
    /// <summary>
    /// The maximum width of a room to be considered a main room
    /// </summary>
    [SerializeField]
    private float minMainRoomHeight;
    /// <summary>
    /// Use AABB separation or simple avoidance behavior for rooms separation?
    /// </summary>
    [SerializeField]
    private bool useAABBSeparation;
    /// <summary>
    /// When using simple avoidance behavior, how much the rooms may separate from their neighbors in each iteration
    /// </summary>
    [SerializeField]
    private float roomSeparationStepMultiplier;
    /// <summary>
    /// Use normal distribution instead of linear randomicity
    /// </summary>
    [SerializeField]
    private bool useNormalRandom;
    /// <summary>
    /// Normal distribution's mean for horizontal dimension randomizer
    /// </summary>
    [SerializeField]
    private float widthNormalRandomMean;
    /// <summary>
    /// Normal distribution's mean for vertical dimension randomizer
    /// </summary>
    [SerializeField]
    private float heightNormalRandomMean;
    /// <summary>
    /// Normal distribution's standard deviation for horizontal dimension randomizer
    /// </summary>
    [SerializeField]
    private float widthNormalRandomStandardDeviation;
    /// <summary>
    /// Normal distribution's standard deviation for vertical dimension randomizer
    /// </summary>
    [SerializeField]
    private float heightNormalRandomStandardDeviation;
    /// <summary>
    /// Number of random rooms to be generated in total (some will be ignored after separation, main room selection and hall creation)
    /// </summary>
    [SerializeField]
    private int numberOfRooms;
    /// <summary>
    /// Max number of iterations to try to separate rooms
    /// </summary>
    [SerializeField]
    private int maxSeparationIterations;
    /// <summary>
    /// Percentage of edges to readd to minimum spanning tree to make the dungeon more fun =]
    /// </summary>
    [SerializeField]
    private float percentageOfEdgesToReaddToSpanningTree = 0.15f;
    /// <summary>
    /// The halls size
    /// </summary>
    [SerializeField]
    private int hallTickness;
    /// <summary>
    /// Just sets Time.scale to be faster or slower
    /// </summary>
    [SerializeField]
    private int debugTimeScale = 3;

    [SerializeField]
    private Color initialRandomRoomsColor = Color.white;
    [SerializeField]
    private Color secundaryRoomsColor = Color.blue;
    [SerializeField]
    private Color mainRoomsColor = Color.red;
    [SerializeField]
    private Color pathsColor = Color.green;

    private List<Room> allRooms;
    private List<Room> secundaryRooms;
    private List<Room> mainRooms;
    private List<Room> halls;
    private List<LineSegment> delaunayTriangulation;
    private List<LineSegment> spanningTree;
    private List<RoomEdge> roomsConnections;
    private Delaunay.Voronoi voronoi;
    private NormalRandomGenerator normalRandomGeneratorWidth;
    private NormalRandomGenerator normalRandomGeneratorHeight;
    private Rect mapRect;
    private int currentNumberOfMainRooms;
    #endregion //Attributes

    #region Step-by-step generation stuff
    private int currentState = 0;
    private int currentSeparationIteraction;
    private float timeToAddRoom = 0.05f;
    private float currentStateTime;
    #endregion //Step-by-step generation stuff

    #region Methods
    void Start()
    {
        allRooms = new List<Room>();
        mainRooms = new List<Room>();
        mapRect = new Rect();

        currentSeparationIteraction = 0;
        currentNumberOfMainRooms = 0;
        currentStateTime = 0;
        currentState = 0;

        if (useNormalRandom)
        {
            normalRandomGeneratorWidth = new NormalRandomGenerator((int)minRoomWidth, (int)maxRoomWidth);
            normalRandomGeneratorHeight = new NormalRandomGenerator((int)minRoomHeight, (int)maxRoomHeight);

            normalRandomGeneratorWidth.Mean = widthNormalRandomMean;
            normalRandomGeneratorHeight.Mean = heightNormalRandomMean;

            normalRandomGeneratorWidth.StandardDeviation = widthNormalRandomStandardDeviation;
            normalRandomGeneratorHeight.StandardDeviation = heightNormalRandomStandardDeviation;
        }

        Time.timeScale = debugTimeScale;
    }

    private void AddRoom()
    {
        Room r;

        if (useNormalRandom)
        {
            r = new Room(0, 0, normalRandomGeneratorWidth.Next(), normalRandomGeneratorHeight.Next());
        }
        else
        {
            r = new Room(0, 0, Random.Range(minRoomWidth, maxRoomWidth), Random.Range(minRoomHeight, maxRoomHeight));
        }
        r.center = ShapeUtils.GetRandomPointInElipse(roomGenerationHorizontalSpace, roomGenerationVerticalSpace);

        if (r.width >= minMainRoomWidth && r.height >= minMainRoomHeight)
        {
            currentNumberOfMainRooms++;
        }

        allRooms.Add(r);
    }

    private void AddRandomMainRoom()
    {
        Room r;

        if (useNormalRandom)
        {
            r = new Room(0, 0, normalRandomGeneratorWidth.Next(), normalRandomGeneratorHeight.Next());
        }
        else
        {
            r = new Room(0, 0, Random.Range(minMainRoomWidth, maxRoomWidth), Random.Range(minMainRoomHeight, maxRoomHeight));
        }
        r.center = ShapeUtils.GetRandomPointInElipse(roomGenerationHorizontalSpace, roomGenerationVerticalSpace);

        currentNumberOfMainRooms++;
        allRooms.Add(r);
    }

    private void AddRooms()
    {
        for (int i = 0; i < numberOfRooms; ++i)
        {
            AddRoom();
        }

        if (currentNumberOfMainRooms < minNumberOfMainRooms)
        {
            if (useNormalRandom)
            {
                normalRandomGeneratorWidth = new NormalRandomGenerator((int)minMainRoomWidth, (int)maxRoomWidth);
                normalRandomGeneratorHeight = new NormalRandomGenerator((int)minMainRoomHeight, (int)maxRoomHeight);
            }

            while (currentNumberOfMainRooms < minNumberOfMainRooms)
            {
                AddRandomMainRoom();
            }
        }
    }

    //Room separation is bad. It was enough for my game and the time were too short :B (https://imgflip.com/i/r85qw)
    private void SeparateRooms()
    {
        bool overlaps = true;
        if (currentSeparationIteraction < maxSeparationIterations)
        {
            overlaps = false;
            roomSeparationStepMultiplier += roomSeparationStepMultiplier / (maxSeparationIterations / 6);
            for (int i = 0; i < allRooms.Count; ++i)
            {
                for (int j = 0; j < allRooms.Count; ++j)
                {
                    if (i == j)
                        continue;
                    Room r = allRooms[j];
                    Room r2 = allRooms[i];

                    if (!useAABBSeparation)
                    {
                        if (allRooms[i].Overlaps(r))
                        {
                            overlaps = true;
                            Vector2 centerDiff = r.center - allRooms[i].center;
                            float xDiff = 0;
                            float yDiff = 0;
                            if (Mathf.Abs(centerDiff.x) >= Mathf.Abs(centerDiff.y))
                            {
                                xDiff = (r.position.x - allRooms[i].position.x) + Mathf.CeilToInt(centerDiff.x / 2);
                            }
                            else
                            {
                                yDiff = (r.position.y - allRooms[i].position.y) + Mathf.CeilToInt(centerDiff.y / 2);
                            }
                            Vector2 toAdd = new Vector2(xDiff, yDiff) * roomSeparationStepMultiplier;
                            toAdd.x = Mathf.CeilToInt(toAdd.x);
                            toAdd.y = Mathf.CeilToInt(toAdd.y);
                            r.position += toAdd;
                            allRooms[j] = r;
                        }
                    }
                    else
                    {
                        if (allRooms[i].Overlaps(r))
                        {
                            overlaps = true;
                            float xPenetration = 0;
                            float yPenetration = 0;
                            Vector2 normal = AABBvsAABB(r2, r, out xPenetration, out yPenetration);

                            Vector2 toAdd = new Vector2(xPenetration / 2, yPenetration / 2);
                            toAdd.x = Mathf.CeilToInt(toAdd.x);
                            toAdd.y = Mathf.CeilToInt(toAdd.y);

                            r.position += toAdd;
                            r2.position -= toAdd;
                            allRooms[j] = r;
                            allRooms[i] = r2;
                        }
                    }
                }
            }
            if (overlaps)
            {
                currentSeparationIteraction++;
            }
            else
            {
                currentSeparationIteraction = maxSeparationIterations;
            }
        }
        else
        {
            //Remove rectangles that still overlaps another one
            for (int i = 0; i < allRooms.Count; ++i)
            {
                for (int j = i + 1; j < allRooms.Count; ++j)
                {
                    Room r = allRooms[j];
                    if (allRooms[i].Overlaps(r))
                    {
                        allRooms.RemoveAt(j);
                        j--;
                    }
                }
            }
            currentState++;
        }
    }

    //Room separation is bad. It was enough for my game and the time were too short :B (https://imgflip.com/i/r85qw)
    private void SeparateRoomsSilently()
    {
        bool overlaps = true;
        while (currentSeparationIteraction < maxSeparationIterations && overlaps)
        {
            roomSeparationStepMultiplier += roomSeparationStepMultiplier / (maxSeparationIterations / 6);
            overlaps = false;
            for (int i = 0; i < allRooms.Count; ++i)
            {
                for (int j = 0; j < allRooms.Count; ++j)
                {
                    if (i == j)
                        continue;
                    Room r = allRooms[j];
                    Room r2 = allRooms[i];

                    if (!useAABBSeparation)
                    {
                        if (allRooms[i].Overlaps(r))
                        {
                            overlaps = true;
                            Vector2 centerDiff = r.center - allRooms[i].center;
                            float xDiff = 0;
                            float yDiff = 0;
                            if (Mathf.Abs(centerDiff.x) >= Mathf.Abs(centerDiff.y))
                            {
                                xDiff = (r.position.x - allRooms[i].position.x) + Mathf.CeilToInt(centerDiff.x / 2);
                            }
                            else
                            {
                                yDiff = (r.position.y - allRooms[i].position.y) + Mathf.CeilToInt(centerDiff.y / 2);
                            }

                            Vector2 toAdd = new Vector2(xDiff, yDiff) * roomSeparationStepMultiplier;
                            toAdd.x = Mathf.CeilToInt(toAdd.x);
                            toAdd.y = Mathf.CeilToInt(toAdd.y);
                            r.position += toAdd; //Time.fixedDeltaTime;
                            allRooms[j] = r;
                        }
                    }
                    else
                    {
                        if (allRooms[i].Overlaps(r))
                        {
                            overlaps = true;
                            float xPenetration = 0;
                            float yPenetration = 0;
                            Vector2 normal = AABBvsAABB(r2, r, out xPenetration, out yPenetration);
                            r.position += new Vector2(xPenetration / 2, yPenetration / 2);
                            r2.position -= new Vector2(xPenetration / 2, yPenetration / 2);
                            allRooms[j] = r;
                            allRooms[i] = r2;
                        }
                    }
                }
            }
            currentSeparationIteraction++;
        }

        //Remove rectangles that still overlaps another one
        for (int i = 0; i < allRooms.Count; ++i)
        {
            for (int j = i + 1; j < allRooms.Count; ++j)
            {
                Room r = allRooms[j];
                if (allRooms[i].Overlaps(r))
                {
                    allRooms.RemoveAt(j);
                    j--;
                }
            }
        }
    }


    private void UpdateMapRect(Rect r)
    {
        if (r.xMin < mapRect.xMin)
        {
            mapRect.xMin = r.xMin;
        }
        else if (r.xMax > mapRect.xMax)
        {
            mapRect.xMax = r.xMax;
        }

        if (r.yMin < mapRect.yMin)
        {
            mapRect.yMin = r.yMin;
        }
        else if (r.yMax > mapRect.yMax)
        {
            mapRect.yMax = r.yMax;
        }
    }

    private void SelectMainRooms()
    {
        int selectedMainRooms = 0;

        for (int i = 0; i < allRooms.Count && selectedMainRooms < maxNumberOfMainRooms; ++i)
        {
            if (allRooms[i].width >= minMainRoomWidth && allRooms[i].height >= minMainRoomHeight && !mainRooms.Contains(allRooms[i]))
            {
                selectedMainRooms++;
                UpdateMapRect(allRooms[i].Rect);
                mainRooms.Add(allRooms[i]);
                allRooms.RemoveAt(i);
                i--;
            }
        }
    }

    private void DelaunayTriangulation()
    {
        List<Vector2> points = new List<Vector2>();

        foreach (Room r in mainRooms)
        {
            points.Add(r.center);
        }

        voronoi = new Delaunay.Voronoi(points, null, new Rect(0, 0, mapRect.width, mapRect.height));
        delaunayTriangulation = voronoi.DelaunayTriangulation();
    }

    private void GetSpanningTree()
    {
        spanningTree = voronoi.SpanningTree(KruskalType.MINIMUM);
    }

    private void AddBackEdgesToSpanningTree()
    {
        int amountOfEdgesToAdd = (int)(percentageOfEdgesToReaddToSpanningTree * delaunayTriangulation.Count);
        List<LineSegment> tempTree = new List<LineSegment>(spanningTree);
        List<LineSegment> tempDelaunay = new List<LineSegment>(delaunayTriangulation);
        List<LineSegment> edgesToAdd = new List<LineSegment>();
        tempTree.Shuffle();
        tempDelaunay.Shuffle();

        for (int i = 0; i < tempDelaunay.Count && edgesToAdd.Count < amountOfEdgesToAdd; ++i)
        {
            if (!tempTree.Contains(tempDelaunay[i]))
            {
                edgesToAdd.Add(tempDelaunay[i]);
            }
        }
        spanningTree.AddRange(edgesToAdd);
    }

    private void CreateHalls()
    {
        roomsConnections = new List<RoomEdge>();
        halls = new List<Room>();

        foreach (LineSegment l in spanningTree)
        {
            Room r0 = null;
            Room r1 = null;

            foreach (Room r in mainRooms)
            {
                if (r.Contains((Vector2)l.p0))
                {
                    r0 = r;
                }
                else if (r.Contains((Vector2)l.p1))
                {
                    r1 = r;
                }
                if (r0 != null && r1 != null)
                {
                    break;
                }
            }
            roomsConnections.Add(new RoomEdge(l, r0, r1));
        }

        foreach (RoomEdge l in roomsConnections)
        {
            Room roomA = l.P0Rect;
            Room roomB = l.P1Rect;
            Room hall;

            Vector2 midpoint = (roomA.position + (roomA.size) / 2 + roomB.position + (roomB.size) / 2) / 2;
            midpoint = new Vector2((int)(midpoint.x), (int)(midpoint.y));
            bool done = false;

            //if roomA can hold a horizontal hall
            if (roomA.yMax >= (midpoint.y + hallTickness / 2) && roomA.yMin <= (midpoint.y - hallTickness / 2))
            {
                //if roomB can hold a horizontal hall
                if (roomB.yMax >= (midpoint.y + hallTickness / 2) && roomB.yMin <= (midpoint.y - hallTickness / 2))
                {
                    //Then we can build a horizontal hall
                    //But first we check who's left and who's right
                    float xDiff = roomA.x - roomB.x;

                    //RoomB's left
                    if (xDiff < 0)
                    {
                        hall = new Room(roomA.xMax, (int)(midpoint.y - hallTickness / 2), Mathf.Abs(roomA.xMax - roomB.xMin), hallTickness);
                    }
                    //RoomA's left
                    else
                    {
                        hall = new Room(roomB.xMax, (int)(midpoint.y - hallTickness / 2), Mathf.Abs(roomB.xMax - roomA.xMin), hallTickness);
                    }
                    done = true;
                    halls.Add(hall);
                    UpdateMapRect(hall.Rect);
                }
            }

            if (!done)
            {
                //if roomA can hold a vertical hall
                if (roomA.xMin <= (midpoint.x - hallTickness / 2) && roomA.xMax >= (midpoint.x + hallTickness / 2))
                {
                    //if roomB can hold a vertical hall
                    if (roomA.xMin <= (midpoint.x - hallTickness / 2) && roomA.xMax >= (midpoint.x + hallTickness / 2))
                    {
                        //Then we can build a vertical hall
                        //But first we check who's top and who's bottom
                        float yDiff = roomA.y - roomB.y;

                        //RoomB's top
                        if (yDiff < 0)
                        {
                            hall = new Room((int)(midpoint.x - hallTickness / 2), roomA.yMax, hallTickness, Mathf.Abs(roomA.yMax - roomB.yMin));
                        }
                        //RoomA's top
                        else
                        {
                            hall = new Room((int)(midpoint.x + hallTickness / 2), roomB.yMax, hallTickness, Mathf.Abs(roomB.yMax - roomA.yMin));
                        }
                        done = true;
                        halls.Add(hall);
                        UpdateMapRect(hall.Rect);
                    }
                }
            }

            //if a L shape hall is needed
            if (!done)
            {
                Room xHall;
                Room yHall;
                Vector2 posDiff = roomA.position - roomB.position;

                if (Mathf.Abs(posDiff.x) >= Mathf.Abs(posDiff.y))
                {
                    //Create the horizontal hall first
                    //RoomA's left
                    if (posDiff.x <= 0)
                    {
                        xHall = new Room();
                        xHall.x = roomA.xMax;
                        xHall.y = roomA.center.y - hallTickness / 2;
                        xHall.height = hallTickness;
                        xHall.width = roomB.center.x - roomA.xMax;

                        if (posDiff.y <= 0)
                        {
                            yHall = new Room();
                            yHall.x = xHall.xMax - hallTickness;
                            yHall.y = xHall.center.y - hallTickness / 2;
                            yHall.yMax = roomB.yMin;
                            yHall.width = hallTickness;
                        }
                        else
                        {
                            yHall = new Room();
                            yHall.x = xHall.xMax - hallTickness;
                            yHall.y = xHall.center.y + hallTickness / 2;
                            yHall.yMax = roomA.yMin;
                            yHall.width = hallTickness;
                        }
                    }
                    //RoomA's left
                    else
                    {
                        xHall = new Room();
                        xHall.x = roomB.xMax;
                        xHall.y = roomB.center.y - hallTickness / 2;
                        xHall.height = hallTickness;
                        xHall.width = roomA.center.x - roomB.xMax;

                        if (posDiff.y <= 0)
                        {
                            yHall = new Room();
                            yHall.x = xHall.xMax - hallTickness;
                            yHall.y = roomA.yMax;
                            yHall.yMax = xHall.center.y - hallTickness / 2;
                            yHall.width = hallTickness;
                        }
                        else
                        {
                            yHall = new Room();
                            yHall.x = xHall.xMax - hallTickness;
                            yHall.y = xHall.center.y + hallTickness / 2;
                            yHall.yMax = roomB.yMax;
                            yHall.width = hallTickness;
                        }
                    }
                    halls.Add(xHall);
                    halls.Add(yHall);
                    UpdateMapRect(xHall.Rect);
                    UpdateMapRect(yHall.Rect);

                }
                else
                {
                    //Create the vertical hall first
                    //RoomA's top
                    if (posDiff.y < 0)
                    {
                        yHall = new Room();
                        yHall.x = roomA.center.x - hallTickness / 2;
                        yHall.y = roomA.yMax;
                        yHall.width = hallTickness;
                        yHall.yMax = roomB.center.y;

                        if (posDiff.x <= 0)
                        {
                            xHall = new Room();
                            xHall.x = yHall.xMin;
                            xHall.y = yHall.yMax - hallTickness / 2;
                            xHall.height = hallTickness;
                            xHall.xMax = roomB.xMin;
                        }
                        else
                        {
                            xHall = new Room();
                            xHall.x = roomB.xMax;
                            xHall.y = yHall.yMax - hallTickness / 2;
                            xHall.height = hallTickness;
                            xHall.xMax = yHall.xMax;
                        }
                    }
                    //RoomA's bottom
                    else
                    {
                        yHall = new Room();
                        yHall.x = roomB.center.x - hallTickness / 2;
                        yHall.y = roomB.yMin;
                        yHall.width = hallTickness;
                        yHall.yMax = roomA.center.y;

                        if (posDiff.x <= 0)
                        {
                            xHall = new Room();
                            xHall.x = yHall.xMax;
                            xHall.y = yHall.yMax;
                            xHall.height = hallTickness;
                            xHall.xMax = roomA.xMax;
                        }
                        else
                        {
                            xHall = new Room();
                            xHall.x = yHall.xMin;
                            xHall.y = yHall.yMax;
                            xHall.height = hallTickness;
                            xHall.xMax = roomA.xMin;
                        }
                    }
                    halls.Add(xHall);
                    halls.Add(yHall);
                    UpdateMapRect(xHall.Rect);
                    UpdateMapRect(yHall.Rect);
                }
            }
        }
    }

    private void ReAddSecundaryRooms()
    {
        secundaryRooms = new List<Room>();

        foreach (Room h in halls)
        {
            foreach (Room r in allRooms)
            {
                if (!mainRooms.Contains(r) && !secundaryRooms.Contains(r))
                {
                    if (h.Overlaps(r))
                    {
                        UpdateMapRect(r.Rect);
                        secundaryRooms.Add(r);
                    }
                }
            }
        }
    }

#if UNITY_EDITOR
    void FixedUpdate()
    {
        {
            if (currentState == 0) //if adding rooms
            {
                if (allRooms.Count < numberOfRooms)
                {
                    currentStateTime += Time.fixedDeltaTime;
                    if (currentStateTime >= timeToAddRoom)
                    {
                        AddRoom();
                    }
                }
                else
                {
                    currentStateTime = 0;
                    currentState++;
                }
            }
            else if (currentState == 1) //if separating rooms
            {
                SeparateRooms();
            }
            else if (currentState == 2)
            {
                if (currentStateTime == 0)
                {
                    SelectMainRooms();
                }
                currentStateTime += Time.fixedDeltaTime;
                if (currentStateTime >= 2)
                {
                    currentState++;
                    currentStateTime = 0;
                }
            }
            else if (currentState == 3) //triangulating
            {
                if (currentStateTime == 0)
                {
                    DelaunayTriangulation();
                }
                currentStateTime += Time.fixedDeltaTime;
                if (currentStateTime >= 2)
                {
                    currentState++;
                    currentStateTime = 0;
                }
            }
            else if (currentState == 4) //generating minimum spanning tree
            {
                if (currentStateTime == 0)
                {
                    GetSpanningTree();
                }
                currentStateTime += Time.fixedDeltaTime;
                if (currentStateTime >= 2)
                {
                    currentState++;
                    currentStateTime = 0;
                }
            }
            else if (currentState == 5) //adding some vertices again
            {
                if (currentStateTime == 0)
                {
                    AddBackEdgesToSpanningTree();
                }
                currentStateTime += Time.fixedDeltaTime;
                if (currentStateTime >= 2)
                {
                    currentState++;
                    currentStateTime = 0;
                }
            }
            else if (currentState == 6) //creating halls
            {
                if (currentStateTime == 0)
                {
                    CreateHalls();
                }
                currentStateTime += Time.fixedDeltaTime;
                if (currentStateTime >= 2)
                {
                    currentState++;
                    currentStateTime = 0;
                }
            }
            else if (currentState == 7) // adding again secundary rooms that overlaps halls
            {
                if (currentStateTime == 0)
                {
                    ReAddSecundaryRooms();
                }
                currentStateTime += Time.fixedDeltaTime;
                if (currentStateTime >= 2)
                {
                    //currentState++;
                    //currentStateTime = 0;
                }
            }
        }
    }

    void Update()
    {
        DrawDungeon();

        if (Input.GetMouseButtonUp(0))
        {
            Application.LoadLevel(Application.loadedLevel);
        }
    }
#endif

    private void DrawDungeon()
    {
        if (currentState == 0 || currentState == 1)
        {
            foreach (Room r in allRooms)
            {
                DebugX.DrawRect(r.Rect, initialRandomRoomsColor);
            }
        }
        else if (currentState == 2)
        {
            foreach (Room r in allRooms)
            {
                DebugX.DrawRect(r.Rect, initialRandomRoomsColor);
            }
            foreach (Room r in mainRooms)
            {
                DebugX.DrawRect(r.Rect, mainRoomsColor);
            }
        }
        else if (currentState == 3)
        {
            foreach (Room r in allRooms)
            {
                DebugX.DrawRect(r.Rect, initialRandomRoomsColor);
            }
            foreach (Room r in mainRooms)
            {
                DebugX.DrawRect(r.Rect, mainRoomsColor);
            }

            if (delaunayTriangulation != null)
            {
                foreach (LineSegment l in delaunayTriangulation)
                {
                    Debug.DrawLine(new Vector3(l.p0.Value.x, l.p0.Value.y), new Vector3(l.p1.Value.x, l.p1.Value.y), pathsColor);
                }
            }
        }
        else if (currentState == 4 || currentState == 5)
        {
            foreach (Room r in allRooms)
            {
                DebugX.DrawRect(r.Rect, initialRandomRoomsColor);
            }
            foreach (Room r in mainRooms)
            {
                DebugX.DrawRect(r.Rect, mainRoomsColor);
            }

            if (spanningTree != null)
            {
                foreach (LineSegment l in spanningTree)
                {
                    Debug.DrawLine(new Vector3(l.p0.Value.x, l.p0.Value.y), new Vector3(l.p1.Value.x, l.p1.Value.y), pathsColor);
                }
            }
        }
        else if (currentState == 6)
        {
            foreach (Room r in allRooms)
            {
                DebugX.DrawRect(r.Rect, new Color(initialRandomRoomsColor.r, initialRandomRoomsColor.g, initialRandomRoomsColor.b, 0.3f));
            }

            foreach (Room r in mainRooms)
            {
                DebugX.DrawRect(r.Rect, mainRoomsColor);
            }

            if (halls != null)
            {
                foreach (Room r in halls)
                {
                    DebugX.DrawRect(r.Rect, secundaryRoomsColor);
                }
            }
        }
        else if (currentState == 7)
        {

            foreach (Room r in mainRooms)
            {
                DebugX.DrawRect(r.Rect, mainRoomsColor);
            }

            if (halls != null)
            {
                foreach (Room r in halls)
                {
                    DebugX.DrawRect(r.Rect, secundaryRoomsColor);
                }
            }

            if (secundaryRooms != null)
            {
                foreach (Room s in secundaryRooms)
                {
                    DebugX.DrawRect(s.Rect, secundaryRoomsColor);
                }
            }
        }

    }

    private Vector2 AABBvsAABB(Room a, Room b, out float xPenetration, out float yPenetration)
    {
        Vector2 n = b.center - a.center;
        Vector2 resultNormal = Vector2.zero;

        // Calculate half extents along x axis for each object
        float aExtent = (a.max.x - a.min.x) / 2;
        float bExtent = (b.max.x - b.min.x) / 2;

        xPenetration = yPenetration = 0;

        // Calculate overlap on x axis
        float xOverlap = aExtent + bExtent - Mathf.Abs(n.x);

        // SAT test on x axis
        if (xOverlap > 0)
        {
            // Calculate half extents along x axis for each object
            aExtent = (a.max.y - a.min.y) / 2;
            bExtent = (b.max.y - b.min.y) / 2;

            // Calculate overlap on y axis
            float yOverlap = aExtent + bExtent - Mathf.Abs(n.y);

            // SAT test on y axis
            if (yOverlap > 0)
            {
                // Find out which axis is axis of least penetration
                if (xOverlap > yOverlap)
                {
                    // Point towards B knowing that n points from A to B
                    if (n.x < 0)
                        resultNormal = new Vector2(-1, 0);
                    else
                        resultNormal = new Vector2(0, 0);
                    xPenetration = xOverlap;
                }
                else
                {
                    // Point toward B knowing that n points from A to B
                    if (n.y < 0)
                        resultNormal = new Vector2(0, -1);
                    else
                        resultNormal = new Vector2(0, 1);
                    yPenetration = yOverlap;
                }
            }
        }
        return resultNormal;
    }

    //The method that generates a room directly without printing steps.
    public void GenerateDungeon()
    {
        AddRooms();
        SeparateRoomsSilently();
        SelectMainRooms();
        DelaunayTriangulation();
        GetSpanningTree();
        AddBackEdgesToSpanningTree();
        CreateHalls();
        ReAddSecundaryRooms();
    }

    #endregion //Methods
}

public class RoomEdge
{
    private LineSegment lineSegment;
    private Room p0Rect;
    private Room p1Rect;

    public RoomEdge(LineSegment lineSegment, Room p0Rect, Room p1Rect)
    {
        this.lineSegment = lineSegment;
        this.p0Rect = p0Rect;
        this.p1Rect = p1Rect;
    }

    public LineSegment LineSegment
    {
        get
        {
            return lineSegment;
        }

        set
        {
            lineSegment = value;
        }
    }

    public Room P0Rect
    {
        get
        {
            return p0Rect;
        }

        set
        {
            p0Rect = value;
        }
    }

    public Room P1Rect
    {
        get
        {
            return p1Rect;
        }

        set
        {
            p1Rect = value;
        }
    }
}