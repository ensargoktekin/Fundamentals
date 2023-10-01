using UnityEngine;
using Game.Factory;

namespace Game.Objects
{
    public class Stone : Block
    {
        #region Variables - Enums
        public Type StoneType
        {
            get => _type;
        }
        public Type InitialStoneType
        {
            get => _initialStoneType;
        }

        [SerializeField] private Type _initialStoneType;
        [SerializeField] private Type _type;
        private short _popCount = 0;

        public enum Type
        {
            Full,
            DamagedLittle,
            DamagedMore,
            Broken,
            None
        }
        #endregion

        #region Abstract Methods
        public override void Accept(BlockVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override void Pop()
        {
            Save.SaveGame.Instance.LevelData.IncreaseScore(20);
            // do not pop if not broken yet
            switch (_type)
            {
                case Type.Full:
                    _type = Type.DamagedLittle;
                    Sprite.sprite = References.StoneSprites[1];
                    return;
                case Type.DamagedLittle:
                    _type = Type.DamagedMore;
                    Sprite.sprite = References.StoneSprites[2];
                    return;
                case Type.DamagedMore:
                    _type = Type.Broken;
                    Pop();
                    break;
                case Type.Broken:
                    break;
                default:
                    break;
            }

            if (HasPopped)
                return;
            else
                HasPopped = true;

            SoundController.Instance.Play(AudioNames.StoneBreak);
            OnPopBegin();
            Sprite.gameObject.SetActive(false);
        }

        public override void PopAndReplace(BlockMakeParams replaceBlock)
        {
            // Do not replace stones.
            Pop();
        }

        public override void Replace(BlockMakeParams replaceBlock, bool playAnim = true, bool increaseScore = true)
        {
            // Do not replace stones.
        }
        #endregion

        #region Virtual Override Methods (base + additional implementation)
        public override void NeighbourPopped()
        {
            base.NeighbourPopped();
            Pop();
        }

        public override void NeighbourPopped(PopDirection popDirection)
        {
            base.NeighbourPopped(popDirection);
            Pop();
        }
        #endregion
    }
}
