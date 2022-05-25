using System;
using System.IO;

namespace DefaultNamespace
{
    public static class Util
    {
        public static bool HasNext(this BinaryReader reader)
        {
            return reader.BaseStream.Position < reader.BaseStream.Length - 1;
        }
    }
}