using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace PokerIA
{
	internal static class Program
	{
		public static void Main(string[] args)
		{
			const int players = 4;
			const int rounds = 1;
			const int minBet = 50;

			List<string> playersNames = new List<string>(players);
			for (int i = 0; i < players; i++) playersNames.Add($"Player {(char) (i + 65)}");

			PokerGame game = new PokerGame(playersNames, minBet);
			for (int i = 0; i < rounds; i++) game.PlayRound();
		}
	}
}