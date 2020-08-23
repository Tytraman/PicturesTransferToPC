using System;
using System.IO;
using System.Linq;
using System.Text;
using MediaDevices;

namespace TransfertFichiersTelephonePC
{
    class Program
    {

        static void Main(string[] args)
        {
            Console.Title = "Transfert photos téléphone";
            char separator = Path.DirectorySeparatorChar;
            
            try
            {
                MediaDevice device = MediaDevice.GetDevices().ElementAt(0);
                long now = Now();
                device.Connect();
                var path = GetPath(device);
            
                var files = path.EnumerateFiles("*.*", SearchOption.AllDirectories);
                Console.WriteLine(device.FriendlyName + " - " + files.Count() + " fichiers\n\n");

                string saveDir = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) + separator + "Transfert photos téléphone" + separator + DateTime.Now.ToString().Replace('/', '-').Replace(':', '-');
                Directory.CreateDirectory(saveDir);
                int total = files.Count();

                for (int i = 0; i < total; i++)
                {
                    var file = files.ElementAt(i);
                    MemoryStream memory = new MemoryStream();
                    device.DownloadFile(file.FullName, memory);
                    memory.Position = 0;
                    string output = saveDir + separator + file.Name;
                    string tempOut = output;
                    while(File.Exists(tempOut))
                    {
                        tempOut = output + "(" + new Random().Next(0, int.MaxValue) + ")";
                    }
                    output = tempOut;
                    FileStream fs = new FileStream(output, FileMode.Create, FileAccess.Write);
                    int length = (int)memory.Length;
                    byte[] bytes = new byte[length];
                    memory.Read(bytes, 0, length);
                    fs.Write(bytes, 0, bytes.Length);
                    memory.Close();
                    fs.Close();
                    int adv = ((i + 1) * 100 / total);
                    ClearLine(1);
                    ClearLine(2);
                    Console.SetCursorPosition(0, 1);
                    Console.Write("\r" + UpdateProgressBar(adv) + "\n" + (i + 1) + "/" + total);
                }
                device.Disconnect();
                Console.WriteLine("\nOpération terminée en " + (Now() - now) + "ms.\nImages enregisrées dans " + saveDir);
            }
            catch(ArgumentOutOfRangeException)
            {
                ErrorQuit("Aucun appareil branché...");
            }
            Console.WriteLine("\nAppuyez sur entrée pour quitter...");
            Console.ReadLine();
        }

        public static void ClearLine(int line)
        {
            Console.SetCursorPosition(0, line);
            Console.Write(new string(' ', Console.WindowWidth - 1));
        }

        public static long Now()
        {
            return DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }

        public static string UpdateProgressBar(int progress)
        {
            string textual = "Progression " + progress.ToString().PadLeft(3, ' ') + "% [";
            int barSize = 20;
            StringBuilder builder = new StringBuilder(barSize);
            progress = progress * barSize / 100;
            return textual + builder.ToString().PadLeft(progress, '#').PadRight(barSize, '-') + "]";
        }


        public static MediaDirectoryInfo GetPath(MediaDevice device)
        {
            try
            {
                return device.GetDirectoryInfo(@"\Phone\DCIM");
            }
            catch (DirectoryNotFoundException)
            {
                try
                {
                    return device.GetDirectoryInfo(@"\Internal Storage\DCIM");
                }catch(DirectoryNotFoundException)
                {
                    ErrorQuit("L'appareil branché est peut-être incompatible avec le programme...");
                    return null;
                }
            }
        }

        public static void ErrorQuit(string message)
        {
            Console.WriteLine(message + "\n\nAppuyez sur entrée pour quitter...");
            Console.ReadLine();
            Environment.Exit(-1);
        }

    }
}
