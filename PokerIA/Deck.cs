using System;
using System.Collections.Generic;
using System.Linq;

namespace PokerIA
{
	public class Deck
	{
		// Attributes
		private const int Regular = 52;
		private Stack<Card> _cards;

		// Getters & Setters
		public Stack<Card> Cards => _cards;

		public Deck()
		{
			// Build the deck of 52 cards
			this._cards = new Stack<Card>(Regular);
			BuildDeck(Regular);
		}

		// Initialize a deck with all 52 cards
		public Deck(int size)
		{
			// Check for any incorrect given size
			if (size % 4 != 0) throw new ArgumentException("Given deck size must be a multiple of 4.");

			// Build the 'this' instance
			this._cards = new Stack<Card>(size);

			// Build a deck of non-regular size
			BuildDeck(size);
		}

		/**
		 * <summary> Populates the deck with cards and shuffles it. </summary>
		 * <param name="size"> The given size of the deck. </param>
		 */
		private void BuildDeck(int size)
		{
			// Iterate through all the possible cards
			for (int i = 0; i < 4; i++) // 4 card families
			{
				for (int j = 0; j < size / 4; j++) // Split the deck size in four equal-length families
				{
					// Add a new card with corresponding value, color & family
					this._cards.Push(new Card(j + 1, (Card.CardColor) (i / 2), (Card.CardFamily) i));
				}
			}

			// Initially shuffle the cards
			ShuffleDeck();
		}

		/**
		 * <summary> Modifies in-place the given deck of cards. </summary>
		 */
		private void ShuffleDeck()
		{
			// Randomly interchange the position of each card in the deck
			Random rnd = new Random();
			this._cards = new Stack<Card>(this._cards.OrderBy(_ => rnd.Next()));
		}

		// Pretty-print
		public override string ToString()
		{
			Console.Write("Deck Order: ");
			this._cards.ToList().ForEach(card => Console.Write(card + " "));
			Console.WriteLine($"\nDeck Size: {this._cards.Count}");
			return "";
		}
	}
}