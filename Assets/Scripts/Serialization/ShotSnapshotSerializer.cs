using System;
using System.IO;
using LiteNetLib.Utils;
using Model;
using UnityEngine;

namespace Serialization
{
    public static class ShotSnapshotSerializer
    {
        public static void PutShotSnapshot(this NetDataWriter writer, ShotSnapshot snapshot)
        {
            writer.Put(BitConverter.GetBytes(snapshot.Target), 0 , 4);
            writer.Put(BitConverter.GetBytes(snapshot.Position.x), 0 , 4);
            writer.Put(BitConverter.GetBytes(snapshot.Position.y), 0 , 4);
            writer.Put(BitConverter.GetBytes(snapshot.Position.z), 0 , 4);
            writer.Put(BitConverter.GetBytes(snapshot.Direction.x), 0 , 4);
            writer.Put(BitConverter.GetBytes(snapshot.Direction.y), 0 , 4);
            writer.Put(BitConverter.GetBytes(snapshot.Direction.z), 0 , 4);
        }

        public static ShotSnapshot ReadShotSnapshot(this BinaryReader stream)
        {
            return new ShotSnapshot(
                target: stream.ReadInt32(),
                position: new Vector3(stream.ReadSingle(), stream.ReadSingle(), stream.ReadSingle()),
                direction: new Vector3(stream.ReadSingle(), stream.ReadSingle(), stream.ReadSingle())
            );
        }
    }
}