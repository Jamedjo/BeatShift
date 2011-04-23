using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;

namespace BeatShiftProcessors
{
    [ContentProcessor(DisplayName = "Numbers Font Texture Processor")]
    public class NumberFontProcessor : FontTextureProcessor
    {
        protected override char GetCharacterForIndex(int index)
        {
            return (char)('0' + index);
        }
    }
}
