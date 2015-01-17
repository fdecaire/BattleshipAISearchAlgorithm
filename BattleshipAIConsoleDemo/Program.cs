using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace BattleshipAIConsoleDemo
{
	class Program
	{
		private static ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		static int MaximumMapSize = 10;
		static int[,] map = new int[MaximumMapSize, MaximumMapSize];
		static Random rand = new Random();

		static void Main(string[] args)
		{
			log4net.Config.XmlConfigurator.Configure();

			//InitializeBoard();
			//SeekShip(2);
			//DumpBoard();

			TestMaximumShotsEvenOnly();
		}

		private static void TestMaximumShotsEvenOnly()
		{
			// 39
			int maxShotsFired = 0;

			for (int i = 0; i < 20000; i++)
			{
				int shotsFired = 0;

				InitializeBoard();
				RandomShipPosition(5);
				RandomShipPosition(4);
				RandomShipPosition(3);
				RandomShipPosition(6); // this is a sub, only a length of 3
				RandomShipPosition(2);

				while (!Sunk(5) && !Sunk(4))
				{
					shotsFired += SeekShip(4);
				}

				while (!Sunk(3) && !Sunk(6) && !Sunk(2))
				{
					shotsFired += SeekShip(2);
				}

				if (shotsFired > maxShotsFired)
				{
					maxShotsFired = shotsFired;
				}
			}

			log.Debug("max shots fired:" + maxShotsFired);
		}

		private static void TestMaximumShotsStepDown()
		{
			// 51
			int maxShotsFired = 0;

			for (int i = 0; i < 20000; i++)
			{
				int shotsFired = 0;

				InitializeBoard();
				RandomShipPosition(5);
				RandomShipPosition(4);
				RandomShipPosition(3);
				RandomShipPosition(6); // this is a sub, only a length of 3
				RandomShipPosition(2);

				while (!Sunk(5))
				{
					shotsFired += SeekShip(5);
				}

				while (!Sunk(4))
				{
					shotsFired += SeekShip(4);
				}

				while (!Sunk(3) && !Sunk(6))
				{
					shotsFired += SeekShip(3);
				}

				while (!Sunk(2))
				{
					shotsFired += SeekShip(2);
				}

				if (shotsFired > maxShotsFired)
				{
					maxShotsFired = shotsFired;
				}
			}

			log.Debug("max shots fired:" + maxShotsFired);
		}

		private static bool Sunk(int lengthOfShip)
		{
			int totalNonHits = 0;

			for (int y = 0; y < MaximumMapSize; y++)
			{
				for (int x = 0; x < MaximumMapSize; x++)
				{
					if (map[x, y] == lengthOfShip)
					{
						totalNonHits++;
					}
				}
			}

			// using the number 6 to represent the sub
			if (lengthOfShip == 6)
			{
				lengthOfShip = 3;
			}

			if (totalNonHits == lengthOfShip)
			{
				return false;
			}
			return true;
		}

		// place a ship of length "lengthOfShip" at a random position and direction on the board
		private static void RandomShipPosition(int lengthOfShip)
		{
			int shipType = lengthOfShip;
			ShipDirection dir;
			int maxXDir;
			int maxYDir;
			int x;
			int y;
			bool spaceAvail;

			if (lengthOfShip == 6)
			{
				// special handling for sub (aka hack)
				lengthOfShip = 3;
			}

			// loop unit success we find a blank spot to put this ship
			do
			{
				dir = (ShipDirection)(rand.Next() % 2);

				maxXDir = 10;
				maxYDir = 10;
				if (dir == ShipDirection.HORIZONTAL)
				{
					maxXDir -= lengthOfShip;
				}
				else
				{
					maxYDir -= lengthOfShip;
				}

				x = rand.Next() % maxXDir;
				y = rand.Next() % maxYDir;

				spaceAvail = IsSpaceAvailable(x, y, dir, lengthOfShip);
			} while (spaceAvail == false);

			// fill in the squares here 
			if (dir == ShipDirection.HORIZONTAL)
			{
				for (int i = 0; i < lengthOfShip; i++)
				{
					map[x + i, y] = shipType;
				}
			}
			else
			{
				for (int i = 0; i < lengthOfShip; i++)
				{
					map[x, y + i] = shipType;
				}
			}
		}

		// check to see if the spaces that this ship would occupy are all empty
		private static bool IsSpaceAvailable(int x, int y, ShipDirection dir, int lengthOfShip)
		{
			if (dir == ShipDirection.HORIZONTAL)
			{
				for (int i = 0; i < lengthOfShip; i++)
				{
					if (map[x + i, y] != 0)
					{
						return false;
					}
				}
			}
			else
			{
				for (int i = 0; i < lengthOfShip; i++)
				{
					if (map[x, y + i] != 0)
					{
						return false;
					}
				}
			}

			return true;
		}

		private static void InitializeBoard()
		{
			for (int y = 0; y < MaximumMapSize; y++)
			{
				for (int x = 0; x < MaximumMapSize; x++)
				{
					map[x, y] = 0;
				}
			}
		}

		private static void DumpBoard()
		{
			for (int y = 0; y < MaximumMapSize; y++)
			{
				string lineOfText = "";
				for (int x = 0; x < MaximumMapSize; x++)
				{
					if (map[x, y] == 0)
					{
						lineOfText += ".";
					}
					else
					{
						lineOfText += map[x, y].ToString();
					}
				}
				log.Debug(lineOfText);
			}
		}

		private static int SeekShip(int lengthOfShip)
		{
			int totalShotsFired = 0;

			// fire on best found positions until we run out of places to fire on

			List<ShipCoord> shipCells = FindShip(lengthOfShip);

			// log ship cells ordered descending by number of occurrences
			/*
			log.Debug("total shots left=" + NumberOfShotsLeft(shipCells));
			for (int i = 0; i < shipCells.Count; i++)
			{
				log.Debug(shipCells[i].X + "," + shipCells[i].Y + " [" + shipCells[i].Occurrences + "]");
			}
			*/
			//DumpBoard();

			while (NumberOfShotsLeft(shipCells) > 0)
			{
				// take a shot
				int cellData = map[shipCells[0].X, shipCells[0].Y];

				totalShotsFired++;

				if (cellData > 1)
				{
					MarkShipAsHit(lengthOfShip);
					return totalShotsFired;
				}
				else
				{
					map[shipCells[0].X, shipCells[0].Y] = 1;
				}

				//log.Debug(shipCells[0].X + "," + shipCells[0].Y);

				shipCells = FindShip(lengthOfShip);

				/*
				log.Debug("total shots left=" + NumberOfShotsLeft(shipCells));
				for (int i = 0; i < shipCells.Count; i++)
				{
					log.Debug(shipCells[i].X + "," + shipCells[i].Y + " [" + shipCells[i].Occurrences + "]");
				}
				*/
			}

			//DumpBoard();

			//log.Debug("total shots fired:" + totalShotsFired);

			return totalShotsFired;
		}

		private static void MarkShipAsHit(int lengthOfShip)
		{
			for (int y = 0; y < MaximumMapSize; y++)
			{
				for (int x = 0; x < MaximumMapSize; x++)
				{
					if (map[x, y] == lengthOfShip)
					{
						map[x, y] = 1;
					}
				}
			}
		}

		private static int NumberOfShotsLeft(List<ShipCoord> shipCells)
		{
			int total = 0;

			for (int i = 0; i < shipCells.Count; i++)
			{
				if (shipCells[i].Occurrences > 0)
				{
					total++;
				}
			}

			return total;
		}

		private static List<ShipCoord> FindShip(int lengthOfShip)
		{
			// find all the places where a 1x3 ship could be positioned
			List<ShipPosition> possibleShipLocations = FindAllShipLocations(lengthOfShip);
			/*
			for (int i = 0; i < possibleShipLocations.Count; i++)
			{
				log.Debug("Possible Ship Location:" + possibleShipLocations[i].X + "," + possibleShipLocations[i].Y+" ("+(possibleShipLocations[i].VerticalOrientation ? "Vertical" : "Horizontal")+")");
			}
			*/
			List<ShipCoord> allCoordinates = new List<ShipCoord>();

			// dump all the ship coordinates
			for (int i = 0; i < possibleShipLocations.Count; i++)
			{
				/*
				if (possibleShipLocations[i].VerticalOrientation)
				{
					log.Debug("Vertical");
				}
				else
				{
					log.Debug("Horizontal");
				}
				*/
				for (int j = 0; j < lengthOfShip; j++)
				{
					if (possibleShipLocations[i].Orientation == ShipDirection.VERTICAL)
					{
						allCoordinates.Add(new ShipCoord
						{
							X = possibleShipLocations[i].X,
							Y = possibleShipLocations[i].Y + j
						});

						//log.Debug(possibleShipLocations[i].X + "," + (possibleShipLocations[i].Y + j));
					}
					else
					{
						allCoordinates.Add(new ShipCoord
						{
							X = possibleShipLocations[i].X + j,
							Y = possibleShipLocations[i].Y
						});
						//log.Debug((possibleShipLocations[i].X + j) + "," + possibleShipLocations[i].Y);
					}
				}
			}

			if (allCoordinates.Count == 0)
			{
				return allCoordinates;
			}

			// sort list by x then y
			var sortedList = (from a in allCoordinates select a).OrderBy(x => x.X).ThenBy(y => y.Y).ToList();

			// dump the sorted list
			/*
			for (int i = 0; i < sortedList.Count; i++)
			{
				log.Debug(sortedList[i].X + "," + sortedList[i].Y);
			}

			log.Debug("");
			*/

			// build list of coords, by count of occurrences
			List<ShipCoord> countedList = new List<ShipCoord>();

			int prevx = sortedList[0].X;
			int prevy = sortedList[0].Y;
			int total = 0;
			for (int i = 0; i < sortedList.Count; i++)
			{
				if (sortedList[i].X != prevx || sortedList[i].Y != prevy)
				{
					countedList.Add(new ShipCoord
					{
						X = prevx,
						Y = prevy,
						Occurrences = total
					});

					//log.Debug(prevx + "," + prevy + " [" + total + "]");
					total = 1;
					prevx = sortedList[i].X;
					prevy = sortedList[i].Y;
				}
				else
				{
					total++;
				}
			}

			// need to account for the last one
			//log.Debug(prevx + "," + prevy + " [" + total + "]");

			countedList.Add(new ShipCoord
			{
				X = prevx,
				Y = prevy,
				Occurrences = total
			});

			// sort by occurrences
			return countedList.OrderByDescending(c => c.Occurrences).ToList();
		}

		// find all the possible vertical and horizontal ship positions available on the map with a ship length of lengthOfShip.
		private static List<ShipPosition> FindAllShipLocations(int lengthOfShip)
		{
			List<ShipPosition> result = new List<ShipPosition>();

			for (int y = 0; y < MaximumMapSize; y++)
			{
				for (int x = 0; x < MaximumMapSize; x++)
				{
					bool possiblePosition = true;

					// do all the verticle ships first.
					for (int i = 0; i < lengthOfShip; i++)
					{
						if (y + i >= MaximumMapSize)
						{
							possiblePosition = false;
							break;
						}

						if (map[x, y + i] == 1)
						{
							possiblePosition = false;
							break;
						}
					}

					if (possiblePosition)
					{
						ShipPosition shipPosition = new ShipPosition
						{
							X = x,
							Y = y,
							Orientation = ShipDirection.VERTICAL
						};

						result.Add(shipPosition);
					}

					possiblePosition = true;

					// do all the horizontal ships
					for (int i = 0; i < lengthOfShip; i++)
					{
						if (x + i >= MaximumMapSize)
						{
							possiblePosition = false;
							break;
						}

						if (map[x + i, y] == 1)
						{
							possiblePosition = false;
							break;
						}
					}

					if (possiblePosition)
					{
						ShipPosition shipPosition = new ShipPosition
						{
							X = x,
							Y = y,
							Orientation = ShipDirection.HORIZONTAL
						};

						result.Add(shipPosition);
					}
				}
			}

			return result;
		}
	}
}
