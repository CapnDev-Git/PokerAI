using System;
using System.Collections.Generic;

namespace PokerIA
{
	public class GameTable
	{
		// Attributes
		private Deck deck;
		private List<Card> cards;
		private Stack<Card> trash;
		private int pot;
		private int callBet;
		private int currPot;
		private List<int> bets;
		
		// Getters & Setters
		public Deck Deck
		{
			get => deck;
			set => deck = value;
		}
		public List<Card> Cards
		{
			get => cards;
			set => cards = value;
		}
		public Stack<Card> Trash
		{
			get => trash;
			set => trash = value;
		}
		public int Pot
		{
			get => pot;
			set => pot = value;
		}
		public List<int> Bets
		{
			get => bets;
			set => bets = value;
		}
		public int CallBet
		{
			get => callBet;
			set => callBet = value;
		}
		public int CurrPot
		{
			get => currPot;
			set => currPot = value;
		}

		// Constructor
		public GameTable(int playersCount, int defaultCallBet)
		{
			this.deck = new Deck();
			this.cards = new List<Card>(5);
			this.trash = new Stack<Card>(playersCount * 2 + 3);
			this.bets = new List<int>(playersCount);
			for (int i = 0; i < playersCount; i++) this.bets.Add(0);
			this.pot = 0;
			this.currPot = 0;
			this.callBet = defaultCallBet;
		}
	}
}