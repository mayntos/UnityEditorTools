using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class PBXProjFixWizard : ScriptableWizard
{
    // PBXProjFixWizard
    // This editor wizard is intended to fix a particular problem:
    // iOS builds made on a Windows machine yield a project.pbxproj file
    // with a syntax error. This prevents opening of XCode project on OSX.
    // ---
    // Unity suggests solving the issue by "manually copying the MapFileParser 
    // executable from an OSX installation of the Unity editor into the
    // '$PROJECT_DIR' directory ".
    // ---
    // This editor wizard offers an alternative solution. PBXProjFixWizard
    // parses the faulty project.pbxproj file, corrects the error, and
    // replaces the faulty file.
    // ---
    // 1) File -> "Fix project.pbxproj file..."
    // 2) Click "Identify Path" button.
    // 3) Select your projext.pbxproj file .
    // 4) Click "Fix".
    // ---
    // Thank you for using PBXProjFixWizard! Feel free to suggest improvements.

    public string filePath;
    private string filePathOld;
    private string filePathNew;

    private const string TARGET_TEXT = "MapFileParser.sh\\\"\"";

    [MenuItem("File/Fix project.pbxproj file...")]
    static void CreateWizard()
    {
        ScriptableWizard.DisplayWizard<PBXProjFixWizard>("project.pbxproj file wizard", "Fix", "Identify Path");
    }

    /// <summary>
    /// Prompt user to select project.pbxproj file.
    /// Next, generate file-path for the temp file 
    /// being written to.
    /// </summary>
    private void OnWizardOtherButton()
    {
        filePath = EditorUtility.OpenFilePanel("Select project.pbxproj file", "", "");
        if (!filePath.Contains("project.pbxproj"))
        {
            Debug.LogWarning("Please select project.pbxproj file.");
            filePath = "";
        }
        else
        {
            filePathOld = filePath.Replace("project.pbxproj", "project_OLD.pbxproj");
            filePathNew = filePath.Replace("project.pbxproj", "project_FIXED.pbxproj");

        }
    }

    /// <summary>
    /// Once the window opens, browse and store the correct file path.
    /// Should check to see if the name is "project.pbxproj"
    /// </summary>
    private void OnWizardCreate()
    {
        string line = "";
        bool targetFound = false;

        // 'using' keyword releases objects using unmanaged resources
        // prevents memory leakage, closes even if an exception occurs
        using (StreamReader srRef = new StreamReader(filePath))
        using (StreamWriter swRef = new StreamWriter(filePathNew))
        {
            Debug.Log("Generating temp file for write...");

            // as we read through lines, copy them to temp file.
            // if we encounter target line, remove parenthesis 
            // and write the corrected line to the temp file.
            while (srRef.Peek() > -1)
            {
                line = srRef.ReadLine();
                if (line.Contains(TARGET_TEXT) && targetFound == false)
                {
                    targetFound = true;
                    Debug.Log("Line found in original file...");
                    int editIndex = line.IndexOf("\"\"");
                    string newLine = line.Remove(editIndex, 1);
                    swRef.WriteLine(newLine);
                    Debug.Log("Line replaced in temp file!");
                }
                else
                {
                    swRef.WriteLine(line);
                }
            }
        }

        // if the original file contained no error
        // then we delete the temp file.
        if (targetFound == false)
        {
            Debug.Log("Original project.pbxproj file contained no syntax error.");
            Debug.Log("Removing temp file...");
            File.Delete(filePathNew);
            return;
        }

        // Else if the original file contained error
        // then we rename the corrected temp-file 
        // to project.pbxproj .
        // -----
        // The original file is then kept as backup.
        else
        {
            Debug.Log("project.pbxproj file has been fixed.");
            File.Move(filePath, filePathOld);
            File.Move(filePathNew, filePath);
        }
    }
}