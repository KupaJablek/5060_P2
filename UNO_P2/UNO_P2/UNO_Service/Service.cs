using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ServiceModel;
using UnoLibrary;

namespace UnoService {
    class Program {
        static void Main() {
            ServiceHost servHost = null;

            try {
                servHost = new ServiceHost(typeof(GameManager));

                // Run the service
                servHost.Open();
                Console.WriteLine("Service started. Please any key to quit.");
            } catch (Exception ex) {
                Console.WriteLine(ex.Message);
            } finally {
                // Wait for a keystroke
                Console.ReadKey();
                servHost?.Close();
            }
        }
    }
}

