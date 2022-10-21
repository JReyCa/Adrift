using System.Collections.Generic;
using UnityEngine;

namespace ReyToolkit.Hex
{
    // Tools involving cube coordinates for a hexagonal grid
    public struct HexCube
    {
        // In order of clockwise rotation
        private static HexCube[] cubeDirections = new HexCube[]
        {
        new HexCube(1, 0, -1), new HexCube(1, -1, 0), new HexCube(0, -1, 1),
        new HexCube(-1, 0, 1), new HexCube(-1, 1, 0), new HexCube(0, 1, -1)
        };

        public enum Direction
        {
            UpRight = 0,
            Right = 1,
            DownRight = 2,
            DownLeft = 3,
            Left = 4,
            UpLeft = 5
        }

        public readonly int x, y, z;

        // Constructor
        public HexCube(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        // Return coordinates of the next hexagon in the given direction.
        // --------------------------------------------------------------
        // direction    => the direction in hexagonal cube space
        public HexCube Neighbour(Direction direction)
        {
            if ((int)direction < 0 || (int)direction > 5)
            {
                Debug.LogWarning("Direction index must be from 0 - 5. Defaulting to 0.");
                return cubeDirections[0];
            }

            return this + cubeDirections[(int)direction];
        }

        public HexCube Neighbour(int direction) => Neighbour((Direction)direction);

        // Get an array of coordinates in a ring around this position.
        // ----------------------------------------------------------------------------
        // stepsAway    => how many steps from the original hex that the ring is offset
        public HexCube[] Ring(int stepsAway)
        {
            if (stepsAway == 0)
                return new HexCube[] { this };

            if (stepsAway < 0)
            {
                Debug.LogWarning("The steps away for a ring must be at least one.");
                return null;
            }

            HexCube[] ring = new HexCube[stepsAway * 6];

            HexCube current = this;
            for (int i = 0; i < stepsAway; i++) // Get an initial position on the ring
                current = current.Neighbour(Direction.Left);

            for (int i = 0; i < 6; i++)         // Travel around the ring, adding each position
            {
                for (int j = 0; j < stepsAway; j++)
                {
                    current = current.Neighbour((Direction)i);
                    ring[i * stepsAway + j] = current;
                }
            }

            return ring;
        }

        // Just print out the x, y, and z values
        public override string ToString()
        {
            return "(" + x + ", " + y + ", " + z + ")";
        }

        // Two hexcubes are equal if they share all three integers
        public override bool Equals(object obj)
        {
            if (!(obj is HexCube otherCube))
                return false;

            return x == otherCube.x && y == otherCube.y && z == otherCube.z;
        }

        // The hash code combines hashes of the three integers
        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;
                hash = hash * 23 + x.GetHashCode();
                hash = hash * 23 + y.GetHashCode();
                hash = hash * 23 + z.GetHashCode();
                return hash;
            }
        }

        // Takes hexagon offset column and row coordinates and returns cube coordinates
        // the hexagons are assumed to have a corner pointing upwards, and indented every even row.
        public static HexCube OffsetToCube(int col, int row)
        {
            int x = col - (-row + (row & 1)) / 2;
            int z = -row;
            int y = -x - z;

            return new HexCube(x, y, z);
        }

        public static HexCube OffsetToCube(Vector2Int offsetCoords) => OffsetToCube(offsetCoords.x, offsetCoords.y);

        // Takes cube coordinates and returns offset coordinates
        public static Vector2Int CubeToOffset(HexCube cube)
        {
            int col = cube.x + (cube.z + (cube.z & 1)) / 2;
            int row = -cube.z;

            return new Vector2Int(col, row);
        }

        // Return the manhattan distance using cube coordinates
        public static int Distance(HexCube a, HexCube b)
        {
            int dx = Mathf.Abs(b.x - a.x);
            int dy = Mathf.Abs(b.y - a.y);
            int dz = Mathf.Abs(b.z - a.z);

            return (dx + dy + dz) / 2;
        }

        public static Dictionary<HexCube, T> InitializeMapDict<T>(int width, int height, T defaultValue)
        {
            Dictionary<HexCube, T> dict = new();

            for (int j = 0; j < height; j++)
            {
                for (int i = 0; i < width; i++)
                {
                    HexCube hexCube = OffsetToCube(i, j);
                    dict.Add(hexCube, defaultValue);
                }
            }

            return dict;
        }

        // Operator overrides
        public static bool operator ==(HexCube a, HexCube b)
            => a.Equals(b);

        public static bool operator !=(HexCube a, HexCube b)
            => !a.Equals(b);

        public static HexCube operator +(HexCube a, HexCube b)
            => new(a.x + b.x, a.y + b.y, a.z + b.z);

        public static HexCube operator -(HexCube a, HexCube b)
            => new(a.x - b.x, a.y - b.y, a.z - b.z);
    }

}
