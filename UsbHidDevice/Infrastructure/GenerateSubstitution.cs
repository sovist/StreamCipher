using System;
using System.Linq;
using System.Windows;

namespace UsbHidDevice.Infrastructure
{
    static class GenerateSubstitution
    {
        public static bool Generate256(out byte[] arr, ref int randIndex, double sigma, double r)
        {
            arr = new byte[256];
            if (sigma != 0)
            {
                if (!generateSub(ref arr, ref randIndex, sigma, r))
                    return false;
            }
            else
            {
                if (!generateSubSigmaNull(ref arr, ref randIndex, r))
                    return false;
            }
            return true;
        }

        private static bool generateSub(ref byte[] arr, ref int randIndex, double sigma, double r)
        {
            DateTime dateTime = DateTime.Now + new TimeSpan(0, 1, 0);
            double calcSigma, calcR;
            do
            {
                if (dateTime < DateTime.Now)
                {
                    var dialogResult = MessageBox.Show("Можливо введені параметри є досить заниженими. \tПродовжити?", "", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (dialogResult == MessageBoxResult.No)
                        return false;
                    dateTime = DateTime.Now + new TimeSpan(0, 1, 0);
                }
                randIndex++;
                arr = new byte[256];
                GenerateForwarSub(ref arr, new Random(randIndex));
                calcSigma = Stat.Sigma256(arr);
                calcR = Math.Abs(Stat.CorrelationCoefficient(arr));
            } while (!(calcSigma <= sigma && calcR <= r));

            return true;
        }

        private static bool generateSubSigmaNull(ref byte[] arr, ref int randIndex, double r)
        {
            DateTime dateTime = DateTime.Now + new TimeSpan(0, 1, 0);
            double calcR;
            do
            {
                if (dateTime < DateTime.Now)
                {
                    var dialogResult = MessageBox.Show("Можливо введені параметри є досить заниженими. \tПродовжити?", "", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (dialogResult == MessageBoxResult.No)
                        return false;
                    dateTime = DateTime.Now + new TimeSpan(0, 1, 0);
                }
                randIndex++;
                arr = genSubSigmaNull256(new Random(randIndex));
                calcR = Math.Abs(Stat.CorrelationCoefficient(arr));
            } while (!(calcR <= r));

            return true;
        }

        private static byte[] genSubSigmaNull256(Random random)
        {
            const int gridDimension = 8, gridInterval = 32, pointPerBlock = 4;

            byte[] arr = new byte[256];
            int[,] countPointPerBlock = new int[gridDimension, gridDimension];

            int len = arr.Length - 1;
            int zeroIndex = (int)((arr.Length - 2) * random.NextDouble());
            int lastVal = ((int)((arr.Length - 3) * random.NextDouble())) + 1;
            arr[len] = (byte)lastVal;

            countPointPerBlock[arr[zeroIndex] / gridInterval, zeroIndex / gridInterval]++;
            countPointPerBlock[arr[len] / gridInterval, len / gridInterval]++;

            for (int j = 0, k = 0; j < arr.Length - 1; k++)
            {
                int rndIndex = (int)(len * random.NextDouble());
                if (rndIndex != zeroIndex)
                    if (arr[rndIndex] == 0 && countPointPerBlock[j / gridInterval, rndIndex / gridInterval] < pointPerBlock)
                    {
                        if (j == lastVal)
                            continue;

                        arr[rndIndex] = (byte)j;
                        countPointPerBlock[j / gridInterval, rndIndex / gridInterval]++;
                        k = 0;
                        j++;
                    }

                if (k > 200) j++;
            }

            //індекси значень яких не вистачає
            int zeroCount = arr.Count(t => t == 0) - 1;
            int[] zeroIndexes = new int[zeroCount];
            for (int i = 0, j = 0; i < arr.Length; i++)
                if (arr[i] == 0 && i != zeroIndex)
                    zeroIndexes[j++] = i;

            //значень яких не вистачає
            int[] val = new int[zeroCount];
            for (int j = 0, m = 0; j < arr.Length; j++)
                if (arr.All(t => t != j))
                    val[m++] = j;

            //розподіл значень за правилом
            for (int i = 0; i < zeroIndexes.Length; i++)
                for (int j = 0; j < zeroIndexes.Length; j++)
                    if (countPointPerBlock[val[j] / gridInterval, zeroIndexes[i] / gridInterval] < pointPerBlock && val[j] != -1)
                    {
                        arr[zeroIndexes[i]] = (byte)val[j];
                        countPointPerBlock[val[j] / gridInterval, zeroIndexes[i] / gridInterval]++;
                        val[j] = -1;
                        zeroIndexes[i] = -1;
                        break;
                    }

            //перевірка чи всі значення розставлено
            bool stopFlag = false;
            for (int i = 0; i < zeroIndexes.Length && !stopFlag; i++)
                if (zeroIndexes[i] != -1)
                    for (int j = 0; j < val.Length; j++)
                        if (val[j] != -1)
                        {
                            arr[zeroIndexes[i]] = (byte)val[j];
                            val[j] = -1;
                            stopFlag = val.All(t => t == -1);//якщо значень не лишилося
                            break;
                        }

            return arr;
        }

        public static void GenerateForwarSub<TSub>(ref TSub[] arr, Random random) where TSub : struct, IComparable<TSub>
        {
            int len = arr.Length - 1;
            int zeroIndex = (int)((arr.Length - 2) * random.NextDouble());
            int lastVal = ((int)((arr.Length - 3) * random.NextDouble())) + 1;
            arr[len] = (TSub)Convert.ChangeType(lastVal, typeof(TSub));
            TSub tableElement0 = new TSub();

            for (int j = 0; j < arr.Length - 1; )
            {
                int rndIndex = (int)(len * random.NextDouble());
                if (rndIndex != zeroIndex)
                    if (arr[rndIndex].CompareTo(tableElement0) == 0)
                    {
                        j++;
                        if (j == lastVal)
                            continue;
                        arr[rndIndex] = (TSub)Convert.ChangeType(j, typeof(TSub));
                    }
            }
        }

        public static void GenerateForwarSub<TSub>(ref TSub[] arr) where TSub : struct, IComparable<TSub>
        {
            GenerateForwarSub(ref arr, new Random());
        }

        public static void GenerateBackSub<TSub>(ref TSub[] arrBack, TSub[] arrForward) where TSub : struct, IComparable<TSub>
        {
            for (int j = 0; j < arrBack.Length; j++)
                arrBack[(int)Convert.ChangeType(arrForward[j], typeof(int))] = (TSub)Convert.ChangeType(j, typeof(TSub));
        }
    }
}
