using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[Serializable()]
public class Index2
{
    public int X;
    public int Y;

    public Index2() { }

    public Index2(int x, int y)
    {
        X = x;
        Y = y;
    }
    public Index2(Vector3 vec)
    {
        X = Mathf.FloorToInt(vec.x);
        Y = Mathf.FloorToInt(vec.y);
    }
    public Vector3 AsVector3()
    {
        return new Vector3((float)X, (float)Y);
    }
    public override string ToString()
    {
        return string.Format("[Index]X : " + X + " Y: " + Y);
    }

    public override int GetHashCode()
    {
        return X ^ (Y.GetHashCode() + int.MaxValue / 2);
    }
    public static implicit operator Index2(Vector3 vec)
    {
        return new Index2(vec);
    }
    public override bool Equals(object obj)
    {
        if (obj == null)
        {
            return false;
        }
        Index2 objIndex = obj as Index2;

        if ((System.Object)objIndex == null)
        {
            return false;
        }
        return (objIndex.X == X && objIndex.Y == Y);
    }

    public static bool operator ==(Index2 a, Index2 b)
    {
        // If both are null, or both are same instance, return true.
        if (System.Object.ReferenceEquals(a, b))
        {
            return true;
        }

        // If one is null, but not both, return false.
        if (((object)a == null) || ((object)b == null))
        {
            return false;
        }
        return (a.X == b.X && a.Y == b.Y);
    }
    public static bool operator !=(Index2 a, Index2 b)
    {
        return !(a == b);
    }
    public static Index2 operator +(Index2 a, Index2 b)
    {
        Index2 retIndex = new Index2();
        retIndex.X = a.X + b.X;
        retIndex.Y = a.Y + b.Y;
        return retIndex;
    }
    public static Index2 operator /(Index2 a, int b)
    {
        Index2 retIndex = new Index2();
        retIndex.X = a.X / b;
        retIndex.Y = a.Y / b;
        return retIndex;
    }
    public static Index2 operator -(Index2 a, Index2 b)
    {
        Index2 retIndex = new Index2();
        retIndex.X = a.X - b.X;
        retIndex.Y = a.Y - b.Y;
        return retIndex;
    }

    public int getSquaredDistanceToIndex(Index2 destIndex)
    {
        int xTotal = destIndex.X - X;
        int yTotal = destIndex.Y - Y;
        return (xTotal * xTotal) + (yTotal * yTotal);
    }
    public int distanceToIndex(Index2 destIndex)
    {
        return (int)Math.Sqrt(getSquaredDistanceToIndex(destIndex));
    }
    public int GetSimpleDistance(Index2 destIndex)
    {
        int xTotal = destIndex.X - X;
        int yTotal = destIndex.Y - Y;
        return Mathf.Abs(xTotal) + Mathf.Abs(yTotal);
    }

    public bool isAdjacent(Index2 destIndex)
    {
        return getSquaredDistanceToIndex(destIndex) < 2;
    }
}


