namespace AdminApi.Entities;

#pragma warning disable CS8618
public class MemberActivity
{
    public List<string> LoginDays { get; set; } = new();
    public List<string> EmailDays { get; set; } = new();
    public List<string> EmailErrorDays { get; set; } = new();
}