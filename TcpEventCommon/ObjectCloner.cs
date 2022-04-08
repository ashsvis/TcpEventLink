using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace TcpEventCommon
{
    /// <summary>
    /// Глубокое клонирование для [Serializable] объектов
    /// </summary>
    public static class ObjectCloner
    {
        public static T DeepClone<T>(this T obj) where T : class
        {
            if (obj == null)
                return null;

            var bf = new BinaryFormatter();
            using (var stream = new MemoryStream())
            {
                bf.Serialize(stream, obj);
                stream.Position = 0;
                return (T)bf.Deserialize(stream);
            }
        }

        /// <summary>
        /// Сериализуем данные экземпляра типа в массив байтов
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static byte[] GetBytes<T>(this T obj) where T : class
        {
            if (obj == null)
                return null;
            var bf = new BinaryFormatter();
            using (var stream = new MemoryStream())
            {
                bf.Serialize(stream, obj);
                stream.Position = 0;
                return stream.GetBuffer();
            }
        }

        /// <summary>
        /// Десериализуем данные экземпляра типа из массива байтов
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static T SetData<T>(byte[] buffer) where T : class
        {
            if (buffer == null || buffer.Length == 0)
                return null;
            var bf = new BinaryFormatter();
            using (var stream = new MemoryStream(buffer))
            {
                stream.Position = 0;
                return (T)bf.Deserialize(stream);
            }
        }
    }
}
