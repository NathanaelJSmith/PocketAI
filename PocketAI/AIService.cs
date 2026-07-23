using System.Diagnostics;

public class AIService
{
    private readonly string pythonScriptFolder;
    public AIService(string pythonScriptFolder)
    {
        this.pythonScriptFolder = pythonScriptFolder;
    }

    //Sends the financial prompt to Python and returns the response
    public string GetPythonAIAdvice(string prompt)
    {
        try
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();

            //Uses the python launcher
            startInfo.FileName = "py";

            //Python script to run
            startInfo.Arguments = "ai_coach.py";

            startInfo.WorkingDirectory = pythonScriptFolder;

            //Allows C# and Python to communicate
            startInfo.RedirectStandardInput = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;

            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;

            using Process process = new Process();

            process.StartInfo = startInfo;
            process.Start();

            //Sends the financial prompt to Pythno
            process.StandardInput.Write(prompt);
            process.StandardInput.Close();

            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            process.WaitForExit();

            if (!string.IsNullOrWhiteSpace(error))
            {
                return "Python error:\n" + error;
            }

            if (string.IsNullOrWhiteSpace(output))
            {
                return "Python did not return any advice.";
            }

            return output;
        }

        catch (Exception exception)
        {
           return $"Unables to run the Python coach: {exception.Message}"; 
        }
        
    }
}