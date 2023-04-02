using System;
using System.Drawing;

namespace I3IPC;

public class Highlight
{
    public string Name;
    public Color Border;
    public Color Background;
    public Color Text;
    public Color Indicator;
    public Color ChildBorder;

    public override string ToString()
    {
        ColorTranslator.ToHtml(this.Indicator);
        return String.Join(
            ' ',
            new string[]
            {
                this.Name,
                this.Border.ToHtml(),
                this.Background.ToHtml(),
                this.Text.ToHtml(),
                this.Indicator.ToHtml(),
                this.ChildBorder.ToHtml(),
            }
        );
    }
}

public static class Extensions
{
    public static string ToHtml(this Color color)
    {
        return ColorTranslator.ToHtml(color);
    }
}
