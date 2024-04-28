namespace Foxy.Params.SourceGenerator.Data
{
    public class ArgumentInfo
    {
        public string Type { get; set; }
        public string Name { get; set; }

        public string ToParameter()
        {
            return $"{Type} {Name}";
        }
    }
}

