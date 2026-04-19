namespace FixTrading.OrderProcessing.Infrastructure.Fix.Configuration;

public sealed class FixSettings
{
    public const string SectionName = "Fix";

    public required string SenderCompId { get; set; }
    public required string TargetCompId { get; set; }
    public required string Host { get; set; }
    public required int Port { get; set; }
    public string BeginString { get; set; } = "FIX.4.4";
    public int HeartBeatInterval { get; set; } = 30;
    public bool ResetOnLogon { get; set; } = true;
    public bool ResetOnLogout { get; set; } = true;
    public bool ResetOnDisconnect { get; set; } = true;
    public string FileStorePath { get; set; } = "store";
    public string FileLogPath { get; set; } = "log";
}