using System;
using System.IO;
using System.Security.Cryptography;
using System.Text.Json;
using MergeStudio.Economy;
using MergeStudio.Gameplay;
using MergeStudio.Persistence;
using MergeStudio.Analytics;

int checks = 0;
void Check(bool condition, string name) { if (!condition) throw new Exception(name); checks++; }
var wallet = new Currency(10);
Check(!wallet.TrySpend(11) && wallet.Balance == 10, "Insufficient balance");
Check(wallet.TrySpend(10) && wallet.Balance == 0, "Spend");
try { wallet.TrySpend(-1); throw new Exception("Negative spend accepted"); } catch (ArgumentOutOfRangeException) { checks++; }
var energy = new EnergySystem(1, 100, 10, 60); energy.Tick(225);
Check(energy.Current == 3 && energy.Timestamp == 220, "Partial interval");
energy.Tick(10000); Check(energy.Current == 10, "Energy cap");
energy.TrySpend(1, 10000); energy.Tick(10001); Check(energy.Current == 9, "No banked energy");
var board = new MergeBoard(2, 2); board.Place(0, new Item("bread", 1)); board.Place(1, new Item("bread", 1));
Check(board.Merge(0, 1) && board[0] == null && board[1].Tier == 2, "Merge");
Check(!board.Merge(1, 1) && !board.Merge(-1, 1), "Invalid merge");
Check(board.Consume("bread", 2) && !board.Consume("bread", 2), "Consumption once");
board.Place(0, new Item("bread", 10)); board.Place(1, new Item("bread", 10));
Check(!board.MoveOrMerge(0, 1) && board[0].Tier == 10 && board[1].Tier == 10, "Tier cap");
Check(board.MoveOrMerge(0, 3) && board[0] == null && board[3].Tier == 10, "Move to empty cell");
Check(!board.MoveOrMerge(-1, 2) && !board.MoveOrMerge(3, 4) && !board.MoveOrMerge(0, 2), "Invalid move");
var recorder = new AnalyticsRecorder(); var analytics = new ConsentAnalyticsService(recorder);
analytics.LogEvent("session_start", null); Check(recorder.Calls == 0, "Consent defaults off");
analytics.SetConsent(true); analytics.LogEvent("session_start", null); Check(recorder.Calls == 1, "Consent permits delivery");
analytics.SetConsent(false); analytics.LogEvent("session_start", null); Check(recorder.Calls == 1, "Revocation stops delivery");
string directory = Path.Combine(Path.GetTempPath(), "MergeStudioDomainCheck", Guid.NewGuid().ToString());
try
{
    var save = new SaveSystem(directory); save.Save(new SaveData { Gold = 53 });
    Check(save.Load().Gold == 53, "Encrypted round trip");
    string file = Path.Combine(directory, "save.dat"); var original = File.ReadAllBytes(file);
    save.Save(new SaveData { Gold = 53 }); Check(!Convert.ToBase64String(original).Equals(Convert.ToBase64String(File.ReadAllBytes(file))), "Random IV");
    File.WriteAllBytes(file, new byte[2]); Check(save.Load().Gold == 53, "Backup recovery");
    save.DeleteSave(); Check(!save.HasSave(), "Delete all save generations");
    save.Save(new SaveData()); var bytes = File.ReadAllBytes(file); bytes[40] ^= 1; File.WriteAllBytes(file, bytes);
    try { save.Load(); throw new Exception("Tampering accepted"); } catch (CryptographicException) { checks++; }
}
finally { Directory.Delete(directory, true); }
Console.WriteLine($"Domain checks: {checks} passed. Save JSON adapter uses System.Text.Json; Unity JsonUtility and device filesystems still require Unity tests.");

namespace MergeStudio.Persistence
{
    // CLI adapter only. Production uses the Unity JsonUtility implementation.
    public static class Serialization
    {
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions { IncludeFields = true };
        public static string ToJson(SaveData data) { data.Validate(); return JsonSerializer.Serialize(data, Options); }
        public static SaveData FromJson(string json) { var data = JsonSerializer.Deserialize<SaveData>(json, Options); data.Validate(); return data; }
    }
}
sealed class AnalyticsRecorder : IAnalyticsService
{
    public int Calls;
    public void LogEvent(string name, System.Collections.Generic.Dictionary<string, object> parameters) => Calls++;
}
