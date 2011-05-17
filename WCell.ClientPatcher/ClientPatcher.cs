using System;
using System.IO;

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

        public ClientPatcher(string fileName)
        {
            _fileName = fileName;
            _directory = Path.GetDirectoryName(_fileName);
        }

        /// <summary>
        /// Creates a backup of the file to be patched
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
                return false;
            }
            return true;
        }

        /// <summary>
        /// Trys to write the patch to the file
        /// </summary>
        /// <returns>A <see>Boolean</see> value indicating whether or not the patching succeeded.</returns>
        private string WritePatch()
        {
            try
            {
                var stream = File.Open(_fileName, FileMode.Open, FileAccess.Write, FileShare.None);
                using (var writer = new BinaryWriter(stream))
                {
                    writer.BaseStream.Seek(ConnectionIndexCheckOffset, SeekOrigin.Begin);
                    writer.Write(ConnectionIndexCheckPatch);

                    writer.BaseStream.Seek(ConnectionIndexSelectionOffset, SeekOrigin.Begin);
                    writer.Write(ConnectionIndexSelectionPatch);

                    writer.BaseStream.Seek(GruntSelectionOffset, SeekOrigin.Begin);
                    writer.Write(GruntSelectionPatch);
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            return "Success!";
        }

        /// <summary>
        /// Trys to patch the client
        /// </summary>
        /// <returns>A <see>Boolean</see> value indicating whether or not the patching succeeded.</returns>
        public bool Patch(out string result)
        {
            if (!Backup("Wow.Backup.exe"))
            {
                result = "Backup failed.";
                return false;
            }

            result = WritePatch();

            return result == "Success!";
        }
    }
}
