﻿using System.Diagnostics;
namespace IMGUI
{
    internal class ToggleButton : Toggle
    {
        public ToggleButton(string name, BaseForm form, bool value, string text, Rect rect)
            : base(name, form, value, text, rect)
        {

        }
        
        public override void OnRender(Cairo.Context g)
        {
            g.DrawBoxModel(Rect, new Content(Layout),
                Result ? Skin.current.Button["Active"] : Skin.current.Button["Normal"]);
        }

        public static new bool DoControl(Cairo.Context g, BaseForm form, Rect rect, string text, bool value, string name)
        {
            if (!form.Controls.ContainsKey(name))
            {
                var toggle = new ToggleButton(name, form, value, text, rect);
                Debug.Assert(toggle != null);
                toggle.OnUpdate();
                toggle.OnRender(g);
            }

            var control = form.Controls[name] as ToggleButton;
            Debug.Assert(control != null);
            control.Active = true;

            return control.Result;
        }
    }
}