﻿using System.Collections.Generic;
using ImGui.Rendering;

//BUG Hover state persists when move from mainRect to outside.
//BUG Abnormal representation when drag from mainRect to outside.
//TODO GUILayout version

namespace ImGui
{
    public partial class GUI
    {
        class ComboBoxContext
        {
            public string[] Texts { get; set; }
            public Form ItemsContainer { get; set; }

            public int SelectedIndex { get; set; }
            public Rect Rect { get; set; }

            public string Text { get; set; }
        }

        private static ComboBoxContext comboBoxContext;//TODO move this to Window.storage

        public static int ComboBox(Rect rect, string[] texts)
        {
            var window = GetCurrentWindow();
            if (window.SkipItems)
                return comboBoxContext.SelectedIndex;
            
            //get or create the root node
            var id = window.GetID(texts);
            var container = window.AbsoluteVisualList;
            var node = (Node)container.Find(visual => visual.Id == id);
            if (node == null)
            {
                //create button node
                node = new Node(id, $"Combobox<{texts[0]}..>");
                container.Add(node);
                node.UseBoxModel = true;
                node.RuleSet.Replace(GUISkin.Current[GUIControlName.ComboBox]);
                
                //set initial states TODO move to shared storage
                comboBoxContext = new ComboBoxContext();
                comboBoxContext.Rect = rect;
                comboBoxContext.Texts = texts;
                comboBoxContext.Text = texts[0];
                comboBoxContext.SelectedIndex = 0;
            }
            node.RuleSet.ApplyStack();
            node.ActiveSelf = true;

            // rect
            node.Rect = window.GetRect(rect);

            // interact
            var pressed = GUIBehavior.ButtonBehavior(node.Rect, node.Id, out var hovered, out var held);
            if(pressed)
            {
                var screenPos = Form.current.ClientToScreen(comboBoxContext.Rect.TopLeft);
                var screenRect = new Rect(screenPos, comboBoxContext.Rect.Size);
                Application.AddFrom(()=>
                {
                    var form = new ComboxBoxItemsForm(
                        screenRect,
                        comboBoxContext.Texts,
                        i => comboBoxContext.SelectedIndex = i);
                    comboBoxContext.ItemsContainer = form;
                    return form;
                });
                node.State = GUIState.Active;
            }

            if (node.State == Active)
            {
                comboBoxContext.ItemsContainer?.Show();
            }

            comboBoxContext.Text = comboBoxContext.Texts[comboBoxContext.SelectedIndex];
            
            // last item state
            window.TempData.LastItemState = node.State;

            //render
            using var g = node.RenderOpen();
            g.DrawBoxModel(comboBoxContext.Text, node.RuleSet, node.Rect);
            g.RenderArrow(node.Rect.Min + new Vector(node.PaddingLeft, 0), node.Height,
                node.RuleSet.FontColor, Internal.Direcion.Down, 1.0);

            return comboBoxContext.SelectedIndex;
        }
    }

    internal partial class GUISkin
    {
        private void InitComboBoxStyles(StyleRuleSet ruleSet)
        {
            StyleRuleSetBuilder builder = new StyleRuleSetBuilder(ruleSet);
            builder
                .Border(1.0, GUIState.Normal)
                .Border(1.0, GUIState.Hover)
                .Border(1.0, GUIState.Active)
                .Padding(5.0, GUIState.Normal)
                .Padding(5.0, GUIState.Hover)
                .Padding(5.0, GUIState.Active)
                .BorderColor(new Color(0.26f, 0.59f, 0.98f, 0.40f), GUIState.Normal)
                .BorderColor(new Color(0.26f, 0.59f, 0.98f, 1.00f), GUIState.Hover)
                .BorderColor(new Color(0.06f, 0.53f, 0.98f, 1.00f), GUIState.Active)
                .BackgroundColor(Color.Rgb(0x65a9d7), GUIState.Normal)
                .BackgroundColor(Color.Rgb(0x28597a), GUIState.Hover)
                .BackgroundColor(Color.Rgb(0x1b435e), GUIState.Active)
                .BackgroundGradient(Gradient.TopBottom)
                .FontColor(Color.Black)
                .AlignmentVertical(Alignment.Center, GUIState.Normal)
                .AlignmentVertical(Alignment.Center, GUIState.Hover)
                .AlignmentVertical(Alignment.Center, GUIState.Active)
                .AlignmentHorizontal(Alignment.Center, GUIState.Normal)
                .AlignmentHorizontal(Alignment.Center, GUIState.Hover)
                .AlignmentHorizontal(Alignment.Center, GUIState.Active)
                .GradientTopDownColor(Color.Rgb(247, 247, 247), Color.Rgb(221, 221, 221), GUIState.Normal)
                .GradientTopDownColor(Color.Rgb(247, 247, 247), Color.Rgb(221, 221, 221), GUIState.Hover)
                .GradientTopDownColor(Color.Rgb(222, 222, 222), Color.Rgb(248, 248, 248), GUIState.Active);
        }
    }

    internal sealed class ComboxBoxItemsForm : Form
    {
        private readonly List<string> textList;
        private System.Action<int> CallBack { get; set; }

        public ComboxBoxItemsForm(Rect rect, string[] texts, System.Action<int> callBack)
            : base(rect, "comboBoxForm", WindowTypes.ToolBox)
        {
            textList = new List<string>(texts);
            CallBack = callBack;
        }

        protected override void OnGUI()
        {
            GUILayout.BeginVertical("CombolBox");
            for (int i = 0; i < textList.Count; i++)
            {
                var itemRect = Rect;
                itemRect.Y += (i + 1) * Rect.Height;
                if(GUI.Button(new Rect(Rect.Width, itemRect.Height), textList[i]))
                {
                    if(CallBack!=null)
                    {
                        CallBack(i);
                    }
                    this.Hide();
                    Application.RemoveForm(this);
                }
            }
            GUILayout.EndVertical();
        }
    }
}