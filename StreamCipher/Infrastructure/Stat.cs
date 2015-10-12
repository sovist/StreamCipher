using System;
using System.Linq;
using System.Threading.Tasks;

namespace StreamCipher.Infrastructure
{
    static public class Stat
    {
        public static double CorrelationCoefficient(byte[] arr)
        {
            double len = arr.Length - 1,
                half = len / 2.0,
                sumXmulY = 0,
                sumX2 = 0,
                sumY2 = 0;

            for (int i = 0; i < arr.Length; i++)
            {
                double x = (arr[i] - half)/len,
                    y = (i - half)/len;
                sumXmulY += x * y;
                sumX2 += x * x;
                sumY2 += y * y;
            }

            return sumXmulY / Math.Sqrt(sumX2 * sumY2);
        }
        public static double CorrelationCoefficient(ushort[] arr)
        {
            double len = arr.Length - 1,
                half = len / 2.0,
                sumXmulY = 0,
                sumX2 = 0,
                sumY2 = 0;

            for (int i = 0; i < arr.Length; i++)
            {
                double x = (arr[i] - half)/len,
                    y = (i - half)/len;
                sumXmulY += x*y;
                sumX2 += x*x;
                sumY2 += y*y;
            };

            return sumXmulY / Math.Sqrt(sumX2 * sumY2);
        }

        public static double StandardDeviation(ulong[] array)
        {
            double n = array.Sum(arg => (double)arg), m = n / array.Length, sigma = array.Sum(t => Math.Pow(t - m, 2));
            return Math.Sqrt(sigma / n);
        }
        /*
        public static double[] AutocorrelationFunction(ushort[] arr)
        {
            double[] temp = new double[arr.Length];
            double[] akfArr = new double[arr.Length];

            double n = (arr.Length - 1) / 2.0;
            for (int i = 0; i < arr.Length; i++)
                temp[i] = arr[i] - n;

            Parallel.For(0, arr.Length, i =>
            {
                for (int j = i, k = 0; j < arr.Length; j++, k++)
                    akfArr[i] += temp[k] * temp[j];
            });

            for (int i = arr.Length - 1; i >= 0; i--)
                akfArr[i] /= akfArr[0];

            return akfArr;
        }*/
        public static double[] AutocorrelationFunction1(byte[] arr)
        {
            double[] temp = new double[arr.Length];
            double[] akfArr = new double[arr.Length];

            double n = (arr.Length - 1)/2.0;
            for (int i = 0; i < arr.Length; i++)
                temp[i] = arr[i] < n ? -1 : 1;

            Parallel.For(0, arr.Length, i =>
            {
                for (int j = i, k = 0; j < arr.Length; j++, k++)
                    akfArr[i] += temp[k] * temp[j];
            });

            for (int i = arr.Length - 1; i >= 0; i--)
                akfArr[i] /= akfArr[0];

            return akfArr;
        }
        public static double[] AutocorrelationFunction1(ushort[] arr)
        {
            double[] temp = new double[arr.Length];
            double[] akfArr = new double[arr.Length];
            double n = (arr.Length - 1) / 2.0;

            for (int i = 0; i < arr.Length; i++)
                temp[i] = arr[i] < n ? -1 : 1;

            Parallel.For(0, temp.Length, i =>
            {
                for (int j = i, k = 0; j < temp.Length; j++, k++)
                    akfArr[i] += temp[k] * temp[j];
            });

            for (int i = akfArr.Length - 1; i >= 0; i--)
                akfArr[i] /= akfArr[0];

            return akfArr;
        }
        /// <summary>
        /// </summary>
        /// <exception cref="ArgumentException"></exception>>
        /// <param name="arr"> array length can be only [16,32,64,128,256] </param>
        /// <returns>value of sigma</returns>
        public static double Sigma(byte[] arr)
        {
            int gridInterval = 0;
            switch (arr.Length)
            {
                case 16: gridInterval = 8; break;
                case 32: gridInterval = 8; break;
                case 64: gridInterval = 16; break;
                case 128: gridInterval = 16; break;
                case 256: gridInterval = 32; break;
            }

            if(gridInterval == 0)
                throw new ArgumentException("array length can be only [16,32,64,128,256]");

            int gridDimension = arr.Length/gridInterval;
            int pointPerBlock = arr.Length/(gridDimension*gridDimension);

            return Sigma256(arr, gridDimension, gridInterval, pointPerBlock);
        }

        public static double Sigma256(byte[] arr, int gridDimension = 8, int gridInterval = 32, int pointPerBlock = 4)
        {
            int[,] countPointPerBlock = new int[gridDimension, gridDimension];

            for (int i = 0; i < arr.Length; i++)
                countPointPerBlock[arr[i] / gridInterval, i / gridInterval]++;

            double rez = 0;
            for (int i = 0; i < gridDimension; i++)
                for (int j = 0; j < gridDimension; j++)
                    rez += Math.Pow(countPointPerBlock[i, j] - pointPerBlock, 2);
            return Math.Sqrt(rez / gridDimension);
        }

        public static double Sigma(ushort[] arr)
        {
            int gridInterval = 0;
            switch (arr.Length)
            {
                case 16: gridInterval = 8; break;
                case 32: gridInterval = 8; break;
                case 64: gridInterval = 16; break;
                case 128: gridInterval = 16; break;
                case 256: gridInterval = 32; break;
                case 65536: gridInterval = 8192; break;
            }

            if (gridInterval == 0)
                throw new ArgumentException("array length can be only [16,32,64,128,256,65536]");

            int gridDimension = arr.Length / gridInterval;
            int pointPerBlock = arr.Length / (gridDimension * gridDimension);

            int[,] countPointPerBlock = new int[gridDimension, gridDimension];

            for (int i = 0; i < arr.Length; i++)
                countPointPerBlock[arr[i] / gridInterval, i / gridInterval]++;

            double rez = 0;
            for (int i = 0; i < gridDimension; i++)
                for (int j = 0; j < gridDimension; j++)
                    rez += Math.Pow(countPointPerBlock[i, j] - pointPerBlock, 2);
            return Math.Sqrt(rez / gridDimension);
        }
    }
}
