using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace BeatShift.GameDebugTools
{
    struct ProfileSection : IDisposable
    {
        
#if DEBUG
        private String SectionName;
#endif
        public ProfileSection(String sectionName, Color rulerColour)//,TimeRuler rulerToUse)
        {
#if DEBUG
            this.SectionName = sectionName;
            DebugSystem.Instance.TimeRuler.BeginMark(SectionName,rulerColour);
#endif
        }

        public void Dispose()
        {
#if DEBUG
            DebugSystem.Instance.TimeRuler.EndMark(SectionName);
#endif
        }
    }
}
