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
                if (i == targetUser) continue;
                
                var comparisonArray = MapPreferences(
                    preferences[targetUser],
                    preferences[i]
                );

                Console.WriteLine(
                    $"DEBUG: mapping for {targetUser + 1} vs {i + 1} -> " +
                    $"[{string.Join(",", comparisonArray)}]"
                );
                
                int inversions = CountInversions(comparisonArray);
                Console.WriteLine($"DEBUG: inversions for {targetUser + 1} vs {i + 1} = {inversions}");

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
            Console.WriteLine("DEBUG: in updated MapPreferences");
            int M = refList.Length;
            var position = new Dictionary<int,int>(M);
            for (int j = 0; j < M; j++)
                position[compList[j]] = j;

            int[] result = new int[M];
            for (int i = 0; i < M; i++)
                result[i] = position[refList[i]];

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