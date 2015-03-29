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
        private int numberParked; //Represents how many normal cars have been parked in the car park so far
        public int NumberParked
        {
            get
            {
                return this.numberParked;
            }
        }

        [ContractPublicPropertyNameAttribute("SubscribersParked")]
        private int subscribersParked; //Represents how many subscribers have been paked in the car park so far.
        public int SubscribersParked
        {
            get
            {
                return this.subscribersParked;
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

        private bool carParkIsOpen;


        private  int spacesAvailable; 
        public int SpacesAvailable
        {
            get
            {
                return this.spacesAvailable;
            }
        }

        
        public static int IDIOT_SPACES = 8; //Spaces reserved for the idiots.
       
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
            carParkIsOpen = true;

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
           Contract.Requires(car != 0, ": The car can't be 0");
           Contract.Requires(car < 1000, ": The car can't be greater than 999");
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
           Contract.Requires(car < 1000, ": The car can't be greater than 999");
           Contract.Requires(spaces.Contains(car), ": The car-park needs to contain the car that is to be removed.");

           //Ensures that the car isn't in the array and that the rest of the array remains unchanged.
           //using car - car because for some reason when 0 was supplied as a parameter code contracts didn't want to do much. 
           //i.e the post condition wasn't being checked.
           Contract.Ensures(!spaces.Contains(car));

           //Every value either needs to be in the same position and value as the old array or 0.
           Contract.Ensures(Contract.ForAll(0, spaces.Length, i => spaces[i].Equals(Contract.OldValue(spaces[i])) 
                             || spaces[i].Equals( car - car))); 
                        
           int firstIndex = Array.IndexOf(spaces, car); //Searches the array for the first index of the car (the only occurance(hopefully!))

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

           //Ensures that the result is either 0 or the true number of spaces available.
           Contract.Ensures(Contract.Result<int>() == spacesAvailable || Contract.Result<int>() == 0);
        
           //Returns the larger of the 2 numbers.
           //If spacesAvailable is < 0 it means that cars have made use of the reserved space but spacesAvailable will still be updated.
           //Returning 0 means that no more non-reserved spaces are available, but normal cars are making use of reserved spaces.
           return Math.Max(0, spacesAvailable);   
       }


        //Allows a car with a reservation to enter the car parks reserved area during hour of operation. 
        //Or enter the car park generally outside these hours
       public void enterReservedArea(int car)
       {
           Contract.Requires(car != 0, ": The car can't be 0");
           Contract.Requires(car < 1000, ": The car can't be greater than 999");
           Contract.Requires(subscriberList.Contains(car),": The car doesn't have a subscription." ); //The list of subscribers is required to contain the car
           Contract.Requires(!spaces.Contains(car), ": The car park can't contain duplicate cars.");

           //Requires that the barrier is down and the number of remaining spaces is equal to the number of cars that are
           //subscribed to a reserved space.
           //OR
           //The the car park isn't full.
           Contract.Requires(barrierIsOpen == false && spaces.Length - numberParked - IDIOT_SPACES >= subscriberList.Length
               || spaces.Length - numberParked - IDIOT_SPACES - subscribersParked > 0, ": The car park is full.");

           //Ensures that the array contains the car and that the rest of the array is unchanged.
           Contract.Ensures(spaces.Contains(car));
           Contract.Ensures(Contract.ForAll(0, spaces.Length, i => spaces[i].Equals(Contract.OldValue(spaces[i]))
                            || spaces[i].Equals(car)));


           //If the barrier isn't open for everyone then the car can only park in the reserved spaces.
           if (barrierIsOpen == false)
           {
               //Calculate the starting position for the reserved spaces then find the first empty reserved space.
               int reservedOffset = spaces.Length  -  subscriberList.Length;
               int firstIndex = Array.IndexOf(spaces, 0, reservedOffset);
               spaces[firstIndex] = car;              
           }
           else
           {
               //Can park any where in the car park if the barrier is open (execpt the idiot spaces)
               int firstIndex = Array.IndexOf(spaces, 0, IDIOT_SPACES);
               spaces[firstIndex] = car;
           }

           subscribersParked++;
       }

       public void makeSubscription(int car)
       {
           Contract.Requires(car != 0);
           Contract.Requires(!subscriberList.Contains(car), ": That car is already subscribed.");
           Contract.Requires(subscriberList.Contains(0), ": No more space on the subsciber list"); //There needs to be an empty space on the list to proceed

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

           Console.WriteLine("The reserved area is open.");
       }

       public void closeCarPark()
       {
           //Ensures all the cars have been crushed... (are 0)
           Contract.Ensures(Contract.ForAll(0, spaces.Length, i => spaces[i].Equals( 0 )));

           //Ensures the boolean is false. (car park is closed)
           Contract.Ensures(!carParkIsOpen);

           for (int i = 0; i < spaces.Length; i++)
           {
               spaces[i] = 0;
           }

           this.carParkIsOpen = false;

           Console.WriteLine("It's 11pm the car park is now closed!");

       }

        //Hideous code that prints the car park as a nice picture
        //Indicates the state of each space. = (empty, ocupied, r = reserved, x = idiot spaces)
       public void printParkingPlan()
       {        
           
           for (int x = 0; x < spaces.Length; x++)
           {
               for (int i = x; i < spaces.Length; i++)
               {
                   Console.Write(" _____ ");

               
                   if (i > 0 && i % 10 == 0) //Divisable by 10? Every 10 elements start a new row...
                   {
                       //Break for a new row
                       break;
                   }
               }

               Console.WriteLine(" ");
               for (int i = x; i < spaces.Length; i++)
               {
                   Console.Write(" |   | ");

                   if (i > 0 && i % 10 == 0) //Divisable by 10? Every 10 elements start a new row...
                   {
                       //Break for a new row
                       break;
                   }
               }

               Console.WriteLine(" ");
               for (int i = x; i < spaces.Length; i++)
               {
                   int reservedOffset = spaces.Length - subscriberList.Length;

                   //Formatting code:
                   if (spaces[i] == 0) // zero
                   {
                       if (i >= reservedOffset)
                       {
                           Console.Write(" | R | ");
                       }
                       else if (i < IDIOT_SPACES)
                       {
                           Console.Write(" | X | ");
                       }
                       else
                       {
                           Console.Write(" |   | ");
                       }

                   }

                   if(spaces[i] > 0 && spaces[i] < 10) // upto 10
                   {
                       Console.Write(" | " + spaces[i] + " | ");
                   }

                   if (spaces[i] > 10 && spaces[i] < 100) // upto 99
                   {
                       Console.Write(" | " + spaces[i] + "| ");
                   }

                   if (spaces[i] > 99 && spaces[i] < 1000) //upto 999
                   {
                       Console.Write(" |" + spaces[i] + "| ");
                   }

                 
                   if (i > 0 && i % 10 == 0) //Divisable by 10? Every 10 elements start a new row...
                   {
                       //Break for a new row
                       break;
                   }
               }

               Console.WriteLine(" ");
               for (int i = x; i < spaces.Length; i++)
               {
                   Console.Write(" |___| ");

              
                   if (i > 0 && i % 10 == 0) //Divisable by 10? Every 10 elements start a new row...
                   {
                       //Break for a new row
                       break;
                   }

                   x = i + 1;
               }
               Console.WriteLine(" ");
           }

           //Add a gap for a footer...
           Console.WriteLine(" ");                
       }


        //Prints as standard console output
       public void printAsText()
       {
           int reservedOffset = spaces.Length - subscriberList.Length;
           Console.WriteLine(" ");

           for (int i = 0; i < spaces.Length; i++)
           {
               if (i < IDIOT_SPACES)
               {
                   Console.WriteLine("X");
               }
               else if (i >=reservedOffset && spaces[i] == 0)
               {
                   Console.WriteLine("R");
               }
               else
               {
                   Console.WriteLine(spaces[i]);
               }
           }
       }

    }


    class Program
    {
        static void Main(string[] args)
        {
            CarPark carPark = new CarPark(25, 5);

            carPark.printParkingPlan();       
            Console.WriteLine("Spaces left: " + carPark.checkAvailability());

            carPark.enterCarPark(1);
            carPark.enterCarPark(3);
            carPark.enterCarPark(2);

            Console.WriteLine("Spaces left: " + carPark.checkAvailability());

            carPark.printAsText();

            carPark.enterCarPark(44);
            carPark.enterCarPark(99);
            carPark.enterCarPark(22);
            carPark.enterCarPark(11);
            carPark.enterCarPark(999);

            carPark.leaveCarPark(1);
            carPark.leaveCarPark(22);

            carPark.makeSubscription(221);
            carPark.enterReservedArea(221);


            carPark.printAsText();
            carPark.printParkingPlan();

            Console.WriteLine("Spaces left: " + carPark.checkAvailability());

            carPark.openReservedArea();
            carPark.leaveCarPark(221);
            carPark.enterReservedArea(221);
            carPark.enterCarPark(191);

            carPark.printParkingPlan();

            carPark.closeCarPark();

            carPark.printParkingPlan();
          

            Console.ReadLine();

        }
    }
}
