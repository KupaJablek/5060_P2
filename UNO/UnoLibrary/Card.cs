using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnoLibrary {

    public enum Colour { Blue, Green, Red, Yellow, Wild }

    public enum Value { 
        zero, one, two, three, four, five,
        six, seven, eight, nine,
        skip, reverse, plus2, wild, wild4,
    }

    public class Card {
        // possible null => Wild cards dont have a colour associated with them

        public Colour colour { get; private set; }

        public Value? value { get; private set; }

        public Card(Colour c, Value v) { 
            this.colour = c;
            this.value = v;
        }

        // toString
        public override string ToString() {
            return "";
        }
    }
}
