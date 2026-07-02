using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarketERP.Models;

[Table("sabit_giderler")]
public class SabitGider
{
    [Column("id")]
    public int Id { get; set; }

    [Required, MaxLength(150)]
    [Column("gider_adi")]
    public string GiderAdi { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    [Column("kategori")]
    public string Kategori { get; set; } = string.Empty;

    [Column("tutar")]
    public decimal Tutar { get; set; }

    [Required, MaxLength(30)]
    [Column("tekrar_tipi")]
    public string TekrarTipi { get; set; } = "Aylik";

    [Range(1, 31)]
    [Column("odeme_gunu")]
    public int OdemeGunu { get; set; } = 1;

    [Column("baslangic_tarihi")]
    public DateTime BaslangicTarihi { get; set; } = DateTime.Today;

    [Column("bitis_tarihi")]
    public DateTime? BitisTarihi { get; set; }

    [Column("aktif_mi")]
    public bool AktifMi { get; set; } = true;

    [MaxLength(1000)]
    [Column("aciklama")]
    public string? Aciklama { get; set; }

    [Column("olusturma_tarihi")]
    public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;

    public ICollection<FinansHareketi> FinansHareketleri { get; set; } = new List<FinansHareketi>();
}
