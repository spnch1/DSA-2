namespace DSA_2
{
    class Program
    {
        static void Main(string[] args)
        {
            bool debugMode = false;
            string inputFile = string.Empty;
            string targetUserArg = string.Empty;
            int argIndex = 0;
            
            if (args.Length > 0 && (args[0] == "--debug" || args[0] == "-d"))
            {
                debugMode = true;
                argIndex = 1;
            }
            
            if (args.Length > argIndex)
                inputFile = args[argIndex++];
                
            if (args.Length > argIndex)
                targetUserArg = args[argIndex];
            
            if (string.IsNullOrEmpty(inputFile) || string.IsNullOrEmpty(targetUserArg) || !debugMode && args.Length != 2)
            {
                Console.WriteLine("Usage: DSA-2 [--debug | -d] <input_file> <target_user>");
                return;
            }
            
            if (!File.Exists(inputFile))
            {
                Console.WriteLine($"Error: File {inputFile} does not exist.");
                return;
            }
            
            string inputFileBaseName = Path.GetFileNameWithoutExtension(inputFile);
            string outputFile = Path.GetFileNameWithoutExtension
                (System.Reflection.Assembly.GetEntryAssembly()?.Location) + "_" + inputFileBaseName + "_output.txt";
            
            var lines = File.ReadAllLines(inputFile);
            var header = lines[0].Split().Select(int.Parse).ToArray();

            int userCount = header[0];
            int movieCount = header[1];

            int[][] preferences = new int[userCount][];
            if (preferences == null)
                throw new ArgumentNullException(nameof(preferences));

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

            int targetUser = int.Parse(targetUserArg) - 1;
            
            if (targetUser < 0 || targetUser >= userCount)
            {
                Console.WriteLine($"Invalid target user index: {targetUser + 1}");
                return;
            }

            var results = new List<(int userId, int inversion)>();

            for (int i = 0; i < userCount; i++)
            {
                if (i == targetUser) continue;
                
                var comparisonArray = MapPreferences(
                    preferences[targetUser],
                    preferences[i]
                );

                if (debugMode)
                    Console.WriteLine(
                        $"DEBUG: mapping for {targetUser + 1} vs {i + 1} -> " +
                        $"[{string.Join(",", comparisonArray)}]"
                    );
                
                int inversions = CountInversions(comparisonArray);
                
                if (debugMode)
                {
                    Console.WriteLine($"DEBUG: inversions for {targetUser + 1} vs {i + 1} = {inversions}");
                }

                results.Add((i + 1, inversions));
            }

            results.Sort((a, b) =>
            {
                int cmp = a.inversion.CompareTo(b.inversion);
                return (cmp != 0) ? cmp : a.userId.CompareTo(b.userId);
            });
            
            using var writer = new StreamWriter(outputFile);
            writer.WriteLine(targetUser + 1);

            foreach (var (userId, inversions) in results)
                writer.WriteLine($"{userId} {inversions}");
        }

        static int[] MapPreferences(int[] refList, int[] compList)
        {
            int M = refList.Length;
    
            int[] refIndices = Enumerable.Range(0, M).ToArray();
            int[] compIndices = Enumerable.Range(0, M).ToArray();
            
            Array.Sort(refIndices, (a, b) => {
                int cmp = refList[b].CompareTo(refList[a]);
                return cmp != 0 ? cmp : a.CompareTo(b);
            });
    
            Array.Sort(compIndices, (a, b) => {
                int cmp = compList[b].CompareTo(compList[a]);
                return cmp != 0 ? cmp : a.CompareTo(b);
            });
            
            var compRanks = new Dictionary<int, int>();
            for (int rank = 0; rank < compIndices.Length; rank++)
                compRanks[compIndices[rank]] = rank;
    
            int[] result = new int[M];
            for (int i = 0; i < refIndices.Length; i++)
                result[i] = compRanks[refIndices[i]];
    
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