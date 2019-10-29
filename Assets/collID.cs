using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class collID
{
    int collider;
    Guid guid;

    public collID(int collider, Guid guid)
    {
        this.collider = collider;
        this.guid = guid;
    }

    public int getCollider()
    {
        return this.collider;
    }

    public Guid getGuid()
    {
        return this.guid;
    }
}
