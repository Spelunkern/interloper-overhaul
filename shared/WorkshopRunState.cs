using System.Text.Json;
namespace InterloperOverhaul;

public sealed class ConversionState
{
    public string Recipe { get; set; } = "";
    public string WorkGuid { get; set; } = "";
    public string OriginalGuid { get; set; } = "";
    public float Condition { get; set; }
}
public sealed class WorkshopRunState
{
    public int Version { get; set; } = 1;
    public HashSet<string> Learned { get; set; } = new();
    public HashSet<string> Issued { get; set; } = new();
    public Dictionary<string,string> Cards { get; set; } = new();
    public List<ConversionState> Work { get; set; } = new();
    public string Serialize() => JsonSerializer.Serialize(this);
    public static WorkshopRunState Deserialize(string json)
    {
        var state = JsonSerializer.Deserialize<WorkshopRunState>(json) ?? throw new InvalidDataException("Missing workshop state.");
        if (state.Version != 1 || state.Learned == null || state.Issued == null || state.Cards == null || state.Work == null ||
            state.Learned.Any(s => VariantPolicy.Find(s) == null) || state.Cards.Values.Any(s => VariantPolicy.Find(s) == null) ||
            state.Work.Any(w => VariantPolicy.Recipe(w.Recipe) == null || !float.IsFinite(w.Condition) || w.Condition <= 0 || w.Condition > 1 ||
                !Guid.TryParse(w.OriginalGuid,out _) || !Guid.TryParse(w.WorkGuid,out _)) ||
            state.Work.Select(w => w.WorkGuid).Distinct().Count() != state.Work.Count)
            throw new InvalidDataException("Invalid workshop save state.");
        return state;
    }
}
