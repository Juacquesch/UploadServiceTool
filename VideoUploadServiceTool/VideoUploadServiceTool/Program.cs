using System.Diagnostics;

public class Program
{
    private const int LongSize = sizeof(long);
    private const int LineBufferSize = 4096;
    private static readonly string FileSizeName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"Filesize.bin");
    //private static readonly string FileToRead = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"Alea-Upload-Service-18.1-out.log");
    private static readonly string FileToRead = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"Alea-Upload-Service-18.1-out-test.log");

    public static void Main(string[] args)
    {
        Console.WriteLine("Program VideoUploadServiceTool started.");
        
        InitializeFileSize(FileSizeName);

        long readBytes = GetReadBytes(FileSizeName);

        if (!File.Exists(FileToRead))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("File does not exist.");
            return;
        }

        Console.WriteLine($"Starting to process the file. Already read {readBytes} bytes before.");
        ProcessFile(FileToRead, FileSizeName, readBytes);
        Console.WriteLine("File processing completed.");
        Console.WriteLine("Program ended.");
    }

    private static void InitializeFileSize(string fileName)
    {
        if (!File.Exists(fileName))
        {
            using (var fs = File.Create(fileName))
            {
                long value = 0L;
                byte[] bytes = BitConverter.GetBytes(value);
                fs.Write(bytes, 0, bytes.Length);
                fs.Flush();
            }
        }
    }
    
    private static long GetReadBytes(string fileName)
    {
        using (var fileStream = File.OpenRead(fileName))
        {
            byte[] buffer = new byte[LongSize];
            fileStream.ReadAtLeast(buffer, buffer.Length);
            return BitConverter.ToInt64(buffer);
        }
    }

    private static void ProcessFile(string fileToRead, string fileSizeName, long readBytes)
    {
        Console.WriteLine($"Processing file: {fileToRead}");
        
        using var fileStream = new FileStream(FileToRead,FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        
        fileStream.Seek(readBytes, SeekOrigin.Begin);

        byte[] lineBuffer = new byte[LineBufferSize];
        int index = 0;

        while (index < LineBufferSize)
        {
            int readByte = fileStream.ReadByte();

            if (readByte == -1)
            {
                UpdateReadBytes(fileSizeName, readBytes + index);
                Console.WriteLine("End of file reached or no more data available.");
                break;
            }

            lineBuffer[index++] = (byte)readByte;

            if (readByte == '\n' && index > 1)
            {
                // Convert the byte array to a string using UTF-8 encoding
                string line = System.Text.Encoding.UTF8.GetString(lineBuffer, 0, index);
                
                Console.WriteLine($"Processing line:\n{line.Trim()}");
                
                ProcessLine(line);
                readBytes += index;
                index = 0;
                Array.Clear(lineBuffer, 0, lineBuffer.Length);
            }
        }
        
        Console.WriteLine($"Finished processing the file. Total bytes read: {readBytes}");
    }

    private static void UpdateReadBytes(string fileName, long readBytes)
    {
        using (var fs = File.OpenWrite(fileName))
        {
            byte[] bytes = BitConverter.GetBytes(readBytes);
            fs.Write(bytes, 0, bytes.Length);
            fs.Flush();
        }
    }

    private static void ProcessLine(string line)
    {
        if (line.Contains("command executed"))
        {
            string program = @"C:\Program Files\Git\usr\bin\bash.exe";

            // runCommand(program, line);
            
            var programAndCommand = line.Split(@"command executed: ")[1];
            
            Console.WriteLine($"Executing command: {programAndCommand}");
            
             program = "C:/Upload_Service/utils/dcm4che-5.22.0/bin/storescu";

             var command = programAndCommand.Split("C:/Upload_Service/utils/dcm4che-5.22.0/bin/storescu ")[1];
            
             // var command = programAndCommand.Substring(@"C:/Upload_Service/utils/dcm4che-5.22.0/bin/storescu ".Length, programAndCommand.Length - "C:/Upload_Service/utils/dcm4che-5.22.0/bin/storescu ".Length);
            
            //Bash command
            /*ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                FileName = program,
                Arguments = $"-c \"{programAndCommand}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            Process.Start(processStartInfo)!.WaitForExit();*/


            /*Process.Start(new ProcessStartInfo(program, $"/bin/bash -c {programAndCommand}")*/
            Process.Start(new ProcessStartInfo(program, programAndCommand)
            {
                RedirectStandardOutput = false,
                UseShellExecute = true,
                CreateNoWindow = false,
                WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory
            })!.WaitForExit();

            /*// var startinfo = new ProcessStartInfo(program, $"/bin/bash -c {programAndCommand}");
            var startinfo = new ProcessStartInfo(program, programAndCommand)
            {

                // RedirectStandardOutput = true,
                UseShellExecute = true,
                CreateNoWindow = false,
                WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory
            };*/

            /*Process process = Process.Start(startinfo);
            process.WaitForExit();*/

            Console.WriteLine($"Command execution completed.\n");
        }
    }
    
    static void runCommand(string program, string line)
    {
        Process process = new Process();
        var programAndCommand = line.Split(@"command executed: ")[1];
            
        Console.WriteLine($"Executing command: {programAndCommand}");
        
        process.StartInfo.FileName = program;
        process.StartInfo.Arguments = $"/bin/bash - c {programAndCommand}";
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.CreateNoWindow = false;
        process.StartInfo.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;
        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        
        //* Read the output (or the error)
        // string output = process.StandardOutput.ReadToEnd();
        // Console.WriteLine(output);
        // string err = process.StandardError.ReadToEnd();
        // Console.WriteLine(err);
        process.WaitForExit();
    }
}