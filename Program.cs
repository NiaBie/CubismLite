using System;
using CubismLite;

namespace CubismLite.Net
{
    class Program
    {
        static void PrintAbstract(Document doc)
        {
            Console.WriteLine("肖像尺寸: " + doc.Width + " * " + doc.Height);
            Console.WriteLine("\t参数(" + doc.Parameters.Length + "):");
            for (int i = 0, n = doc.Parameters.Length; i < n; ++i)
                Console.WriteLine("\t\t" + doc.Parameters[i].Name
                    + ": [范围 " + doc.Parameters[i].MinValue.ToString("F1")
                    + " - " + doc.Parameters[i].MaxValue.ToString("F1")
                    + "] [默认值 " + doc.Parameters[i].Default.ToString("F1") + "]");
            Console.WriteLine("\t图层(" + doc.Parts.Length + "):");
            for (int i = 0, n = doc.Parts.Length; i < n; ++i)
                Console.WriteLine("\t\t" + doc.Parts[i].Name
                    + ": " + doc.Parts[i].Deformers.Length
                    + " 个坐标系， " + doc.Parts[i].Components.Length + " 个组件。");
            Console.WriteLine();
        }

        static void Main(string[] args)
        {
            Document haru = Document.FromFile("../../../Assets/haru.moc");
            // PrintAbstract(haru);
            
            Document shizuku = Document.FromFile("../../../Assets/shizuku.moc");
              PrintAbstract(shizuku);

            Document wanko = Document.FromFile("../../../Assets/wanko.moc");
            if (wanko != null)
              PrintAbstract(wanko);

            Document hiyori = Document.FromFile("../../../Assets/Epsilon.moc");
            if (hiyori != null)
              PrintAbstract(hiyori);
        }
    }
}
