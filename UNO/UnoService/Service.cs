using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using UnoLibrary;

namespace UnoService {
    [ServiceContract]
    public interface IService {
        [OperationContract]
        void PlayCard(int playerID, Card card);
        [OperationContract]
        void DrawCard();
        [OperationContract]
        void EndTurn();
    }
    public class Service : IService {
        GameManager gm;

        public Service() {
            gm = new GameManager();
        }

        public void StartNewGame() {
            gm.setFirstCard();
        }
        public void ShuffleDeck() {
            gm.shuffleDeck();
        }
        public void DealCards() {
            //implement in gm
        }



        public void DrawCard() {
            gm.draw();
        }

        public void EndTurn() {
            //implement in gm
        }

        public void PlayCard(int playerID, Card card) {
            //implement in gm
        }
        public List<Card> GetPlayerHand(int playerID) {
            //implement in gm
        }





        //static void Main(string[] args) {
        //    ServiceHost sh = null;
        //    try {

        //        // address for host
        //        //servHost = new ServiceHost(typeof(), new Uri("net.tcp://localhost:13000/UnoLibrary/"));

        //        // Run the service
        //        //sh.Open();

        //        Console.WriteLine("UnoLibrary has started. Press any key to quit");

        //    } catch (Exception ex) {
        //        Console.WriteLine(ex.Message);
        //    } finally {
        //        Console.ReadKey();
        //        if (sh != null) { 
        //            sh.Close();
        //        }
        //    }
    }
}
}
