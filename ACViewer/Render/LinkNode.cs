using System.Collections.Generic;

using ACE.Server.WorldObjects;

namespace ACViewer.Render
{
    public class LinkNode
    {
        public WorldObject WorldObject { get; set; }
        public LinkNode Parent { get; set; }
        public List<LinkNode> Children { get; set; }

        public LinkNode(WorldObject wo)
        {
            WorldObject = wo;
        }

        private void AddChild(LinkNode childNode)
        {
            if (Children == null)
                Children = new List<LinkNode>();

            Children.Add(childNode);
        }

        public bool AddParentChains()
        {
            return AddParentChain_Generator() | AddParentChain_Links();
        }

        public bool AddChildTrees()
        {
            return AddChildTree_Generator() | AddChildTree_Links();
        }

        private bool AddParentChain_Generator()
        {
            if (WorldObject.Generator == null) return false;

            Parent = new LinkNode(WorldObject.Generator);
            Parent.AddChild(this);

            Parent.AddParentChain_Generator();    // recurse

            return true;
        }

        private bool AddChildTree_Generator()
        {
            if (!WorldObject.IsGenerator) return false;

            foreach (var profile in WorldObject.GeneratorProfiles)
            {
                foreach (var spawnedObj in profile.Spawned.Values)
                {
                    var spawned = spawnedObj.TryGetWorldObject();

                    if (spawned == null) continue;

                    var spawnedNode = new LinkNode(spawned);
                    spawnedNode.Parent = this;
                    
                    AddChild(spawnedNode);

                    spawnedNode.AddChildTree_Generator();   // recurse
                }
            }
            return true;
        }

        public LinkNode GetHeadNode()
        {
            if (Parent != null)
                return Parent.GetHeadNode();
            else
                return this;
        }

        private bool AddParentChain_Links()
        {
            if (WorldObject.ParentLink == null) return false;

            Parent = new LinkNode(WorldObject.ParentLink);
            Parent.AddChild(this);

            Parent.AddParentChain_Links();    // recurse

            return true;
        }

        private bool AddChildTree_Links()
        {
            if (WorldObject.ChildLinks.Count == 0) return false;

            foreach (var childLink in WorldObject.ChildLinks)
            {
                var childNode = new LinkNode(childLink);
                childNode.Parent = this;

                AddChild(childNode);

                childNode.AddChildTree_Links();   // recurse
            }
            return true;
        }
    }
}
