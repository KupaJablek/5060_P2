using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

using UnoLibrary;
using System.ServiceModel;  // WCF types

using System.Threading;
using System.Runtime.InteropServices;   // Need this for DllImport()
using System.ServiceModel.PeerResolvers;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace UNO_Client {
    class Program {
        

        private class CBObject : ICallback {
            public void Update(int nextPlayer, bool gameStatus,
                Card top, Colour cColour,
                List<Card> h) {

                activeClientID = nextPlayer;
                gameOver = gameStatus;
                hand = h;
                topOfDiscard = top;
                currentColour = cColour;

                if (clientID == activeClientID) {
                    // Release this client's main thread to let this user "count"
                    Console.Write("It's your turn. Press enter to count.");
                    waitHandle.Set();
                } else {
                    Console.WriteLine("not your turn");
                }
            }
            public void WaitingRoomPlayers(int numPlayers) {
                Console.Clear();
                Console.WriteLine($"Welcome to UNO Player {clientID + 1}\n\nWaiting for all players");
                Console.WriteLine("Number of players in waiting room: {0}", numPlayers);
                
                Console.WriteLine("Press 's' to start game or any other key to leave the waiting room.");
            }

            private static int clientID, activeClientID = 0;
            private static bool gameOver = false;
            private static bool gameStarted = false;

            private static EventWaitHandle waitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
            private static CBObject cbObj = new CBObject();


            private static IGameManager gm = null;

            private static List<Card> hand = null;
            private static Card topOfDiscard = null;
            private static Colour currentColour = new Colour();

            private static int cardIndex = 0;
            private static Colour nextColour = new Colour();

            

            public static void Main() {
                
                if (connect()) {
                    

                    

                    do {
                        //waitHandle.WaitOne();

                        if (gameOver) {
                            Console.ReadKey();
                            // end of game stuff here
                        } else {
                            // Player loop here
                            printDeckDetails();
                            
                            if (printHand()) {
                                PlayCard();
                                gm.EndTurn(cardIndex, nextColour);
                            } else { 
                                Console.WriteLine("No playable cards in hand.");
                                Console.WriteLine("press enter to draw another.");
                                Console.ReadLine();
                                gm.DrawCard();

                                // call server and draw
                                if (printHand()) {
                                    PlayCard();
                                    gm.EndTurn(cardIndex, nextColour);
                                } else {
                                    Console.WriteLine("Out of luck this time!");
                                    gm.EndTurn(-1, nextColour);
                                }
                            }
                            waitHandle.Reset();
                        }
                    } while (!gameOver);

                } else {
                    Console.WriteLine("ERROR: Unable to connect to the service!");
                }
            }

            private static void printDeckDetails() {
                Card c = topOfDiscard;
                if (c.value == Value.wild || c.value == Value.wild4) {
                    Console.WriteLine($"Top card of deck:\n\t{c.colour} {currentColour}");
                } else {
                    Console.WriteLine($"Top card of deck:\n\t{c.ToString()}");
                }
            }

            public static void PlayCard() {
                bool cardPlayed = false;

                Console.WriteLine("Enter the number of the card you'd like to play");
                Console.WriteLine("OR type \"c\" to see your hand again:");
                do {
                    bool valid = false;
                    int userChoice = 0;
                    do {
                        Console.WriteLine("Enter choice: ");
                        string input = Console.ReadLine();
                        if (input == "c") {
                            printHand();
                        } else if (int.TryParse(input, out userChoice) == true) {
                            if (userChoice > 0 && userChoice <= hand.Count) {
                                valid = true;
                            } else {
                                Console.WriteLine("invalid choice, try again");
                            }
                        } else {
                            Console.WriteLine("invalid choice, try again");
                        }
                    } while (!valid);

                    // valid card choice
                    // see if card can be played
                    Card c = hand[userChoice - 1];

                    if (c.colour == topOfDiscard.colour || c.value == topOfDiscard.value) {
                        // Remove card from player's hand and add to discard pile
                        // play
                        cardIndex = userChoice - 1;
                        nextColour = c.colour;
                        cardPlayed = true;
                    } else if (c.colour == Colour.Wild) {
                        Console.WriteLine("Please select new active colour:");
                        // colour selection
                        nextColour = chooseColour();
                        cardPlayed = true;
                        cardIndex = userChoice - 1;
                    } else {
                        Console.WriteLine("Invalid choice, please try again!");
                    }
                } while (!cardPlayed);

                // can end turn now
                // call end turn and pass current card and colour to it
                Console.WriteLine($"Played card: {hand[cardIndex]}");

                //checking if player is out of cards and is the winner
                if (hand.Count == 0) {
                    Console.WriteLine($"Player {clientID} Wins");
                }
            }

            public static Colour chooseColour() { 
                Colour colour = new Colour();

                bool valid = false;
                Console.WriteLine("1. Red");
                Console.WriteLine("2. Green");
                Console.WriteLine("3. Blue");
                Console.WriteLine("4. Yellow");
                Console.WriteLine("Enter the number of the colour youd like:");

                int choice = 0;
                while (!valid) {
                    string input = Console.ReadLine();

                    if (int.TryParse(input, out choice)) {
                        if (choice > 0 || choice < 5) {
                            colour = Enum.GetValues(typeof(Colour)).Cast<Colour>().ToList().ElementAt(choice - 1);
                            valid = true;
                        } else {
                            Console.WriteLine("Invalid choice, please try again!");
                            Console.WriteLine("Enter choice: ");
                        }
                    } else {
                        Console.WriteLine("Invalid choice, please try again!");
                        Console.WriteLine("Enter choice: ");
                    }
                }
                return colour;
            }

            private static bool printHand() {
                Console.WriteLine("Cards in hand");
                bool playableCard = false;

                int i = 1;
                foreach (Card c in hand) {
                    if (topOfDiscard.colour == Colour.Wild) {
                        if (c.colour == currentColour || c.colour == Colour.Wild) {
                            playableCard = true;
                        }
                        Console.WriteLine(i + ". " + c);
                    } else {
                        if (c.colour == topOfDiscard.colour || c.value == topOfDiscard.value || c.colour == Colour.Wild) {
                            
                            playableCard = true;
                        }
                        Console.WriteLine(i + ". " + c);
                    }
                    i++;
                }
                return playableCard;
            }

            private static bool connect() {
                
                try {


                    //InstanceContext context = new InstanceContext(cbObj);
                    //gm = new GameManager(context);
                    
                    DuplexChannelFactory<IGameManager> channel = new DuplexChannelFactory<IGameManager>(cbObj, "UNO_Client");
                    gm = channel.CreateChannel();
                    gm.RegisterClient();

                    clientID = gm.JoinGame();


                    Console.WriteLine($"Welcome to UNO Player {clientID + 1}\n\nWaiting for all players");
                    
                    //waiting room before game
                    //Console.WriteLine("Number of players in waiting room: {0}", gm.JoinWaitingRoom());
                    gm.JoinWaitingRoom();
                    //char temp = Console.ReadKey().KeyChar;
                    if (Console.ReadKey().KeyChar != 's') {
                        gm.LeaveWaitingRoom();
                        gm.UnregisterClient();
                        
                    }
                    //throw new Exception("Player has left");


                    //Console.WriteLine("Welcome to uno");


                    //start game now
                    gameStarted = true;

                    gm.StartGame();
                    
                    gm.populateDeck();
                    gm.shuffleDeck();
                    gm.DealCards(5, gm.callbackCount());
                    gm.setFirstCard();
                    gm.UpdateAllClients();

                    // start game loop somewhere here
                    //if (clientID == 1) {
                    //    cbObj.Update(clientID, false, new Card(Colour.Red, Value.Nine), Colour.Red, new List<Card> { 
                    //        new Card(Colour.Green, Value.Nine),
                    //        new Card(Colour.Blue, Value.Nine),
                    //        new Card(Colour.Yellow, Value.Nine),
                    //    });
                    //}

                    return true;
                } catch (Exception ex) {
                    Console.WriteLine(ex.Message);
                    return false;
                }
            }
        }
    }
}
