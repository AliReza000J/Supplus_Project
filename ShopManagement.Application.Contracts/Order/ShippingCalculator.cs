using System.Text.Json;

namespace ShopManagement.Application.Contracts.Order
{
    public static class ShippingCalculator
    {
        private const double BaseWeightThreshold = 400.0;   // تا 400 گرم
        private const double MediumWeightThreshold = 800.0; // تا 800 گرم

        private const double Tier1Cost = 100_000; // تا 400 گرم
        private const double Tier2Cost = 125_000; // 400 تا 800 گرم
        private const double Tier3Cost = 150_000; // بالاتر از 800 گرم

        private const double DefaultItemWeight = 0.0;

        public static double ExtractWeightInGrams(string? specificationsJson)
        {
            if (string.IsNullOrWhiteSpace(specificationsJson))
                return DefaultItemWeight;

            try
            {
                using var doc = JsonDocument.Parse(specificationsJson);
                var root = doc.RootElement;

                foreach (var prop in root.EnumerateObject())
                {
                    if (string.Equals(prop.Name, "ProductWeight", StringComparison.OrdinalIgnoreCase))
                    {
                        if (prop.Value.ValueKind == JsonValueKind.Number && prop.Value.TryGetDouble(out var numWeight))
                            return numWeight;

                        if (prop.Value.ValueKind == JsonValueKind.String &&
                            double.TryParse(prop.Value.GetString(), out var strWeight))
                            return strWeight;
                    }
                }
            }
            catch
            {
                return DefaultItemWeight;
            }

            return DefaultItemWeight;
        }

        public static double CalculateCostByTotalWeight(double totalWeightInGrams)
        {
            if (totalWeightInGrams <= BaseWeightThreshold)
                return Tier1Cost;

            if (totalWeightInGrams <= MediumWeightThreshold)
                return Tier2Cost;

            return Tier3Cost;
        }

        public static double CalculateShippingCost(IEnumerable<(string? Specifications, int Quantity)> items)
        {
            double totalWeight = 0;

            foreach (var (Specifications, Quantity) in items)
            {
                var unitWeight = ExtractWeightInGrams(Specifications);
                totalWeight += unitWeight * Math.Max(1, Quantity);
            }

            return CalculateCostByTotalWeight(totalWeight);
        }
    }
}