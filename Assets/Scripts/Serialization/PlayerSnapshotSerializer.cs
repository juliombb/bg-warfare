using System;
using System.IO;
using LiteNetLib.Utils;
using UnityEngine;

namespace DefaultNamespace.Serialization
{
    public static class PlayerSnapshotSerializer
    {
        public static void PutPlayerSnapshot(this NetDataWriter writer, PlayerSnapshot snapshot)
        {
            writer.Put(BitConverter.GetBytes(snapshot.Sequence), 0 , 4);
            writer.Put(BitConverter.GetBytes(snapshot.Position.x), 0 , 4);
            writer.Put(BitConverter.GetBytes(snapshot.Position.y), 0 , 4);
            writer.Put(BitConverter.GetBytes(snapshot.Position.z), 0 , 4);
            writer.Put(BitConverter.GetBytes(snapshot.Rotation.y), 0 , 4);
        }

        public static PlayerSnapshot ReadPlayerSnapshot(this BinaryReader stream)
        {
            return new PlayerSnapshot(
                sequence: stream.ReadInt32(),
                position: new Vector3(stream.ReadSingle(), stream.ReadSingle(), stream.ReadSingle()),
                rotation: Quaternion.Euler(0, stream.ReadSingle(), 0)
            );
        }
    }
}