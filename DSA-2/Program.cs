namespace DSA_2
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Usage: DSA_2 <input_file> <target_user>");
                return;
            }
            
            if (!File.Exists(args[0]))
            {
                Console.WriteLine($"Error: File {args[0]} does not exist.");
                return;
            }
            
            string inputFile = args[0];
            string outputFile = Path.GetFileNameWithoutExtension
                (System.Reflection.Assembly.GetEntryAssembly()?.Location) + "_output.txt";
            
            var lines = File.ReadAllLines(inputFile);
            var header = lines[0].Split().Select(int.Parse).ToArray();

            int userCount = header[0];
            int movieCount = header[1];

            int[][] preferences = new int[userCount][];
            if (preferences == null) throw new ArgumentNullException(nameof(preferences));

            if (lines.Length < userCount + 1)
            {
                Console.WriteLine($"Error: Expected {userCount} user rows but got only {lines.Length - 1}");
                return;
            }
            
            for (int i = 0; i < userCount; i++)
            {
                var lineParts = lines[i + 1].Trim().Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (lineParts.Length != movieCount + 1)
                {
                    Console.WriteLine($"Invalid data at line {i + 2}: expected {movieCount + 1} values, got {lineParts.Length}");
                    return;
                }
                var parts = lineParts.Skip(1).Select(int.Parse).ToArray();
                preferences[i] = parts;
            }

            int targetUser = int.Parse(args[1]) - 1;
            
            if (targetUser < 0 || targetUser >= userCount)
            {
                Console.WriteLine($"Invalid target user index: {targetUser + 1}");
                return;
            }

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