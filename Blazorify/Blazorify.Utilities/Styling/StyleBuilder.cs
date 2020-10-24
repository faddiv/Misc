﻿using System;

namespace Blazorify.Utilities.Styling
{
    public class StyleBuilder : IStyleBuilder
    {
        public StyleDefinition Create()
        {
            return new StyleDefinition();
        }

        public StyleDefinition this[params object[] values]
        {
            get
            {
                return Create().AddMultiple(values);
            }
        }

        public StyleDefinition this[params (string, string, Func<bool>)[] values]
        {
            get
            {
                return Create().AddMultiple(values);
            }
        }

        public StyleDefinition this[params (string, Func<string>, bool)[] values]
        {
            get
            {
                return Create().AddMultiple(values);
            }
        }

        public StyleDefinition this[params (string, Func<string>, Func<bool>)[] values]
        {
            get
            {
                return Create().AddMultiple(values);
            }
        }

    }
}
