using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using Console = Colorful.Console;

class MainClass
{
    static bool shouldInsert;
    static bool shouldSwitch;
    static bool moveDoubleExtensionFiles;
    static bool recursiveSearch;
    static string objectName;
    static string[] specificExtensions;

    static string[] files;
    static List<string> filesList;
    public static void Main(string[] args)
    {
        if (args.Length > 0)
        {
            CLI(args);
            return;
        }
        Console.WriteLine("---[GMR'S AUTO RESEQUENCER v1.0.0]---\n", Color.Red);
        Console.WriteLine("DO NOT RE-FOCUS ANY OTHER PROGRAM'S WINDOW UNTIL OPERATIONS ARE COMPLETE.", Color.Aquamarine);
        Console.WriteLine("Choose an option.", Color.CornflowerBlue);
        Console.WriteLine("1. Insert", Color.LightSeaGreen);
        Console.WriteLine("2. Switch", Color.LightSeaGreen);
        int.TryParse(Console.ReadLine(), out int choice);
        switch (choice)
        {
            case 1:
                shouldInsert = true;
                break;
            case 2:
                shouldSwitch = true;
                break;
            default:
                Console.WriteLine("That's not an option. Please try again.", Color.Red);
                Console.ReadKey();
                return;
        }
        Console.WriteLine("Please input the file name to manipulate without the extension or number.", Color.CornflowerBlue);
        Console.WriteLine("LETTERS AND UNDERSCORES ONLY. CASE INSENSITIVE.", Color.Aquamarine);
        objectName = Console.ReadLine().Trim().ToLower();

        #region Meta files
        Console.WriteLine("Move double extension files (like .png.meta files)?", Color.CornflowerBlue);
        Console.ResetColor();
        Console.WriteLine("y/n", Color.LightGreen);
        char.TryParse(Console.ReadLine(), out char moveMetaFilesChoice);
        moveDoubleExtensionFiles = moveMetaFilesChoice == 'y';
        #endregion

        #region Extensions
        Console.WriteLine("Move a specific extension? Hit enter to move all extensions. Seperate multiple extensions with a comma (jpg,png,obj,ai, etc.).", Color.CornflowerBlue);
        string inputExtensions = Console.ReadLine();
        specificExtensions = SpecificExtensions(inputExtensions);
        #endregion

        #region Recursive file searching
        Console.WriteLine("Recursively search for files to move? This will move files in folders in the program's directory as well.", Color.CornflowerBlue);
        Console.WriteLine("y/n", Color.LightGreen);
        char.TryParse(Console.ReadLine(), out char recursiveSearchChoice);
        recursiveSearch = recursiveSearchChoice == 'y';
        #endregion

        GetFiles();

        if (shouldInsert)
        {
            Console.WriteLine("Which number do you want to move?", Color.CornflowerBlue);
            int.TryParse(Console.ReadLine(), out int numToMove);

            Console.WriteLine("At what number do you want to insert it?", Color.CornflowerBlue);
            int.TryParse(Console.ReadLine(), out int toMoveTo);

            Insert(numToMove, toMoveTo);

            Console.WriteLine("Success.", Color.Green);
        }
        if (shouldSwitch)
        {
            Console.WriteLine("Which number do you want to switch?", Color.CornflowerBlue);
            int.TryParse(Console.ReadLine(), out int numToMove);

            Console.WriteLine("What number do you want to switch it with?", Color.CornflowerBlue);
            int.TryParse(Console.ReadLine(), out int toMoveTo);

            Switch(numToMove, toMoveTo);

            Console.WriteLine("Success.", Color.Green);
        }
        Thread.Sleep(2500);
    }

    public static void CLI(string[] args)
    {
        //-insert ball 0 4 [moveDoubleExtensionFiles] [specificExtensions] [recursiveSearch]
        bool noCommandRecognized = true;

        string[] help = { "-help", "--help", "-h", "--h" };
        if (help.Contains(args[0], StringComparer.OrdinalIgnoreCase))
        {
            Console.WriteLine("HELP", Color.Red);
            //TODO: Refactor the help text.
            //https://stackoverflow.com/questions/9725675/is-there-a-standard-format-for-command-line-shell-help-text
            Console.WriteLine("-insert = Runs the \"insert\" function. Usage: -insert [0] [1] [2] [3], where [0] is the prefix of the files you want to work with (string, set to \"nn\" (case-insensitive)  for no name and just numbered files [temporary hack]), [1] is the number of the file(s) to insert (int), and [2] is the number to insert it into (int). [3] is optional, and is a boolean (y/n) that defaults to false (n). It is used to denote whether to move additional extension files like \"file.png.meta\". If yes or true, will move files with additional extensions like that. If no or false, it will leave them alone.\n");
            Console.WriteLine("\n\n");
            Console.WriteLine("-switch = Runs the \"switch\" function. Usage: -switch [0] [1] [2] [3], where [0] is the prefix of the files you want to work with (string, set to \"nn\" (case-insensitive)  for no name and just numbered files [temporary hack]), [1] is the first number of the files to switch (int), and [2] is the second number to switch the files with (int). [3] is optional, and is a boolean (y/n) that defaults to false (n). It is used to denote whether to move additional extension files like \"file.png.meta\". If yes or true, will move files with additional extensions like that. If no or false, it will leave them alone.\n");
            Console.WriteLine("\n\n");
            Console.WriteLine("-help | --help | -h | --h = This command. Scroll up for help.\n");
            return;
        }

        string[] commands = { "insert", "switch" };
        foreach (string command in commands)
        {
            if (args[0].ToLower().Trim().Equals(command))
                noCommandRecognized = false;
        }
        if (noCommandRecognized)
        {
            Console.WriteLine("Hm. That command wasn't recognized. Try running -help?");
            Console.WriteLine("Exiting...");
            Console.ReadKey();
            return;
        }

        //CLI variables
        //Default values for optional variables.
        moveDoubleExtensionFiles = false;
        specificExtensions = null;
        recursiveSearch = false;

        //Input from args.
        //Set to empty object name if "nn".
        objectName = args[1].Trim().ToLower() != "nn" ?  args[1].Trim().ToLower() : string.Empty;
        int.TryParse(args[2], out int numToMove);
        int.TryParse(args[3], out int toMoveTo);
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i].ToLower().Equals("-movedoubleextensionfiles"))
                moveDoubleExtensionFiles = true;

            if (args[i].ToLower().Equals("-specificextensions"))
                specificExtensions = SpecificExtensions(args[i + 1]);

            if (args[i].ToLower().Equals("-recursivesearch"))
                recursiveSearch = true;
        }

        //Get files.
        GetFiles();

        switch (args[0].ToLower())
        {
            case "insert":
                Insert(numToMove, toMoveTo);
                break;

            case "switch":
                Switch(numToMove, toMoveTo);
                break;
        }
    }

    public static string[] SpecificExtensions(string input)
    {
        specificExtensions = input.Length > 0 ? input.ToLower().Replace(" ", "").Split(',') : null;
        if (specificExtensions != null)
        {
            for (int x = 0; x < specificExtensions.Length; x++)
            {
                specificExtensions[x] = "." + specificExtensions[x];
            }
        }
        return specificExtensions;
    }

    public static void GetFiles()
    {
        if (specificExtensions == null)
        {
            files = recursiveSearch ? Directory.GetFiles(AssemblyDirectory, "*.*", SearchOption.AllDirectories) : Directory.GetFiles(AssemblyDirectory, "*.*", SearchOption.TopDirectoryOnly);
        }
        else
        {
            if (moveDoubleExtensionFiles)
            {
                string[] filesFirst = recursiveSearch ? Directory.GetFiles(AssemblyDirectory, "*.*", SearchOption.AllDirectories)
                .Where(file => specificExtensions
                .Contains(Path.GetExtension(file))).ToArray() : Directory.GetFiles(AssemblyDirectory, "*.*", SearchOption.TopDirectoryOnly)
                .Where(file => specificExtensions
                .Contains(Path.GetExtension(file))).ToArray();
                string[] filesSecond = recursiveSearch ? Directory.GetFiles(AssemblyDirectory, "*.*", SearchOption.AllDirectories)
                .Where(file => specificExtensions
                .Contains(Path.GetExtension(Path.GetFileNameWithoutExtension(file)))).ToArray() : Directory.GetFiles(AssemblyDirectory, "*.*", SearchOption.TopDirectoryOnly)
                .Where(file => specificExtensions
                .Contains(Path.GetExtension(Path.GetFileNameWithoutExtension(file)))).ToArray();
                files = filesFirst.Concat(filesSecond).ToArray();
            }
            else
            {
                files = recursiveSearch ? Directory.GetFiles(AssemblyDirectory, "*.*", SearchOption.AllDirectories)
               .Where(file => specificExtensions
               .Contains(Path.GetExtension(file))).ToArray() : Directory.GetFiles(AssemblyDirectory, "*.*", SearchOption.TopDirectoryOnly)
               .Where(file => specificExtensions
               .Contains(Path.GetExtension(file))).ToArray();
            }
        }
        //filesList = moveMetaFiles ? (from file in files where RemoveNonLetters(Path.GetFileNameWithoutExtension(file).Split('.')[0]).ToLower().Equals(objectName) select Path.GetFileName(file)).ToList() : (from file in files where RemoveNonLetters(Path.GetFileNameWithoutExtension(file)).ToLower().Equals(objectName) select Path.GetFileName(file)).ToList();
        filesList = moveDoubleExtensionFiles ? (from file in files where RemoveNonLetters(Path.GetFileNameWithoutExtension(file).Split('.')[0]).ToLower().Equals(objectName) select file).ToList() : (from file in files where RemoveNonLetters(Path.GetFileNameWithoutExtension(file)).ToLower().Equals(objectName) select file).ToList();
        filesList = filesList.OrderByNumber().ToList();
    }

    public static void Insert(int numToMove, int toMoveTo)
    {
        List<string> fileToInsert = new List<string>();
        List<string> filesGettingBumped = new List<string>();
        List<string> filesGettingDinked = new List<string>();
        bool gettingBumped = false;
        bool gettingDinked = false;
        foreach (string file in filesList)
        {
            if (RemoveNonNumbers(file).Equals(numToMove.ToString()))
            {
                fileToInsert.Add(file);
            }
            if (int.Parse(RemoveNonNumbers(file)) >= toMoveTo && int.Parse(RemoveNonNumbers(file)) < numToMove)
            {
                filesGettingBumped.Add(file);
                gettingBumped = true;
            }
            if (int.Parse(RemoveNonNumbers(file)) <= toMoveTo && int.Parse(RemoveNonNumbers(file)) > numToMove)
            {
                filesGettingDinked.Add(file);
                gettingDinked = true;
            }
        }
        foreach (string s in fileToInsert)
        {
            File.Move(Path.GetDirectoryName(s) + "\\" + Path.GetFileName(s), Path.GetDirectoryName(s) + "\\" + Path.GetFileName(s) + ".progress");
        }

        if (gettingBumped)
        {
            foreach (string s in filesGettingBumped)
            {
                File.Move(Path.GetDirectoryName(s) + "\\" + Path.GetFileName(s), Path.GetDirectoryName(s) + "\\" + Path.GetFileName(s) + ".progress");
            }

            foreach (string s in filesGettingBumped)
            {
                int num = int.Parse(RemoveNonNumbers(s));
                int bumped = num + 1;
                File.Move(Path.GetDirectoryName(s) + "\\" + Path.GetFileName(s) + ".progress", Path.GetDirectoryName(s) + "\\" + Path.GetFileName(s).Replace(num.ToString(), bumped.ToString()));
            }
        }
        if (gettingDinked)
        {
            foreach (string s in filesGettingDinked)
            {
                File.Move(Path.GetDirectoryName(s) + "\\" + Path.GetFileName(s), Path.GetDirectoryName(s) + "\\" + Path.GetFileName(s) + ".progress");
            }

            foreach (string s in filesGettingDinked)
            {
                int num = int.Parse(RemoveNonNumbers(s));
                int bumped = num - 1;
                File.Move(Path.GetDirectoryName(s) + "\\" + Path.GetFileName(s) + ".progress", Path.GetDirectoryName(s) + "\\" + Path.GetFileName(s).Replace(num.ToString(), bumped.ToString()));
            }
        }

        //TODO: 99.9% sure I can ditch the else.
        if (recursiveSearch)
        {
            foreach (string progressFile in Directory.GetFiles(AssemblyDirectory, "*.progress", SearchOption.AllDirectories))
            {
                File.Move(progressFile, Path.GetDirectoryName(progressFile) + "\\" + Path.GetFileNameWithoutExtension(progressFile).Replace(numToMove.ToString(), toMoveTo.ToString()));
            }
        }
        else
        {
            foreach (string progressFile in Directory.GetFiles(AssemblyDirectory, "*.progress", SearchOption.TopDirectoryOnly))
            {
                File.Move(progressFile, AssemblyDirectory + "\\" + Path.GetFileNameWithoutExtension(progressFile).Replace(numToMove.ToString(), toMoveTo.ToString()));
            }
        }
    }

    public static void Switch(int numToMove, int toMoveTo)
    {
        List<string> filesIncoming = new List<string>();
        List<string> filesGettingReplaced = new List<string>();

        foreach (string file in filesList)
        {
            if (RemoveNonNumbers(file).Equals(numToMove.ToString()))
            {
                filesIncoming.Add(file);
            }
            if (RemoveNonNumbers(file).Equals(toMoveTo.ToString()))
            {
                filesGettingReplaced.Add(file);
            }
        }

        foreach (string file in filesGettingReplaced)
        {
            File.Move(Path.GetDirectoryName(file) + "\\" + Path.GetFileName(file), Path.GetDirectoryName(file) + "\\" + Path.GetFileName(file) + ".progress");
        }

        foreach (string file in filesIncoming)
        {
            File.Move(Path.GetDirectoryName(file) + "\\" + Path.GetFileName(file), Path.GetDirectoryName(file) + "\\" + Path.GetFileName(file).Replace(numToMove.ToString(), toMoveTo.ToString()));
        }

        //TODO: 99.9% sure I can ditch the else.
        if (recursiveSearch)
        {
            foreach (string progressFile in Directory.GetFiles(AssemblyDirectory, "*.progress", SearchOption.AllDirectories))
            {
                File.Move(progressFile, Path.GetDirectoryName(progressFile) + "\\" + Path.GetFileNameWithoutExtension(progressFile).Replace(toMoveTo.ToString(), numToMove.ToString()));
            }
        }
        else
        {
            foreach (string progressFile in Directory.GetFiles(AssemblyDirectory, "*.progress", SearchOption.TopDirectoryOnly))
            {
                File.Move(progressFile, AssemblyDirectory + "\\" + Path.GetFileNameWithoutExtension(progressFile).Replace(toMoveTo.ToString(), numToMove.ToString()));
            }
        }
    }

    public static string RemoveNonLetters(string word)
    {
        Regex reg = new Regex("[^a-zA-Z'_]");
        return reg.Replace(word, string.Empty);
    }

    public static string RemoveNonNumbers(string input)
    {
        string resultString;
        try
        {
            Regex regexObj = new Regex(@"[^\d]");
            resultString = regexObj.Replace(input, "");
        }
        catch (ArgumentException)
        {
            throw new Exception("Didn't even hand me something I could work with, boi.");
        }
        return resultString;
    }

    public static string AssemblyDirectory
    {
        get
        {
            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);
            return Path.GetDirectoryName(path);
        }
    }
}

public static class CommonSense
{
    public static IEnumerable<string> OrderByNumber(this IEnumerable<string> input)
    {
        IEnumerable<string> newArray = input as string[] ?? input.ToArray();
        int maxLen = newArray.Select(s => s.Length).Max();

        return newArray.Select(s => new
        {
            OrgStr = s,
            SortStr = Regex.Replace(s, @"(\d+)|(\D+)", m => m.Value.PadLeft(maxLen, char.IsDigit(m.Value[0]) ? ' ' : '\xffff'))
        })
        .OrderBy(x => x.SortStr)
        .Select(x => x.OrgStr);
    }
}