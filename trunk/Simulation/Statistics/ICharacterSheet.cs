namespace DawnOnline.Simulation.Statistics
{
    public interface ICharacterSheet
    {
        IMonitor iFatigue { get; }
        IMonitor iDamage { get; }
    }
}
