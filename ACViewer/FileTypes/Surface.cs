using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ACE.Entity.Enum;
using ACViewer.Entity;

namespace ACViewer.FileTypes
{
    public class Surface
    {
        public ACE.DatLoader.FileTypes.Surface _surface;
        
        /// <summary>
        /// Used to store a NewTextureId when replaced via Clothing Table.
        /// </summary>
        public uint NewTextureId;

        public Surface(ACE.DatLoader.FileTypes.Surface surface)
        {
            _surface = surface;
        }

        public TreeNode BuildTree(uint surfaceID)
        {
            var treeView = new TreeNode($"{surfaceID:X8}");

            var type = new TreeNode($"Type: {_surface.Type}");
            treeView.Items.Add(type);

            if (_surface.Type.HasFlag(SurfaceType.Base1Image) || _surface.Type.HasFlag(SurfaceType.Base1ClipMap))
            {
                var origTextureID = new TreeNode($"Texture ID: {_surface.OrigTextureId:X8}");
                treeView.Items.Add(origTextureID);

                if (_surface.OrigPaletteId != 0)
                {
                    var origPaletteID = new TreeNode($"Palette ID: {_surface.OrigPaletteId:X8}");
                    treeView.Items.Add(origPaletteID);
                }
            }
            else
            {
                var swatch = new TreeNode($"Color: {Color.ToRGBA(_surface.ColorValue)}");
                treeView.Items.Add(swatch);
            }

            //if (_surface.Translucency != 0.0f)
            //{
                var translucency = new TreeNode($"Translucency: {_surface.Translucency}");
                treeView.Items.Add(translucency);
            //}

            //if (_surface.Luminosity != 0.0f)
            //{
                var luminosity = new TreeNode($"Luminosity: {_surface.Luminosity}");
                treeView.Items.Add(luminosity);
            //}

            //if (_surface.Diffuse != 1.0f)
            //{
                var diffuse = new TreeNode($"Diffuse: {_surface.Diffuse}");
                treeView.Items.Add(diffuse);
            //}

            return treeView;
        }
    }
}
