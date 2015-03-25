using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Specs_Assignment
{
    public class CarPark
    {
        private int day; //The current day of the week. 1 for monday, 2 for tuesday ... 6 for saturday, 7 for sunday.
        private int time; // The current hour of the day. 0 - 23;

        [ContractPublicPropertyNameAttribute("Spaces")]
        private int[] spaces;//The sum of all the spaces in the car park
        public int [] Spaces
        {
           get
            {
               //Returns a shallow copy of the arrays contents.
               //Crude defence against using the (=) operator to overwrite the array.
               //This means things like: carPark.Spaces[0] = 55 don't affect the arrays data.
               //But still allow the data to be read by user/client.
                return (int[]) spaces.Clone();
            }
        }

        [ContractPublicPropertyNameAttribute("SubscriberList")]
        private int [] subscriberList;//A list of the current subscribers
        public int[] SubscriberList
        {
            get
            {
                //Returns a shallow copy of the arrays contents.
                //Same reason as in int[] Spaces
                return (int[])subscriberList.Clone();
            }
        }

        [ContractPublicPropertyNameAttribute("NumberParked")]
        private int numberParked; //Represents how many cars have been parked in the car park so far
        public int NumberParked
        {
            get
            {
                return this.numberParked;
            }
        }

        [ContractPublicPropertyNameAttribute("BarrierIsOpen")]
        private bool barrierIsOpen;
        public bool BarrierIsOpen
        {
            get
            {
                return this.barrierIsOpen;
            }
        }

        private  int spacesAvailable; 
        public int SpacesAvailable
        {
            get
            {
                return this.spacesAvailable;
            }
        }

        

        public static int IDIOT_SPACES = 8;
       

        //Sets the car-park up for a new day. Takes two parameters: The day to set the carpark at and the size of the carpark
        //in terms of how many spaces there are.
       public CarPark(int carParkSize, int reservedSpaces)
        {
            //Day has to be greater than 0.
            
           // Contract.Requires(day > 0, "The day needs to be greater than 0.");
           // Contract.Requires(day <= 7, "The day needs to be equal to or less than 7.");

            //carParkSize has to be greater than or equal to 10 to allow 8 idiotspaces spaces AND at least 1 normal space AND 1 reserved space.
            Contract.Requires(carParkSize >= 10, "The size needs to be greater than or equal to 10.");

            Contract.Requires(reservedSpaces != 0 && reservedSpaces < carParkSize, 
                ": reservedSpaces needs to be greater than 0 and less than the car parks size");

            Contract.Requires(carParkSize - IDIOT_SPACES - reservedSpaces > 0, ": You need at least 1 non-reserved space.");

            //this.day = day;
            //this.time = 9;

            subscriberList = new int[reservedSpaces];
            spaces = new int[carParkSize];
            barrierIsOpen = false;

            //Set the available spaces
            spacesAvailable = spaces.Length - numberParked - IDIOT_SPACES - subscriberList.Length;
        }


        //Allows any car without a reservation to enter the carpark.
        //Takes the car that is to enter the car park as a paramter and needs to be a unique integer
       public void enterCarPark(int car)
       {
           //The car can not already be in the car park
           //The car can not be on the subscirber list.
           //There needs to be room in the non reserved area for a new car.
           //Ensures that the rest of the array is unchanged except the new car.
           Contract.Requires(car != 0);
           Contract.Requires(!spaces.Contains(car), ": The car park can't contain duplicate cars.");
           Contract.Requires(!subscriberList.Contains(car), ": Only cars without a reservation may use this method.");
           
           //Either the reserved area isn't full or the barrier is open and the whole carpark isn't full.
           Contract.Requires(spaces.Length - numberParked - IDIOT_SPACES - subscriberList.Length > 0 
               || barrierIsOpen == true && spaces.Length - numberParked - IDIOT_SPACES >0, ": There is no more room!");

           Contract.Ensures(spaces.Contains(car));
           Contract.Ensures(Contract.ForAll(0, spaces.Length, i => spaces[i].Equals( Contract.OldValue(spaces[i]) ) 
                            || spaces[i].Equals(car)) );

           //Gets the first empty element (0) of the arrray to write to. 
           //C# treats 0 as an empty element.
           int firstIndex = Array.IndexOf(spaces, 0, IDIOT_SPACES);
           spaces[firstIndex] = car;
        
           numberParked++;
 
           //Update the available spaces
           //If 0 then there are no more reserved spaces left.  
           //If cars are still using this method and spaces available is 0 then cars must be entering the reserved space.
       
           spacesAvailable = spaces.Length - numberParked - IDIOT_SPACES - subscriberList.Length;
           
       }


        //Allows any car from any area to leave the car park.
        //Takes the car that is to leave the car park as a parameter
       public void leaveCarPark(int car)
       {
           //The car needs to be in the car park in order for it to be removed.
           Contract.Requires(car != 0, ": The car can't be 0.");
           Contract.Requires(spaces.Contains(car), ": The car-park needs to contain the car that is to be removed.");

           //Ensures that the car isn't in the array and that the rest of the array remains unchanged.
           //using car - car because for some reason when 0 was supplied as a parameter code contracts didn't want to do much. 
           //i.e the post condition wasn't being checked.
           Contract.Ensures(!spaces.Contains(car));
           Contract.Ensures(Contract.ForAll(0, spaces.Length, i => spaces[i].Equals(Contract.OldValue(spaces[i])) 
                             || spaces[i].Equals( car - car)));
             

           int firstIndex = Array.IndexOf(spaces, car); //Searches the array for the first index of the car (the only occurance(hopefully!!))

           spaces[firstIndex] = 0;
           
           numberParked--;
   
           //Update the available spaces
           spacesAvailable = spaces.Length - numberParked - IDIOT_SPACES - subscriberList.Length;
       }


        //Reports on the number of non-reserved spaces available.
       public int checkAvailability()
       {
           //Available =
           //Number of spaces (minus) the number parked (minues) 8 idiot spaces (minus) the number of subscriber spaces.
           Contract.Ensures(this.spacesAvailable == spaces.Length - numberParked - IDIOT_SPACES - subscriberList.Length);
        
           //Returns the larger of the 2.
           //If spacesAvailable is < 0 it means that cars have made use of the reserved space but spacesAvailable will still be updated.
           //Returning 0 means that no more non-reserved spaces are available.
           return Math.Max(0, spacesAvailable);
       }


        //Allows a car with a reservation to enter the car parks reserved area during hour of operation. 
        //Or enter the car park generally outside these hours
       public void enterReservedArea(int car)
       {
           Contract.Requires(car != 0);
           Contract.Requires(subscriberList.Contains(car),": The car doesn't have a subscription." ); //The list of subscribers is required to contain the car
           Contract.Requires(!spaces.Contains(car), ": The car park can't contain duplicate cars.");

           //Requires that the barrier is down and the number of remaining spaces is equal to the number of cars that are
           //subscribed to a reserved space.
           //OR
           //The the car park isn't full.
           Contract.Requires(barrierIsOpen == false && spaces.Length - numberParked - IDIOT_SPACES >= subscriberList.Length
               || spaces.Length - numberParked - IDIOT_SPACES > 0, ": The car park is full.");

           //Ensures that the array contains the car and that the rest of the array is unchanged.
           Contract.Ensures(spaces.Contains(car));
           Contract.Ensures(Contract.ForAll(0, spaces.Length, i => spaces[i].Equals(Contract.OldValue(spaces[i]))
                            || spaces[i].Equals(car)));


           //If the barrier isn't open for everyone then the car can only park in the reserved spaces.
           if (barrierIsOpen == false)
           {
               //Calculate the starting position for the reserved spaces then find the first empty reserved space.
               int reservedOffset = spaces.Length -  subscriberList.Length;
               int firstIndex = Array.IndexOf(spaces, 0, reservedOffset);
               spaces[firstIndex] = car;
           }
           else
           {
               int firstIndex = Array.IndexOf(spaces, 0, IDIOT_SPACES);
               spaces[firstIndex] = car;
           }
       }

       public void makeSubscription(int car)
       {
           Contract.Requires(car != 0);
           Contract.Requires(!subscriberList.Contains(car));
           Contract.Requires(SubscriberList.Contains(0)); //There needs to be an empty space to proceed

           Contract.Ensures(subscriberList.Contains(car));
           Contract.Ensures(Contract.ForAll(0, subscriberList.Length, i => subscriberList[i].Equals(Contract.OldValue(subscriberList[i]))
                            || subscriberList[i].Equals(car)));
          

           int firstIndex = Array.IndexOf(subscriberList, 0);
           subscriberList[firstIndex] = car;          
       }

       public void openReservedArea()
       {
           Contract.Ensures(barrierIsOpen);

           this.barrierIsOpen = true;
       }

       public void printParkingPlan()
       {
           Console.WriteLine(" ");

           for (int i = 0; i < spaces.Length; i++)
           {
               Console.WriteLine(spaces[i]);
           }

           Console.WriteLine(" ");

       }


    }


    class Program
    {
        static void Main(string[] args)
        {
            CarPark carPark = new CarPark(18, 5);

            carPark.makeSubscription(4);
            carPark.makeSubscription(55);
            carPark.makeSubscription(66);
            carPark.makeSubscription(22);
            carPark.makeSubscription(11);

            carPark.enterReservedArea(4);
            carPark.enterReservedArea(55);
            carPark.enterReservedArea(66);
         

            Console.WriteLine("Spaces left: " + carPark.checkAvailability());
            carPark.printParkingPlan();

            carPark.enterCarPark(67);
            carPark.enterCarPark(12);
            carPark.enterCarPark(98);
            carPark.enterCarPark(1);
            carPark.enterCarPark(88);

            carPark.openReservedArea();
            carPark.enterCarPark(100);
            carPark.enterCarPark(101);
          
            


            carPark.printParkingPlan();
           

            Console.WriteLine("Spaces left: " + carPark.checkAvailability());

            Console.ReadLine();

        }
    }
}
