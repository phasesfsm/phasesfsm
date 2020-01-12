using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Phases
{
    sealed class Serialization
    {
        /// <summary>Desc
        /// Document structure based on tokens
        /// </summary>
        public sealed class Token
        {
            public const byte InvalidToken = 0x00;

            //File types id
            public const byte EncryptedCompressedFile = 0xA9;
            public const byte CompressedData = 0xAC;
            public const byte EncryptedData = 0xAE;

            //Document specifics
            public const byte StartFile = 0x40; //Includes file version

            //Document info
            //File version 2
            public const byte CodeGenerationLanguage = 0x10;
            public const byte CodeGenerationFolder = 0x11;
            public const byte CodeGenerationExec = 0x12;

            //Book info
            public const byte StartBookInformation = 0x41;
            //Book parameters
            public const byte DefaultBookSheetSize = 0x42;

            public const byte EndBookInformation = 0x4f;

            //Book variables
            public const byte StartBookVariables = 0x50;

                public const byte BooleanInputVariable = 0x51;
                public const byte EventInputVariable = 0x52;
                public const byte MessageFlagVariable = 0x53;
                public const byte BooleanOutputVariable = 0x54;
                public const byte EventOutputVariable = 0x55;
                public const byte BooleanFlagVariable = 0x56;
                public const byte FlipFlopFlagVariable = 0x57;
                public const byte CounterFlagVariable = 0x58;

            public static bool IsVariable(byte[] data, int index)
            {
                return Is(data, index, BooleanInputVariable, CounterFlagVariable);
            }

            public const byte VariablesRelation = 0x5d;
            public const byte EndBookVariables = 0x5e;

            //First Sheets definitions
            public const byte StartSheetDefinition = 0x20;
            //Sheet parameters
            public const byte SheetName = 0x21;
            public const byte SheetSize = 0x22;
            public const byte SheetZoom = 0x23;
            public const byte SheetViewPoint = 0x24;
            public const byte SheetGrid = 0x25;

            public const byte StartDrawObjects = 0x30;

                //Second all object base definitions (repeated for each object)
                public const byte StartObjectDefinition = 0x31;
                //using Object base parameters
                public const byte EndObjectDefinition = 0x32;

                //Forth all objects specific parameters
                public const byte StartObjectParameters = 0x33;
                //ObjectID
                //using Object specific parameters
                public const byte EndObjectParameters = 0x34;

                //Finaly all object relations
                public const byte StartObjectRelations = 0x35;
                //ObjectID
                //object relation types
                public const byte EndObjectRelations = 0x36;

            public const byte EndDrawObjects = 0x37;

            //Sheet objects
            public const byte EndSheetDefinition = 0x3f;

            //Object base parameters
            public const byte ObjectType = 0x60;
            public const byte ObjectId = 0x61;
            public const byte ObjectName = 0x62;
            public const byte ObjectDescription = 0x63;

            //Object specific parameters
            public const byte StringValue = 0x70;    //Lenght; bytes
            public const byte BoolValue = 0x71;
            public const byte ByteValue = 0x72;
            public const byte IntValue = 0x73;
            public const byte FloatValue = 0x74;
            public const byte DoubleValue = 0x75;
            public const byte PointValue = 0x76;
            public const byte PointFValue = 0x77;
            public const byte SizeValue = 0x78;
            public const byte SizeFValue = 0x79;
            public const byte RectangleValue = 0x7a;
            public const byte RectangleFValue = 0x7b;
            public const byte DateTime = 0x7c;

            public const byte ValuesList = 0x7f;  //List, ValueTypeToken, Length[1], values ...

            //Object relation types
            public const byte RelationObject = 0x80; //ObjectID
            public const byte RelationName = 0x81; //Object name
            //public const byte RelationListStart = 0x82;
            //ObjectID
            //...
            //public const byte RelationListEnd = 0x80;

            public const byte StartGlobalPriorityList = 0x90;
            public const byte EndGlobalPriorityList = 0x91;

            public const byte EndFile = 0x5f;

            public static bool Is(byte[] data, int index, byte token)
            {
                return index < data.Length && data[index] == token;
            }

            public static bool Is(byte[] data, int index, byte fromToken, byte toToken)
            {
                return index < data.Length && data[index] >= fromToken && data[index] <= toToken;
            }

            public static bool Deserialize(byte[] data, ref int index, byte token)
            {
                if(index < data.Length && data[index] == token)
                {
                    index++;
                    return true;
                }
                return false;
            }

            public static bool Deserialize(byte[] data, int index, out byte token)
            {
                token = InvalidToken;
                if (index >= data.Length) return false;
                token = data[index];
                index++;
                return true;
            }
        }

        public static byte[] SerializeId(int id)
        {
            return BitConverter.GetBytes(id);
        }

        public static bool DeserializeId(byte[] data, ref int index, ref int id)
        {
            if (index >= data.Length) return false;
            id = BitConverter.ToInt32(data, index);
            index += 4;
            return true;
        }

        public static byte[] SerializeParameter(string value)
        {
            var data = new List<byte>();
            data.Add(Token.StringValue);
            var text = Encoding.Unicode.GetBytes(value);
            data.AddRange(BitConverter.GetBytes((short)text.Length));
            data.AddRange(text);
            return data.ToArray();
        }

        public static byte[] SerializeParameter(string[] value)
        {
            var data = new List<byte>();
            data.Add(Token.ValuesList);
            data.Add((byte)value.Length);
            foreach(string str in value)
            {
                data.Add(Token.StringValue);
                var text = Encoding.Unicode.GetBytes(str);
                data.AddRange(BitConverter.GetBytes((short)text.Length));
                data.AddRange(text);
            }
            return data.ToArray();
        }

        public static byte[] SerializeParameter(bool value)
        {
            var data = new List<byte>();
            data.Add(Token.BoolValue);
            data.AddRange(BitConverter.GetBytes(value));
            return data.ToArray();
        }

        public static byte[] SerializeParameter(byte value)
        {
            var data = new List<byte>();
            data.Add(Token.ByteValue);
            data.Add(value);
            return data.ToArray();
        }

        public static byte[] SerializeParameter(int value)
        {
            var data = new List<byte>();
            data.Add(Token.IntValue);
            data.AddRange(BitConverter.GetBytes(value));
            return data.ToArray();
        }

        public static byte[] SerializeParameter(float value)
        {
            var data = new List<byte>();
            data.Add(Token.FloatValue);
            data.AddRange(BitConverter.GetBytes(value));
            return data.ToArray();
        }

        public static byte[] SerializeParameter(double value)
        {
            var data = new List<byte>();
            data.Add(Token.DoubleValue);
            data.AddRange(BitConverter.GetBytes(value));
            return data.ToArray();
        }

        public static byte[] SerializeParameter(Point point)
        {
            var data = new List<byte>();
            data.Add(Token.PointValue);
            data.AddRange(BitConverter.GetBytes(point.X));
            data.AddRange(BitConverter.GetBytes(point.Y));
            return data.ToArray();
        }

        public static byte[] SerializeParameter(PointF point)
        {
            var data = new List<byte>();
            data.Add(Token.PointFValue);
            data.AddRange(BitConverter.GetBytes(point.X));
            data.AddRange(BitConverter.GetBytes(point.Y));
            return data.ToArray();
        }

        public static byte[] SerializeParameter(Size size)
        {
            var data = new List<byte>();
            data.Add(Token.SizeValue);
            data.AddRange(BitConverter.GetBytes(size.Width));
            data.AddRange(BitConverter.GetBytes(size.Height));
            return data.ToArray();
        }

        public static byte[] SerializeParameter(SizeF size)
        {
            var data = new List<byte>();
            data.Add(Token.SizeFValue);
            data.AddRange(BitConverter.GetBytes(size.Width));
            data.AddRange(BitConverter.GetBytes(size.Height));
            return data.ToArray();
        }

        public static byte[] SerializeParameter(Rectangle rect)
        {
            var data = new List<byte>();
            data.Add(Token.RectangleValue);
            data.AddRange(BitConverter.GetBytes(rect.X));
            data.AddRange(BitConverter.GetBytes(rect.Y));
            data.AddRange(BitConverter.GetBytes(rect.Width));
            data.AddRange(BitConverter.GetBytes(rect.Height));
            return data.ToArray();
        }

        public static byte[] SerializeParameter(RectangleF rect)
        {
            var data = new List<byte>();
            data.Add(Token.RectangleFValue);
            data.AddRange(BitConverter.GetBytes(rect.X));
            data.AddRange(BitConverter.GetBytes(rect.Y));
            data.AddRange(BitConverter.GetBytes(rect.Width));
            data.AddRange(BitConverter.GetBytes(rect.Height));
            return data.ToArray();
        }

        public static byte[] SerializeParameter(DateTime date)
        {
            var data = new List<byte>();
            data.Add(Token.DateTime);
            data.AddRange(BitConverter.GetBytes(date.ToBinary()));
            return data.ToArray();
        }

        public static bool DeserializeParameter(byte[] data, ref int index, ref string value)
        {
            if (data[index] != Serialization.Token.StringValue) return false;
            index++;
            int len = BitConverter.ToInt16(data, index);
            index += 2;
            value = Encoding.Unicode.GetString(data, index, len);
            index += len;
            return true;
        }

        public static bool DeserializeParameter(byte[] data, ref int index, out string[] value)
        {
            value = null;
            if (data[index] != Serialization.Token.ValuesList) return false;
            index++;
            int list_len = data[index++];
            value = new string[list_len];
            int len;
            for(int i = 0; i < value.Length; i++)
            {
                if (data[index] != Serialization.Token.StringValue) return false;
                index++;
                len = BitConverter.ToInt16(data, index);
                index += 2;
                value[i] = Encoding.Unicode.GetString(data, index, len);
                index += len;
            }
            return true;
        }

        public static bool DeserializeParameter(byte[] data, ref int index, out bool value)
        {
            value = false;
            if (data[index] != Serialization.Token.BoolValue) return false;
            index++;
            value = BitConverter.ToBoolean(data, index);
            index++;
            return true;
        }

        public static bool DeserializeParameter(byte[] data, ref int index, ref byte value)
        {
            if (data[index] != Serialization.Token.ByteValue) return false;
            index++;
            value = data[index];
            index++;
            return true;
        }

        public static bool DeserializeParameter(byte[] data, ref int index, ref int value)
        {
            if (data[index] != Serialization.Token.IntValue) return false;
            index++;
            value = BitConverter.ToInt32(data, index);
            index += 4;
            return true;
        }

        public static bool DeserializeParameter(byte[] data, ref int index, ref float value)
        {
            if (data[index] != Serialization.Token.FloatValue) return false;
            index++;
            value = BitConverter.ToSingle(data, index);
            index += 4;
            return true;
        }

        public static bool DeserializeParameter(byte[] data, ref int index, ref double value)
        {
            if (data[index] != Serialization.Token.DoubleValue) return false;
            index++;
            value = BitConverter.ToDouble(data, index);
            index += 8;
            return true;
        }

        public static bool DeserializeParameter(byte[] data, ref int index, ref Point value)
        {
            if (data[index] != Serialization.Token.PointValue) return false;
            index++;
            value = new Point(BitConverter.ToInt32(data, index), BitConverter.ToInt32(data, index + 4));
            index += 8;
            return true;
        }

        public static bool DeserializeParameter(byte[] data, ref int index, ref PointF value)
        {
            if (data[index] != Serialization.Token.PointFValue) return false;
            index++;
            value = new PointF(BitConverter.ToSingle(data, index), BitConverter.ToSingle(data, index + 4));
            index += 8;
            return true;
        }

        public static bool DeserializeParameter(byte[] data, ref int index, ref Size value)
        {
            if (data[index] != Serialization.Token.SizeValue) return false;
            index++;
            value = new Size(BitConverter.ToInt32(data, index), BitConverter.ToInt32(data, index + 4));
            index += 8;
            return true;
        }

        public static bool DeserializeParameter(byte[] data, ref int index, ref SizeF value)
        {
            if (data[index] != Serialization.Token.SizeFValue) return false;
            index++;
            value = new SizeF(BitConverter.ToSingle(data, index), BitConverter.ToSingle(data, index + 4));
            index += 8;
            return true;
        }

        public static bool DeserializeParameter(byte[] data, ref int index, ref Rectangle value)
        {
            if (data[index] != Serialization.Token.RectangleValue) return false;
            index++;
            value = new Rectangle(BitConverter.ToInt32(data, index), BitConverter.ToInt32(data, index + 4),
                BitConverter.ToInt32(data, index + 8), BitConverter.ToInt32(data, index + 12));
            index += 16;
            return true;
        }

        public static bool DeserializeParameter(byte[] data, ref int index, ref RectangleF value)
        {
            if (data[index] != Serialization.Token.RectangleFValue) return false;
            index++;
            value = new RectangleF(BitConverter.ToSingle(data, index), BitConverter.ToSingle(data, index + 4),
                BitConverter.ToSingle(data, index + 8), BitConverter.ToSingle(data, index + 12));
            index += 16;
            return true;
        }

        public static bool DeserializeParameter(byte[] data, ref int index, ref DateTime value)
        {
            if (data[index] != Serialization.Token.DateTime) return false;
            index++;
            value = DateTime.FromBinary(BitConverter.ToInt64(data, index));
            index += 8;
            return true;
        }

    }
}
