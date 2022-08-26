using System;

namespace PokerIA
{
    public class Card
    {
        // Enums
        public enum CardColor
        {
            Black,
            Red
        }

        public enum CardFamily
        {
            Spades,
            Clubs,
            Diamonds,
            Hearts
        }

        private enum CardNames
        {
            A = 1,
            J = 11,
            Q = 12,
            K = 13
        }

        // Constants
        private const ConsoleColor Black = ConsoleColor.White;
        private const ConsoleColor Red = ConsoleColor.DarkRed;
        private const ConsoleColor Plain = ConsoleColor.White;

        // Attributes
        private int value;
        private CardColor color;
        private CardFamily family;

        // Getters
        public int Value => value;
        public CardColor Color => color;
        public CardFamily Family => family;

        // Initialize a new card
        public Card(int value, CardColor color, CardFamily family)
        {
            this.value = value;
            this.color = color;
            this.family = family;
        }

        // Pretty-print
        public override string ToString()
        {
            string fam;
            var val = this.value > 10 || this.value == 1 ? ((CardNames) this.value).ToString() : this.value.ToString();
            switch (this.family)
            {
                case CardFamily.Spades:
                    fam = "♠";
                    break;
                case CardFamily.Clubs:
                    fam = "♣";
                    break;
                case CardFamily.Diamonds:
                    fam = "♦";
                    break;
                case CardFamily.Hearts: // CardFamily.HEARTS:
                    fam = "♥";
                    break;
                default:
                    fam = "X";
                    break;
            }

            Console.ForegroundColor = this.color == CardColor.Black ? Black : Red;
            Console.Write(""); Console.Write($"{val + fam}");
            Console.ForegroundColor = Plain;
            return "";
        }
    }
}