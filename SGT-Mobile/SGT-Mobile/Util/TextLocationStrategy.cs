using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Data;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace SGTMobile.Util
{
    class TextLocationStrategy : LocationTextExtractionStrategy
    {
        public List<textChunk> objectResult = new List<textChunk>();

        public override void EventOccurred(IEventData data, EventType type)
        {
            if (!type.Equals(EventType.RENDER_TEXT))
                return;

            TextRenderInfo renderInfo = (TextRenderInfo)data;

            string curFont = renderInfo.GetFont().GetFontProgram().ToString();

            float curFontSize = renderInfo.GetFontSize();

            IList<TextRenderInfo> text = renderInfo.GetCharacterRenderInfos();
            foreach (TextRenderInfo t in text)
            {
                string letter = t.GetText();
                Vector letterStart = t.GetBaseline().GetStartPoint();
                Vector letterEnd = t.GetAscentLine().GetEndPoint();
                Rectangle letterRect = new Rectangle(letterStart.Get(0), letterStart.Get(1), letterEnd.Get(0) - letterStart.Get(0), letterEnd.Get(1) - letterStart.Get(1));

                if (letter != " " && !letter.Contains(' '))
                {
                    textChunk chunk = new textChunk();
                    chunk.text = letter;
                    chunk.rect = letterRect;
                    chunk.fontFamily = curFont;
                    chunk.fontSize = (int)curFontSize;
                    chunk.spaceWidth = t.GetSingleSpaceWidth() / 2f;

                    objectResult.Add(chunk);
                }
            }
        }
    }
    public class textChunk
    {
        public string text { get; set; }
        public Rectangle rect { get; set; }
        public string fontFamily { get; set; }
        public int fontSize { get; set; }
        public float spaceWidth { get; set; }
    }
}
