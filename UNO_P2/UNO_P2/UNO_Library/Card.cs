using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.Serialization;

namespace UnoLibrary {

    [DataContract]
    public enum Colour { [EnumMember] Blue, [EnumMember] Green, [EnumMember] Red, [EnumMember] Yellow, [EnumMember] Wild }

    [DataContract]
    public enum Value {
        [EnumMember] Zero, [EnumMember] One, [EnumMember] Two, [EnumMember] Three, [EnumMember] Four, [EnumMember] Five,
        [EnumMember] Six, [EnumMember] Seven, [EnumMember] Eight, [EnumMember] Nine,
        [EnumMember] Skip, [EnumMember] Reverse, [EnumMember] plus2, [EnumMember] wild, [EnumMember] wild4,
    }

    [DataContract]
    public class Card {
        // possible null => Wild cards dont have a colour associated with them

        [DataMember]
        public Colour colour { get; private set; }

        [DataMember]
        public Value? value { get; private set; }

        public Card(Colour c, Value v) { 
            this.colour = c;
            this.value = v;
        }

        // toString
        public override string ToString() {
            string output = "";

            output +=  colour.ToString() + " ";

            switch (this.value) {
                case Value.wild:
                    break; // nothing to add
                case Value.wild4:
                    output += "Draw 4";
                    break;
                case Value.plus2:
                    output += "Draw 2";
                    break;
                default:
                    output += this.value.ToString();
                    break;
            }
            return output;
        }
    }
}
