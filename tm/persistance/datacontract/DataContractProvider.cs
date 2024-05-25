using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace tm.persistance.datacontract
{
    public class DataContractProvider : IPersistanceProvider
    {

        private string path;

        public DataContractProvider(string path)
        {
            this.path = path;
        }

        public Game Load()
        {
            SerializationMethod serializationMethod = SerializationMethod.DataContractSerializer;

            Game loadObj = null;
            if (serializationMethod == SerializationMethod.DataContractSerializer)
            {
                using (ZipArchive zip = ZipFile.Open(path, ZipArchiveMode.Read))
                {
                    ZipArchiveEntry cFile = zip.GetEntry("save.save");
                    DataContractSerializer ser = new DataContractSerializer(typeof(Game));
                    loadObj = (Game)ser.ReadObject(cFile.Open());
                    zip.Dispose();
                }
            }
            if (serializationMethod == SerializationMethod.NewtonsoftJsonSerializer)
            {
                using (ZipArchive zip = ZipFile.Open(path, ZipArchiveMode.Read))
                {
                    ZipArchiveEntry cFile = zip.GetEntry("save.save");
                    StreamReader osr = new StreamReader(cFile.Open(), Encoding.Default);
                    string content = osr.ReadToEnd();
                    loadObj = JsonConvert.DeserializeObject<Game>(content, new JsonSerializerSettings()
                    { ReferenceLoopHandling = ReferenceLoopHandling.Ignore, TypeNameHandling = TypeNameHandling.Auto });
                    zip.Dispose();
                }
            }
            return loadObj;
        }

        public void Save(Game game)
        {
            SerializationMethod serializationMethod = SerializationMethod.DataContractSerializer;

            path = path.Replace(".csave", ".save");
            if (serializationMethod == SerializationMethod.DataContractSerializer)
            {
                using (FileStream writer = new FileStream(path, FileMode.Create, FileAccess.Write))
                {
                    DataContractSerializerSettings dcss = new DataContractSerializerSettings() { MaxItemsInObjectGraph = int.MaxValue, PreserveObjectReferences = true };
                    DataContractSerializer ser = new DataContractSerializer(typeof(Game), dcss);
                    ser.WriteObject(writer, game);
                }
            }
            if (serializationMethod == SerializationMethod.NewtonsoftJsonSerializer)
            {
                JsonSerializer jsonSerializer = new JsonSerializer();
                string output = JsonConvert.SerializeObject(game, Formatting.None, new JsonSerializerSettings()
                { ReferenceLoopHandling = ReferenceLoopHandling.Ignore, TypeNameHandling = TypeNameHandling.Auto });
                using (FileStream writer = new FileStream(path, FileMode.Create, FileAccess.Write))
                {
                    var sr = new StreamWriter(writer);
                    sr.Write(output);
                }
            }

            //ObjectGraphValidator ogv = new ObjectGraphValidator();
            //ogv.ValidateObjectGraph(game);

            /*FileStream fs = new FileStream(path.Replace(".csave", ".bin"), FileMode.Create);

            // Construct a BinaryFormatter and use it to serialize the data to the stream.
            BinaryFormatter formatter = new BinaryFormatter();
            try
            {
                formatter.Serialize(fs, game);
            }
            catch (SerializationException e)
            {
                Console.WriteLine("Failed to serialize. Reason: " + e.Message);
                throw;
            }
            finally
            {
                fs.Close();
            }*/


            if (File.Exists(path.Split('.')[0] + ".csave"))
            {
                File.Delete(path.Split('.')[0] + ".csave");
            }

            using (ZipArchive zip = ZipFile.Open(path.Split('.')[0] + ".csave", ZipArchiveMode.Create))
            {
                zip.CreateEntryFromFile(path, "save.save");
                File.Delete(path);
                zip.Dispose();
            }

        }
    }
}
