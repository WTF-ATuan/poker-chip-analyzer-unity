using UnityEngine;

namespace PokerChipAnalyzer.Domain
{
    /// <summary>
    /// Blind settings for poker game
    /// </summary>
    [System.Serializable]
    public class BlindSettings
    {
        [Header("Blind Values")]
        public int smallBlind = 1;
        public int bigBlind = 2;
        
        [Header("Game Settings")]
        public string gameType = "No Limit Hold'em";
        public int maxPlayers = 9;
        
        [Header("Display Settings")]
        public bool showInBB = true; // Show values in big blinds
        public bool showInDollars = true; // Show values in dollars
        
        /// <summary>
        /// Convert dollar amount to big blinds
        /// </summary>
        /// <param name="dollars">Dollar amount</param>
        /// <returns>Big blind count</returns>
        public float DollarsToBB(float dollars)
        {
            return bigBlind > 0 ? dollars / bigBlind : 0f;
        }
        
        /// <summary>
        /// Convert big blinds to dollar amount
        /// </summary>
        /// <param name="bb">Big blind count</param>
        /// <returns>Dollar amount</returns>
        public float BBToDollars(float bb)
        {
            return bb * bigBlind;
        }
        
        /// <summary>
        /// Get formatted string for value
        /// </summary>
        /// <param name="dollars">Dollar amount</param>
        /// <returns>Formatted string</returns>
        public string FormatValue(float dollars)
        {
            string result = "";
            
            if (showInDollars)
            {
                result += $"${dollars:F0}";
            }
            
            if (showInBB)
            {
                float bb = DollarsToBB(dollars);
                if (showInDollars)
                    result += $" ({bb:F1} BB)";
                else
                    result = $"{bb:F1} BB";
            }
            
            return result;
        }
        
        /// <summary>
        /// Create default blind settings
        /// </summary>
        /// <returns>Default settings</returns>
        public static BlindSettings CreateDefault()
        {
            return new BlindSettings
            {
                smallBlind = 1,
                bigBlind = 2,
                gameType = "No Limit Hold'em",
                maxPlayers = 9,
                showInBB = true,
                showInDollars = true
            };
        }
    }
}
