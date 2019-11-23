using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class CollID
{
    int collider;
    Guid guid;

    public CollID(int collider, Guid guid)
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
