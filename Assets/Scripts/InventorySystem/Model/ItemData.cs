using System.ComponentModel;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace InventorySystem.Model
{
    [CreateAssetMenu(fileName = "NewItemAsset", menuName = "RPG/Inventory/Item Data")]
    public class ItemData : ScriptableObject, INotifyPropertyChanged
    {
        [Header("Basic Info")]
        public string itemID;

        [SerializeField] private string _itemName;
        [TextArea(3, 5)] [SerializeField] private string _description;

        public string iconPath; // 图片路径

        [Header("Categorization")]
        public ItemCategory category;
        public ItemRarity rarity;

        [Header("Stacking")]
        public bool isStackable;
        public int maxStack = 99;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string itemName
        {
            get => _itemName;
            set
            {
                if (_itemName == value) return;
                _itemName = value;
                OnPropertyChanged();
            }
        }

        public string description
        {
            get => _description;
            set
            {
                if (_description == value) return;
                _description = value;
                OnPropertyChanged();
            }
        }
    }
}
