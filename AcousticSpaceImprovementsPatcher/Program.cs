using System;
using System.Collections.Generic;
using System.Linq;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;

namespace AcousticSpaceImprovementsPatcher
{
    public class Program
    {
        private static readonly ModKey KeyAcousticTemplateFixes = ModKey.FromNameAndExtension("AcousticTemplateFixes.esp");
        
        public static async Task<int> Main(string[] args)
        {
            return await SynthesisPipeline.Instance
                .AddPatch<ISkyrimMod, ISkyrimModGetter>(RunPatch)
                .SetTypicalOpen(GameRelease.SkyrimSE, "AcousticSpaceImprovementsPatcher.esp")
                .AddRunnabilityCheck(state =>
                { 
                    state.LoadOrder.AssertListsMod(KeyAcousticTemplateFixes, "\n\nMissing AcousticTemplateFixes.esp\n\n");
                })
                .Run(args);
        }

        public static void RunPatch(IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            var templateFixesEsp = state.LoadOrder.GetIfEnabled(KeyAcousticTemplateFixes);
            if (templateFixesEsp.Mod == null) return;
            var masterCells = templateFixesEsp.Mod.Cells;

            // Check if cell acoustic template matches one in templatefixesesp otherwise deep copy and set in patcher
            foreach (var cellBlockGetter in masterCells.Records) {
                foreach (var cellSubBlock in cellBlockGetter.SubBlocks) {
                    foreach (var cell in cellSubBlock.Cells) {
                        if (!cell.ToLink().TryResolveContext<ISkyrimMod, ISkyrimModGetter, ICell, ICellGetter>(state.LinkCache, out var winningCellContext)) continue;
                        var patchCell = winningCellContext.GetOrAddAsOverride(state.PatchMod);
                        if (!cell.AcousticSpace.IsNull) {
                            patchCell.AcousticSpace.FormKey = cell.AcousticSpace.FormKey;
                        }
                    }
                }
            }
        }
    }
}
