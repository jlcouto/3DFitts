using UnityEngine;

public class SimpleVector3
{
    public float x;
    public float y;
    public float z;

    public SimpleVector3(float x, float y, float z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public SimpleVector3(Vector3 vector)
    {
        this.x = vector.x;
        this.y = vector.y;
        this.z = vector.z;
    }

    public static Vector3 ToVector3(SimpleVector3 vector)
    {
        return new Vector3(vector.x, vector.y, vector.z);
    }

    public static SimpleVector3 FromVector3(Vector3 vector)
    {
        return new SimpleVector3(vector);
    }

    public override string ToString()
    {
        return "(" + this.x + ", " + this.y + ", " + this.z + ")";
    }
}

public class SimpleVector2
{
    public float x;
    public float y;

    public SimpleVector2(float x, float y, float z)
    {
        this.x = x;
        this.y = y;
    }

    public SimpleVector2(Vector2 vector)
    {
        this.x = vector.x;
        this.y = vector.y;
    }

    public static Vector2 ToVector2(SimpleVector2 vector)
    {
        return new Vector2(vector.x, vector.y);
    }

    public static SimpleVector2 FromVector2(Vector2 vector)
    {
        return new SimpleVector2(vector);
    }

    public override string ToString()
    {
        return "(" + this.x + ", " + this.y + ")";
    }
}
