using System.Drawing;

namespace FileVisor.Models
{
    internal class CreatedElementType
    {
        public enum ElementType
        {
            Directory, TXT, RTF, BMP, Other
        }

        public ElementType ID { get; set; }
        public Icon Icon { get; set; }
        public string Name { get; set; }
        public string Extension { get; set; }
    }
}
