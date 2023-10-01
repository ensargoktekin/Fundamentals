using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace Game.Objects
{
    public abstract class Block : MonoBehaviour
    {
        #region Events        
        public static event System.EventHandler<PoppedEventArgs> PopBegan;
        public static event System.EventHandler<PoppedEventArgs> PopFinished;
        public static event System.EventHandler<ReplacedEventArgs> ReplaceBlock;
        #endregion

        #region Variables
        [SerializeField] protected bool CanBePopped = true;
        [SerializeField] protected Vector3 InitialScale;
        [SerializeField] protected SpriteRenderer Sprite;
        // Tweens are used to handle animation states effectively. More Tween can be added for different games
        [SerializeField] protected Tween TweenHandle;
        [SerializeField] protected Tween TweenShake1Handle;
        [SerializeField] protected Tween TweenShake2Handle;
        [SerializeField] private Tween _rotateTween = null;
        [SerializeField] private Tween _rotateTweenSprite = null;
        [SerializeField] private Tween _scaleTween = null;
        [SerializeField] private Slot _slot; // indicates location of the block
        [SerializeField] private States _states;
        [SerializeField] private float _blockPopTime = .45f; // Change this depending on the animation for different games
        #endregion

        #region States
        [System.Flags]
        private enum States
        {
            None = 0,
            IsHighlighted = 1 << 0,
            IsVisible = 1 << 1,
            IsSelected = 1 << 2,
            HasPopped = 1 << 3,
        }
        #endregion

        #region Crucial Auto Properties
        public Slot Slot
        {
            get => _slot;
            set => _slot = value;
        }
        public bool IsHighlighted
        {
            get
            {
                return _states.HasFlag(States.IsHighlighted);
            }
            protected set
            {
                if (value)
                    _states |= States.IsHighlighted;
                else
                    _states &= ~States.IsHighlighted;
            }
        }
        public bool IsSelected
        {
            get
            {
                return _states.HasFlag(States.IsSelected);
            }
            protected set
            {
                if (value)
                    _states |= States.IsSelected;
                else
                    _states &= ~States.IsSelected;
            }
        }
        public bool IsVisible
        {
            get
            {
                return _states.HasFlag(States.IsVisible);
            }
            protected set
            {
                if (value)
                    _states |= States.IsVisible;
                else
                    _states &= ~States.IsVisible;
            }
        }
        public bool HasPopped
        {
            get
            {
                return _states.HasFlag(States.HasPopped);
            }
            protected set
            {
                if (value)
                    _states |= States.HasPopped;
                else
                    _states &= ~States.HasPopped;
            }
        }
        public int SortingOrder
        {
            get
            {
                return Sprite.sortingOrder;
            }
            set
            {
                Sprite.sortingOrder = value;
            }
        }
        public bool HasBranch
        {
            get
            {
                if (_slot == null)
                    return false;
                return _slot.HasBranch;
            }
        }
        #endregion

        

        #region Abstract Functions
        // Abstract Functions should be implemented all inherited classes
        public abstract void Accept(BlockVisitor visitor);
        public abstract void Pop();
        public abstract void PopAndReplace(Factory.BlockMakeParams replaceBlock);
        public abstract void Replace(Factory.BlockMakeParams replaceBlock, bool playAnim = true, bool increaseScore = true);
        #endregion

        #region Virtual Functions
        // Virtual Functions can be used in all inherited classes with some extensions.
        // Base parts that are same for all inherited classes are implemented here

        /// <summary>
        /// Moves block to the given destination and re assings its slot
        /// </summary>
        /// <param name="slot">Slot to move to</param>
        /// <param name="moveTime">Move duration</param>
        public virtual void Move(Slot slot, float moveTime = .2f)
        {
            if (HasPopped)
                return;
            if (slot == null)
                return;
            // Clear old reference.
            if (_slot != null)
                _slot.Block = null;

            // Set new reference.
            _slot = slot;
            _slot.Block = this;
            //Debug.Log("block move: " + slot.Position);
            switch (_slot.SlotType)
            {
                case Slot.Type.HasItem:
                case Slot.Type.Regular:
                    IsVisible = true;
                    break;
                case Slot.Type.Invisible:
                    IsVisible = false;
                    break;
                default:
                    throw new System.NotImplementedException();
            }

            if (TweenHandle != null) TweenHandle.Kill();
            TweenHandle = transform.DOMove(slot.Position, moveTime).SetEase(Ease.OutSine);
        }
        
        /// <summary>
        /// Moves block to the given destination
        /// </summary>
        /// <param name="slot">Slot to move to</param>
        /// <param name="moveTime">Move duration</param>
        /// <param name="returnToPos">Return block to the original position after animation</param>
        /// <param name="callback">Animation completed callback</param>
        public virtual void MoveTo(Slot slot, float moveTime = .2f, bool returnToPos = true, System.Action callback = null)
        {
            if (HasPopped)
                return;
            if (slot == null)
                return;

            Sprite.sortingOrder--;

            Vector3 origPos = transform.position;
            if (TweenHandle != null) TweenHandle.Kill();
            TweenHandle = transform.DOMove(slot.Position, moveTime).SetEase(Ease.OutSine).OnComplete(() =>
            {
                if (returnToPos)
                    transform.position = origPos;
                callback?.Invoke();
            });
        }
        public virtual void Highlight()
        {
            IsHighlighted = true;
        }
        public virtual void Dehighlight()
        {
            if (this == null)
                return;

            IsHighlighted = false;
            IsSelected = false;

            try
            {
                if (transform == null)
                    return;

                if (_scaleTween != null) _scaleTween.Kill();
                _scaleTween = transform.DOScale(InitialScale, 0.2f);

                if (_rotateTween != null) _rotateTween.Kill();
                _rotateTween = transform.DOLocalRotate(Vector3.zero, .2f);

                if (_rotateTweenSprite != null) _rotateTweenSprite.Kill();
                if (Sprite != null)
                    _rotateTweenSprite = Sprite.transform.DOLocalRotate(Vector3.zero, .2f);

                if (Sprite != null)
                    Sprite.sortingOrder = 0;
            }
            catch (System.Exception ex)
            {
                string errLog = "Dehighlight\n";
                errLog += $"transform: {transform != null}\n";
                errLog += $"_scaleTween: {_scaleTween != null}\n";
                errLog += $"_rotateTween: {_rotateTween != null}\n";
                errLog += $"_rotateTweenSprite: {_rotateTweenSprite != null}\n";
                errLog += $"Sprite: {Sprite != null}\n";
                FirebaseController.Instance.SetCrashlyticsLog(errLog);
                FirebaseController.Instance.SendException(ex);
                FirebaseController.Instance.SetCrashlyticsLog("");
            }
        }
        public virtual void Select()
        {
            IsHighlighted = true;
            IsSelected = true;
            Highlight();
            if (_scaleTween != null)
                _scaleTween.Kill();
            _scaleTween = transform.DOScale(InitialScale * 1.7f, 0.2f);
            if (_rotateTween != null)
                _rotateTween.Kill();
            _rotateTween = transform.DOShakeRotation(1, 20, 10, 90);
            Sprite.sortingOrder = 10;
        }
        public virtual void Deselect()
        {
            IsHighlighted = true;
            IsSelected = false;
            if (_scaleTween != null)
                _scaleTween.Kill();
            _scaleTween = transform.DOScale(InitialScale, 0.2f);
            if (_rotateTween != null)
                _rotateTween.Kill();
            _rotateTween = transform.DOLocalRotate(Vector3.zero, .2f);
            if (_rotateTweenSprite != null)
                _rotateTweenSprite.Kill();
            _rotateTweenSprite = Sprite.transform.DOLocalRotate(Vector3.zero, .2f);
            Sprite.sortingOrder = 8;
        }
        public virtual void Release()
        {
            Destroy(gameObject);
        }
        public virtual void Initialize()
        {
            InitialScale = transform.localScale;
            BaseScale = transform.localScale;
        }
        public virtual void ChangeLayer(string layerName, string sortLayerName)
        {
            int layer = LayerMask.NameToLayer(layerName);
            gameObject.layer = layer;
            Sprite.gameObject.layer = layer;
            Sprite.sortingLayerID = SortingLayer.NameToID(sortLayerName);
        }
        public virtual void PopWithNoEffect(bool destroyBlock = false)
        {
            try
            {
                if (this == null) return;

                HasPopped = true;
                //_slot.Pop();
                _slot.Block = null;
                Save.SaveGame.Instance.LevelData.IncreaseScore(1);
                if (destroyBlock)
                {
                    FinishPop();
                    Destroy(gameObject);
                }
            }
            catch (System.Exception ex)
            {
                string errLog = "PopWithNoEffect 4\n";
                errLog += $"HasPopped: {HasPopped}\n";
                errLog += $"destroyBlock: {destroyBlock}\n";
                errLog += $"gameObject: {gameObject != null}\n";
                errLog += $"_slot: {_slot != null}\n";
                if (_slot != null) errLog += $"_slot.Block: {_slot.Block}\n";
                errLog += $"LevelData: {Save.SaveGame.Instance.LevelData != null}\n";
                errLog += $"ex: {ex.Message}\n{ex.StackTrace}\n";
                FirebaseController.Instance.SetCrashlyticsLog(errLog);
                FirebaseController.Instance.SendException(ex);
                FirebaseController.Instance.SetCrashlyticsLog("");

                HasPopped = true;
                if (gameObject != null && destroyBlock)
                {
                    FinishPop();
                    Destroy(gameObject);
                }
            }
        }

        /// <summary>
        /// When a neighbour of the block pops, pop the block's slot
        /// </summary>
        public virtual void NeighbourPopped(PopDirection popDirection)
        {
            // Debug.LogWarning("NeighbourPopped 1 block, popDirection: " + popDirection.ToString(), this);
            //if (_slot.HasIce)
            // Debug.LogWarning($"DIRECTION {popDirection}");
            _slot.NeighbourPop(popDirection);
        }

        public virtual void NeighbourPopped()
        {
            // Debug.LogWarning("NeighbourPopped block");
            // Debug.LogWarning($"{this.GetBranchType} -- DIRECTION");
            _slot.NeighbourPop(new PopDirection());
        }

        /// <summary>
        /// A Block can affect and pop other blocks on pop. This returns a collection of those blocks.
        /// </summary>
        /// <returns>HashSet of Blocks which will also pop when this Block pops.</returns>
        public virtual List<Block> AdditionalBlocksToPop()
        {
            return null;
        }

        /// <summary>
        /// A Block can affect and pop other blocks on pop (generally neighbour blocks)
        /// This returns a collection of those blocks.
        /// </summary>
        /// <returns>HashSet of Blocks which will also pop when this Block pops.</returns>
        public virtual Dictionary.NeighbourPopDict EffectedBlocks()
        {
            Dictionary.NeighbourPopDict returnDict = new Dictionary.NeighbourPopDict();
            Slot mySlot = Slot;
            Grid myGrid = Slot.Grid;

            // get right neighbour
            Slot rightNeighbour = myGrid.GetRightNeighbour(mySlot);
            if (rightNeighbour != null && rightNeighbour != null)
            {
                if (returnDict.ContainsKey(rightNeighbour))
                {
                    PopDirection value = returnDict[rightNeighbour];
                    value.Right = true;
                    returnDict[rightNeighbour] = value;
                }
                else
                {
                    returnDict.Add(rightNeighbour, new PopDirection() { Right = true });
                }
            }

            // get left neighbour
            Slot leftNeighbour = myGrid.GetLeftNeighbour(mySlot);
            if (leftNeighbour != null && leftNeighbour != null)
            {
                if (returnDict.ContainsKey(leftNeighbour))
                {
                    PopDirection value = returnDict[leftNeighbour];
                    value.Left = true;
                    returnDict[leftNeighbour] = value;
                }
                else
                {
                    returnDict.Add(leftNeighbour, new PopDirection() { Left = true });
                }
            }

            // get top neighbour
            Slot topNeighbour = myGrid.GetTopNeighbour(mySlot);
            if (topNeighbour != null && topNeighbour != null)
            {
                if (returnDict.ContainsKey(topNeighbour))
                {
                    PopDirection value = returnDict[topNeighbour];
                    value.Top = true;
                    returnDict[topNeighbour] = value;
                }
                else
                {
                    returnDict.Add(topNeighbour, new PopDirection() { Top = true });
                }
            }

            // get bottom neighbour
            Slot bottomNeighbour = myGrid.GetBottomNeighbour(mySlot);
            if (bottomNeighbour != null && bottomNeighbour != null)
            {
                if (returnDict.ContainsKey(bottomNeighbour))
                {
                    PopDirection value = returnDict[bottomNeighbour];
                    value.Bottom = true;
                    returnDict[bottomNeighbour] = value;
                }
                else
                {
                    returnDict.Add(bottomNeighbour, new PopDirection() { Bottom = true });
                }
            }

            return returnDict;
        }

        protected virtual void OnPopBegin()
        {
            //_slot.Pop();
            _slot.Block = null;
            PopBegan?.Invoke(this, new PoppedEventArgs(this));
            Save.SaveGame.Instance.LevelData.IncreaseScore(1);
        }

        protected virtual void OnPopBegin(Slot slimeSlot)
        {
            //_slot.Pop();
            _slot.Block = null;
            PopBegan?.Invoke(this, new PoppedEventArgs(this, slimeSlot));
            Save.SaveGame.Instance.LevelData.IncreaseScore(1);
        }

        protected virtual void OnPopBegin(Factory.BlockMakeParams replaceBlockParams)
        {
            //_slot.Pop();
            _slot.Block = null;
            PopBegan?.Invoke(this, new PoppedEventArgs(this, replaceBlockParams));
            Save.SaveGame.Instance.LevelData.IncreaseScore(1);
        }

        protected virtual void OnPopFinish()
        {
            PopFinished?.Invoke(this, new PoppedEventArgs(this));
        }

        protected virtual void OnPopFinish(Slot slimeSlot)
        {
            PopFinished?.Invoke(this, new PoppedEventArgs(this, slimeSlot));
        }

        protected virtual void OnPopFinish(Factory.BlockMakeParams replaceBlockParams)
        {
            PopFinished?.Invoke(this, new PoppedEventArgs(this, replaceBlockParams));
        }

        protected virtual void OnReplace(Factory.BlockMakeParams replaceBlockParams, bool playAnim = true)
        {
            ReplaceBlock?.Invoke(this, new ReplacedEventArgs(this, replaceBlockParams, playAnim));
        }

        protected virtual void OnReplace(Factory.BlockMakeParams replaceBlockParams, bool playAnim = true, bool increaseScore = true)
        {
            ReplaceBlock?.Invoke(this, new ReplacedEventArgs(this, replaceBlockParams, playAnim, increaseScore));
        }
        #endregion

        #region Base Functions
        // These are the functions that can be used in all inherited functions with no extension
        public void OnInput()
        {
            try
            {
                if (CanShake)
                    transform.localScale = BaseScale;

                CanShake = false;
                if (_t1 != null) _t1.Kill();
                if (_t2 != null) _t2.Kill();
            }
            catch (System.Exception ex)
            {
                Debug.LogError(ex);
                FirebaseController.Instance.SetCrashlyticsLog("Block | OnInput");
                FirebaseController.Instance.SendException(ex);
            }
        }

        public void KillTween()
        {
            if (_t1 != null) _t1.Kill();
            if (_t2 != null) _t2.Kill();
        }

        public void Shake()
        {
            //transform.localScale = Vector3.one;
            if (TweenShake1Handle != null) TweenShake1Handle.Kill();
            TweenShake1Handle = transform.DOShakeScale(0.04f, 0.05f, 10, 0).OnComplete(() =>
            {
                if (TweenShake2Handle != null) TweenShake2Handle.Kill();
                TweenShake2Handle = transform.DOShakeScale(1, new Vector3(0.01f, 0.01f, 0), 3, 0).SetEase(Ease.Linear).SetLoops(-1);
            });
        }

        public void GetOnTopLayer()
        {
            Sprite.sortingOrder = 100;
        }

        public void FinishPop()
        {
            OnPopFinish();
        }
        public void FinishPop(Factory.BlockMakeParams replaceBlockParams)
        {
            OnPopFinish(replaceBlockParams);
        }

        protected void OnDestroy()
        {
            //DOTween.KillAll();
            KillTween();
            StopAllCoroutines();
            if (TweenHandle != null) TweenHandle.Kill();
            if (TweenShake1Handle != null) TweenShake1Handle.Kill();
            if (TweenShake2Handle != null) TweenShake2Handle.Kill();
            if (_rotateTween != null) _rotateTween.Kill();
            if (_scaleTween != null) _scaleTween.Kill();
            if (_rotateTweenSprite != null) _rotateTweenSprite.Kill();
        }
        #endregion

        #region Event Parameters
        public class PoppedEventArgs : System.EventArgs
        {
            private Slot _slimeSlot;
            private Block _block;
            private bool _replace;
            //private bool _playReplaceAnim;
            private Factory.BlockMakeParams _replaceBlockParams;

            public PoppedEventArgs(Block block)
            {
                _block = block;
                _replace = false;
            }
            public PoppedEventArgs(Block block, Slot slimeSlot)
            {
                _block = block;
                _replace = false;
                _slimeSlot = slimeSlot;
            }
            public PoppedEventArgs(Block block, Factory.BlockMakeParams replaceBlockParams)
            {
                _block = block;
                _replace = true;
                _replaceBlockParams = replaceBlockParams;
            }
            //public PoppedEventArgs(Block block, Factory.BlockMakeParams replaceBlockParams, bool playAnim=true)
            //{
            //    _block = block;
            //    _replace = true;
            //    _playReplaceAnim = playAnim;
            //    _replaceBlockParams = replaceBlockParams;
            //}

            public Slot SlimeSlot => _slimeSlot;
            public Block Block => _block;
            public bool Replace => _replace;
            //public bool PlayAnim => _playReplaceAnim;
            public Factory.BlockMakeParams ReplaceBlockParams => _replaceBlockParams;
        }

        public class ReplacedEventArgs : System.EventArgs
        {
            private Slot _slimeSlot;
            private Block _block;
            private bool _replace;
            private bool _playReplaceAnim;
            private bool _increaseScore;
            private Factory.BlockMakeParams _replaceBlockParams;

            public ReplacedEventArgs(Block block)
            {
                _block = block;
                _replace = false;
            }
            public ReplacedEventArgs(Block block, Slot slimeSlot)
            {
                _block = block;
                _replace = false;
                _slimeSlot = slimeSlot;
            }
            public ReplacedEventArgs(Block block, Factory.BlockMakeParams replaceBlockParams)
            {
                _block = block;
                _replace = true;
                _replaceBlockParams = replaceBlockParams;
            }
            public ReplacedEventArgs(Block block, Factory.BlockMakeParams replaceBlockParams, bool playAnim = true)
            {
                _block = block;
                _replace = true;
                _playReplaceAnim = playAnim;
                _replaceBlockParams = replaceBlockParams;
            }
            public ReplacedEventArgs(Block block, Factory.BlockMakeParams replaceBlockParams, bool playAnim, bool increaseScore)
            {
                _block = block;
                _replace = true;
                _playReplaceAnim = playAnim;
                _replaceBlockParams = replaceBlockParams;
                _increaseScore = increaseScore;
            }

            public Slot SlimeSlot => _slimeSlot;
            public Block Block => _block;
            public bool Replace => _replace;
            public bool PlayAnim => _playReplaceAnim;
            public bool IncreaseScore => _increaseScore;
            public Factory.BlockMakeParams ReplaceBlockParams => _replaceBlockParams;
        }
        #endregion
    }
}
