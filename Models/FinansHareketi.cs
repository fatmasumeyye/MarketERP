using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarketERP.Models;

[Table("finans_hareketleri")]
public class FinansHareketi
{
    [Column("id")]
    public int Id { get; set; }

    [Required, MaxLength(20)]
    [Column("tip")]
    public string Tip { get; set; } = "Gider";

    [Required, MaxLength(100)]
    [Column("kategori")]
    public string Kategori { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    [Column("baslik")]
    public string Baslik { get; set; } = string.Empty;

    [Column("tutar")]
    public decimal Tutar { get; set; }

    [Column("tarih")]
    public DateTime Tarih { get; set; } = DateTime.Today;

    [Required, MaxLength(30)]
    [Column("durum")]
    public string Durum { get; set; } = "Bekliyor";

    [Required, MaxLength(30)]
    [Column("odeme_yontemi")]
    public string OdemeYontemi { get; set; } = "BankaHavalesi";

    [MaxLength(1000)]
    [Column("aciklama")]
    public string? Aciklama { get; set; }

    [Column("sabit_gider_id")]
    public int? SabitGiderId { get; set; }

    [Column("olusturan_kullanici_id")]
    public int? OlusturanKullaniciId { get; set; }

    [Column("olusturma_tarihi")]
    public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;

    [MaxLength(50)]
    [Column("kaynak_tipi")]
    public string? KaynakTipi { get; set; }

    [Column("kaynak_id")]
    public int? KaynakId { get; set; }

    [MaxLength(100)]
    [Column("kaynak_no")]
    public string? KaynakNo { get; set; }

    [Column("otomatik_mi")]
    public bool OtomatikMi { get; set; }

    public SabitGider? SabitGider { get; set; }
    public Employee? OlusturanKullanici { get; set; }
}
