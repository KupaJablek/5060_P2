using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnoLibrary {
    public class GameManager {
        private List<Card> deck;
        private List<Card> discard;
        private int cardsIndex;
        private Dictionary<int, List<Card>> players;

        private Colour currentColour; // will represent colour of top of discard

        public GameManager() {
            populateDeck();
            shuffleDeck();

            discard = new List<Card>();
            //setFirstCard(); // only needs to happen at start of new game
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
            deck.Add(new Card(Colour.Blue, Value.zero));
            deck.Add(new Card(Colour.Green, Value.zero));
            deck.Add(new Card(Colour.Red, Value.zero));
            deck.Add(new Card(Colour.Yellow, Value.zero));

            // add two of each card for all colours. No wild
            foreach (Colour currentColour in Enum.GetValues(typeof(Colour))) {
                if (currentColour != Colour.Wild) {
                    foreach (Value val in Enum.GetValues(typeof(Value))) {
                        switch (val) {
                            case Value.zero:
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

            // set first card in discard

            // work off of cards index in event player hands are dealt before first card is set

            bool firstCard = false;
            int firstCardIndex = cardsIndex;

            while (!firstCard) {
                Card c = deck[cardsIndex];
                if (c.colour == Colour.Wild ||
                    c.value == Value.skip ||
                    c.value == Value.plus2 ||
                    c.value == Value.reverse) 
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

        public void DealCards(int numCards, int numPlayers)
        {
            // Create a new dictionary of player hands
            players = new Dictionary<int, List<Card>>();

            // Deal cards to each player
            for (int i = 0; i < numPlayers; i++)
            {
                List<Card> hand = new List<Card>();

                for (int j = 0; j < numCards; j++)
                {
                    hand.Add(draw());
                }

                players.Add(i, hand);
            }
        }

        public bool EndTurn(int playerId)
        {
            List<Card> playerHand = players[playerId];

            // Check if player has any playable cards
            foreach (Card card in playerHand)
            {
                if (card.colour == currentColour ||
                    card.value == Value.wild ||
                    card.value == Value.wild4)
                {
                    return false;
                }
            }

            // Player has no playable cards, end turn
            return true;
        }

        public bool PlayCard(int playerId, int cardIndex)
        {
            List<Card> playerHand = players[playerId];
            Card card = playerHand[cardIndex];

            // Check if card is playable
            if (card.colour == currentColour ||
                card.value == Value.wild ||
                card.value == Value.wild4)
            {
                // Remove card from player's hand and add to discard pile
                playerHand.RemoveAt(cardIndex);
                discard.Add(card);

                // Update current colour
                if (card.value != Value.wild && card.value != Value.wild4)
                {
                    currentColour = card.colour;
                }

                return true;
            }

            return false;
        }

        public List<Card> GetPlayerHand(int playerId)
        {
            return players[playerId];
        }
    }
}
