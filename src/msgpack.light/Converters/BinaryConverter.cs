using System;

namespace MsgPack.Light.Converters
{
    internal class BinaryConverter : IMsgPackConverter<byte[]>
    {
        private MsgPackContext _context;

        public void Initialize(MsgPackContext context)
        {
            _context = context;
        }

        public void Write(byte[] value, IMsgPackWriter writer)
        {
            if (value == null)
            {
                _context.NullConverter.Write(value, writer);
                return;
            }

            WriteBinaryHeaderAndLength((uint)value.Length, writer);

            writer.Write(value);
        }

        // We will have problem with binary blobs greater than int.MaxValue bytes.
        public byte[] Read(IMsgPackReader reader)
        {
            var type = reader.ReadDataType();

            uint length;
            switch (type)
            {
                case DataTypes.Null:
                    return null;

                case DataTypes.Bin8:
                    length = IntConverter.ReadUInt8(reader);
                    break;

                case DataTypes.Bin16:
                    length = IntConverter.ReadUInt16(reader);
                    break;

                case DataTypes.Bin32:
                    length = IntConverter.ReadUInt32(reader);
                    break;

                default:
                    throw ExceptionUtils.BadTypeException(type, DataTypes.Bin8, DataTypes.Bin16, DataTypes.Bin32, DataTypes.Null);
            }

            var segment = reader.ReadBytes(length);
            var array = new byte[segment.Count];
            Array.Copy(segment.Array, segment.Offset, array, 0, segment.Count);
            return array;
        }

        private void WriteBinaryHeaderAndLength(uint length, IMsgPackWriter writer)
        {
            if (length <= byte.MaxValue)
            {
                writer.Write(DataTypes.Bin8);
                IntConverter.WriteByteValue((byte) length, writer);
            }
            else if (length <= ushort.MaxValue)
            {
                writer.Write(DataTypes.Bin16);
                IntConverter.WriteUShortValue((ushort) length, writer);
            }
            else
            {
                writer.Write(DataTypes.Bin32);
                IntConverter.WriteUIntValue(length, writer);
            }
        }
    }
}