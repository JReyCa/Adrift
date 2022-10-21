using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ReyToolkit.Hex
{
    // A data structure that wraps a dictionary, where the key is hex cube coordinates and the value is generic.
    // This is meant to represent some chunk of a hexagonal grid map.
    public class HexMap<T>
    {
        // The wrapped dictionary
        protected Dictionary<HexCube, T> wrappedDict;
        protected readonly int width, height;

        // Accessors
        public T this[HexCube coords]
        {
            get { return wrappedDict[coords]; }
            set { wrappedDict[coords] = value; }
        }
        public T this[int x, int y]
        {
            get { return wrappedDict[HexCube.OffsetToCube(x, y)]; }
            set { wrappedDict[HexCube.OffsetToCube(x, y)] = value; }
        }
        public bool TryGetValue(HexCube coords, out T value) => wrappedDict.TryGetValue(coords, out value);
        public bool TryGetValue(int x, int y, out T value) => TryGetValue(HexCube.OffsetToCube(x, y), out value);

        // Constructor
        public HexMap(int width, int height, T defaultValue)
        {
            wrappedDict = new();
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    HexCube coords = HexCube.OffsetToCube(x, y);
                    wrappedDict.Add(coords, defaultValue);
                }
            }

            this.width = width;
            this.height = height;
        }

        // Get a list of the coordinates of neighbours of some position that match some predicate.
        public List<HexCube> FindNeighbours(HexCube coords, Func<T, bool> criterion)
        {
            List<HexCube> neighbourList = new();

            for (int i = 0; i < 6; i++)
            {
                HexCube nCoords = coords.Neighbour(i);

                if (!TryGetValue(nCoords, out T neighbour))
                    continue;

                if (criterion.Invoke(neighbour))
                    neighbourList.Add(nCoords);
            }

            return neighbourList;
        }

        public List<HexCube> FindNeighbours(int x, int y, Func<T, bool> criterion) => FindNeighbours(HexCube.OffsetToCube(x, y), criterion);

        // When the criterion accepts two input values, the first is the value of the reference position, and the second that of the neighbour.
        public List<HexCube> FindNeighbours(HexCube coords, Func<T, T, bool> criterion)
        {
            List<HexCube> neighbourList = new();

            for (int i = 0; i < 6; i++)
            {
                HexCube nCoords = coords.Neighbour(i);

                if (!TryGetValue(nCoords, out T neighbour))
                    continue;

                if (criterion.Invoke(wrappedDict[coords], neighbour))
                    neighbourList.Add(nCoords);
            }

            return neighbourList;
        }

        public List<HexCube> FindNeighbours(int x, int y, Func<T, T, bool> criterion) => FindNeighbours(HexCube.OffsetToCube(x, y), criterion);

        // Convert the hex map to a 2D array, where the indices represent offset coordinates.
        public T[,] ToRawMap()
        {
            T[,] rawMap = new T[width, height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    rawMap[x, y] = this[x, y];
                }
            }

            return rawMap;
        }
    }
}
