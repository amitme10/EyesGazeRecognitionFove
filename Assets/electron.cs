using System;
using System.Collections.Generic;

public static class electron
{
    public static int realStep = 1;
    public static int Step = 0;
    public static string GameTime = DateTime.Now.ToString("HH:mm:ss tt");
    public static bool tresPass = false; // when step has changed
    public static bool clipper = false; // when step has changed - the using for the correctness
    public static bool timePass = false; // when step has changed - the using for the sendTimes
    public static bool done = false; // when the game is over
    public static bool finishedSendingCordinates = false; // whenfinished sending the cordinates on the wall then sign to FOVELookSample to close the Game
    public static bool[] samplesFinishedSigns = { false, false }; // dealing with the finish of the sending cordinates of the 2 samples - for us to be sure that all components that send cordinates are done and we can close the game
    //public static int[] corrects = {0,0,1,1,0,1,0};
    public static int[] corrects = { 0, 2, 5, 7, 8, 11, 12 };
    internal static string currentObject = null;
}