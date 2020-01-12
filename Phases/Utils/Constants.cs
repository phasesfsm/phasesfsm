using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Phases.DrawableObjects;

namespace Phases
{
    sealed class Constants
    {
        public sealed class ImageIndex
        {
            public const int Transitions = 0;
            public const int Transition = 1;
            public const int SuperTransition = 2;

            public const int States = 3;
            public const int State = 4;
            public const int SuperState = 5;
            public const int Nested = 6;

            public const int Links = 7;
            public const int Origin = 8;
            public const int Abort = 9;
            public const int Alias = 10;
            public const int End = 11;

            public const int From = 12;
            public const int To = 13;
            public const int In = 14;
            public const int Out = 15;

            public const int Sheet = 16;
            public const int SubSheet = 17;

            public const int Texts = 18;

            public static int Get(DrawableObject @object)
            {
                if (@object is SimpleTransition) return ImageIndex.Transition;
                if (@object is SimpleState) return ImageIndex.State;
                if (@object is Alias) return ImageIndex.Alias;
                if (@object is Origin) return ImageIndex.Origin;
                if (@object is End) return ImageIndex.End;
                if (@object is Abort) return ImageIndex.Abort;
                if (@object is SuperState) return ImageIndex.SuperState;
                if (@object is SuperTransition) return ImageIndex.SuperTransition;
                if (@object is Nested) return ImageIndex.Nested;
                if (@object is Relation) return ImageIndex.Transition;
                if (@object is Text) return ImageIndex.Texts;
                if (@object is StateAlias) return ImageIndex.Alias;
                if (@object is Equation) return ImageIndex.Texts;
                throw new Exception("Missing object type.");
            }
        }
    }
}
