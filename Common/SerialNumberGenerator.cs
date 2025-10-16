using AssetTaking.Models;

namespace AssetTaking.Common
{
    public static class SerialNumberGenerator
    {
        /// <summary>
        /// Generate automatic serial number based on category
        /// Format: {CategoryCode}{5-digit-number} (e.g., RND12391, SPAREPART10001)
        /// </summary>
        /// <param name="context">Database context</param>
        /// <param name="kategoriBarang">Category of the item</param>
        /// <returns>Unique serial number</returns>
        public static string GenerateSerialNumber(DbRndAssetTakingContext context, string kategoriBarang)
        {
            // Get category code
            string categoryCode = GetCategoryCode(kategoriBarang);
            
            // Get the last serial number for this category
            var lastSerial = context.TblRAssetSerials
                .Where(s => s.SerialNumber.StartsWith(categoryCode))
                .OrderByDescending(s => s.SerialNumber)
                .FirstOrDefault();

            int nextNumber = 1;
            
            if (lastSerial != null)
            {
                // Extract the numeric part from the last serial number
                string numericPart = lastSerial.SerialNumber.Substring(categoryCode.Length);
                if (int.TryParse(numericPart, out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            // Format with 5 digits (padded with zeros)
            string serialNumber = $"{categoryCode}{nextNumber:D5}";
            
            // Ensure uniqueness (in case of concurrent operations)
            while (context.TblRAssetSerials.Any(s => s.SerialNumber == serialNumber))
            {
                nextNumber++;
                serialNumber = $"{categoryCode}{nextNumber:D5}";
            }

            return serialNumber;
        }

        /// <summary>
        /// Generate multiple serial numbers for a given quantity
        /// </summary>
        /// <param name="context">Database context</param>
        /// <param name="kategoriBarang">Category of the item</param>
        /// <param name="quantity">Number of serial numbers needed</param>
        /// <returns>List of unique serial numbers</returns>
        public static List<string> GenerateSerialNumbers(DbRndAssetTakingContext context, string kategoriBarang, int quantity)
        {
            var serialNumbers = new List<string>();
            
            for (int i = 0; i < quantity; i++)
            {
                string serialNumber = GenerateSerialNumber(context, kategoriBarang);
                serialNumbers.Add(serialNumber);
                
                // Add temporary record to prevent duplicates in the same batch
                // This will be replaced with actual records later
                var tempSerial = new TblRAssetSerial
                {
                    SerialNumber = serialNumber,
                    AssetId = 0, // Temporary, will be updated later
                    Status = 0,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "system"
                };
                context.TblRAssetSerials.Add(tempSerial);
            }
            
            return serialNumbers;
        }

        /// <summary>
        /// Get category code based on category name
        /// </summary>
        /// <param name="kategoriBarang">Category name</param>
        /// <returns>Category code</returns>
        private static string GetCategoryCode(string kategoriBarang)
        {
            return kategoriBarang?.ToUpper() switch
            {
                "RND" => "RND",
                "SPAREPART" => "SPR",
                "TOOLS" => "TLS",
                "EQUIPMENT" => "EQP",
                "FURNITURE" => "FUR",
                "ELECTRONICS" => "ELC",
                "AUTOMOTIVE" => "AUT",
                "SAFETY" => "SFT",
                "CONSUMABLE" => "CON",
                _ => "GEN" // General category for unknown types
            };
        }

        /// <summary>
        /// Validate if a serial number follows the correct format
        /// </summary>
        /// <param name="serialNumber">Serial number to validate</param>
        /// <returns>True if valid format</returns>
        public static bool IsValidSerialFormat(string serialNumber)
        {
            if (string.IsNullOrWhiteSpace(serialNumber))
                return false;

            // Check if it matches the pattern: 3+ letters followed by 5 digits
            var validPrefixes = new[] { "RND", "SPR", "TLS", "EQP", "FUR", "ELC", "AUT", "SFT", "CON", "GEN" };
            
            foreach (var prefix in validPrefixes)
            {
                if (serialNumber.StartsWith(prefix) && serialNumber.Length == prefix.Length + 5)
                {
                    string numericPart = serialNumber.Substring(prefix.Length);
                    return int.TryParse(numericPart, out _);
                }
            }

            return false;
        }

        /// <summary>
        /// Get category from serial number
        /// </summary>
        /// <param name="serialNumber">Serial number</param>
        /// <returns>Category code</returns>
        public static string GetCategoryFromSerial(string serialNumber)
        {
            if (string.IsNullOrWhiteSpace(serialNumber))
                return "GEN";

            var validPrefixes = new[] { "RND", "SPR", "TLS", "EQP", "FUR", "ELC", "AUT", "SFT", "CON", "GEN" };
            
            foreach (var prefix in validPrefixes)
            {
                if (serialNumber.StartsWith(prefix))
                {
                    return prefix;
                }
            }

            return "GEN";
        }
    }
}