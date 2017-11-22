using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nameless.Libraries.Yggdrasil.Aerith;
using Nameless.Libraries.Yggdrasil.Lain;
using System.Reflection;
using System.IO;
using Nameless.Libraries.Yggdrasil.Exceptions;

namespace Nameless.Apps.Broom
{
    class BroomCleaner
    {
        const int printLength = 20;
        /// <summary>
        /// A lain ghost is called to report the cleaning result
        /// </summary>
        static WiredGhost Ghost;
        /// <summary>
        /// Broom Hatter Version
        /// </summary>
        static String Version
        {
            get { return Assembly.GetAssembly(typeof(BroomCleaner)).GetName().Version.ToString(3); }
        }
        /// <summary>
        /// Broom Hatter current directory path
        /// </summary>
        static String CurrentPath
        {
            get { return Environment.CurrentDirectory; }
        }
        /// <summary>
        /// Ryoga Hibiki log file path
        /// </summary>
        static String LogPath
        {
            get { return Path.Combine(CurrentPath, "cleaning.log"); }
        }
        /// <summary>
        /// Ryoga Hibiki Developer
        /// </summary>
        static String Dev
        {
            get { return "A Nameless Wolf"; }
        }
        /// <summary>
        /// Gets the help menu.
        /// </summary>
        /// <value>
        /// The help menu.
        /// </value>
        static string HelpMenu
        {
            get
            {
                return
                    "Broom Hatter {0}\n" +
                    "Dev {1}\n" +
                    "-P\t\t\tClean the given path\n" +
                    "\t\t\tExample:\n" +
                    "\t\t\t-P <path_directory>\n" +
                    "-C\t\t\tCleans the current path\n" +
                    "-L\t\t\tPrints the log\n" +
                    "-H,\t\t\tPrints Broom Hatter Help";
            }
        }
        /// <summary>
        /// Gets the available command options
        /// </summary>
        static string[] CommandOptions => new string[] { "-P", "-C", "-L", "-H" };
        /// <summary>
        /// Run the broom command
        /// </summary>
        /// <param name="args">command arguments</param>
        static void Main(string[] args)
        {
            InitLog();
            List<String> input = args.Select(x => x.ToUpper()).Distinct().ToList();
            List<TaskCommand> tasks = new List<TaskCommand>();
            try
            {
                //Se llena la lista de trabajo para Ryoga
                for (int i = 0; i < input.Count; i++)
                    if (CommandOptions.Contains(input[i]))
                        tasks.Add(new TaskCommand() { CommandString = input[i], CommandParameters = GetParameters(input[i], i, input) });
                //Se resulven las tareas solicitadas
                foreach (TaskCommand task in tasks)
                    SolveTask(task);
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.Message);
            }
        }
        /// <summary>
        /// Resuelve la tarea solicitada
        /// </summary>
        /// <param name="task">La tarea a resolver</param>
        private static void SolveTask(TaskCommand task)
        {
            try
            {
                switch (task.CommandString)
                {
                    case "-L":
                        PrintLog();
                        break;
                    case "-P":
                        CleanSolution(task.CommandParameters[0]);
                        break;
                    case "-C":
                        CleanSolution(CurrentPath);
                        break;
                    case "-H":
                        Console.WriteLine(String.Format(HelpMenu, Version, Dev));
                        break;
                    default:
                        throw new NamelessException("Command not found");
                }
            }
            catch (Exception exc)
            {
                throw exc;
            }
        }
        /// <summary>
        /// Initialize the application Log
        /// </summary>
        private static void InitLog()
        {
            //Crea el archivo de Log
            if (!File.Exists(LogPath))
                File.WriteAllLines(LogPath, new String[] { });
            Ghost = new WiredGhost(new FileInfo(LogPath), true);
        }
        /// <summary>
        /// Gets the parameters for a command string
        /// </summary>
        /// <param name="cmdString">The command string.</param>
        /// <param name="index">The index.</param>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        /// <exception cref="NamelessException">Sintax Error</exception>
        private static string[] GetParameters(string cmdString, int index, List<string> input)
        {
            String[] result;
            switch (cmdString)
            {
                case "-L":
                case "-H":
                    result = null;
                    break;
                case "-C":
                    result = new String[] { CurrentPath };
                    if (!Directory.Exists(CurrentPath))
                        throw new NamelessException(String.Format("Directory '{0}' is missing", CurrentPath));
                    break;
                case "-P":
                    if ((index + 1) < input.Count && !input[index | +1].Contains('-'))
                    {
                        result = new String[] { input[index + 1] };
                        if (!Directory.Exists(input[index + 1]))
                            if (Directory.Exists(Path.Combine(CurrentPath, input[index + 1])))
                                result = new String[] { Path.Combine(CurrentPath, input[index + 1]) };
                            else
                                throw new NamelessException(String.Format("Directory '{0}' is missing", input[index + 1]));
                    }
                    else
                        throw new NamelessException("Sintax Error");
                    break;
                default:
                    throw new NamelessException("Command not found");
            }
            return result;
        }
        /// <summary>
        /// Prints the Log current output
        /// </summary>
        private static void PrintLog()
        {
            String[] lines = File.ReadAllLines(LogPath);
            var printLines = lines.Length > printLength ? lines.Skip(lines.Length - printLength) : lines;
            printLines.ToList().ForEach(x => Console.WriteLine(x));
            Console.ReadLine();
        }
        /// <summary>
        /// Cleans the given path
        /// </summary>
        /// <param name="dirPath">The application dir path</param>
        private static void CleanSolution(string dirPath)
        {
            Console.WriteLine("Do you want to clean?\n{0}\nY,N", dirPath);
            char result = Console.ReadLine().FirstOrDefault();
            while (result != 'Y' && result != 'N')
            {
                Console.WriteLine("Please type only\nY or N", dirPath);
                result = Console.ReadLine().FirstOrDefault();
            }
            if (result == 'Y')
            {
                AerithScanner scn = new AerithScanner(dirPath, true, true);
                var filter = new SolutionFilter(dirPath);
                scn.Find(filter, FileFound, DirectoryFound);
                scn.Files.ToList().ForEach(x =>
                {
                    File.Delete(x.FullName);
                    Ghost.AppendEntry(String.Format("File deleted: {0}", x.FullName));
                });
                scn.Directories.ToList().ForEach(x =>
                {
                    Directory.Delete(x.FullName);
                    Ghost.AppendEntry(String.Format("Directory deleted: {0}", x.FullName));
                });
                Console.WriteLine("Task Completed");
            }
            else
                Console.WriteLine("Operation canceled");

        }
        /// <summary>
        /// Prints the found directory
        /// </summary>
        /// <param name="input">Task input parameter</param>
        /// <param name="startDirectory">Initial directory</param>
        /// <param name="currentDirectory">Current directory</param>
        private static void DirectoryFound(ref object input, DirectoryInfo startDirectory, DirectoryInfo currentDirectory)
        {
            Console.WriteLine("Directory Found: {0}", currentDirectory.Name);
        }
        /// <summary>
        /// Prints the found file
        /// </summary>
        /// <param name="input">Task input parameter</param>
        /// <param name="startDirectory">Initial directory</param>
        /// <param name="currentFile">Current file</param>
        private static void FileFound(ref object input, DirectoryInfo startDirectory, FileInfo currentFile)
        {
            Console.WriteLine("File Found: {0}", currentFile.Name);
        }
    }
}