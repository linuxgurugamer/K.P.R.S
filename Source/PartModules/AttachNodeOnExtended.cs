using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using ClickThroughFix;
using ToolbarControl_NS;
using SpaceTuxUtility;
using KSP.UI.Screens;
using System.Collections;
using static KPRS.RegisterToolbar;
using Highlighting;

namespace KPRS.PartModules
{
    internal class KPBR_AnimateTower : ModuleAnimateGeneric
    {

        void Start()
        {
            var p = part.attachNodes;
            StartCoroutine(SlowUpdate());

        }

#pragma warning disable 0108
        public void Toggle()
        {
            StartCoroutine(SlowUpdate());

            base.Toggle();
        }
#pragma warning restore 0108

        IEnumerator SlowUpdate()
        {
            int breakCnt = 0;
            Part bottomNodeAttachedPart = null;

            if (HighLogic.LoadedSceneIsFlight)
                yield return new WaitForSeconds(1f);

            foreach (AttachNode node in part.attachNodes)
            {
                if (node != null && node.id == "bottom")
                {
                    bottomNodeAttachedPart = node.attachedPart;
                    break;
                }
            }

            while (true)
            {
                //
                // Detach all attached part
                //
                if (HighLogic.LoadedSceneIsFlight)
                {
                    for (int i = vessel.parts.Count - 1; i >= 0; i--)
                    {
                        var p = vessel.parts[i];
                        if (p.parent == this.part && p != bottomNodeAttachedPart)
                        {
                            if (this.IsMoving() || Progress < 1)
                            {
                                p.decouple();
                            }
                        }
                    }
                }
                else
                {
                    // need to detach the part in the editor here
                    for (int i = EditorLogic.fetch.ship.parts.Count - 1; i >= 0; i--)
                    {
                        var p = EditorLogic.fetch.ship.parts[i];
                        if (p != null && p.parent == this.part && p != bottomNodeAttachedPart)
                        {
                            if (this.IsMoving() || Progress < 1)
                            {
                                Log.Info("Highlighter.colorPartEditorDetached: " + Highlighter.colorPartEditorDetached);
                                p.SetHighlightColor(Highlighter.colorPartEditorDetached);
                                p.SetHighlight(active: false, recursive: false);
                                p.SetHighlightType(Part.HighlightType.Disabled);
                                p.SetOpacity(0.4f);
                                p.setParent();
                                p.transform.parent = null;
                                p.onDetach();

                                // Now clear any attach nodes
                                var n = p.FindAttachNodeByPart(this.part);
                                var n2 = n.FindOpposingNode();
                                n.attachedPart = null;
                                if (n2 != null)
                                    n2.attachedPart = null;
                            }

                        }
                    }
                }

                foreach (AttachNode node in part.attachNodes)
                {
                    if (node != null && node.id != "bottom")
                    {
                        if (this.IsMoving())
                        {
                            node.Hide();
                        }
                        else
                        {
                            if (Progress == 1)
                                node.Unhide();
                            else
                                node.Hide();
                        }

                    }
                }
                        //yield return new WaitForSeconds(1f);
                        yield return new WaitForSeconds(0.1f);

                // If not moving, allow at least two cycles to make sure all nodes have been addressed
                if (!this.IsMoving())
                {
                    breakCnt++;
                    if (breakCnt > 2)
                    {
                        yield break;
                    }
                }
                else
                    breakCnt = 0;
            }
        }
    }

    public static class AttachNodeExtensions
    {
        public static void Unhide(this AttachNode node)
        {
            //Log.Info("Unhiding node: " + node.id);
            node.nodeType = AttachNode.NodeType.Stack;
            node.radius = 0.4f;
        }

        public static void Hide(this AttachNode node)
        {
            //Log.Info("Hiding node: " + node.id);
            node.nodeType = AttachNode.NodeType.Dock;
            node.radius = 0.001f;
            // (node.attachedPart != null && !HighLogic.LoadedSceneIsFlight)
            //  node.attachedPart.decouple();
        }
    }
}
