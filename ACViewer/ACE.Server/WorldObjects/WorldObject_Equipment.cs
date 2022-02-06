using System;
using System.Collections.Generic;

using ACE.Common;
using ACE.Common.Extensions;
using ACE.Database.Models.World;
using ACE.Server.Factories;

namespace ACE.Server.WorldObjects
{
    partial class WorldObject
    {
        public static List<WorldObject> GenerateWieldedTreasureSets(List<TreasureWielded> items)
        {
            var curIdx = 0;
            List<WorldObject> results = null;
            GenerateWieldedTreasureSets(items, ref results, ref curIdx);
            return results;
        }

        private static void GenerateWieldedTreasureSets(List<TreasureWielded> items, ref List<WorldObject> results, ref int curIdx, bool skip = false)
        {
            var rng = ThreadSafeRandom.Next(0.0f, 1.0f);
            var probability = 0.0f;
            var rolled = false;
            var continued = false;

            for (; curIdx < items.Count; curIdx++)
            {
                var item = items[curIdx];

                if (item.ContinuesPreviousSet)
                {
                    if (!continued)
                    {
                        curIdx--;
                        return;
                    }
                    else
                        continued = false;
                }

                var skipNext = true;

                if (!skip)
                {
                    if (item.SetStart || probability >= 1.0f)
                    {
                        rng = ThreadSafeRandom.Next(0.0f, 1.0f);
                        probability = 0.0f;
                        rolled = false;
                    }

                    probability += item.Probability;

                    if (rng < probability && !rolled)
                    {
                        rolled = true;
                        skipNext = false;

                        // item roll successful, add to generated list
                        var wo = CreateWieldedTreasure(item);

                        if (wo != null)
                        {
                            if (results == null)
                                results = new List<WorldObject>();

                            results.Add(wo);
                        }
                    }
                }

                if (item.HasSubSet)
                {
                    curIdx++;
                    GenerateWieldedTreasureSets(items, ref results, ref curIdx, skipNext);
                    continued = true;
                }
            }
        }

        public static WorldObject CreateWieldedTreasure(TreasureWielded item)
        {
            var wo = WorldObjectFactory.CreateNewWorldObject(item.WeenieClassId);
            if (wo == null) return null;

            if (item.PaletteId > 0)
                wo.PaletteTemplate = (int)item.PaletteId;

            if (item.Shade > 0)
                wo.Shade = item.Shade;

            if (item.StackSize > 0)
            {
                var stackSize = item.StackSize;

                var hasVariance = item.StackSizeVariance > 0;
                if (hasVariance)
                {
                    var minStack = Math.Max(1, (item.StackSize * (1.0f - item.StackSizeVariance)).Round());
                    var maxStack = item.StackSize;
                    stackSize = ThreadSafeRandom.Next(minStack, maxStack);
                }
                wo.SetStackSize(stackSize);
            }
            return wo;
        }
    }
}
