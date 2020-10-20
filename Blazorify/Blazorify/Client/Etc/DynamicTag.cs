using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System.Collections.Generic;

namespace Blazorify.Client.Etc
{
    public class DynamicTag : ComponentBase
    {
        public ElementReference Ref { get; private set; }

        [Parameter]
        public string Tag { get; set; }

        [Parameter]
        public RenderFragment ChildContent { get; set; }

        [Parameter(CaptureUnmatchedValues = true)]
        public Dictionary<string, object> Attributes { get; set; }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, Tag);
            if (Attributes != null)
            {
                builder.AddMultipleAttributes(2, Attributes);
            }
            builder.AddElementReferenceCapture(1, reference =>
            {
                Ref = reference;
            });
            if (ChildContent != null)
            {
                builder.AddContent(3, ChildContent);
            }
            builder.CloseElement();
        }
    }
}
