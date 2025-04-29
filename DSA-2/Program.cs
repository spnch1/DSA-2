namespace DSA_2
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: DSA2 <input_file>");
                return;
            }

            string inputFile = args[0];
            string outputFile = Path.GetFileNameWithoutExtension
                (System.Reflection.Assembly.GetEntryAssembly()?.Location) + "_output.txt";

            try
            {
                var lines = File.ReadAllLines(inputFile);
                var header = lines[0].Split().Select(int.Parse).ToArray();

                int userCount = header[0];

                int[][] preferences = new int[userCount][];
                if (preferences == null) throw new ArgumentNullException(nameof(preferences));

                for (int i = 0; i < userCount; i++)
                {
                    var parts = lines[i + 1].Split().Select(int.Parse).ToArray();
                    preferences[i] = parts;
                }

                int targetUser = int.Parse(lines[0].Split()[2]) - 1;

                var results = new List<(int userId, int inversion)>();

                for (int i = 0; i < userCount; i++)
                {
                    if (i == targetUser)
                        continue;

                    var comparisonArray = MapPreferences
                        (preferences[targetUser], preferences[i]);
                    int inversions = CountInversions(comparisonArray);
                    results.Add((i + 1, inversions));
                }

                results.Sort((a, b) => a.inversion.CompareTo(b.inversion));
                using (var writer = new StreamWriter(outputFile))
                {
                    foreach (var (userId, inversions) in results)
                    {
                        writer.WriteLine($"{userId} {inversions}");
                    }
                }
            }
            catch
            {
                Console.WriteLine("Error");
            }
        }

        static int[] MapPreferences(int[] refList, int[] compList)
        {
            int[] indexMap = new int[refList.Length];
            for (int i = 0; i < compList.Length; i++)
            {
                indexMap[compList[i] - 1] = i;
            }
            
            int[] result = new int[refList.Length];
            for (int i = 0; i < refList.Length; i++)
            {
                result[i] = indexMap[refList[i] - 1];
            }

            return result;
        }
        
        static int CountInversions(int[] arr)
        {
            int[] temp = new int[arr.Length];
            return MergeSort(arr, temp, 0, arr.Length - 1);
        }

        static int MergeSort(int[] arr, int[] temp, int left, int right)
        {
            int invCount = 0;
            if (right > left)
            {
                int mid = (right + left) / 2;

                invCount += MergeSort(arr, temp, left, mid);
                invCount += MergeSort(arr, temp, mid + 1, right);
                invCount += Merge(arr, temp, left, mid + 1, right);
            }
            return invCount;
        }
        static int Merge(int[] arr, int[] temp, int left, int mid, int right)
        {
            int i = left;
            int j = mid;
            int k = left;
            int invCount = 0;

            while (i <= mid - 1 && j <= right)
            {
                if (arr[i] <= arr[j])
                    temp[k++] = arr[i++];
                else
                {
                    temp[k++] = arr[j++];
                    invCount += (mid - i);
                }
            }
            
            while (i <= mid - 1)
                temp[k++] = arr[i++];
            
            while (j <= right)
                temp[k++] = arr[j++];

            for (i = left; i <= right; i++)
                arr[i] = temp[i];

            return invCount;
        }
    }
}