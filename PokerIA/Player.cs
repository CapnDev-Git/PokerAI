using System;
using System.Collections.Generic;

namespace PokerIA
{
	public class Player
	{
		// Constants
		private const int HandSize = 2;
		private const ConsoleColor Plain = ConsoleColor.White;
		private const ConsoleColor MoneyEmpty = ConsoleColor.DarkGray;
		private const ConsoleColor MoneyLow = ConsoleColor.Red;
		private const ConsoleColor MoneyMedium = ConsoleColor.Yellow;
		private const ConsoleColor MoneyHigh = ConsoleColor.DarkGreen;
 
		// Attributes
		private readonly string name;
		private int _balance;
		private List<Card> _hand;
		private bool _folded;

		// Getters & Setters
		public string Name => name;
		public int Balance
		{
			get => _balance;
			set => _balance = value;
		}
		public bool Folded
		{
			get => _folded;
			set => _folded = value;
		}
		public List<Card> Hand
		{
			get => _hand;
			set => _hand = value;
		}

		// Constructor
		public Player(string name)
		{
			this.name = name;
			this._balance = 100;
			this._folded = false;
			this._hand = new List<Card>(HandSize);
		}

		// Pretty-print
		public override string ToString()
		{
			Console.ForegroundColor = this._balance > 0 ? Plain : MoneyEmpty;
			Console.Write($" {this.name}, [");
			Console.Write($"{this._hand[0]}, ");
			Console.Write($"{this._hand[1]}], ");
			switch (this._balance)
			{
				case > 0 and < 25:
					Console.ForegroundColor = MoneyLow;
					break;
				case >= 25 and < 75:
					Console.ForegroundColor = MoneyMedium;
					break;
				case >= 75:
					Console.ForegroundColor = MoneyHigh;
					break;
				default:
					Console.ForegroundColor = MoneyEmpty;
					break;
			}
			Console.Write($"{this._balance}$");
			Console.ForegroundColor = Plain;
			return "";
		}
	}
}