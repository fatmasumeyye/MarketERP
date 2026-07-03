using MarketERP.Data;
using MarketERP.Helpers;
using MarketERP.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace MarketERP.Controllers;

[PermissionAuthorize("role.manage")]
public class FinansController : Controller
{
    private static readonly string[] Tipler = ["Gelir", "Gider"];
    private static readonly string[] Durumlar = ["Bekliyor", "Odendi", "Gecikti", "Iptal"];
    private static readonly string[] OdemeYontemleri = ["Nakit", "KrediKarti", "BankaHavalesi", "POS", "Diger"];
    private static readonly string[] TekrarTipleri = ["Aylik", "Yillik", "TekSeferlik"];
    private static readonly string[] VarsayilanKategoriler =
    [
        "Kira", "Elektrik", "Su", "Internet", "Muhasebeci Ucreti",
        "Personel Maasi", "Yazilim Aboneligi", "Vergi", "Bakim ve Onarim", "Diger"
    ];

    private readonly AppDbContext _context;

    public FinansController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        if (!IsAdmin()) return Forbid();

        var monthStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var nextMonth = monthStart.AddMonths(1);
        var paidThisMonth = _context.FinansHareketleri.AsNoTracking()
            .Where(h => h.Durum == "Odendi" && h.Tarih >= monthStart && h.Tarih < nextMonth);

        return View(new FinansIndexViewModel
        {
            BuAyToplamGelir = await paidThisMonth.Where(h => h.Tip == "Gelir")
                .SumAsync(h => (decimal?)h.Tutar) ?? 0,
            BuAyToplamGider = await paidThisMonth.Where(h => h.Tip == "Gider")
                .SumAsync(h => (decimal?)h.Tutar) ?? 0,
            BekleyenGiderSayisi = await _context.FinansHareketleri
                .CountAsync(h => h.Tip == "Gider" && h.Durum == "Bekliyor"),
            GecikenGiderSayisi = await _context.FinansHareketleri
                .CountAsync(h => h.Tip == "Gider" && h.Durum == "Gecikti"),
            SonHareketler = await _context.FinansHareketleri.AsNoTracking()
                .Include(h => h.SabitGider)
                .OrderByDescending(h => h.Tarih).ThenByDescending(h => h.Id)
                .Take(10).ToListAsync()
        });
    }

    [HttpGet]
    public async Task<IActionResult> Hareketler(
        string? tip, string? durum, string? kategori,
        DateTime? baslangicTarihi, DateTime? bitisTarihi)
    {
        if (!IsAdmin()) return Forbid();

        var query = _context.FinansHareketleri.AsNoTracking()
            .Include(h => h.SabitGider)
            .Include(h => h.OlusturanKullanici)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(tip)) query = query.Where(h => h.Tip == tip);
        if (!string.IsNullOrWhiteSpace(durum)) query = query.Where(h => h.Durum == durum);
        if (!string.IsNullOrWhiteSpace(kategori)) query = query.Where(h => h.Kategori == kategori);
        if (baslangicTarihi.HasValue) query = query.Where(h => h.Tarih >= baslangicTarihi.Value.Date);
        if (bitisTarihi.HasValue)
        {
            var exclusiveEnd = bitisTarihi.Value.Date.AddDays(1);
            query = query.Where(h => h.Tarih < exclusiveEnd);
        }

        var model = new FinansHareketlerViewModel
        {
            Hareketler = await query.OrderByDescending(h => h.Tarih)
                .ThenByDescending(h => h.Id).ToListAsync(),
            Kategoriler = await _context.FinansHareketleri.AsNoTracking()
                .Select(h => h.Kategori).Where(k => k != "")
                .Distinct().OrderBy(k => k).ToListAsync(),
            Tip = tip,
            Durum = durum,
            Kategori = kategori,
            BaslangicTarihi = baslangicTarihi,
            BitisTarihi = bitisTarihi
        };

        SetFinanceOptions();
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> HareketEkle(int? sabitGiderId)
    {
        if (!IsAdmin()) return Forbid();

        var model = new FinansHareketi
        {
            Tarih = DateTime.Today,
            Durum = "Bekliyor",
            OdemeYontemi = "BankaHavalesi",
            Tip = "Gider"
        };

        if (sabitGiderId.HasValue)
        {
            var sabitGider = await _context.SabitGiderler.AsNoTracking()
                .FirstOrDefaultAsync(g => g.Id == sabitGiderId.Value);
            if (sabitGider != null)
            {
                model.SabitGiderId = sabitGider.Id;
                model.Kategori = sabitGider.Kategori;
                model.Baslik = sabitGider.GiderAdi;
                model.Tutar = sabitGider.Tutar;
            }
        }

        await SetMovementFormOptionsAsync();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> HareketEkle(FinansHareketi model, string? tutarMetni)
    {
        if (!IsAdmin()) return Forbid();

        ValidateAndSetAmount(model, tutarMetni);
        ValidateMovement(model);
        if (!ModelState.IsValid)
        {
            await SetMovementFormOptionsAsync();
            return View(model);
        }

        model.Tip = model.Tip.Trim();
        model.Kategori = model.Kategori.Trim();
        model.Baslik = model.Baslik.Trim();
        model.Durum = model.Durum.Trim();
        model.OdemeYontemi = model.OdemeYontemi.Trim();
        model.Aciklama = model.Aciklama?.Trim();
        model.Tarih = model.Tarih.Date;
        model.OlusturanKullaniciId = HttpContext.Session.GetInt32("EmployeeId");
        model.OlusturmaTarihi = DateTime.Now;

        _context.FinansHareketleri.Add(model);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Finans hareketi kaydedildi.";
        return RedirectToAction(nameof(Hareketler));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> OdendiYap(
        int id,
        DateTime odemeTarihi,
        string odemeYontemi,
        string? odemeNotu)
    {
        if (!IsAdmin()) return Forbid();

        var movement = await _context.FinansHareketleri.FindAsync(id);
        if (movement == null)
        {
            TempData["Error"] = "Finans hareketi bulunamadı.";
            return RedirectToAction(nameof(Hareketler));
        }

        if (movement.Durum == "Iptal")
        {
            TempData["Error"] = "İptal edilmiş finans hareketi ödenemez.";
            return RedirectToAction(nameof(Hareketler));
        }

        if (movement.Durum == "Odendi")
        {
            TempData["Error"] = "Bu finans hareketi daha önce ödendi olarak işaretlenmiş.";
            return RedirectToAction(nameof(Hareketler));
        }

        if (movement.Durum != "Bekliyor")
        {
            TempData["Error"] = "Yalnız bekleyen finans hareketleri ödendi olarak işaretlenebilir.";
            return RedirectToAction(nameof(Hareketler));
        }

        if (odemeTarihi == default || odemeTarihi.Date > DateTime.Today)
        {
            TempData["Error"] = "Ödeme tarihi bugün veya geçmiş bir tarih olmalıdır.";
            return RedirectToAction(nameof(Hareketler));
        }

        odemeYontemi = odemeYontemi?.Trim() ?? string.Empty;
        if (!OdemeYontemleri.Contains(odemeYontemi, StringComparer.Ordinal))
        {
            TempData["Error"] = "Geçerli bir ödeme yöntemi seçmelisiniz.";
            return RedirectToAction(nameof(Hareketler));
        }

        odemeNotu = odemeNotu?.Trim();
        string paymentAudit = $"Ödeme tarihi: {odemeTarihi:dd.MM.yyyy} | Ödeme yöntemi: {GetPaymentMethodLabel(odemeYontemi)}";
        if (!string.IsNullOrWhiteSpace(odemeNotu))
        {
            paymentAudit += $" | Ödeme notu: {odemeNotu}";
        }

        string description = string.IsNullOrWhiteSpace(movement.Aciklama)
            ? paymentAudit
            : $"{movement.Aciklama.Trim()} | {paymentAudit}";

        string finalDescription = description.Length <= 1000
            ? description
            : description[..1000];

        int updatedRows = await _context.FinansHareketleri
            .Where(h => h.Id == id && h.Durum == "Bekliyor")
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(h => h.Durum, "Odendi")
                .SetProperty(h => h.Tarih, odemeTarihi.Date)
                .SetProperty(h => h.OdemeYontemi, odemeYontemi)
                .SetProperty(h => h.Aciklama, finalDescription));

        if (updatedRows == 0)
        {
            TempData["Error"] = "Finans hareketinin durumu başka bir işlem tarafından değiştirildi. Listeyi yenileyip tekrar kontrol edin.";
            return RedirectToAction(nameof(Hareketler));
        }

        TempData["Success"] = "Finans hareketi ödendi olarak işaretlendi.";
        return RedirectToAction(nameof(Hareketler));
    }

    [HttpGet]
    public async Task<IActionResult> SabitGiderler()
    {
        if (!IsAdmin()) return Forbid();

        var items = await _context.SabitGiderler.AsNoTracking()
            .OrderByDescending(g => g.AktifMi)
            .ThenBy(g => g.OdemeGunu).ThenBy(g => g.GiderAdi)
            .ToListAsync();
        var ids = items.Select(x => x.Id).ToList();
        var movements = await _context.FinansHareketleri.AsNoTracking()
            .Where(h => h.SabitGiderId.HasValue && ids.Contains(h.SabitGiderId.Value))
            .OrderByDescending(h => h.Tarih).ToListAsync();
        var monthStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var monthEnd = monthStart.AddMonths(1);
        var validMovements = movements.Where(h => h.Durum != "Iptal").ToList();
        ViewBag.ScheduleInfo = items.ToDictionary(
            item => item.Id,
            item => BuildScheduleInfo(
                item,
                validMovements.Where(h => h.SabitGiderId == item.Id).ToList(),
                monthStart,
                monthEnd));
        return View(items);
    }

    private static SabitGiderScheduleViewModel BuildScheduleInfo(
        SabitGider item,
        IReadOnlyCollection<FinansHareketi> movements,
        DateTime monthStart,
        DateTime monthEnd)
    {
        var lastMovement = movements.OrderByDescending(h => h.Tarih).FirstOrDefault();
        var processedThisMonth = movements.Any(h => h.Tarih >= monthStart && h.Tarih < monthEnd);
        var result = new SabitGiderScheduleViewModel
        {
            LastMovement = lastMovement,
            ProcessedThisMonth = processedThisMonth
        };

        if (!item.AktifMi)
        {
            result.NextDateLabel = "Devre dışı";
            return result;
        }

        var today = DateTime.Today;
        DateTime candidate;
        if (item.TekrarTipi == "TekSeferlik")
        {
            if (lastMovement != null)
            {
                result.NextDateLabel = "Tek seferlik işlendi";
                return result;
            }

            if (item.BaslangicTarihi.Date < today)
            {
                result.NextDateLabel = "Süresi geçti";
                return result;
            }

            candidate = item.BaslangicTarihi.Date;
        }
        else if (item.TekrarTipi == "Yillik")
        {
            candidate = new DateTime(today.Year, item.BaslangicTarihi.Month, Math.Min(item.OdemeGunu, DateTime.DaysInMonth(today.Year, item.BaslangicTarihi.Month)));
            if (candidate < today || movements.Any(h => h.Tarih.Year == candidate.Year)) candidate = candidate.AddYears(1);
        }
        else
        {
            candidate = new DateTime(today.Year, today.Month, Math.Min(item.OdemeGunu, DateTime.DaysInMonth(today.Year, today.Month)));
            if (candidate < today || processedThisMonth) { var next = today.AddMonths(1); candidate = new DateTime(next.Year, next.Month, Math.Min(item.OdemeGunu, DateTime.DaysInMonth(next.Year, next.Month))); }
        }
        if (candidate < item.BaslangicTarihi.Date) candidate = item.BaslangicTarihi.Date;
        if (item.BitisTarihi.HasValue && candidate > item.BitisTarihi.Value.Date)
        {
            result.NextDateLabel = "Tamamlandı";
            return result;
        }

        result.NextDate = candidate;
        result.NextDateLabel = candidate.ToString("dd.MM.yyyy");
        return result;
    }

    [HttpGet]
    public IActionResult SabitGiderEkle()
    {
        if (!IsAdmin()) return Forbid();

        SetFinanceOptions();
        return View(new SabitGider
        {
            BaslangicTarihi = DateTime.Today,
            OdemeGunu = 1,
            TekrarTipi = "Aylik",
            AktifMi = true
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SabitGiderEkle(SabitGider model, string? tutarMetni)
    {
        if (!IsAdmin()) return Forbid();

        ValidateAndSetAmount(model, tutarMetni);
        if (!TekrarTipleri.Contains(model.TekrarTipi, StringComparer.Ordinal))
            ModelState.AddModelError(nameof(model.TekrarTipi), "Gecerli bir tekrar tipi secin.");

        if (model.BitisTarihi.HasValue && model.BitisTarihi.Value.Date < model.BaslangicTarihi.Date)
            ModelState.AddModelError(nameof(model.BitisTarihi), "Bitis tarihi baslangic tarihinden once olamaz.");

        if (!ModelState.IsValid)
        {
            SetFinanceOptions();
            return View(model);
        }

        model.GiderAdi = model.GiderAdi.Trim();
        model.Kategori = model.Kategori.Trim();
        model.TekrarTipi = model.TekrarTipi.Trim();
        model.Aciklama = model.Aciklama?.Trim();
        model.BaslangicTarihi = model.BaslangicTarihi.Date;
        model.BitisTarihi = model.BitisTarihi?.Date;
        model.OlusturmaTarihi = DateTime.Now;

        _context.SabitGiderler.Add(model);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Sabit gider tanimi kaydedildi.";
        return RedirectToAction(nameof(SabitGiderler));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SabitGiderDurumDegistir(int id)
    {
        if (!IsAdmin()) return Forbid();

        var item = await _context.SabitGiderler.FindAsync(id);
        if (item == null)
        {
            TempData["Error"] = "Sabit gider bulunamadi.";
            return RedirectToAction(nameof(SabitGiderler));
        }

        item.AktifMi = !item.AktifMi;
        await _context.SaveChangesAsync();
        TempData["Success"] = item.AktifMi
            ? "Sabit gider aktiflestirildi."
            : "Sabit gider pasiflestirildi.";
        return RedirectToAction(nameof(SabitGiderler));
    }

    private void ValidateMovement(FinansHareketi model)
    {
        if (!Tipler.Contains(model.Tip, StringComparer.Ordinal))
            ModelState.AddModelError(nameof(model.Tip), "Gecerli bir hareket tipi secin.");
        if (!Durumlar.Contains(model.Durum, StringComparer.Ordinal))
            ModelState.AddModelError(nameof(model.Durum), "Gecerli bir durum secin.");
        if (!OdemeYontemleri.Contains(model.OdemeYontemi, StringComparer.Ordinal))
            ModelState.AddModelError(nameof(model.OdemeYontemi), "Gecerli bir odeme yontemi secin.");
        if (model.SabitGiderId.HasValue
            && !_context.SabitGiderler.Any(g => g.Id == model.SabitGiderId.Value))
            ModelState.AddModelError(nameof(model.SabitGiderId), "Secilen sabit gider bulunamadi.");
    }

    private void ValidateAndSetAmount(FinansHareketi model, string? tutarMetni)
    {
        ViewData["TutarMetni"] = tutarMetni;
        if (TryParsePositiveAmount(tutarMetni, out var amount, out var error))
            model.Tutar = amount;
        else
            ModelState.AddModelError("TutarMetni", error);
    }

    private void ValidateAndSetAmount(SabitGider model, string? tutarMetni)
    {
        ViewData["TutarMetni"] = tutarMetni;
        if (TryParsePositiveAmount(tutarMetni, out var amount, out var error))
            model.Tutar = amount;
        else
            ModelState.AddModelError("TutarMetni", error);
    }

    private static bool TryParsePositiveAmount(string? value, out decimal amount, out string error)
    {
        amount = 0;
        if (string.IsNullOrWhiteSpace(value))
        {
            error = "Tutar zorunludur.";
            return false;
        }

        var normalized = value.Trim().Replace(" ", string.Empty);
        var lastComma = normalized.LastIndexOf(',');
        var lastDot = normalized.LastIndexOf('.');

        if (lastComma >= 0 && lastDot >= 0)
        {
            normalized = lastComma > lastDot
                ? normalized.Replace(".", string.Empty).Replace(',', '.')
                : normalized.Replace(",", string.Empty);
        }
        else
        {
            normalized = normalized.Replace(',', '.');
        }

        if (!decimal.TryParse(
                normalized,
                NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture,
                out amount))
        {
            error = "Tutar sayisal olmalidir. Ornek: 35000,50 veya 35000.50.";
            return false;
        }

        if (amount <= 0)
        {
            error = "Tutar sifirdan buyuk olmalidir.";
            return false;
        }

        error = string.Empty;
        return true;
    }

    private async Task SetMovementFormOptionsAsync()
    {
        SetFinanceOptions();
        ViewBag.SabitGiderler = await _context.SabitGiderler.AsNoTracking()
            .Where(g => g.AktifMi).OrderBy(g => g.GiderAdi).ToListAsync();
    }

    private void SetFinanceOptions()
    {
        ViewBag.Tipler = Tipler;
        ViewBag.Durumlar = Durumlar;
        ViewBag.OdemeYontemleri = OdemeYontemleri;
        ViewBag.TekrarTipleri = TekrarTipleri;
        ViewBag.Kategoriler = VarsayilanKategoriler;
    }

    private static string GetPaymentMethodLabel(string paymentMethod)
    {
        return paymentMethod switch
        {
            "KrediKarti" => "Kredi Kartı",
            "BankaHavalesi" => "Banka Havalesi",
            "Diger" => "Diğer",
            _ => paymentMethod
        };
    }

    private bool IsAdmin()
    {
        return (HttpContext.Session.GetString("Roles") ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Contains("Admin", StringComparer.OrdinalIgnoreCase);
    }
}
