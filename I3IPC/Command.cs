using System;
namespace I3IPC;
public class Commands
{
    public static string Subscribe(string sub)
    {
        return String.Format($"-t subscribe [\"\"\"{sub}\"\"\"]");
    }
}