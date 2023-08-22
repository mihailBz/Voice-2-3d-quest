using System.Diagnostics;

public class PythonRunner
{
    public void Run(string usrInput)
    {
        var psi = new ProcessStartInfo();
        // point to python virtual env
        psi.FileName = @"C:\Users\msh\PycharmProjects\shap-e\venv\Scripts\python.exe";

        // Provide arguments
        var script = @"C:\Users\msh\PycharmProjects\shap-e\main.py";
        psi.Arguments = string.Format("\"{0}\" \"{1}\"", script, usrInput);

        // Process configuration
        psi.UseShellExecute = false;
        psi.CreateNoWindow = false;
        psi.RedirectStandardOutput = true;
        psi.RedirectStandardError = true;

        // Execute process and get output
        var errors = "nothing";
        var results = "nothing";

        using (var process = Process.Start(psi))
        {
            errors = process.StandardError.ReadToEnd();
            results = process.StandardOutput.ReadToEnd();
        }
        UnityEngine.Debug.Log(results);
        UnityEngine.Debug.Log(errors);
    }
}
