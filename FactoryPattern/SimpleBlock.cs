using System.Collections.Generic;
using UnityEngine;
using Game.Factory;

namespace Game.Objects
{
    public class SimpleBlock : Block
    {
        #region Variables - Enums
        public Type SimpleBlockType => _type;

        [SerializeField] private Type _type;
        [SerializeField] private Animator _popAnim;
        [SerializeField] private SpriteRenderer _glowSprite;

        public enum Type
        {
            None,
            Red,
            Green,
            Blue,
            Yellow,
            Purple
        }
        #endregion

        #region Abstract Methods
        public override void Accept(BlockVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override void Pop()
        {
            if (HasPopped)
                return;
            else
                HasPopped = true;

            Slot.Pop();
            OnPopBegin();

            _popAnim.Play(_type.ToString());
        }

        public override void PopAndReplace(BlockMakeParams replaceBlock)
        {
            if (HasPopped)
                return;

            HasPopped = true;
            Slot.Pop();
            OnPopBegin(replaceBlock);
            _popAnim.Play(_type.ToString());
        }

        public override void Replace(BlockMakeParams replaceBlock, bool playAnim = true, bool increaseScore = true)
        {
            OnReplace(replaceBlock, playAnim, increaseScore);
        }
        #endregion

        #region Virtual Override Methods (base + additional implementation)
        public override void Highlight()
        {
            if (HasPopped)
                return;

            base.Highlight();
            _glowSprite.gameObject.SetActive(true);
        }

        public override void Dehighlight()
        {
            if (HasPopped)
                return;

            base.Dehighlight();

            if (_glowSprite != null && _glowSprite.gameObject != null)
            {
                _glowSprite.gameObject.SetActive(false);
                _glowSprite.sortingOrder = -1;
            }
        }

        public override void Select()
        {
            base.Select();
            _glowSprite.sortingOrder = 9;
        }

        public override void Deselect()
        {
            base.Deselect();
            _glowSprite.sortingOrder = 7;
        }

        public override void ChangeLayer(string layerName, string sortLayerName)
        {
            base.ChangeLayer(layerName, sortLayerName);
            _glowSprite.sortingLayerID = SortingLayer.NameToID(sortLayerName);
        }
        #endregion
    }
}
