using MessagePack;
using MessagePack.Resolvers;
using System.IO;
using System.Text;

namespace Milvaneth.Common
{
    public class Serializer<T> where T : class
    {
        private readonly string filePath;
        private byte[] entropy;
        private static bool defaultIsSet;

        public Serializer(string path, string pass)
        {
            filePath = path;
            entropy = Encoding.UTF8.GetBytes(pass);
            MessagePackSerializer.SetDefaultResolver(ContractlessStandardResolver.Instance);
            defaultIsSet = true;
        }

        public void Save(T obj)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            var raw = MessagePackSerializer.Serialize(obj);

            using (var writer = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
            {
                var data = Encrypter.Encrypt(raw, entropy);
                writer.Write(data, 0, data.Length);
                writer.Flush();
            }
        }

        public T Load()
        {
            using (var reader = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var data = Encrypter.Decrypt(ReadStream(reader), entropy);
                return MessagePackSerializer.Deserialize<T>(data);
            }
        }

        public static byte[] Serialize(T obj)
        {
            if (defaultIsSet) return MessagePackSerializer.Serialize(obj);

            MessagePackSerializer.SetDefaultResolver(ContractlessStandardResolver.Instance);
            defaultIsSet = true;

            return MessagePackSerializer.Serialize(obj);
        }

        public static T Deserialize(byte[] data)
        {
            if (defaultIsSet) return MessagePackSerializer.Deserialize<T>(data);

            MessagePackSerializer.SetDefaultResolver(ContractlessStandardResolver.Instance);
            defaultIsSet = true;

            return MessagePackSerializer.Deserialize<T>(data);
        }

        private static byte[] ReadStream(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (var ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }
    }
}
