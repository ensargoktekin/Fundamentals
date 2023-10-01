using System;
using System.Runtime.InteropServices;

namespace Game.Factory
{
    // New object should be added to "BlockMakeType" enum and
    // "BlockMakeSubType" struct
    [Serializable]
    public enum BlockMakeType
    {
        None,
        Simple,
        Rocket,
        Stone
    }

    [StructLayout(LayoutKind.Explicit)]
    [Serializable]
    public struct BlockMakeSubType
    {
        [FieldOffset(0)] [NonSerialized]
        public SimpleBlock.Type SimpleBlockType;
        [FieldOffset(0)] [NonSerialized]
        public Rocket.Type RocketType;
        [FieldOffset(0)] [NonSerialized]
        public Stone.Type StoneType;

        [FieldOffset(0)]
        [UnityEngine.SerializeField]
        private int _value;

        public override string ToString()
        {
            string str = "";
            str += "SimpleBlockType: " + SimpleBlockType + "\n";
            str += "RocketType: " + RocketType + "\n";
            str += "StoneType: " + StoneType + "\n";
            str += "_value: " + _value + "\n";
            return str;
        }
    }

    [Serializable]
    public struct BlockMakeParams
    {
        public BlockMakeType BlockType;
        public BlockMakeSubType SubType;

        public override string ToString()
        {
            string str = "";
            str += "BlockType: " + BlockType + "\n";
            str += "SubType: " + SubType + "\n";
            return str;
        }
    }
}
