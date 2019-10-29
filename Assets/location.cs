using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class location
{
    float x;
    float y;
    int step;

    public location(float x, float y, int step)
    {
        this.x = x;
        this.y = y;
        this.step = step;
    }

    public float getX()
    {
        return this.x;
    }

    public float getY()
    {
        return this.y;
    }

    public int getStep()
    {
        return this.step;
    }
}