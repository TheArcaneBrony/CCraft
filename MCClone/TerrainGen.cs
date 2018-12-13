using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace MCClone
{
    class TerrainGen
    {
        public static void GenTerrain(List<Chunk> chunkList)
        {
                for (int x = -16; x < 16; x++)
                {
                    for (int z = -16; z < 16; z++)
                    {
                        GetChunk(chunkList, x, z);
                    }
                }
        }
        public static Chunk GenChunk(List<Chunk> chunkList,int X, int Z)
        {
            Chunk chunk = new Chunk(X, Z);
            chunkList.Add(chunk);
            for (byte x2 = 0; x2 < 0x10; x2++)
            {
                for (byte z2 = 0; z2 < 0x10; z2++)
                {
                    UInt16 by = GetHeight(X * 16 + x2, Z * 16 + z2);
                    by = Math.Max(by, (UInt16)1);
                    for (int y = by-1; y < by; y++)
                    {
                        chunk.Blocks.Add(new Block(x2, (UInt16)y, z2));
                    }
                }
            }
            try
            {
                Task.Run(() => { File.WriteAllText($"Worlds/{MainWindow.world.Name}/ChunkData/{X}.{Z}.json", JsonConvert.SerializeObject(chunk)); });
            }
            catch (IOException)
            {
                Console.WriteLine($"Failed to save chunk: {X}/{Z}");
            }
            catch
            {
                throw;
            }
            return chunk;
        }
        public static Chunk GetChunk(List<Chunk> chunkList, int X, int Z)
        {
            if (File.Exists($"Worlds/{MainWindow.world.Name}/ChunkData/{X}.{Z}.json"))
            {
                Chunk ch = JsonConvert.DeserializeObject<Chunk>(File.ReadAllText($"Worlds/{MainWindow.world.Name}/ChunkData/{X}.{Z}.json"));
                chunkList.Add(ch);
                Logger.LogQueue.Add($"Loaded chunk {X}/{Z}");
                return ch;
            }
            else
            {
                Logger.LogQueue.Add($"Generating chunk {X}/{Z}");
                return GenChunk(chunkList, X, Z);
            }
        }
        public static UInt16 GetHeight(int x, int z) => (UInt16)Math.Abs(((Math.Sin(Util.DegToRad(x)) * 25 + Math.Sin(Util.DegToRad(z)) * 10) * 1.2));// 2;
    }
}