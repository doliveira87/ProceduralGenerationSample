using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Room
{
    private Rect rect;
    private bool isMainRoom;

    public Room()
    {
        this.Rect = new Rect();
    }

    public Room(Room r) : this(r.Rect)
    {

    }

    public Room(Rect rect)
    {
        this.rect = rect;
    }

    public Room(float x, float y, float width, float height) : this(new Rect(x, y, width, height))
    {
    }

    public bool Overlaps(Room r)
    {
        return this.rect.Overlaps(r.Rect);
    }

    public bool Contains(Vector2 point)
    {
        return rect.Contains(point);
    }

    public Vector2 position
    {
        get { return rect.position; }
        set { rect.position = value; }
    }

    public Vector2 size
    {
        get { return rect.size; }
        set { rect.size = value; }
    }

    public Vector2 center
    {
        get { return rect.center; }
        set { rect.center = value; }
    }

    public float width
    {
        get { return rect.width; }
        set { rect.width = value; }
    }

    public float height
    {
        get { return rect.height; }
        set { rect.height = value; }
    }

    public float x
    {
        get { return rect.x; }
        set { rect.x = value; }
    }

    public float y
    {
        get { return rect.y; }
        set { rect.y = value; }
    }

    public float xMin
    {
        get { return rect.xMin; }
        set { rect.xMin = value; }
    }

    public float xMax
    {
        get { return rect.xMax; }
        set { rect.xMax = value; }
    }

    public float yMin
    {
        get { return rect.yMin; }
        set { rect.yMin = value; }
    }

    public float yMax
    {
        get { return rect.yMax; }
        set { rect.yMax = value; }
    }

    public Vector2 min
    {
        get { return rect.min; }
        set { rect.min = value; } 
    }

    public Vector2 max
    {
        get { return rect.max; }
        set { rect.max = value; }
    }

    public List<GameObject> GroundTiles
    {
        get;
        set;
    }

    public List<GameObject> WallTiles
    {
        get;
        set;
    }

    public Rect Rect
    {
        get
        {
            return rect;
        }

        set
        {
            rect = value;
        }
    }

    public bool IsMainRoom
    {
        get
        {
            return isMainRoom;
        }

        set
        {
            isMainRoom = value;
        }
    }
}
