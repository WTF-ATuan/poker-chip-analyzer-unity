using UnityEngine;
using System.Collections.Generic;

namespace PokerChipAnalyzer.Domain
{
    /// <summary>
    /// Configuration for chip denominations and colors
    /// </summary>
    [System.Serializable]
    public class ChipDenominationConfig
    {
        [Header("Chip Settings")]
        public string chipSetName = "Default";
        public List<ChipDenomination> denominations = new List<ChipDenomination>();
        
        [Header("Default Values")]
        public float defaultChipThickness = 3.3f; // mm
        public int defaultStackSize = 20;
        
        /// <summary>
        /// Get denomination by color
        /// </summary>
        /// <param name="color">Chip color</param>
        /// <returns>Denomination or null if not found</returns>
        public ChipDenomination GetDenominationByColor(Color color)
        {
            foreach (var denom in denominations)
            {
                if (ColorDistance(denom.color, color) < 0.1f) // Tolerance for color matching
                {
                    return denom;
                }
            }
            return null;
        }
        
        /// <summary>
        /// Add or update denomination
        /// </summary>
        /// <param name="denomination">Denomination to add/update</param>
        public void SetDenomination(ChipDenomination denomination)
        {
            int index = denominations.FindIndex(d => d.colorName == denomination.colorName);
            if (index >= 0)
            {
                denominations[index] = denomination;
            }
            else
            {
                denominations.Add(denomination);
            }
        }
        
        /// <summary>
        /// Calculate color distance for matching
        /// </summary>
        /// <param name="color1">First color</param>
        /// <param name="color2">Second color</param>
        /// <returns>Distance value</returns>
        private float ColorDistance(Color color1, Color color2)
        {
            float r = color1.r - color2.r;
            float g = color1.g - color2.g;
            float b = color1.b - color2.b;
            return Mathf.Sqrt(r * r + g * g + b * b);
        }
        
        /// <summary>
        /// Create default chip set
        /// </summary>
        /// <returns>Default configuration</returns>
        public static ChipDenominationConfig CreateDefault()
        {
            var config = new ChipDenominationConfig();
            config.chipSetName = "Standard Casino";
            
            config.denominations.Add(new ChipDenomination
            {
                colorName = "White",
                color = Color.white,
                value = 1,
                displayName = "$1"
            });
            
            config.denominations.Add(new ChipDenomination
            {
                colorName = "Red",
                color = Color.red,
                value = 5,
                displayName = "$5"
            });
            
            config.denominations.Add(new ChipDenomination
            {
                colorName = "Green",
                color = Color.green,
                value = 25,
                displayName = "$25"
            });
            
            config.denominations.Add(new ChipDenomination
            {
                colorName = "Black",
                color = Color.black,
                value = 100,
                displayName = "$100"
            });
            
            config.denominations.Add(new ChipDenomination
            {
                colorName = "Purple",
                color = new Color(0.5f, 0f, 1f),
                value = 500,
                displayName = "$500"
            });
            
            return config;
        }
    }
    
    /// <summary>
    /// Individual chip denomination
    /// </summary>
    [System.Serializable]
    public class ChipDenomination
    {
        [Header("Chip Properties")]
        public string colorName;
        public Color color;
        public int value; // Dollar value
        public string displayName;
        
        [Header("Stack Properties")]
        public int typicalStackSize = 20;
        public float chipThickness = 3.3f; // mm
        
        public ChipDenomination()
        {
            colorName = "Unknown";
            color = Color.gray;
            value = 1;
            displayName = "$1";
            typicalStackSize = 20;
            chipThickness = 3.3f;
        }
    }
}
