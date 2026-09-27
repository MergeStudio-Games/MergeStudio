using System.Linq;
using NUnit.Framework;
using MergeStudio.Gameplay;

namespace MergeStudio.Tests
{
    public sealed class FoodCatalogTests
    {
        [Test]
        public void AtlasHasThirtySixDistinctFoodsWithSeparateContentBounds()
        {
            var sprites = FoodCatalog.Load();
            Assert.AreEqual(36, sprites.Count);
            Assert.AreEqual(36, sprites.Select(sprite => sprite.name).Distinct().Count());
            foreach (var sprite in sprites)
            {
                Assert.Greater(sprite.rect.width, 80);
                Assert.Greater(sprite.rect.height, 80);
                Assert.AreSame(sprites[0].texture, sprite.texture);
            }
            for (int i = 0; i < sprites.Count; i++)
                for (int j = i + 1; j < sprites.Count; j++)
                    Assert.IsFalse(sprites[i].rect.Overlaps(sprites[j].rect), sprites[i].name + " overlaps " + sprites[j].name);
        }
    }
}
