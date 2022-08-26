using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;

namespace PokerIA
{
    public class PokerGame
    {
        // Constants
        private const int MinBlind = 10;
        private const ConsoleColor Plain = ConsoleColor.White;
        private const ConsoleColor RoundPrint = ConsoleColor.DarkBlue;
        private const ConsoleColor PlayersStatusesPrint = ConsoleColor.Red;
        private const ConsoleColor TableStatusPrint = ConsoleColor.Red;
        private const ConsoleColor RevealedCardsPrint = ConsoleColor.White;
        private const ConsoleColor RoleInfosPrint = ConsoleColor.DarkYellow;
        private const ConsoleColor BettingPrint = ConsoleColor.Red;
        private const ConsoleColor Warning = ConsoleColor.DarkRed;
        private const ConsoleColor CallBetPrint = ConsoleColor.DarkBlue;
        private const ConsoleColor RaiseBetPrint = ConsoleColor.DarkBlue;
        private const ConsoleColor BettingInfoPrint = ConsoleColor.White;
        private const ConsoleColor TextHighlight = ConsoleColor.Yellow;
        private const ConsoleColor PotPrint = ConsoleColor.DarkGreen;
        private const ConsoleColor CurrPotPrint = ConsoleColor.DarkGreen;

        // Attributes
        private int _round;
        private readonly List<Player> _players;
        private readonly GameTable _gameTable;
        private int _dealer;
        private Tuple<int, int> _blinds;

        // Getters
        public int Round => _round;
        public List<Player> Players => _players;
        public GameTable GameTable => _gameTable;
        public int Dealer => _dealer;
        public Tuple<int, int> Blinds => _blinds;

        // Constructor
        public PokerGame(List<string> names, int defaultCallBet)
        {
            // Check for any errors regarding the players' list
            if (names.Count == 1 || names.Count != names.Distinct().Count())
                throw new ArgumentException("Invalid list of players given.");

            // Initialize the game properties
            this._round = 1;
            this._players = new List<Player>(names.Count);
            foreach (var name in names) this._players.Add(new Player(name));
            this._gameTable = new GameTable(this._players.Count, defaultCallBet);
            this._dealer = 0;
            this._blinds = this._players.Count > 2 ? new Tuple<int, int>(1, 2) : new Tuple<int, int>(0, 0);
        }

        /**
		 * <summary> Resets all the game properties for a new round. </summary>
		 */
        private void RoundReset()
        {
            // Reset all stats
            this._round++;
            this._gameTable.Deck = new Deck();
            this._gameTable.Cards = new List<Card>(5);
            this._gameTable.Trash = new Stack<Card>(this._players.Count * 2 + 3);
            this._players.ForEach(player => player.Folded = false);

            // Set the next dealer
            int nextDealer = (this._dealer + 1) % this._players.Count;
            this._dealer = nextDealer;

            // Set the blinds indexes
            this._blinds = new Tuple<int, int>((nextDealer + 1) % this._players.Count,
                (nextDealer + 2) % this._players.Count);
        }

        /**
		 * <summary> Plays through an entire round, resetting certain game properties if needed. </summary>
		 */
        public void PlayRound() // ╠ ╣ ║ ╚ ╝ ╔ ╗ ╩ ╦ ═ ╬
        {
            // Reset everything for a new round
            if (this._round > 1) RoundReset();
            if (this._players.Count > 2) ApplyBlinds();

            // Hand out 2 cards to each player
            foreach (var player in this._players)
            {
                // Reset the player's hand if necessary & add 2 new cards
                if (player.Hand.Count == 2) player.Hand = new List<Card>(2);
                for (int i = 0; i < 2; i++) player.Hand.Add(this._gameTable.Deck.Cards.Pop());
            }

            // Play the round
            BettingRound(); // Pre-flop betting round // TODO fix all folding => determine winner part + might still have some bugs for multiple call + raise turns
            BurnReveal(3); // Burn a card & reveal the Flop
            BettingRound(); // Flop betting round
            BurnReveal(); // Burn a card & reveal the Turn
            BettingRound(); // Turn betting round
            BurnReveal(); // Burn a card & reveal the River
            BettingRound(); // Final/River betting round 

            // TODO: Betting round starting left to button (= next in list)
            // TODO: Determine winner
        }

        /**
         * <summary> Skips a card reveals a specific amount of cards onto the table. </summary>
         * <param name="amount"> The number of cards to be revealed. </param> 
         */
        private void BurnReveal(int amount = 1)
        {
            // Burn the top element of the deck of cards in the trash
            this._gameTable.Trash.Push(this._gameTable.Deck.Cards.Pop());

            // Reveal the top element of the deck of cards to the table
            for (int i = 0; i < amount; i++) this._gameTable.Cards.Add(this._gameTable.Deck.Cards.Pop());
        }

        /**
		 * <summary> Sets the bets of all players for the current betting round. </summary>
		 */
        private void ApplyBlinds()
        {
            // Withdraw the blinds from both players
            BetMoney(this._blinds.Item1, MinBlind / 2);
            BetMoney(this._blinds.Item2, MinBlind);

            // Set the current max bet to call 
            this._gameTable.CallBet = MinBlind;
            this._gameTable.CurrPot += MinBlind * 3 / 2;
        }

        /**
		 * <summary> Sets the bets of all players for the current betting round. </summary>
		 */
        private void BetMoney(int playerIndex, int amount)
        {
            this._players[playerIndex].Balance -= amount;
            this._gameTable.Bets[playerIndex] += amount;
        }

        /**
		 * <summary> Sets the bets of all players for the current betting round. </summary>
		 */
        private void BettingRound()
        {
            // Print the beginning of a new round
            PrintMainInfos();

            // Initialize the betting log
            string currBetLog = "";
            PrintBettingStatuses(currBetLog);

            // Loop through each player of the table
            int i = 0;
            while (i < this._players.Count)
            {
                // Initialize the bet
                int currBet = Int32.MaxValue;
                int indexPlayer = (i + this._blinds.Item2 + 1) % this._players.Count;
                Player currPlayer = this._players[indexPlayer];

                // Skip any player that has folded already
                if (currPlayer.Folded)
                {
                    WriteLineCol($"{currPlayer.Name} has folded already!", TextHighlight);
                    continue;
                }

                // Display the possible actions depending on the current bets & players' balances 
                WriteCol("Possible actions: ", BettingInfoPrint);
                WriteCol("fold", TextHighlight);
                WriteCol(", ", Plain);

                // Only allow to call if you haven't bet enough to reach the current maximum bet
                if (this._gameTable.Bets[indexPlayer] != this._gameTable.CallBet) WriteCol("call", TextHighlight);
                else WriteCol("check", TextHighlight);

                // Allow to raise at all times
                WriteCol(" or ", Plain);
                WriteCol("raise", TextHighlight);
                WriteLineCol(".", Plain);

                // Keep asking for a bet until it's valid (essentially affordable)
                while (currBet > currPlayer.Balance)
                {
                    // Display betting message 
                    WriteCol($"{currPlayer.Name}, enter an action or a betting amount: ", Plain);
                    string input = Console.ReadLine();

                    // Only allow the betting for the ones still playing
                    switch (input)
                    {
                        case "fold" or "Fold" or "f" or "fd":
                            // Fold the current player
                            Fold(currPlayer);
                            currBet = 0; // to exit the loop

                            // Log the folding
                            currBetLog +=
                                $"{currPlayer.Name} folded!\n";
                            break;
                        case "check" or "Check" or "0" or "ch":
                            // If there was no previous bet to call to, allow to check (bet = 0$)
                            if (this._gameTable.Bets[indexPlayer] - this._gameTable.CallBet == 0) currBet = 0;
                            else
                            {
                                // If a calling bet was established, inform the player & ask a new value / action 
                                WriteCol($"You must call to continue ", Warning);
                                WriteCol($"(", Plain);
                                WriteCol($"{this._gameTable.CallBet}$", TextHighlight);
                                WriteCol("). Otherwise ", Plain);
                                WriteCol("fold", TextHighlight);
                                WriteLineCol(".", Plain);
                                break;
                            }
                            
                            
                            
                            // TODO ; mettre les calls & pots dans la table
                            // seulement le log des bets dans la partie betting 
                            
                            
                            
                            

                            // Log the check
                            currBetLog += $"{currPlayer.Name} checked!\n";
                            break;
                        case "call" or "Call" or "ca":
                            // Only allow to call when there's a calling bet set
                            if (this._gameTable.CallBet - this._gameTable.Bets[indexPlayer] == 0 &&
                                this._gameTable.CallBet != 0)
                            {
                                // Inform the player that he has already bet the calling bet
                                WriteCol("You already bet enough to reach the calling bet. ", Warning);
                                WriteCol("You can ", Plain);
                                WriteCol("fold", TextHighlight);
                                WriteCol(", ", Plain);
                                WriteCol("check ", TextHighlight);
                                WriteCol("or ", Plain);
                                WriteCol("raise", TextHighlight);
                                WriteLineCol(".", Plain);
                                break;
                            }
                            else if (this._gameTable.CallBet == 0)
                            {
                                // Inform the player that there isn't any calling bet
                                WriteCol("There is nothing to call. ", Warning);
                                WriteCol("You can ", Plain);
                                WriteCol("fold", TextHighlight);
                                WriteCol(", ", Plain);
                                WriteCol("check ", TextHighlight);
                                WriteCol("or ", Plain);
                                WriteCol("bet", TextHighlight);
                                WriteLineCol(".", Plain);
                                break;
                            }
                            else currBet = this._gameTable.CallBet - this._gameTable.Bets[indexPlayer];

                            // Log the call
                            currBetLog +=
                                $"{currPlayer.Name} called the {this._gameTable.CallBet}$ ({this._gameTable.Bets[indexPlayer] - this._gameTable.CallBet}$)!\n";
                            break;
                        case "":
                            // CLean the console & ask again
                            currBet = Int32.MaxValue;
                            PrintMainInfos();
                            PrintBettingStatuses(currBetLog);
                            break;
                        default:
                            // TODO: Create a parser / lexer to fix bad input ; regex ?

                            // Convert the given amount
                            currBet = Convert.ToInt32(input);

                            // Only allow to bet what's affordable for the player betting
                            if (currBet > currPlayer.Balance)
                            {
                                WriteCol("You can't bet that much money! You only have ", Plain);
                                WriteCol($"{currPlayer.Balance}$", Warning);
                                WriteLineCol("!", Plain);
                                break;
                            }

                            // Only allow to raise from a certain amount
                            if (currBet > this._gameTable.CallBet && currBet < this._gameTable.CallBet * 2)
                            {
                                WriteCol("You can only raise from ", Plain);
                                WriteCol($"{this._gameTable.CallBet * 2}$ ", Warning);
                                WriteLineCol("minimum!", Plain);
                                currBet = Int32.MaxValue;
                                break;
                            }

                            // Display an info message depending on the bet amount
                            if (currBet == currPlayer.Balance && currPlayer.Balance != 0)
                            {
                                // TODO: special case for all-ins for calling amount / setting calling bet
                                this._gameTable.CallBet = currBet;

                                // Log the all-in
                                currBetLog += $"{currPlayer.Name} went all-in ({currPlayer.Balance}$).\n";
                            }
                            else if (currBet >= this._gameTable.CallBet * 2)
                            {
                                // Log the raise
                                this._gameTable.CallBet = currBet;
                                currBetLog += $"{currPlayer.Name} raised to {currBet}$!\n";
                                currBet -= this._gameTable.Bets[indexPlayer];
                            }
                            else
                            {
                                // Log the bet
                                currBetLog += currBet != this._gameTable.CallBet
                                    ? $"{currPlayer.Name} bet {currBet}$!\n"
                                    : $"{currPlayer.Name} called ({this._gameTable.CallBet}$)!\n";
                            }

                            break;
                    }
                }

                // Increase the betting round's pot & withdraw the money from the player's balance
                this._gameTable.CurrPot += currBet;
                this._gameTable.Bets[indexPlayer] += currBet;
                currPlayer.Balance -= currBet;

                // Clear & print the infos
                PrintMainInfos();
                PrintBettingStatuses(currBetLog);

                // Check for no more raise to follow, otherwise keep betting
                if (i == this._players.Count - 1)
                    i = this._gameTable.Bets.TrueForAll(bet => bet == this._gameTable.CallBet)
                        ? this._players.Count
                        : 0;
                else i++;
            }

            // Increase the global pot & reset other indicators
            this._gameTable.Pot += this._gameTable.CurrPot;
            this._gameTable.Bets = new List<int>(this._players.Count);
            for (int j = 0; j < this._players.Count; j++) this._gameTable.Bets.Add(0);
            this._gameTable.CurrPot = 0;
            this._gameTable.CallBet = 0;

            // Clear the console & print the main infos
            PrintMainInfos();
        }

        /**
		 * <summary> Allows a player to fold and not play the round. </summary>
		 */
        private void Fold(Player player)
        {
            // player.Hand.ForEach(card => this._gameTable.Trash.Push(card));
            // player.Hand = new List<Card>(2);
            player.Folded = true;
        }

        // Pretty-print & tools
        public override string ToString()
        {
            // Print round number
            WriteLineCol($"Round n°{this._round}:", Plain);

            // Print each player's stats
            this._players.ForEach(Console.WriteLine);

            // Print the table's content
            WriteLineCol($"Dealer: [{this._players[this._dealer].Name}]", Plain);
            if (this._players.Count > 2)
                WriteLineCol(
                    $"Blinds: [{this._players[this._blinds.Item1].Name}, {this._players[this._blinds.Item2].Name}]",
                    Plain);

            // Print the table's content
            WriteLineCol("═══════════════════════════", Plain);
            WriteCol($"Table: ", Plain);
            this._gameTable.Cards.ForEach(card => Console.Write(card + " "));

            // Print the trash's content
            WriteLineCol("\n═══════════════════════════", Plain);
            WriteCol($"Trash: ", Plain);
            foreach (var card in this._gameTable.Trash) WriteCol(card + " ", Plain);

            // Print deck
            WriteLineCol("\n═══════════════════════════", Plain);
            WriteCol(this._gameTable.Deck.ToString(), Plain);
            return "═══════════════════════════\n═══════════════════════════\n";
        }

        private void WriteLineCol(string s, ConsoleColor c)
        {
            Console.ForegroundColor = c;
            Console.WriteLine(s);
        }

        private void WriteCol(string s, ConsoleColor c)
        {
            Console.ForegroundColor = c;
            Console.Write(s);
        }

        private void PrintMainInfos()
        {
            // Print round number
            Console.Clear();
            WriteLineCol($"══════ ROUND {this._round} ══════", RoundPrint);

            // Print each players' statuses
            PrintPlayerStatuses();

            // Print the table status
            PrintTableStatus();
        }

        private void PrintPlayerStatuses()
        {
            // Print each player's stats
            WriteLineCol("╔═══ PLAYERS' STATUSES", PlayersStatusesPrint);
            for (int i = 0; i < this._players.Count; i++)
            {
                // Deal with each bullet points 
                if (i == this._players.Count - 1) WriteCol("╚═", PlayersStatusesPrint);
                else WriteCol("╠═", PlayersStatusesPrint);
                Console.Write(this._players[i]);

                // Print in-game roles
                if (i == this._dealer) WriteLineCol(" [DEALER]", RoleInfosPrint);
                if (this._players.Count > 2)
                {
                    if (i == this._blinds.Item1) WriteLineCol($" [SMALL BLIND -{MinBlind / 2}$]", RoleInfosPrint);
                    if (i == this._blinds.Item2) WriteLineCol($" [BIG BLIND -{MinBlind}$]", RoleInfosPrint);
                }
            }
        }

        private void PrintTableStatus()
        {
            // Print the title
            WriteLineCol("\n\n╔═════ TABLE STATUS ══════", TableStatusPrint);

            // Print the currently revealed cards
            WriteCol("╚═ ", TableStatusPrint);
            WriteCol("Revealed Cards: ", RevealedCardsPrint);
            for (int i = 0; i < this._gameTable.Cards.Capacity; i++)
            {
                if (i < this._gameTable.Cards.Count)
                {
                    WriteCol("[", Plain);
                    Console.Write(this._gameTable.Cards[i]);
                    WriteCol("] ", Plain);
                }
                else WriteCol("[] ", Plain);
            }
        }

        private void PrintBettingStatuses(string bettingLog)
        {
            // Print betting round announcer
            WriteLineCol("\n\n══════ BETTING ROUND ══════", BettingPrint);
            WriteCol($"{this._gameTable.CallBet}$ ", CallBetPrint);
            WriteCol("to call, raise from ", Plain);
            WriteCol($"{this._gameTable.CallBet * 2}$ ", RaiseBetPrint);
            WriteLineCol("minimum.", Plain);
            WriteCol("Current betting round pot: ", Plain);
            WriteLineCol($"{this._gameTable.CurrPot}$", CurrPotPrint);
            // WriteCol("Current bets : [", Plain);
            // for (int i = 0; i < this._gameTable.Bets.Count - 1; i++) WriteCol(this._gameTable.Bets[i] + ", ", Plain);
            // WriteLineCol(this._gameTable.Bets[this._gameTable.Bets.Count - 1].ToString() + "]", Plain);
            WriteLineCol("═══════════════════════════", Plain);

            WriteCol(bettingLog, Plain);
            if (bettingLog.Length != 0) WriteLineCol("═══════════════════════════", Plain);
        }
    }
}