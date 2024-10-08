using Robust.Shared.Serialization;

namespace Content.Shared._NF.CartridgeLoader.Cartridges;

[Serializable, NetSerializable]
public sealed class LedgerStatsUiState : BoundUserInterfaceState
{
    public readonly LedgerStats Stats;
    public long TotalIncome { get; }
    public long TotalExpenses { get; }

    public LedgerStatsUiState(LedgerStats stats)
    {
        Stats = stats;
        TotalIncome = stats.TotalIncome();
        TotalExpenses = stats.TotalExpenses();
    }
}

public enum LedgerIncomeType : byte
{
    // Frontier jobs & commerce
    VendingTax = 0,
    CargoOrderTax = 1,
    MailIncome = 2,
    // NFSD activity
    BluespaceEventRewards = 3,
    AntiSmuggling = 4,
    // Pirate/syndie stuff (why?)
    ShipSalesTax = 5,
    BlackMarketATMTax = 6,
    // Catchalls for errors
    Other = 7,
    // Total count - do not use for deposits.
    Count = 8,
}

public enum LedgerExpenseType : byte
{
    // Station ATM withdrawal categories
    Payroll = 0,
    WorkOrder = 1,
    Supplies = 2,
    Bounty = 3,
    Other = 4,
    // Total count - do not use for deposits.
    Count = 5,
}

[DataDefinition]
[Serializable, NetSerializable]
public partial record struct LedgerStats
{
    public long[] Incomes { get; init; } = new long[(int)LedgerIncomeType.Count];
    public long[] Expenses { get; init; } = new long[(int)LedgerExpenseType.Count];

    public readonly long TotalIncome()
    {
        long ret = 0;
        foreach (var income in Incomes)
        {
            ret += income;
        }
        return ret;
    }

    public readonly long TotalExpenses()
    {
        long ret = 0;
        foreach (var expense in Expenses)
        {
            ret += expense;
        }
        return ret;
    }
}