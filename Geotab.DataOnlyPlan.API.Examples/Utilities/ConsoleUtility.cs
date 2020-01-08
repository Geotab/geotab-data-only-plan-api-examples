using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Geotab.DataOnlyPlan.API.Examples.Utilities
{
    /// <summary>
    /// Contains methods to assist in working with the console.
    /// </summary>
    public static class ConsoleUtility
    {
        const string Separator = "======================================================================";
        const string Separator2 = "----------------------------------------------------------------------";

        /// <summary>
        /// Prompts the user for input and returns that input.
        /// </summary>
        /// <param name="promptMessage">The message to prompt the user with.  Will be prefixed with <c>"> Enter "</c>.</param>
        /// <returns>The user input.</returns>
        public static string GetUserInput(string promptMessage)
        {
            Console.ForegroundColor = Common.ConsoleColorForUserPrompts;
            Console.Write("> Enter {0}:", promptMessage);
            Console.ForegroundColor = Common.ConsoleColorForUserInput;
            string userInput = Console.ReadLine();
            Console.ForegroundColor = Common.ConsoleColorDefault;
            return userInput;
        }

        /// <summary>
        /// Prompts the user to input a file path followed by a file name until valid entries are made.  The validated full path is returned.
        /// </summary>
        /// <param name="fileTypeDescription">A description of the file type being sought (e.g. '<c>config</c>').  For use in user prompts.</param>
        /// <returns>The validated full path.</returns>
        public static string GetUserInputFilePath(string fileTypeDescription)
        {
            string filePath = string.Empty;
            string fileName = string.Empty;
            string fileFullPath = string.Empty;
            bool filePathIsValid = false;
            bool fileNameIsValid = false;

            // Get the user to enter a valid directory path.
            while (!filePathIsValid)
            {
                Console.ForegroundColor = Common.ConsoleColorForUserPrompts;
                Console.Write($"> Enter {fileTypeDescription} folder (e.g. 'C:\\Temp'):");
                Console.ForegroundColor = Common.ConsoleColorForUserInput;
                filePath = Console.ReadLine();
                Console.ForegroundColor = Common.ConsoleColorDefault;
                if (Directory.Exists(filePath))
                {
                    filePathIsValid = true;
                }
                else
                {
                    ConsoleUtility.LogError($"The folder entered does not exist.");
                }
            }

            // Get the use to enter a valid filename.
            while (!fileNameIsValid)
            {
                Console.ForegroundColor = Common.ConsoleColorForUserPrompts;
                Console.Write($"> Enter {fileTypeDescription} file name (e.g. 'FileName.csv'):");
                Console.ForegroundColor = Common.ConsoleColorForUserInput;
                fileName = Console.ReadLine();
                Console.ForegroundColor = Common.ConsoleColorDefault;
                fileFullPath = Path.Combine(filePath, fileName);
                if (File.Exists(fileFullPath))
                {
                    fileNameIsValid = true;
                }
                else
                {
                    ConsoleUtility.LogError($"The file '{fileName}' does not exist in folder '{filePath}'.");
                }
            }
            return fileFullPath;
        }

        /// <summary>
        /// Prompts the user for input and returns that input.  The input is masked in the console as it is being entered.  For use with  passwords and other sensitive information that should not be displayed on the user's screen.
        /// </summary>
        /// <param name="promptMessage">The message to prompt the user with.  Will be prefixed with <c>"> Enter "</c>.</param>
        /// <returns>The user input.</returns>
        public static string GetUserInputMasked(string promptMessage)
        {
            Console.ForegroundColor = Common.ConsoleColorForUserPrompts;
            Console.Write("> Enter {0}:", promptMessage);
            Console.ForegroundColor = Common.ConsoleColorForUserInput;

            string userInput = "";
            while (true)
            {
                ConsoleKeyInfo key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Enter)
                {
                    break;
                }
                else if (key.Key == ConsoleKey.Backspace)
                {
                    if (userInput.Length > 0)
                    {
                        userInput = userInput.Remove(userInput.Length - 1);
                        Console.Write("\b \b");
                    }
                }
                else if (key.KeyChar != '\u0000')
                {
                    // KeyChar == '\u0000' if the key pressed does not correspond 
                    // to a printable character (e.g. F1, PrtScr, etc.)
                    userInput += key.KeyChar;
                    Console.Write("*");
                }
            }
            Console.WriteLine("");
            Console.ForegroundColor = Common.ConsoleColorDefault;
            return userInput;
        }

        /// <summary>
        /// Prompts the user for input (will be prefixed with <c>"> Enter "</c>) twice and checks to ensure that both values match.  If both values do not match, user is prompted to re-enter values.  Repeats until a matching pair of values is entered and returns that input.  The input is masked in the console as it is being entered.  For use with  passwords and other sensitive information that should not be displayed on the user's screen.
        /// </summary>
        /// <param name="promptMessage">The message to prompt the user with.</param>
        /// <param name="verifyPromptMessage">The message to prompt the user with indicating that re-entry of the same value is required.</param>
        /// <returns>The verified user input.</returns>
        public static string GetVerifiedUserInputMasked(string promptMessage, string verifyPromptMessage)
        {
            string input = GetUserInputMasked(promptMessage);
            string verifyInput = GetUserInputMasked(verifyPromptMessage);

            if (verifyInput != input)
            {
                LogError("Values entered do not match.  Try again.");
                input = GetVerifiedUserInputMasked(promptMessage, verifyPromptMessage);
            }
            return input;
        }

        /// <summary>
        /// Adds <c>"COMPLETE."</c> to a log line.
        /// </summary>
        public static void LogComplete(ConsoleColor consoleColor = Common.ConsoleColorForSuccess)
        {
            Console.ForegroundColor = consoleColor;
            Console.WriteLine("COMPLETE.");
            Console.ForegroundColor = Common.ConsoleColorDefault;
        }

        /// <summary>
        /// Adds a log line that prefixes the supplied message with <c>"ERROR: "</c>.
        /// </summary>
        /// <param name="errorMessage">The message to be logged.</param>  
        public static void LogError(string errorMessage)
        {
            Console.ForegroundColor = Common.ConsoleColorForErrors;
            Console.WriteLine($"ERROR: {errorMessage}");
            Console.ForegroundColor = Common.ConsoleColorDefault;
        }

        /// <summary>
        /// Adds a log line that prefixes the supplied <see cref="Exception"/> with <c>"ERROR: "</c>.
        /// </summary>
        /// <param name="exception">The <see cref="Exception"/> to be logged.</param>   
        public static void LogError(Exception exception)
        {
            Console.ForegroundColor = Common.ConsoleColorForErrors;
            Console.WriteLine($"ERROR: {exception.Message}\n{exception.StackTrace}");
            Console.ForegroundColor = Common.ConsoleColorDefault;
        }

        /// <summary>
        /// Adds an "Example Menu" header.
        /// </summary>
        public static void LogExamplesMenuHeader()
        {
            Console.ForegroundColor = Common.ConsoleColorDefault;
            Console.WriteLine("");
            Console.WriteLine("");
            LogSeparator2();
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("Examples Menu:");
            Console.ForegroundColor = Common.ConsoleColorDefault;
        }

        /// <summary>
        /// Adds log lines indicating that an example has finished.
        /// </summary>
        /// <param name="exampleName">The name of the example that hs finished.</param>
        public static void LogExampleFinished(string exampleName)
        {
            Console.ForegroundColor = Common.ConsoleColorDefault;
            LogInfo($"Finished {exampleName}");
            LogSeparator2();
        }

        /// <summary>
        /// Adds log lines indicating that an example is being started.
        /// </summary>
        /// <param name="exampleName">The name of the example being started.</param>
        public static void LogExampleStarted(string exampleName)
        {
            Console.ForegroundColor = Common.ConsoleColorDefault;
            LogSeparator2();
            LogInfo($"Started {exampleName}");
        }

        /// <summary>
        /// Adds a separator line followed by a line with the specified header text.
        /// </summary>
        /// <param name="headerText">The text to display in the header after the separator line.</param>
        public static void LogHeader(string headerText)
        {
            Console.ForegroundColor = Common.ConsoleColorDefault;
            LogSeparator2();
            LogInfo($"{headerText}");
        }

        /// <summary>
        /// Adds a log line that prefixes the supplied message with <c>"INFO: "</c>.
        /// </summary>
        /// <param name="infoMessage">The message to be logged.</param>
        public static void LogInfo(string infoMessage)
        {
            Console.ForegroundColor = Common.ConsoleColorDefault;
            Console.WriteLine($"INFO: {infoMessage}");
        }

        /// <summary>
        /// Adds a log line that prefixes the supplied message with <c>"INFO: "</c>.  The infoMessagePart1 and infoMessagePart2 values are concatenated (with a single space between them) and the <see cref="ConsoleColor"/> specified by infoMessagePart2Color is applied to the infoMessagePart2 portion of the message. 
        /// </summary>
        /// <param name="infoMessagePart1"></param>
        /// <param name="infoMessagePart2"></param>
        /// <param name="infoMessagePart2Color"></param>
        public static void LogInfoMultiPart(string infoMessagePart1, string infoMessagePart2, ConsoleColor infoMessagePart2Color)
        {
            Console.ForegroundColor = Common.ConsoleColorDefault;
            Console.Write($"INFO: {infoMessagePart1} ");
            Console.ForegroundColor = infoMessagePart2Color;
            Console.WriteLine($"{infoMessagePart2} ");
            Console.ForegroundColor = Common.ConsoleColorDefault;
        }

        /// <summary>
        /// Starts a log line that prefixes the supplied message with <c>"INFO: "</c>.
        /// </summary>
        /// <param name="infoMessage">The message to be logged.</param>
        public static void LogInfoStart(string infoMessage)
        {
            Console.ForegroundColor = Common.ConsoleColorDefault;
            Console.Write($"INFO: {infoMessage}");
        }

        /// <summary>
        /// Starts a log line that prefixes the supplied message with <c>"INFO: "</c>.  The infoMessagePart1 and infoMessagePart2 values are concatenated (with a single space between them) and the <see cref="ConsoleColor"/> specified by infoMessagePart2Color is applied to the infoMessagePart2 portion of the message. 
        /// </summary>
        /// <param name="infoMessagePart1"></param>
        /// <param name="infoMessagePart2"></param>
        /// <param name="infoMessagePart2Color"></param>
        public static void LogInfoStartMultiPart(string infoMessagePart1, string infoMessagePart2, ConsoleColor infoMessagePart2Color)
        {
            Console.ForegroundColor = Common.ConsoleColorDefault;
            Console.Write($"INFO: {infoMessagePart1} ");
            Console.ForegroundColor = infoMessagePart2Color;
            Console.Write($"{infoMessagePart2} ");
            Console.ForegroundColor = Common.ConsoleColorDefault;
        }

        /// <summary>
        /// Adds a log line that prefixes the supplied message with <c>"- "</c>.
        /// </summary>
        /// <param name="listItem">The message to be logged.</param>   
        public static void LogListItem(string listItem)
        {
            Console.ForegroundColor = Common.ConsoleColorDefault;
            Console.WriteLine($"- {listItem}");
        }

        /// <summary>
        /// Adds a log line that prefixes the supplied message with <c>"- "</c>.  The listItemId is then added in the colour specified by listItemIdColor, followed by a space and the listItem.
        /// </summary>
        /// <param name="listItemId">The Id of the item.</param>
        /// <param name="listItem">The name of the item.</param>
        /// <param name="listItemIdColor">The <see cref="ConsoleColor"/> to be applied to the listItemId.</param>
        /// <param name="listItemColor">The <see cref="ConsoleColor"/> to be applied to the listItem.</param>
        public static void LogListItem(string listItemId, string listItem, ConsoleColor listItemIdColor, ConsoleColor listItemColor)
        {
            Console.ForegroundColor = listItemIdColor;
            Console.Write($"- {listItemId} ");
            Console.ForegroundColor = listItemColor;
            Console.WriteLine($"{listItem}");
            Console.ForegroundColor = Common.ConsoleColorDefault;
        }

        /// <summary>
        /// Writes a list of items into a set of columns.
        /// </summary>
        /// <param name="listItems">The items to be written</param>
        /// <param name="listItemIdColor">The <see cref="ConsoleColor"/> to be applied to the listItem Ids.</param>
        /// <param name="listItemColor">The <see cref="ConsoleColor"/> to be applied to the listItem values.</param>
        public static void LogListItems(IList<KeyValuePair<int, string>> listItems, ConsoleColor listItemIdColor, ConsoleColor listItemColor)
        {
            // Determine the maximum length of any item.  This will determine column width.
            int maxItemLength = 0;
            foreach (KeyValuePair<int, string> listItem in listItems)
            {
                int currentItemLength = listItem.Value.Length;
                if (currentItemLength > maxItemLength)
                {
                    maxItemLength = currentItemLength;
                }
            }

            int itemCount = listItems.Count;
            if (itemCount <= 10)
            {
                foreach (KeyValuePair<int, string> listItem in listItems)
                {
                    Console.Write($"- ");
                    Console.ForegroundColor = listItemIdColor;
                    Console.Write($"{listItem.Key.ToString()} ");
                    Console.ForegroundColor = listItemColor;
                    Console.WriteLine($"{listItem.Value}");
                }
            }
            else
            {
                // Split items into two columns.
                for (int listItemNumber = 0; listItemNumber < listItems.Count; listItemNumber++)
                {
                    KeyValuePair<int, string> listItem;

                    // First column.
                    listItem = listItems[listItemNumber];

                    Console.Write($"- ");
                    Console.ForegroundColor = listItemIdColor;
                    Console.Write($"{listItem.Key.ToString().PadRight(2)} ");
                    Console.ForegroundColor = listItemColor;
                    Console.Write($"{listItem.Value.PadRight(maxItemLength)} ");
                    listItemNumber++;
                    if (listItemNumber >= listItems.Count)
                    {
                        Console.WriteLine("");
                        break;
                    }

                    // Second column.
                    listItem = listItems[listItemNumber];
                    Console.Write($"- ");
                    Console.ForegroundColor = listItemIdColor;
                    Console.Write($"{listItem.Key.ToString().PadRight(2)} ");
                    Console.ForegroundColor = listItemColor;
                    Console.WriteLine($"{listItem.Value.PadRight(maxItemLength)}");
                }
            }
            Console.ForegroundColor = Common.ConsoleColorDefault;
        }

        /// <summary>
        /// Adds a log line that prefixes the supplied message with <c>"- "</c>.  The listItem is then added, followed by <c>"..."</c> and the result.  The result is coloured based on the value of resultColor.
        /// </summary>
        /// <param name="listItem">The name of the item.</param>
        /// <param name="result">The result information.</param>
        /// <param name="resultColor">The <see cref="ConsoleColor"/> to be applied to the result.</param>
        public static void LogListItemWithResult(string listItem, string result, ConsoleColor resultColor)
        {
            Console.ForegroundColor = Common.ConsoleColorDefault;
            Console.Write($"- {listItem}...");
            Console.ForegroundColor = resultColor;
            Console.WriteLine(result);
            Console.ForegroundColor = Common.ConsoleColorDefault;
        }

        /// <summary>
        /// Logs the properties and values of the subject object.
        /// </summary>
        /// <param name="obj">The object for which to log properties and their value.</param>
        public static void LogObjectProperties(object obj)
        {
            Console.ForegroundColor = Common.ConsoleColorDefault;
            Console.Write("Data: ");
            Console.ForegroundColor = Common.ConsoleColorForUnchangedData;

            if (obj == null)
            {
                Console.WriteLine("Object is null");
                return;
            }

            Type type = obj.GetType();
            Console.WriteLine($"Type: {type.Name}");
            PropertyInfo[] properties = type.GetProperties();
            Console.WriteLine($"Value: {obj.ToString()}");
            Console.WriteLine($"Properties (N = {properties.Length}):");
            foreach (var property in properties)
            {
                if (property.GetIndexParameters().Length == 0)
                {
                    Console.WriteLine($"   {property.Name} ({property.PropertyType.Name}): {property.GetValue(obj)}");
                }
                else
                {
                    Console.WriteLine($"   {property.Name} ({property.PropertyType.Name}): <Indexed>");
                }
            }
        }

        /// <summary>
        /// Adds <c>"OK"</c> to a log line.
        /// </summary>
        public static void LogOk()
        {
            Console.ForegroundColor = Common.ConsoleColorForSuccess;
            Console.WriteLine("OK");
            Console.ForegroundColor = Common.ConsoleColorDefault;
        }

        /// <summary>
        /// Adds a separator line made-up of "=".
        /// </summary>
        public static void LogSeparator()
        {
            Console.ForegroundColor = Common.ConsoleColorDefault;
            Console.WriteLine(Separator);
        }

        /// <summary>
        /// Adds a separator line made-up of "-".
        /// </summary>
        public static void LogSeparator2()
        {
            Console.ForegroundColor = Common.ConsoleColorDefault;
            Console.WriteLine(Separator2);
        }

        /// <summary>
        /// Adds log lines indicating that a test has completed successfully and indicating the result.
        /// </summary>
        /// <param name="obj">The object returned by the method being tested.</param>
        public static void LogTestResultOk(object obj = null)
        {
            Console.ForegroundColor = Common.ConsoleColorForSuccess;
            Console.WriteLine("OK");
            LogObjectProperties(obj);
            Console.ForegroundColor = Common.ConsoleColorDefault;
            LogSeparator2();
        }

        /// <summary>
        /// Adds log lines indicating that a test is being initiated.
        /// </summary>
        /// <param name="testNumber">The number of the test within the current test batch.</param>
        /// <param name="methodName">The name of the method being tested.</param>
        /// <param name="parameters">The parameter values being supplied to the method.</param>
        public static void LogTestStart(int testNumber, string methodName, Dictionary<string, object> parameters = null)
        {
            Console.ForegroundColor = Common.ConsoleColorDefault;
            LogSeparator2();
            Console.WriteLine($"TEST {testNumber.ToString()}");
            Console.Write("Method: ");
            Console.ForegroundColor = Common.ConsoleColorForUserInput;
            Console.WriteLine(methodName);

            Console.ForegroundColor = Common.ConsoleColorForUnchangedData;
            if (parameters == null)
            {
                Console.WriteLine($"Parameters: None");
            }
            else
            {
                Console.WriteLine($"Parameters (N = {parameters.Count.ToString()}):");
                foreach (KeyValuePair<string, object> parameter in parameters)
                {
                    Console.WriteLine($"   {parameter.Key}: {parameter.Value.ToString()}");
                }
            }

            Console.ForegroundColor = Common.ConsoleColorDefault;
            Console.Write("Result: ... ");
        }

        /// <summary>
        /// Adds log lines indicating that the specified utility has finished.
        /// </summary>
        /// <param name="utilityName">The name of the utility.</param>
        public static void LogUtilityShutdown(string utilityName)
        {
            Console.ForegroundColor = Common.ConsoleColorDefault;
            Console.WriteLine("");
            Console.WriteLine(Separator);
            Console.ForegroundColor = Common.ConsoleColorForUserPrompts;
            Console.WriteLine($"{utilityName} finished. Press any key to exit.");
            Console.ForegroundColor = Common.ConsoleColorDefault;
        }

        /// <summary>
        /// Adds log lines indicating that the specified utility has started.
        /// </summary>
        /// <param name="utilityName">The name of the utility.</param>
        public static void LogUtilityStartup(string utilityName)
        {
            Console.ForegroundColor = Common.ConsoleColorDefault;
            Console.WriteLine("");
            Console.WriteLine(Separator);
            Console.WriteLine($"{utilityName} started.");
            Console.WriteLine(Separator);
        }

        /// <summary>
        /// Adds a log line that prefixes the supplied message with <c>"WARNING: "</c>.
        /// </summary>
        /// <param name="warningMessage">The message to be logged.</param>
        public static void LogWarning(string warningMessage)
        {
            Console.ForegroundColor = Common.ConsoleColorForWarnings;
            Console.WriteLine($"WARNING: {warningMessage}");
            Console.ForegroundColor = Common.ConsoleColorDefault;
        }
    }
}