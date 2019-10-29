using System;
using System.IO;

public static class logger
{
    public static string timeStamp;

    public static string leftX;
    public static string leftY;
    public static string rightX;
    public static string rightY;

    public static string objectIndactor;


    public static void Write()
    {
        string path = String.Format("Assets/Resources/log_{0}.txt", electron.GameTime.Replace(':','-'));

        using (StreamWriter writer = File.AppendText(path))
        {
            writer.WriteLine(leftX + "~~~" + leftY + "~~~" + rightX + "~~~" + rightY + "~~~" + objectIndactor);
        }

    }

}
