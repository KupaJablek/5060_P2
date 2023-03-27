using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UnoLibrary;

namespace UnoClient {
    public partial class Form1 : Form {
        private GameManager gm;
        public Form1() {
            InitializeComponent();

            gm = new GameManager();
        }
        private void btnDraw_Click(object sender, EventArgs e) {
            Card card = gm.draw();
            MessageBox.Show("You drew a " + card.ToString());
        }
        private void btnNewGame_Click(object sender, EventArgs e) {
            gm.populateDeck();
            gm.shuffleDeck();
            gm.setFirstCard();

            MessageBox.Show("New game started!");
        }


    }
}
