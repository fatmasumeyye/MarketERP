namespace MarketERP.Models;

public class StockReconciliationViewModel
{
    public List<StockReconciliationItemViewModel> Items { get; set; } = [];
    public string StatusFilter { get; set; } = "all";
    public int TotalProductCount { get; set; }
    public int MatchedCount { get; set; }
    public int DifferenceCount { get; set; }
    public int MissingLedgerCount { get; set; }
}

public class StockReconciliationItemViewModel
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Barcode { get; set; } = string.Empty;
    public int CurrentQuantity { get; set; }
    public int LedgerQuantity { get; set; }
    public int Difference => CurrentQuantity - LedgerQuantity;
    public bool HasLedger { get; set; }
    public string Status => !HasLedger
        ? "Ledger Yok"
        : Difference == 0
            ? "Uyumlu"
            : "Fark Var";
}
