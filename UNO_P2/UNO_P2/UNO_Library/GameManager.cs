using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.Runtime.InteropServices;

namespace UnoLibrary {

    public interface ICallback {
        [OperationContract(IsOneWay = true)]
        void Update(int nextPlayer, bool gameOver, 
            Card topOfDiscard, Colour topColour,
            List<Card> hand);
        [OperationContract(IsOneWay = true)]
        void WaitingRoomPlayers(int numPlayers);
    }

    [ServiceContract(CallbackContract = typeof(ICallback))]
    public interface IGameManager {
        [OperationContract]
        void UpdateAllClients();
        [OperationContract]
        int callbackCount();
        [OperationContract]
        int JoinWaitingRoom();
        [OperationContract]
        int LeaveWaitingRoom();
        [OperationContract]
        bool StartGame();
        [OperationContract]
        void RegisterClient();
        [OperationContract]
        void UnregisterClient();
        [OperationContract]
        int JoinGame();

        [OperationContract]
        bool PlayCard(int cardIndex);

        [OperationContract]
        void DrawCard();

        [OperationContract(IsOneWay = true)]
        void EndTurn(int cardIndex, Colour nextColour);

        [OperationContract]
        void populateDeck();
        [OperationContract]
        void shuffleDeck();
        [OperationContract]
        void setFirstCard();
        [OperationContract]
        void DealCards(int numCards, int numPlayers);

    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class GameManager : IGameManager {
        private int numberPlayers = 0;
        public int JoinWaitingRoom() {
            numberPlayers++;
            UpdateWaitingClients();
            return numberPlayers;
        }
        public int LeaveWaitingRoom() {
            numberPlayers--;
            UpdateWaitingClients();
            Console.WriteLine("Goodbye");
            
            return numberPlayers;
        }
        public bool StartGame() {
            if (numberPlayers > 1) {
                return true;
            }
            return false;
        }
        public void RegisterClient() {
            ICallback callback = OperationContext.Current.GetCallbackChannel<ICallback>();
            callbacks.Add(callbacks.Count, callback);
        }
        public void UnregisterClient() {
            ICallback callback = OperationContext.Current.GetCallbackChannel<ICallback>();
            if (callbacks.ContainsValue(callback)) {
                for (int i = 0; i < callbacks.Count; i++) {
                    if (callbacks[i] == callback) {
                        callbacks.Remove(i);
                        Dictionary<int, ICallback> tempCBs = new Dictionary<int, ICallback>();
                        for (int j = 0; j < callbacks.Count; j++) {
                            if (j >= i) {
                                tempCBs.Add(j, callbacks.ElementAt(j).Value);
                            }
                            else {
                                tempCBs.Add(j, callbacks[j]);
                            }
                        }
                        callbacks.Clear();
                        foreach (KeyValuePair<int, ICallback> pair in tempCBs) {
                            callbacks.Add(pair.Key, pair.Value);
                        }
                        break;
                    }
                }
            }
            UpdateWaitingClients();
            LeaveGame();
        }
        // game variables
        private List<Card> deck;
        private List<Card> discard;
        private int cardsIndex;
        private Colour currentColour; // will represent colour of top of discard

        private Dictionary<int, List<Card>> players;

        // server variables
        private int playerIndex = 0; // to track index of player in player list
        private int nextPlayerID = 1; // id of player

        private bool gameStarted = false;

        private readonly Dictionary<int, ICallback> callbacks = null;
        public int callbackCount() {
            return callbacks.Count;
        }


    public GameManager() {
            populateDeck();
            shuffleDeck();


            discard = new List<Card>();
            //setFirstCard(); // only needs to happen at start of new game

            // server variables
            playerIndex = 0;
            nextPlayerID = 1;
            callbacks = new Dictionary<int, ICallback>();
        }

        public void populateDeck() {
            this.deck = new List<Card>();
            cardsIndex = 0; // reset cards index

            // add wild cards to deck
            for (int i = 0; i < 4; i++) {
                deck.Add(new Card(Colour.Wild, Value.wild4));
                deck.Add(new Card(Colour.Wild, Value.wild));
            }

            // add one zero card for each colour
            deck.Add(new Card(Colour.Blue, Value.Zero));
            deck.Add(new Card(Colour.Green, Value.Zero));
            deck.Add(new Card(Colour.Red, Value.Zero));
            deck.Add(new Card(Colour.Yellow, Value.Zero));

            // add two of each card for all colours. No wild
            foreach (Colour currentColour in Enum.GetValues(typeof(Colour))) {
                if (currentColour != Colour.Wild) {
                    foreach (Value val in Enum.GetValues(typeof(Value))) {
                        switch (val) {
                            case Value.Zero:
                            case Value.wild:
                            case Value.wild4:
                                break;
                            default:
                                for (int i = 0; i < 2; i++) {
                                    deck.Add(new Card(currentColour, val));
                                }
                                break;
                        }
                    }
                }
            }
        }

        public void shuffleDeck() {
            Random rng = new Random();
            deck = deck.OrderBy(card => rng.Next()).ToList();

            cardsIndex = 0;
        }

        public void setFirstCard() {
            // at the start of uno, the top card of the deck is turned over.
            // it has to be a numbered colour card. no wild or special cards
            bool firstCard = false;
            int firstCardIndex = cardsIndex;

            while (!firstCard) {
                Card c = deck[cardsIndex];
                if (c.colour == Colour.Wild ||
                    c.value == Value.Skip ||
                    c.value == Value.plus2 ||
                    c.value == Value.Reverse) 
                {
                    // swap this card with next one on stack
                    firstCardIndex++;

                    // swap current card with next option in deck
                    Card swap = deck[cardsIndex];
                    deck[cardsIndex] = deck[firstCardIndex];
                    deck[firstCardIndex] = swap;

                } else {
                    // found numbered colour card
                    firstCard = true;
                    discard.Add(c);
                    cardsIndex++;
                    currentColour = c.colour;
                }
            }
        }

        public Card draw() {

            Card card = null;

            if (cardsIndex != deck.Count) {
                return deck[cardsIndex++];
            } else {
                // Basically a reshuffle of discard to make it the new draw pile

                // preserve hands, keep top card of discard, and set current deck to remainder of discard.
                Card top = discard.ElementAt(discard.Count - 1);

                // set deck to discard minus last played card
                deck = discard;
                deck.RemoveAt(deck.Count - 1);
                this.shuffleDeck();

                // clear discard, leave top card
                discard.Clear();
                discard.Add(top);

                // call draw method again
                this.draw();
            }
            return card;
        }

        public void DealCards(int numCards, int numPlayers) {
            // Create a new dictionary of player hands
            players = new Dictionary<int, List<Card>>();

            // Deal cards to each player
            for (int i = 0; i < numPlayers; i++) {
                List<Card> hand = new List<Card>();

                for (int j = 0; j < numCards; j++) {
                    hand.Add(draw());
                }

                players.Add(i, hand);
            }
        }

        public bool PlayCard(int cardIndex) {
            List<Card> playerHand = players[playerIndex];
            Card card = playerHand[cardIndex];

            // Check if card is playable
            if (card.colour == currentColour ||
                card.value == Value.wild ||
                card.value == Value.wild4) {
                // Remove card from player's hand and add to discard pile
                playerHand.RemoveAt(cardIndex);
                discard.Add(card);

                // Update current colour
                if (card.value != Value.wild && card.value != Value.wild4) {
                    currentColour = card.colour;
                }
                return true;
            }
            return false;
        }

        public List<Card> GetPlayerHand(int playerId) {
            return players[playerId];
        }

        public Card TopOfDiscard() {
            return discard.Last();
        }

        // SERVER METHODS

        public void DrawCard() {
            players[playerIndex].Add(draw());
        }

        public int JoinGame() {
            // Identify which client is calling this method
            ICallback cb = OperationContext.Current.GetCallbackChannel<ICallback>();
            int i = 0;
            if (callbacks.ContainsValue(cb)) {
                // This client is already registered, so just return the id of registered client
                i = callbacks.Values.ToList().IndexOf(cb);
                return callbacks.Keys.ElementAt(i);
            }

            callbacks.Add(nextPlayerID, cb);

            /*
                not meant to be here but testing out the gameplay loop    
            */

            //gameStarted = true;
            //populateDeck();
            //shuffleDeck();
            //DealCards(5, callbacks.Count());
            //setFirstCard();

            return nextPlayerID++;
        }

        public void LeaveGame() {
            ICallback cb = OperationContext.Current.GetCallbackChannel<ICallback>();
            if (callbacks.ContainsValue(cb)) {
                // Identify which client is currently calling this method
                // - Get the index of the client within the callbacks collection
                int i = callbacks.Values.ToList().IndexOf(cb);
                // - Get the unique id of the client as stored in the collection
                int id = callbacks.ElementAt(i).Key;

                // Remove this client from receiving callbacks from the service
                callbacks.Remove(id);

                // Make sure the counting sequence isn't disrupted by removing this client
                if (i == playerIndex) {
                    // Need to signal the next client to count instead 
                    UpdateAllClients();
                }

                else if (playerIndex > i)
                    // This prevents a player from being "skipped over" in the turn-taking
                    // of this "game"
                    playerIndex--;
            }
        }

        public void UpdateAllClients() {
            foreach (ICallback cb in callbacks.Values) {

                Card tod = TopOfDiscard();
                Colour colour = new Colour();
                if (tod.colour == Colour.Wild) {
                    colour = currentColour;
                } else {
                    colour = tod.colour;
                }

                // top of discard, player hand, 
                cb.Update(callbacks.Keys.ElementAt(playerIndex), false,
                    tod, colour, GetPlayerHand(playerIndex));
            }
        }
        public void UpdateWaitingClients() {
            foreach (ICallback cb in callbacks.Values) {
                cb.WaitingRoomPlayers(numberPlayers);
            }
        }

        public string JoinedMessage() {

            return $"\tPlayer {callbacks.Count()} has joined the lobby...";
        }

        public string WelcomeMessage() {
            string welcome = "Players in Lobby";
            for (int i = 0; i < callbacks.Count() - 1; i++) {
                if (i != 0) { 
                    welcome += "\t";
                }
                welcome += $"Player {i + 1}\n";
            }
            return welcome;
        }

        // this will take a card index, and current colour choice
        // process the card choice, set top card of discard.
        // then resolve its effects
        public void EndTurn(int cardIndex, Colour nextColour) {
            int nextPlayerIndex = (playerIndex + 1) % callbacks.Count;

            // only do if card has been played
            if (cardIndex != -1) {
                Card c = players[playerIndex].ElementAt(cardIndex);
                // card from player hand to discard
                discard.Add(c);
                // remove played card from hand
                players[playerIndex].RemoveAt(cardIndex);

                currentColour = nextColour;
                // handle special effects here

                // will effect next player index
                switch (c.value) { 
                    case Value.Reverse: 
                        break;
                    case Value.Skip:
                        break;
                    case Value.plus2:
                        break;
                    case Value.wild4:
                        break;
                }
            }

            playerIndex = nextPlayerIndex;

            UpdateAllClients();
        }
    }
}
