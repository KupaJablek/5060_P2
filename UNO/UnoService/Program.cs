using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using UnoLibrary;

namespace UnoService {
    internal class Program {
        static void Main(string[] args) {
            ServiceHost sh = null;
            try {

                // address for host
                //servHost = new ServiceHost(typeof(), new Uri("net.tcp://localhost:13000/UnoLibrary/"));

                // Run the service
                //sh.Open();

                Console.WriteLine("UnoLibrary has started. Press any key to quit");

            } catch (Exception ex) {
                Console.WriteLine(ex.Message);
            } finally {
                Console.ReadKey();
                if (sh != null) { 
                    sh.Close();
                }
            }
        }
    }
}
