using System;

namespace I3Helper;

public static class Commands
{
    public static string Subscribe(string sub)
    {
        return String.Format($"""-t subscribe '["{sub}"]' -m""");
    }
}
