using System.Globalization;
using System.Text;
using MarketERP.Models;

namespace MarketERP.Helpers
{
    public static class LeaveCalculator
    {
        private static readonly HashSet<string> AnnualLeaveNames =
            new(StringComparer.Ordinal)
            {
                "yillik",
                "yillik izin",
                "senelik",
                "senelik izin",
                "annual",
                "annual leave"
            };

        public static int CalculateRequestedDays(DateTime startDate, DateTime endDate)
        {
            if (endDate.Date < startDate.Date)
            {
                return 0;
            }

            return (endDate.Date - startDate.Date).Days + 1;
        }

        public static int CalculateSeniorityYears(DateTime? hireDate, DateTime asOfDate)
        {
            if (hireDate == null || hireDate.Value.Date > asOfDate.Date)
            {
                return 0;
            }

            var years = asOfDate.Year - hireDate.Value.Year;
            if (hireDate.Value.Date > asOfDate.Date.AddYears(-years))
            {
                years--;
            }

            return Math.Max(0, years);
        }

        public static int CalculateAnnualLeaveRight(
            DateTime? hireDate,
            DateTime asOfDate)
        {
            var seniorityYears = CalculateSeniorityYears(hireDate, asOfDate);

            if (seniorityYears < 1)
            {
                return 0;
            }

            if (seniorityYears <= 5)
            {
                return 14;
            }

            if (seniorityYears < 15)
            {
                return 20;
            }

            return 26;
        }

        public static bool IsAnnualLeave(string? leaveReason)
        {
            if (string.IsNullOrWhiteSpace(leaveReason))
            {
                return false;
            }

            return AnnualLeaveNames.Contains(Normalize(leaveReason));
        }

        public static int CalculateAnnualLeaveDays(
            IEnumerable<EmployeeLeave> leaves)
        {
            return leaves
                .Where(l => IsAnnualLeave(l.LeaveReason))
                .Sum(l => CalculateRequestedDays(l.StartDate, l.EndDate));
        }

        private static string Normalize(string value)
        {
            var decomposed = value.Trim().ToLowerInvariant()
                .Replace('ı', 'i')
                .Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder(decomposed.Length);

            foreach (var character in decomposed)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(character) !=
                    UnicodeCategory.NonSpacingMark)
                {
                    builder.Append(character);
                }
            }

            return string.Join(
                ' ',
                builder.ToString()
                    .Normalize(NormalizationForm.FormC)
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries));
        }
    }
}
