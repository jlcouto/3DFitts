using System.Collections;
using System.Collections.Generic;
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

    public static SimpleVector3 FromVector3(Vector3 vector)
    {
        return new SimpleVector3(vector);
    }
}
