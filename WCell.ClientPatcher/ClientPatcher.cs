using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace WCell.ClientPatcher
{
    public sealed class ClientPatcher
    {
        /// <summary>
        /// Offset to bytes for Connection index check patch
        /// </summary>
        private const long ConnectionIndexCheckOffset = 0x90A8D;

        /// <summary>
        /// Bytes to patch Connection index check with
        /// </summary>
        private static readonly byte[] ConnectionIndexCheckPatch = new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90, 0x90 };

        /// <summary>
        /// Offset to bytes for Connection index selection patch
        /// </summary>
        private const long ConnectionIndexSelectionOffset = 0x91229;

        /// <summary>
        /// Bytes to patch Connection index selection with
        /// </summary>
        private static readonly byte[] ConnectionIndexSelectionPatch = new byte[] {0xB8, 0x00, 0x00, 0x00, 0x00};

        /// <summary>
        /// Offset to bytes for Grunt selection patch
        /// </summary>
        private const long GruntSelectionOffset = 0xD6FBD;

        /// <summary>
        /// Bytes to patch Grunt selection check with
        /// </summary
        private static readonly byte[] GruntSelectionPatch = new byte[] {0xEB};
        
        /// <summary>
        /// Name and path of the file to be patched
        /// </summary>
        private readonly string _fileName;

        /// <summary>
        /// Path to the file to be patched, used for creating the backup
        /// </summary>
        private readonly string _directory;

        private RichTextBox _richTextBox;

        private int _clientBuild;

        public ClientPatcher(string fileName)
        {
            _fileName = fileName;
            _directory = Path.GetDirectoryName(_fileName);
        }

        /// <summary>
        /// Reads the executable and searches for the build number writing it to <see cref="_clientBuild"/>
        /// </summary>
        /// <returns>A <see>Boolean</see> value indicating whether or not the build number could be found.</returns>
        private bool FindVersion()
        {
            var content = File.ReadAllText(_fileName);
            var offset = content.IndexOf("Version %s (%s) %s");

            if (offset <= 0)
            {
                WriteLine("Invalid executable file.");
                return false;
            }

            //skip over "Version"
            while (content[offset] != 0)
            {
                offset++;
            }

            //skip spaces
            do
            {
                offset++;
            } while (content[offset] == 0);

            //skip version number
            while (content[offset] != 0)
            {
                offset++;
            }

            //skip spaces
            while (content[offset] == 0)
            {
                offset++;
            }

            //read in the build number
            var sb = new StringBuilder();
            while (content[offset] != 0)
            {
                sb.Append(content[offset++]);
            }
            
            //parse the build number from the string
            if(!Int32.TryParse(sb.ToString(), out _clientBuild))
            {
                return false;
            }

            WriteLine("Found client build: " + _clientBuild);

            return true;
        }

        /// <summary>
        /// Creates a backup of the file to be patched.
        /// </summary>
        /// <param name="fileName">Name of the file to create backup into.</param>
        /// <returns>A <see>Boolean</see> value indicating whether or not the backup succeeded.</returns>
        private bool Backup(string fileName)
        {
            try
            {
                var backupFileName = Path.Combine(_directory, fileName);
                File.Copy(_fileName, backupFileName);
            }
            catch (Exception ex)
            {
                WriteLine("Backup failed." + ex.Message);
                return false;
            }

            WriteLine("File successfully backed up to " + fileName);
            return true;
        }

        /// <summary>
        /// Trys to write the patch to the file.
        /// </summary>
        /// <returns>A <see>Boolean</see> value indicating whether or not the patching succeeded.</returns>
        private bool WritePatch()
        {
            try
            {
                if (_clientBuild != 13623)
                {
                    WriteLine("Unsupported client build");
                    return false;
                }

                var stream = File.Open(_fileName, FileMode.Open, FileAccess.Write, FileShare.None);
                using (var writer = new BinaryWriter(stream))
                {
                    WriteLine("Patching Connection Index Check");
                    writer.BaseStream.Seek(ConnectionIndexCheckOffset, SeekOrigin.Begin);
                    writer.Write(ConnectionIndexCheckPatch);
                    WriteLine("Done");

                    WriteLine("Patching Connection Index Selection");
                    writer.BaseStream.Seek(ConnectionIndexSelectionOffset, SeekOrigin.Begin);
                    writer.Write(ConnectionIndexSelectionPatch);
                    WriteLine("Done");

                    WriteLine("Patching Grunt Selection");
                    writer.BaseStream.Seek(GruntSelectionOffset, SeekOrigin.Begin);
                    writer.Write(GruntSelectionPatch);
                    WriteLine("Done");
                }
            }
            catch (Exception ex)
            {
                WriteLine("Patching failed: " + ex.Message);
                return false;
            }

            _richTextBox.ForeColor = Color.Green;
            WriteLine("Success!");
            return true;
        }

        /// <summary>
        /// Trys to patch the client.
        /// </summary>
        /// <param name="richTextBox">The rich text box to output to, or null to output to the console.</param>
        /// <returns>A <see>Boolean</see> value indicating whether or not the patching succeeded.</returns>
        public bool Patch(RichTextBox richTextBox)
        {
            _richTextBox = richTextBox;

            if(!FindVersion())
                return false;

            if (!Backup("Wow.Backup.exe"))
                return false;

            return WritePatch();
        }

        /// <summary>
        /// Writes a line of text to the rich text box or the console if no text box reference
        /// has been set.
        /// </summary>
        /// <param name="output">The text to write.</param>
        public void WriteLine(string output)
        {
            if (_richTextBox != null)
                _richTextBox.AppendText(output + Environment.NewLine);
            else
                Console.WriteLine(output);
        }
    }
}
