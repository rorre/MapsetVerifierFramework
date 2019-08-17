using System.IO;

namespace MapsetVerifierFramework.objects.resources
{
    public class FileAbstraction : TagLib.File.IFileAbstraction
    {
        public FileAbstraction(string aFilePath)
        {
            ReadStream = aFilePath != null ? new FileStream(aFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite) : null;
            Name = aFilePath;
        }

        public string Name { get; }

        public Stream ReadStream { get; }

        public Stream WriteStream
        {
            get { return ReadStream; }
        }

        public void CloseStream(Stream aStream)
        {
            aStream.Position = 0;
        }

        public TagLib.File GetTagFile()
        {
            if (Name == null || ReadStream == null)
                return null;

            return TagLib.File.Create(this);
        }
    }
}
