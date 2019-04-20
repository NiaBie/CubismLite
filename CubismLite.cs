using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CubismLite
{
    public class Parameter
    {
        public string Name { get; set; }
        public float MinValue { get; set; }
        public float MaxValue { get; set; }
        public float Default { get; set; }
    }

    public class ParameterSample
    {
        public string Name { get; set; }
        public int Count { get; set; }
        public float[] Values { get; set; }
    }

    public class ParameterSamples
    {
        public ParameterSample[] Data { get; set; }
    }

    public class RotationDeformer
    {
        public class Desc
        {
            public float CenterX { get; set; }
            public float CenterY { get; set; }
            public float ScaleX { get; set; }
            public float ScaleY { get; set; }
            public float Rotation { get; set; }
        }
        public string Name { get; set; }
        public string Parent { get; set; }
        public ParameterSamples Samples { get; set; }
        public Desc[] Data { get; set; }
    }

    public class CurvedSurfaceDeformer
    {
        public string Name { get; set; }
        public string Parent { get; set; }
        public int DivisionX { get; set; }
        public int DivisionY { get; set; }
        public ParameterSamples Samples { get; set; }
        public float[][] Data { get; set; }
    }

    public class Component
    {
        public string Name { get; set; }
        public string Parent { get; set; }
        public ParameterSamples Samples { get; set; }

        public int Order { get; set; }
        public int[] Orders { get; set; }
        public float[] Opacities { get; set; }

        public int TextureId { get; set; }
        public int VertexCount { get; set; }
        public int TriangleCount { get; set; }
        public int[] Indices { get; set; }
        public float[][] Data { get; set; }
        public float[] Uvs { get; set; }

        public int ColorCompositionType { get; set; }
    }

    public class Part
    {
        public byte Status { get; set; }
        public string Name { get; set; }
        public object[] Deformers { get; set; }
        public Component[] Components { get; set; }

        public bool IsVisible { get { return (Status & 0x40) > 0; } }
        public bool IsLocked { get { return (Status & 0x80) > 0; } }
    }

    public class Document
    {
        public Parameter[] Parameters { get; set; }
        public Part[] Parts { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        List<object> Objects = new List<object>() { null };

        public static Document FromFile(string fileName)
        {
            Document result = null;
            using (Stream s = File.OpenRead(fileName))
            using (BinaryReader br = new BinaryReader(s))
                if (br.ReadInt32() == 0x09636f6d)
                    result = SerializationHelper.DeserializeDocument(br);
            return result;
        }
        public object DeserializeQuery(BinaryReader br)
        {
            object result = null;

            switch (br.ReadByte())
            {
                case 0x03: result = SerializationHelper.DeserializeParameter(br, this); break;
                case 0x05: result = SerializationHelper.DeserializePart(br, this); break;
                case 0x0f: result = SerializationHelper.DeserializeObjectArray(br, this); break;
                case 0x19: result = SerializationHelper.DeserializeInt32Array(br); break;
                case 0x1b: result = SerializationHelper.DeserializeSingleArray(br); break;
                case 0x21: return Objects[SerializationHelper.DeserializeInt32(br)];
                case 0x06:
                case 0x32:
                case 0x33:
                case 0x3c:
                    result = SerializationHelper.DeserializeString(br); break;
                case 0x41: result = SerializationHelper.DeserializeCurvedSurfaceDeformer(br, this); break;
                case 0x42: result = SerializationHelper.DeserializeParameterSamples(br, this); break;
                case 0x43: result = SerializationHelper.DeserializeParameterSample(br, this); break;
                case 0x44: result = SerializationHelper.DeserializeRotationDeformer(br, this); break;
                case 0x45: result = SerializationHelper.DeserializeRotationDeformerDesc(br); break;
                case 0x46: result = SerializationHelper.DeserializeComponent(br, this); break;
                case 0x81: return DeserializeQuery(br);
                default: break;
            }
            Objects.Add(result);

            return result;
        }
    }

    public static class SerializationHelper
    {
        public static int DeserializeInt32(BinaryReader br)
        {
            byte[] data = br.ReadBytes(4);
            Array.Reverse(data, 0, 4);
            return BitConverter.ToInt32(data, 0);
        }
        public static float DeserializeSingle(BinaryReader br)
        {
            byte[] data = br.ReadBytes(4);
            Array.Reverse(data, 0, 4);
            return BitConverter.ToSingle(data, 0);
        }
        public static int DeserializeArrayLength(BinaryReader br)
        {
            int result = 0;
            byte b;
            do
            {
                b = br.ReadByte();
                result = (result << 7) + (b & 0x7f);
            } while (b >= 0x80);
            return result;
        }

        public static string DeserializeString(BinaryReader br)
        {
            int length = DeserializeArrayLength(br);
            return Encoding.ASCII.GetString(br.ReadBytes(length), 0, length);
        }
        public static int[] DeserializeInt32Array(BinaryReader br)
        {
            int[] result = new int[DeserializeArrayLength(br)];
            for (int i = 0, n = result.Length; i < n; ++i)
                result[i] = DeserializeInt32(br);
            return result;
        }
        public static float[] DeserializeSingleArray(BinaryReader br)
        {
            float[] result = new float[DeserializeArrayLength(br)];
            for (int i = 0, n = result.Length; i < n; ++i)
                result[i] = DeserializeSingle(br);
            return result;
        }
        public static object[] DeserializeObjectArray(BinaryReader br, Document doc)
        {
            object[] result = new object[DeserializeArrayLength(br)];
            for (int i = 0, n = result.Length; i < n; ++i)
                result[i] = doc.DeserializeQuery(br);
            return result;
        }

        public static Parameter DeserializeParameter(BinaryReader br, Document doc)
        {
            Parameter result = new Parameter();
            result.MinValue = DeserializeSingle(br);
            result.MaxValue = DeserializeSingle(br);
            result.Default = DeserializeSingle(br);
            result.Name = (string)doc.DeserializeQuery(br);
            return result;
        }
        public static ParameterSample DeserializeParameterSample(BinaryReader br, Document doc)
        {
            ParameterSample result = new ParameterSample();
            result.Name = (string)doc.DeserializeQuery(br);
            result.Count = DeserializeInt32(br);
            result.Values = (float[])doc.DeserializeQuery(br);
            return result;
        }
        public static ParameterSamples DeserializeParameterSamples(BinaryReader br, Document doc)
        {
            ParameterSamples result = new ParameterSamples();
            result.Data = ToParameterSampleArray((object[])doc.DeserializeQuery(br));
            return result;
        }
        public static RotationDeformer.Desc DeserializeRotationDeformerDesc(BinaryReader br)
        {
            RotationDeformer.Desc result = new RotationDeformer.Desc();
            result.CenterX = DeserializeSingle(br);
            result.CenterY = DeserializeSingle(br);
            result.ScaleX = DeserializeSingle(br);
            result.ScaleY = DeserializeSingle(br);
            result.Rotation = DeserializeSingle(br);
            return result;
        }
        public static RotationDeformer DeserializeRotationDeformer(BinaryReader br, Document doc)
        {
            RotationDeformer result = new RotationDeformer();
            result.Name = (string)doc.DeserializeQuery(br);
            result.Parent = (string)doc.DeserializeQuery(br);
            result.Samples = (ParameterSamples)doc.DeserializeQuery(br);
            result.Data = ToRotationDeformerDescArray((object[])doc.DeserializeQuery(br));
            return result;
        }
        public static CurvedSurfaceDeformer DeserializeCurvedSurfaceDeformer(BinaryReader br, Document doc)
        {
            CurvedSurfaceDeformer result = new CurvedSurfaceDeformer();
            result.Name = (string)doc.DeserializeQuery(br);
            result.Parent = (string)doc.DeserializeQuery(br);
            result.DivisionX = DeserializeInt32(br);
            result.DivisionY = DeserializeInt32(br);
            result.Samples = (ParameterSamples)doc.DeserializeQuery(br);
            result.Data = ToSingleArrayArray((object[])doc.DeserializeQuery(br));
            return result;
        }
        public static Component DeserializeComponent(BinaryReader br, Document doc)
        {
            Component result = new Component();
            result.Name = (string)doc.DeserializeQuery(br);
            result.Parent = (string)doc.DeserializeQuery(br);
            result.Samples = (ParameterSamples)doc.DeserializeQuery(br);
            result.Order = DeserializeInt32(br);
            result.Orders = DeserializeInt32Array(br);
            result.Opacities = DeserializeSingleArray(br);
            result.TextureId = DeserializeInt32(br);
            result.VertexCount = DeserializeInt32(br);
            result.TriangleCount = DeserializeInt32(br);
            result.Indices = (int[])doc.DeserializeQuery(br);
            result.Data = ToSingleArrayArray((object[])doc.DeserializeQuery(br));
            result.Uvs = (float[])doc.DeserializeQuery(br);
            result.ColorCompositionType = DeserializeInt32(br);
            return result;
        }
        public static Part DeserializePart(BinaryReader br, Document doc)
        {
            Part result = new Part();
            result.Status = br.ReadByte();
            result.Name = (string)doc.DeserializeQuery(br);
            result.Deformers = (object[])doc.DeserializeQuery(br);
            result.Components = ToComponentArray((object[])doc.DeserializeQuery(br));
            return result;
        }
        public static Document DeserializeDocument(BinaryReader br)
        {
            Document result = new Document();
            byte[] reserved = br.ReadBytes(4);
            result.Parameters = ToParameterArray((object[])result.DeserializeQuery(br));
            result.Parts = ToPartArray((object[])result.DeserializeQuery(br));
            result.Width = DeserializeInt32(br);
            result.Height = DeserializeInt32(br);
            reserved = br.ReadBytes(4);
            return result;
        }

        static float[][] ToSingleArrayArray(object[] value)
        {
            float[][] result = new float[value.Length][];
            for (int i = 0; i < result.Length; ++i)
                result[i] = (float[])value[i];
            return result;
        }
        static Parameter[] ToParameterArray(object[] value)
        {
            Parameter[] result = new Parameter[value.Length];
            for (int i = 0; i < result.Length; ++i)
                result[i] = (Parameter)value[i];
            return result;
        }
        static ParameterSample[] ToParameterSampleArray(object[] value)
        {
            ParameterSample[] result = new ParameterSample[value.Length];
            for (int i = 0; i < result.Length; ++i)
                result[i] = (ParameterSample)value[i];
            return result;
        }
        static RotationDeformer.Desc[] ToRotationDeformerDescArray(object[] value)
        {
            RotationDeformer.Desc[] result = new RotationDeformer.Desc[value.Length];
            for (int i = 0; i < result.Length; ++i)
                result[i] = (RotationDeformer.Desc)value[i];
            return result;
        }
        static Component[] ToComponentArray(object[] value)
        {
            Component[] result = new Component[value.Length];
            for (int i = 0; i < result.Length; ++i)
                result[i] = (Component)value[i];
            return result;
        }
        static Part[] ToPartArray(object[] value)
        {
            Part[] result = new Part[value.Length];
            for (int i = 0; i < result.Length; ++i)
                result[i] = (Part)value[i];
            return result;
        }
    }
}
