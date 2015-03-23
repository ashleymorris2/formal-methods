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

            Contract.Requires(reservedSpaces != 0 && reservedSpaces < carParkSize);
            Contract.Requires(carParkSize - IDIOT_SPACES - reservedSpaces > 0, "You need at least 1 non-reserved space.");

            //this.day = day;
            //this.time = 9;

            subscriberList = new int[reservedSpaces];
            spaces = new int[carParkSize];

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
           Contract.Requires(spaces.Length - numberParked - IDIOT_SPACES - subscriberList.Length > 0, ": The non reserved area is full.");

           Contract.Ensures(spaces.Contains(car));
           Contract.Ensures(Contract.ForAll(0, spaces.Length, i => spaces[i].Equals( Contract.OldValue(spaces[i]) ) || spaces[i].Equals(car)) );

           //Gets the first empty element (0) of the arrray to write to. 
           //C# treats 0 as an empty element.
           int firstIndex = Array.IndexOf(spaces, 0);
           spaces[firstIndex] = car;
        
           numberParked++;
 

           //Update the available spaces
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
           Contract.Ensures(Contract.ForAll(0, spaces.Length, i => spaces[i].Equals(Contract.OldValue(spaces[i])) || spaces[i].Equals( car - car)));
             

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
        
           return spacesAvailable;
       }


       public void enterReservedArea(int car)
       {

       }



    }


    class Program
    {
        static void Main(string[] args)
        {
            CarPark carPark = new CarPark(18, 5);


            Console.WriteLine("Spaces left: " + carPark.checkAvailability());

            carPark.enterCarPark(5);
            carPark.enterCarPark(4);
            carPark.enterCarPark(9);
            carPark.enterCarPark(7);
            carPark.leaveCarPark(5);
            Console.WriteLine(" ");

            for (int i = 0; i < carPark.Spaces.Length; i++)
            {
                Console.WriteLine(carPark.Spaces[i]);
            }

          
            carPark.enterCarPark(44);
         


            Console.WriteLine(" ");


            for (int i = 0; i < carPark.Spaces.Length; i++)
            {
                Console.WriteLine(carPark.Spaces[i]);
            }

            Console.WriteLine("Spaces left: " + carPark.checkAvailability());

            Console.ReadLine();

        }
    }
}
