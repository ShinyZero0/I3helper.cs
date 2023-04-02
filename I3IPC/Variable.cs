public class Variable
{
    public string Name;
    public string Value;
    public Variable(string name, string value)
    {
        Name = name;
        Value = value;
    }
    public override string ToString()
    {
        return String.Format($"set ${this.Name} {this.Value}");
    }
}
