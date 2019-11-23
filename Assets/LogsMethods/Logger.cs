using System;
using System.IO;

public class Logger
{
    public string timeStamp { get; set; }

    public string leftX { get; set; }
    public string leftY { get; set; }
    public string rightX { get; set; }
    public string rightY { get; set; }

    public string objectIndactor { get; set; }


    public void Write()
    {
        string path = String.Format("Assets/Resources/log_{0}.txt", CommonData.GameTime.Replace(':','-'));

        using (StreamWriter writer = File.AppendText(path))
        {
            writer.WriteLine(leftX + "~~~" + leftY + "~~~" + rightX + "~~~" + rightY + "~~~" + objectIndactor);
        }

    }

}
