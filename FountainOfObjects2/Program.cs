using System.Dynamic;
using static FountainOfObjects2.Program;

namespace FountainOfObjects2
{
    internal class Program
    {
        static void Main(string[] args)
        {
            GameFactory.MakeGame().GameRun();
        }



        public static class GameFactory
        {
            public static FountainOFObjects MakeGame()
            {
                while (true)
                {
                    Console.WriteLine("Small, medium, large?");
                    string? input = Console.ReadLine();
                    if (input == "small") return MakeSmallGame();
                    if (input == "medium") return MakeMediumGame();
                    if (input == "large") return MakeLargeGame();
                    Console.WriteLine("I didn't understand that!");
                }
            }

            public static FountainOFObjects MakeSmallGame()
            {
                Map map = new Map(4, 4);
                Maelstrom maelstrom = new Maelstrom();

                map.SetRoom(0, 0, new Entrance());
                map.SetRoom(0, 2, new Fountain());
                map.SetRoom(2, 1, new Pit());

                return new FountainOFObjects(map);
            }

            public static FountainOFObjects MakeMediumGame()
            {
                Map map = new Map(6, 6);
                map.SetRoom(2, 2, new Entrance());
                map.SetRoom(4, 5, new Fountain());
                return new FountainOFObjects(map);
            }

            public static FountainOFObjects MakeLargeGame()
            {
                Map map = new Map(8, 8);
                map.SetRoom(6, 6, new Entrance());
                map.SetRoom(0, 5, new Fountain());

                return new FountainOFObjects(map);
            }
        }




        public class FountainOFObjects
        {
            public Map Map { get; }
            public Player Player { get; }
            public Maelstrom Maelstrom { get; }


            public FountainOFObjects(Map map)
            {
                Map = map;
                Player = new Player();
                Maelstrom = new Maelstrom();
                SetPlayerLocationAtEntrance(Map, Player);
            }

            void SetPlayerLocationAtEntrance(Map map, Player player)
            {
                for (int row = 0; row < Map.Rows; row++)
                    for (int column = 0; column < Map.Columns; column++)
                        if (Map.GetRoom(row, column) is Entrance)
                        {
                            Player.PlayerLocation = new Location(row, column);
                        }
            }

            void SetMaelstromLocation(int row, int column, Maelstrom maelstrom)
            {
                for (int i = 0; i < Map.Rows; i++)
                    for (int j = 0; column < Map.Columns; j++)
                        if (Map.GetRoom(row, column) is EmptyRoom)
                        {
                            maelstrom.MaelstromLocation = new Location(row, column);
                        }
            }


            public void GameRun()
            {
                PlayerInput playerInput = new PlayerInput();

                while (!HasWon() && !PitDeath())
                {


                    ShowMap();
                    new MaelstromSense().SenseStuff(this);
                    new PitSense().SenseStuff(this);
                    new FountainSense().SenseStuff(this);
                    new EntranceSense().SenseStuff(this);
                    IAction action = playerInput.ChooseAction();
                    action.ExecuteAction(this);


                }
                if (HasWon())
                {
                    Console.WriteLine("The fountain of objects has been reactivated, and you escaped with your life");
                    Console.WriteLine("You win!!!");
                }
                if (PitDeath()) Console.WriteLine("You died in a pit");
            }

            public bool HasWon()
            {

                Room playerRoom = Map.GetRoomAtLocation(Player.PlayerLocation);
                if (playerRoom is not Entrance) return false;
                for (int row = 0; row < Map.Rows; row++)
                    for (int column = 0; column < Map.Columns; column++)
                    {
                        if (Map.GetRoom(row, column) is Fountain fountainRoom)
                            if (fountainRoom.IsOff) return false;

                    }

                return true;

            }

            public bool PitDeath()
            {
                Room playerRoom = Map.GetRoomAtLocation(Player.PlayerLocation);
                if (playerRoom is not Pit) return false;
                return true;
            }

            /*public void MaelstromMove()
            {
                Room playerRoom = Map.GetRoomAtLocation(Player.PlayerLocation);
                if(playerRoom is Maelstrom)
                {
                    
                }
            }*/



            public void ShowMap()
            {
                for (int row = 0; row < Map.Rows; row++)
                {

                    for (int column = 0; column < Map.Columns; column++)
                        //if (Player.PlayerLocation == new Location(row, column))
                        if (Map.GetRoomAtLocation(Player.PlayerLocation) == Map.GetRoom(row, column))
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.Write("@");
                            Console.ForegroundColor = ConsoleColor.Gray;
                        }
                        else if (Map.GetRoom(row, column) is Entrance)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.Write("#");
                            Console.ForegroundColor = ConsoleColor.Gray;
                        }
                        else if (Map.GetRoom(row, column) is Fountain)
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.Write("&");
                            Console.ForegroundColor = ConsoleColor.Gray;
                        }
                        else if (Map.GetRoom(row, column) is Pit)
                        {
                            Console.ForegroundColor = ConsoleColor.Blue;
                            Console.Write("*");
                            Console.ForegroundColor = ConsoleColor.Gray;
                        }
                        else if (Map.GetRoomAtLocation(Maelstrom.MaelstromLocation) == Map.GetRoom(row, column))
                        {
                            Console.ForegroundColor = ConsoleColor.Magenta;
                            Console.Write("X");
                            Console.ForegroundColor = ConsoleColor.Gray;
                        }

                        else Console.Write("0");
                    Console.WriteLine();
                }


            }

        }


        public enum Move { North, South, East, West }
        public enum Senses { Hear, Smell, Touch }
        enum Action { EnableFountain }



        internal class PlayerInput
        {
            public IAction ChooseAction()
            {
                do
                {
                    Console.WriteLine("What do you want to do? ");
                    string? input = Console.ReadLine();
                    IAction? chosenAction = input switch
                    {
                        "move north" => new MovementDirection(Move.North),
                        "move south" => new MovementDirection(Move.South),
                        "move west" => new MovementDirection(Move.West),
                        "move east" => new MovementDirection(Move.East),
                        "enable fountain" => new EnableFountain(),
                        _ => null
                    };
                    if (chosenAction != null) return chosenAction;

                    Console.WriteLine("I do not understand that");
                } while (true);

            }
        }

        public interface ISense
        {
            void SenseStuff(FountainOFObjects game);
        }

        public class MaelstromSense : ISense
        {
            public void SenseStuff(FountainOFObjects game)
            {
                if (IsNeighbor(game)) Console.WriteLine("You hear the growling and groaning of a maelstrom nearby.");
            }

            public bool IsNeighbor(FountainOFObjects game)
            {
                Location playerLocation = game.Player.PlayerLocation;
                Location maelstromLocation = game.Maelstrom.MaelstromLocation;
                if (game.Map.GetRoom(playerLocation.Row - 1, playerLocation.Column - 1) == game.Map.GetRoomAtLocation(maelstromLocation)) return true;
                if (game.Map.GetRoom(playerLocation.Row - 1, playerLocation.Column + 0) == game.Map.GetRoomAtLocation(maelstromLocation)) return true;
                if (game.Map.GetRoom(playerLocation.Row - 1, playerLocation.Column + 1) == game.Map.GetRoomAtLocation(maelstromLocation)) return true;
                if (game.Map.GetRoom(playerLocation.Row + 0, playerLocation.Column - 1) == game.Map.GetRoomAtLocation(maelstromLocation)) return true;
                if (game.Map.GetRoom(playerLocation.Row + 0, playerLocation.Column + 0) == game.Map.GetRoomAtLocation(maelstromLocation)) return true;
                if (game.Map.GetRoom(playerLocation.Row + 0, playerLocation.Column + 1) == game.Map.GetRoomAtLocation(maelstromLocation)) return true;
                if (game.Map.GetRoom(playerLocation.Row + 1, playerLocation.Column - 1) == game.Map.GetRoomAtLocation(maelstromLocation)) return true;
                if (game.Map.GetRoom(playerLocation.Row + 1, playerLocation.Column + 0) == game.Map.GetRoomAtLocation(maelstromLocation)) return true;
                if (game.Map.GetRoom(playerLocation.Row + 1, playerLocation.Column + 1) == game.Map.GetRoomAtLocation(maelstromLocation)) return true;
                return false;

            }
        }
        public class PitSense : ISense
        {

            public void SenseStuff(FountainOFObjects game)
            {
                if (IsNeighbor(game)) Console.WriteLine("You feel a draft. There is a pit in a nearby room.");
            }

            public bool IsNeighbor(FountainOFObjects game)
            {
                Location playerLocation = game.Player.PlayerLocation;

                if (game.Map.GetRoom(playerLocation.Row - 1, playerLocation.Column - 1) is Pit) return true;
                if (game.Map.GetRoom(playerLocation.Row - 1, playerLocation.Column + 0) is Pit) return true;
                if (game.Map.GetRoom(playerLocation.Row - 1, playerLocation.Column + 1) is Pit) return true;
                if (game.Map.GetRoom(playerLocation.Row + 0, playerLocation.Column - 1) is Pit) return true;
                if (game.Map.GetRoom(playerLocation.Row + 0, playerLocation.Column + 0) is Pit) return true;
                if (game.Map.GetRoom(playerLocation.Row + 0, playerLocation.Column + 1) is Pit) return true;
                if (game.Map.GetRoom(playerLocation.Row + 1, playerLocation.Column - 1) is Pit) return true;
                if (game.Map.GetRoom(playerLocation.Row + 1, playerLocation.Column + 0) is Pit) return true;
                if (game.Map.GetRoom(playerLocation.Row + 1, playerLocation.Column + 1) is Pit) return true;
                return false;

            }
        }

        public class FountainSense : ISense
        {

            public void SenseStuff(FountainOFObjects game)
            {
                Location playerLocation = game.Player.PlayerLocation;
                Room playerRoom = game.Map.GetRoomAtLocation(playerLocation);

                if (playerRoom is Fountain fountainRoom)
                {
                    if (fountainRoom.IsOn) Console.WriteLine("You hear the rushing water from the Fountain of Objects. It has been reactivated");

                    else Console.WriteLine("You hear water dripping in this room. The Fountain of Objects is here");
                }



            }
        }

        public class EntranceSense : ISense
        {
            public void SenseStuff(FountainOFObjects game)
            {
                Room playerRoom = game.Map.GetRoomAtLocation(game.Player.PlayerLocation);
                if (playerRoom is Entrance) Console.WriteLine("You are at the entrance");
            }

        }

        public interface IAction
        {
            void ExecuteAction(FountainOFObjects game);
        }

        public class EnableFountain : IAction
        {
            public void ExecuteAction(FountainOFObjects game)
            {
                Location playerLocation = game.Player.PlayerLocation;
                Room playerRoom = game.Map.GetRoomAtLocation(playerLocation);
                Fountain? fountainRoom = playerRoom as Fountain;

                if (playerRoom != fountainRoom)
                {
                    Console.WriteLine("You can't do this in this room");
                    return;
                }
                fountainRoom.IsOn = true;
            }
        }

        public class MovementDirection : IAction
        {

            private readonly Move _direction;

            public MovementDirection(Move direction)
            {
                _direction = direction;
            }

            public void ExecuteAction(FountainOFObjects game)
            {
                Location current = game.Player.PlayerLocation;
                Location nextLocation = GetDirection(current, _direction);
                if (game.Map.IsOnMap(nextLocation))
                    game.Player.PlayerLocation = nextLocation;
                else Console.WriteLine("You can't move there");
            }

            public Location GetDirection(Location PlayerLocation, Move moveDirection)
            {
                return moveDirection switch
                {
                    Move.North => new Location(PlayerLocation.Row - 1, PlayerLocation.Column),
                    Move.South => new Location(PlayerLocation.Row + 1, PlayerLocation.Column),
                    Move.East => new Location(PlayerLocation.Row, PlayerLocation.Column + 1),
                    Move.West => new Location(PlayerLocation.Row, PlayerLocation.Column - 1),

                };


            }

        }


        public record Location(int Row, int Column);

        public class Map
        {
            private Room[,] _rooms = new Room[4, 4];

            public int Rows { get; }
            public int Columns { get; }

            public Map(int numberOfRows, int numberOfColumns)
            {

                Rows = numberOfRows;
                Columns = numberOfColumns;
                _rooms = new Room[Rows, Columns];


                for (int row = 0; row < numberOfRows; row++)
                {
                    for (int column = 0; column < numberOfColumns; column++)
                    {

                        _rooms[row, column] = new EmptyRoom();

                    }

                }

            }






            public bool IsOnMap(Location location)
            {
                if (location.Column < 0) return false;
                if (location.Column >= Columns) return false;
                if (location.Row < 0) return false;
                if (location.Row >= Rows) return false;
                return true;

            }



            public void SetRoom(int row, int column, Room room) => _rooms[row, column] = room;

            public Room GetRoom(int row, int column) => IsOnMap(new Location(row, column)) ? _rooms[row, column] : new OffTheMap();



            public Room GetRoomAtLocation(Location location) => IsOnMap(location) ? _rooms[location.Row, location.Column] : new OffTheMap();

        }


        public interface ICharacter { }

        public class Player : ICharacter
        {
            public Location PlayerLocation { get; set; } = new Location(0, 0);
        }

        public class Maelstrom : ICharacter
        {
            public Location MaelstromLocation { get; set; } = new Location(0, 0);
        }

        public abstract class Room { }
        public class EmptyRoom : Room { }
        public class Entrance : Room { }
        public class Fountain : Room
        {

            public bool IsOn { get; set; }
            public bool IsOff => !IsOn;
        }
        public class Pit : Room { }
        public class OffTheMap : Room { }

    }


}
