using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Game.Factory
{
    public class BlockFactory : MonoBehaviour
    {
        #region Variables
        public static BlockFactory Instance
        {
            get => s_instance;
        }

        private static BlockFactory s_instance;

        [SerializeField] private Transform _blockParent;
        [SerializeField] private SimpleBlock _simpleBlockDummyPrefab;
        [SerializeField] private SimpleBlock _simpleBlockRedPrefab;
        [SerializeField] private SimpleBlock _simpleBlockGreenPrefab;
        [SerializeField] private SimpleBlock _simpleBlockBluePrefab;
        [SerializeField] private SimpleBlock _simpleBlockYellowPrefab;
        [SerializeField] private SimpleBlock _simpleBlockPurplePrefab;
        [SerializeField] private Rocket _rocketDummyPrefab;
        [SerializeField] private Rocket _rocketHorizontalPrefab;
        [SerializeField] private Rocket _rocketVerticalPrefab;
        [SerializeField] private Rocket _rocketBilinearPrefab;
        [SerializeField] private Stone _stonePrefab1;
        [SerializeField] private Stone _stonePrefab2;
        [SerializeField] private Stone _stonePrefab3;
        #endregion

        #region Initialization
        private void Awake()
        {
            if (s_instance)
            {
                Debug.LogWarning($"Additional BlockFactory object is trying to be created. Destroying instance {this}...");
                Destroy(this);
            }
            else
            {
                s_instance = this;
            }
        }
        #endregion

        #region Block Creation
        // Blocks are instantiated here, more functions should be added
        // when new objects added to the game. All object creation is
        // handled here, so new objects can be added easily.

        public Block MakeBlock(BlockMakeParams makeParams)
        {
            if (makeParams.BlockType == BlockMakeType.None)
                return null;
            var block = CreateNewBlock(makeParams);
            return block;
        }

        private Block GetPrefab(SimpleBlock.Type type)
        {
            switch (type)
            {
                case SimpleBlock.Type.None:
                    return _simpleBlockDummyPrefab;
                case SimpleBlock.Type.Red:
                    return _simpleBlockRedPrefab;
                case SimpleBlock.Type.Green:
                    return _simpleBlockGreenPrefab;
                case SimpleBlock.Type.Blue:
                    return _simpleBlockBluePrefab;
                case SimpleBlock.Type.Yellow:
                    return _simpleBlockYellowPrefab;
                case SimpleBlock.Type.Purple:
                    return _simpleBlockPurplePrefab;
                default:
                    throw new System.NotImplementedException();
            }
        }

        private Block GetPrefab(Rocket.Type type)
        {
            switch (type)
            {
                case Rocket.Type.None:
                    return _rocketDummyPrefab;
                case Rocket.Type.Horizontal:
                    return _rocketHorizontalPrefab;
                case Rocket.Type.Vertical:
                    return _rocketVerticalPrefab;
                case Rocket.Type.Bilinear:
                    return _rocketBilinearPrefab;
                default:
                    throw new System.NotImplementedException();
            }
        }

        private Block GetPrefab(Stone.Type type)
        {
            switch (type)
            {
                case Stone.Type.Full:
                    return _stonePrefab1;
                case Stone.Type.DamagedLittle:
                    return _stonePrefab2;
                case Stone.Type.DamagedMore:
                    return _stonePrefab3;
                case Stone.Type.Broken:
                default:
                    throw new System.NotImplementedException();
            }
        }

        private Block CreateNewBlock(BlockMakeParams makeParams)
        {
            Assert.AreNotEqual(BlockMakeType.None, makeParams.BlockType);
            Block prefab = makeParams.BlockType switch
            {
                BlockMakeType.Simple => GetPrefab(makeParams.SubType.SimpleBlockType),
                BlockMakeType.Rocket => GetPrefab(makeParams.SubType.RocketType),
                BlockMakeType.Stone => GetPrefab(makeParams.SubType.StoneType),
                BlockMakeType.Empty => null
                _ => throw new System.NotImplementedException(),
            };
            //Assert.IsNotNull(prefab);
            Block block;
            if (prefab != null)
            {
                block = Instantiate(prefab);
            }
            else
                return null;

            block.transform.SetParent(_blockParent, true);
            block.Initialize();
            return block;
        }
        #endregion
    }
}
