namespace MarketERP.Models;

public class FinansIndexViewModel
{
    public decimal BuAyToplamGelir { get; set; }
    public decimal BuAyToplamGider { get; set; }
    public decimal BuAyNetDurum => BuAyToplamGelir - BuAyToplamGider;
    public int BekleyenGiderSayisi { get; set; }
    public int GecikenGiderSayisi { get; set; }
    public List<FinansHareketi> SonHareketler { get; set; } = [];
}

public class FinansHareketlerViewModel
{
    public List<FinansHareketi> Hareketler { get; set; } = [];
    public List<string> Kategoriler { get; set; } = [];
    public string? Tip { get; set; }
    public string? Durum { get; set; }
    public string? Kategori { get; set; }
    public DateTime? BaslangicTarihi { get; set; }
    public DateTime? BitisTarihi { get; set; }
}
