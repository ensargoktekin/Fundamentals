using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Game.Factory;
using DG.Tweening;
using MyBox;

namespace Game.Objects
{
    public class Rocket : Block
    {
        #region Variables - Enums
        public Type RocketType
        {
            get => _type;
        }
        [SerializeField] private Type _type;
        [SerializeField] private float _rocketAnimTime = 3;
        [SerializeField] private GameObject _lineAnimObject;
        [ConditionalField(nameof(_type), false, Type.Bilinear)]
        [SerializeField] private GameObject _lineAnimObject2;
        [SerializeField] private GameObject _effectObject1;
        [SerializeField] private GameObject _effectObject2;
        [ConditionalField(nameof(_type), false, Type.Bilinear)]
        [SerializeField] private GameObject _effectObject3;
        [ConditionalField(nameof(_type), false, Type.Bilinear)]
        [SerializeField] private GameObject _effectObject4;
        private Tween _rotateTween = null;
        private Tween _scaleTween = null;
        private Tween _effectTween1 = null;
        private Tween _effectTween2 = null;
        private Tween _effectTween3 = null;
        private Tween _effectTween4 = null;

        public enum Type
        {
            None,
            Horizontal,
            Vertical,
            Bilinear
        }

        #endregion

        #region Initialization
        private void OnEnable()
        {
            DrawChain.ChainChanged += ChainChangedHandler;
        }

        private void OnDisable()
        {
            DrawChain.ChainChanged -= ChainChangedHandler;
            StopAllCoroutines();
            if (_rotateTween != null) _rotateTween.Kill();
            if (_scaleTween != null) _scaleTween.Kill();
            if (_effectTween1 != null) _effectTween1.Kill();
            if (_effectTween2 != null) _effectTween2.Kill();
            if (_effectTween3 != null) _effectTween3.Kill();
            if (_effectTween4 != null) _effectTween4.Kill();
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
            SoundController.Instance.Play(AudioNames.Rocket);
            OnPopBegin();
            PlayPopAnim();
        }

        public override void PopAndReplace(BlockMakeParams replaceBlock)
        {
            if (HasPopped)
                return;
            else
                HasPopped = true;
            OnPopBegin(replaceBlock);
            PlayPopAnim();
        }

        public override void Replace(BlockMakeParams replaceBlock, bool playAnim = true, bool increaseScore = true)
        {
            OnReplace(replaceBlock, playAnim, increaseScore);
        }
        #endregion

        #region Virtual Override Methods (base + additional implementation)
        public override void Highlight()
        {
            base.Highlight();
            SetHighlightedAnim(true);
        }

        // Completely overritten due to the nature of Rocket object !!
        public override void Dehighlight()
        {
            if (this == null)
                return;

            IsHighlighted = false;
            IsSelected = false;

            try
            {
                if (transform == null)
                    return;

                if (_rotateTween != null) _rotateTween.Kill();
                if (_scaleTween != null) _scaleTween.Kill();
                _scaleTween = Sprite.transform.DOScale(InitialScale, 0.2f);
                _rotateTween = Sprite.transform.DOLocalRotate(Vector3.zero, 0);
                Sprite.sortingOrder = 0;
                SetHighlightedAnim(false);
            }
            catch (System.Exception ex)
            {
                string errLog = "Dehighlight\n";
                errLog += $"transform: {transform != null}\n";
                errLog += $"_scaleTween: {_scaleTween != null}\n";
                errLog += $"_rotateTween: {_rotateTween != null}\n";
                errLog += $"Sprite: {Sprite != null}\n";
                FirebaseController.Instance.SetCrashlyticsLog(errLog);
                FirebaseController.Instance.SendException(ex);
                FirebaseController.Instance.SetCrashlyticsLog("");
            }
        }

        public override void Select()
        {
            base.Select();
            Highlight();
            if (_rotateTween != null) _rotateTween.Kill();
            if (_scaleTween != null) _scaleTween.Kill();
            _scaleTween = Sprite.transform.DOScale(InitialScale * 1.7f, 0.2f);
            _rotateTween = Sprite.transform.DOShakeRotation(1, 20, 10, 90);
            Sprite.sortingOrder = 10;
        }

        // Completely overritten due to the nature of Rocket object !!
        public override void Deselect()
        {
            IsHighlighted = true;
            IsSelected = false;
            if (_rotateTween != null) _rotateTween.Kill();
            if (_scaleTween != null) _scaleTween.Kill();
            _scaleTween = Sprite.transform.DOScale(InitialScale, 0.2f);
            _rotateTween = Sprite.transform.DOLocalRotate(Vector3.zero, 0);
            Sprite.sortingOrder = 8;
        }

        public override void ChangeLayer(string layerName, string sortLayerName)
        {
            base.ChangeLayer(layerName, sortLayerName);
            switch (_type)
            {
                case Type.Horizontal:
                case Type.Vertical:
                    _lineAnimObject.GetComponent<ParticleSystemRenderer>().sortingLayerID = SortingLayer.NameToID(sortLayerName);
                    break;
                case Type.Bilinear:
                    _lineAnimObject.GetComponent<ParticleSystemRenderer>().sortingLayerID = SortingLayer.NameToID(sortLayerName);
                    _lineAnimObject2.GetComponent<ParticleSystemRenderer>().sortingLayerID = SortingLayer.NameToID(sortLayerName);
                    break;
                default:
                    break;
            }
        }

        // Completely overritten due to the nature of Rocket object !!
        public override List<Block> AdditionalBlocksToPop()
        {
            var blocks = new List<Block>();
            List<Slot> slots = null;
            Assert.IsNotNull(Slot);
            Assert.IsNotNull(Slot.Grid);
            Slot mySlot = Slot;
            Grid myGrid = Slot.Grid;
            switch (_type)
            {
                case Type.Horizontal:
                    slots = myGrid.GetAllSlotsHorizontal(mySlot);
                    break;
                case Type.Vertical:
                    slots = myGrid.GetAllSlotsVertical(mySlot);
                    break;
                case Type.Bilinear:
                    var horizontalSlots = myGrid.GetAllSlotsHorizontal(mySlot);
                    var verticalSlots = myGrid.GetAllSlotsVertical(mySlot);
                    slots = horizontalSlots;
                    slots.AddRange(verticalSlots);
                    break;
            }
            foreach (var slot in slots)
            {
                var block = slot.Block;
                if (block && !block.IsHighlighted)
                {
                    blocks.Add(block);
                }
            }
            return blocks;
        }
        #endregion

        #region Helpers - Utils
        private void PlayPopAnim()
        {
            float length = 40;
            switch (_type)
            {
                case Type.Horizontal:
                    if (_effectTween1 != null) _effectTween1.Kill();
                    if (_effectTween2 != null) _effectTween2.Kill();
                    _effectObject1.SetActive(true);
                    _effectObject1.transform.localPosition = new Vector3(0, 0, 0);
                    _effectTween1 = _effectObject1.transform.DOBlendableLocalMoveBy(new Vector3(length, 0, 0), RocketAnimTime).SetEase(Ease.Linear);
                    _effectObject2.SetActive(true);
                    _effectObject2.transform.localPosition = new Vector3(0, 0, 0);
                    _effectTween2 = _effectObject2.transform.DOBlendableLocalMoveBy(new Vector3(-length, 0, 0), RocketAnimTime).SetEase(Ease.Linear);
                    Save.SaveGame.Instance.LevelData.IncreaseScore(20 + 40);
                    break;
                case Type.Vertical:
                    if (_effectTween1 != null) _effectTween1.Kill();
                    if (_effectTween2 != null) _effectTween2.Kill();
                    _effectObject1.SetActive(true);
                    _effectObject1.transform.localPosition = new Vector3(0, 0, 0);
                    _effectTween1 = _effectObject1.transform.DOBlendableLocalMoveBy(new Vector3(0, length, 0), RocketAnimTime).SetEase(Ease.Linear);
                    _effectObject2.SetActive(true);
                    _effectObject2.transform.localPosition = new Vector3(0, 0, 0);
                    _effectTween2 = _effectObject2.transform.DOBlendableLocalMoveBy(new Vector3(0, -length, 0), RocketAnimTime).SetEase(Ease.Linear);
                    Save.SaveGame.Instance.LevelData.IncreaseScore(20 + 40);
                    break;
                case Type.Bilinear:
                    if (_effectTween1 != null) _effectTween1.Kill();
                    if (_effectTween2 != null) _effectTween2.Kill();
                    if (_effectTween3 != null) _effectTween3.Kill();
                    if (_effectTween4 != null) _effectTween4.Kill();
                    _effectObject1.SetActive(true);
                    _effectObject1.transform.localPosition = new Vector3(0, 0, 0);
                    _effectTween1 = _effectObject1.transform.DOBlendableLocalMoveBy(new Vector3(length, 0, 0), RocketAnimTime).SetEase(Ease.Linear);
                    _effectObject2.SetActive(true);
                    _effectObject2.transform.localPosition = new Vector3(0, 0, 0);
                    _effectTween2 = _effectObject2.transform.DOBlendableLocalMoveBy(new Vector3(-length, 0, 0), RocketAnimTime).SetEase(Ease.Linear);
                    _effectObject3.SetActive(true);
                    _effectObject3.transform.localPosition = new Vector3(0, 0, 0);
                    _effectTween3 = _effectObject3.transform.DOBlendableLocalMoveBy(new Vector3(0, length, 0), RocketAnimTime).SetEase(Ease.Linear);
                    _effectObject4.SetActive(true);
                    _effectObject4.transform.localPosition = new Vector3(0, 0, 0);
                    _effectTween4 = _effectObject4.transform.DOBlendableLocalMoveBy(new Vector3(0, -length, 0), RocketAnimTime).SetEase(Ease.Linear);
                    Save.SaveGame.Instance.LevelData.IncreaseScore(30 + 80);
                    break;
                default:
                    break;
            }
        }
        #endregion
    }
}
