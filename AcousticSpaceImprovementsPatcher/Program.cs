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
            foreach (var masterCellBlockGetter in masterCells.Records) {
                foreach (var masterCellSubBlock in masterCellBlockGetter.SubBlocks) {
                    foreach (var templateFixesCell in masterCellSubBlock.Cells)
                    {
                        
                        if (!templateFixesCell
                                .ToLink()
                                .TryResolveContext<ISkyrimMod, ISkyrimModGetter, ICell, ICellGetter>(
                                    state.LinkCache,
                                    out var winningCellContext)
                           ) continue;

                        // Don't patch if acoustic space matches existing
                        if (templateFixesCell.AcousticSpace.Equals(winningCellContext.Record.AcousticSpace))
                        {
                            continue;
                        }
                        
                        var patchCell = winningCellContext.GetOrAddAsOverride(state.PatchMod);
                        if (!templateFixesCell.AcousticSpace.IsNull) {
                            patchCell.AcousticSpace.FormKey = templateFixesCell.AcousticSpace.FormKey;
                        }
                    }
                }
            }
        }
    }
}
