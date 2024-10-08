using Content.Server._NF.Bank.Components;
using Content.Server._NF.SectorServices;
using Content.Shared._NF.CartridgeLoader.Cartridges;
using JetBrains.Annotations;

namespace Content.Server._NF.Bank;
public sealed partial class SectorLedgerSystem : EntitySystem
{
    [Dependency] SectorServiceSystem _sector = default!;
    public override void Initialize()
    {
        base.Initialize();
    }

    // TODO: add account detail when splitting bank balances
    [PublicAPI]
    public void AddIncome(LedgerIncomeType incomeType, int amount)
    {
        if (TryComp(_sector.GetServiceEntity(), out SectorLedgerStatsComponent? component))
        {
            if (incomeType < 0 || incomeType >= LedgerIncomeType.Count)
                incomeType = LedgerIncomeType.Other;
            component.Stats.Incomes[(int)incomeType] += amount;
            UpdateLedgerStats();
        }
    }

    [PublicAPI]
    public void AddExpense(LedgerExpenseType expenseType, int amount)
    {
        if (TryComp(_sector.GetServiceEntity(), out SectorLedgerStatsComponent? component))
        {
            if (expenseType < 0 || expenseType >= LedgerExpenseType.Count)
                expenseType = LedgerExpenseType.Other;
            component.Stats.Expenses[(int)expenseType] += amount;
            UpdateLedgerStats();
        }
    }

    private void UpdateLedgerStats() => RaiseLocalEvent(new LedgerUpdatedEvent()); // Frontier: remove EntityUid from args
}

// Frontier: removed station EntityUid as an argument in LogisticStatsUpdatedEvent
public sealed class LedgerUpdatedEvent : EntityEventArgs
{
    public LedgerUpdatedEvent()
    {
    }
}
