using MarketERP.Models;

namespace MarketERP.Helpers;

public static class SaleQueryExtensions
{
    public static IQueryable<Sale> ActiveSales(this IQueryable<Sale> query)
    {
        return query.Where(s => s.Status != Sale.CancelledStatus);
    }
}
