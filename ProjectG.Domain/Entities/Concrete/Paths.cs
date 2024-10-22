using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectG.DomainLayer.Entities.Concrete
{
    public static class Paths
    {
        public static string AHMenuImagePath { get; set; } = string.Empty;
        public static string GeneralImagePath { get; set; } = string.Empty;
        public static string PostCancelImagePath { get; set; } = string.Empty;
        public static string MailBoxImagePath { get; set; } = string.Empty;

        public static string ChatWindowImagePath {  get; set; } = string.Empty;

        public static string MailBoxTextImagePath { get; set; } = string.Empty;

        //public static string MailBoxTextImagePath2 { get; set; } = string.Empty;
        public static string TesseractPath { get; set; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Tesseract-OCR", "tessdata");



    }
}
