using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ACViewer.Entity;

namespace ACViewer.FileTypes
{
    public class Sound
    {
        public ACE.DatLoader.FileTypes.Wave _audio;

        public Sound(ACE.DatLoader.FileTypes.Wave audio)
        {
            _audio = audio;
        }

        public TreeNode BuildTree(uint soundID)
        {
            var treeView = new TreeNode($"{soundID:X8}");

            var typeStr = _audio.Header[0] == 0x55 ? "MP3" : "WAV";
            var type = new TreeNode($"Type: {typeStr}");

            var headerSize = new TreeNode($"Header size: {_audio.Header.Length}");
            var dataSize = new TreeNode($"Data size: {_audio.Data.Length}");

            treeView.Items.AddRange(new List<TreeNode>() { type, headerSize, dataSize });

            return treeView;
        }
    }
}
