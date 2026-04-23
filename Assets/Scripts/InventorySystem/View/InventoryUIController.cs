using System.ComponentModel;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using InventorySystem.ViewModel;
using InventorySystem.Model;
using InventorySystem.Utils;

public class InventoryUIController : MonoBehaviour
{
    [Header("ViewModel Reference")]
    public InventoryViewModel viewModel;

    private VisualElement root;
    private InventoryBindableData bindableData;

    private Button backButton;
    private Button resetButton;
    private Button displayButton;

    private Button[] categoryTabs = new Button[6];
    private Button sortByRarity;
    private Button sortByRecent;
    private Button uiSwitchButton;

    private VisualElement[] itemSlots = new VisualElement[42];
    private Label[] itemCounts = new Label[42];

    private Button deleteItemButton;
    private Button sortButton;
    private Button useItemButton;

    private VisualElement itemPreviewPanel;
    private Label previewTitle;
    private Label previewDescription;

    private bool isDeleteMode = false;
    private HashSet<int> itemsToDelete = new HashSet<int>();

    private void Awake()
    {
        root = GetComponent<UIDocument>().rootVisualElement;

        backButton = root.Q<Button>("BackButton");
        resetButton = root.Q<Button>("ResetButton");
        displayButton = root.Q<Button>("DisplayButton");

        for (int i = 0; i < 6; i++)
            categoryTabs[i] = root.Q<Button>($"CategoryTab{i + 1}");

        sortByRarity = root.Q<Button>("SortByRarity");
        sortByRecent = root.Q<Button>("SortByRecent");
        uiSwitchButton = root.Q<Button>("UISwitchButton");

        for (int i = 0; i < 42; i++)
        {
            itemSlots[i] = root.Q<VisualElement>($"ItemSlot{i}");
            itemCounts[i] = root.Q<Label>($"ItemCount{i}");
        }

        deleteItemButton = root.Q<Button>("DeleteItemButton");
        sortButton = root.Q<Button>("SortButton");
        useItemButton = root.Q<Button>("UseItemButton");

        itemPreviewPanel = root.Q<VisualElement>("ItemPreviewPanel");
        previewTitle = root.Q<Label>("PreviewTitle");
        previewDescription = root.Q<Label>("PreviewDescription");

        RegisterEvents();
    }

    private void Start()
    {
        if (viewModel != null)
        {
            bindableData = viewModel.bindableData;
            if (bindableData != null)
            {
                // 我们使用更安全、逻辑可控的 C# 事件驱动机制，无需 UXML 引擎层面的 dataSource 绑定
                SubscribeBindableChanges();
            }
        }
    }

    private void RegisterEvents()
    {
        backButton.clicked += OnBackButtonClicked;
        resetButton.clicked += OnResetButtonClicked;
        displayButton.clicked += OnDisplayButtonClicked;

        for (int i = 0; i < 6; i++)
        {
            int idx = i + 1;
            categoryTabs[i].clicked += () => OnCategoryTabClicked(idx);
        }

        sortByRarity.clicked += OnSortByRarityClicked;
        sortByRecent.clicked += OnSortByRecentClicked;
        uiSwitchButton.clicked += OnUISwitchButtonClicked;

        deleteItemButton.clicked += OnDeleteItemButtonClicked;
        sortButton.clicked += OnSortButtonClicked;
        useItemButton.clicked += OnUseItemButtonClicked;

        // 为每个物品槽添加点击事件
        for (int i = 0; i < 42; i++)
        {
            if (itemSlots[i] != null)
            {
                int index = i;
                itemSlots[i].RegisterCallback<ClickEvent>(evt => {
                    OnItemSlotClicked(index);
                    // 阻止事件冒泡，避免触发根容器的点击事件
                    evt.StopPropagation();
                });
            }
        }

        // 为预览面板添加点击事件，阻止事件冒泡
        if (itemPreviewPanel != null)
        {
            itemPreviewPanel.RegisterCallback<ClickEvent>(evt => {
                evt.StopPropagation();
            });
        }

        // 为根容器添加点击事件，点击其他区域关闭预览
        root.RegisterCallback<ClickEvent>(evt => {
            if (bindableData != null && bindableData.isPreviewVisible)
            {
                if (viewModel != null)
                {
                    viewModel.SelectItem(-1);
                }
            }
        });
    }

    private void SubscribeBindableChanges()
    {
        if (bindableData == null) return;

        // 2. 对于控制样式的、无法被组件原生直接容纳的控制量，采用C#事件监听
        bindableData.OnCategoryChanged += RefreshUI;
        bindableData.OnPreviewStateChanged += UpdatePreviewPanel;
        bindableData.OnTabChanged += () =>
        {
            UpdateSortTabHighlight(bindableData.activeSortTab);
            UpdateCategoryTabHighlight(bindableData.activeCategoryTab);
        };

        RefreshUI();
    }

    private void UnsubscribeBindableChanges()
    {
        if (bindableData != null)
        {
            bindableData.OnCategoryChanged -= RefreshUI;
            bindableData.OnPreviewStateChanged -= UpdatePreviewPanel;
            // 匿名 delegate 将在 OnDisable 中随对象销毁而回收
        }
    }

    private void OnEnable()
    {
        if (viewModel != null)
        {
            viewModel.inventoryModel.OnInventoryChanged += RefreshUI;
            RefreshUI();
        }
    }

    private void OnDisable()
    {
        if (viewModel != null && viewModel.inventoryModel != null)
            viewModel.inventoryModel.OnInventoryChanged -= RefreshUI;

        UnsubscribeBindableChanges();
    }

    private void RefreshUI()
    {
        if (viewModel == null) return;

        for (int i = 0; i < 42; i++)
        {
            string countText = viewModel.GetItemCountText(i);
            itemCounts[i].text = countText;

            string iconPath = viewModel.GetIconPath(i);
            if (!string.IsNullOrEmpty(iconPath))
            {
                Sprite icon = ItemIconLoader.LoadItemIcon(iconPath);
                itemSlots[i].style.backgroundImage = icon != null ? new StyleBackground(icon) : null;
            }
            else
            {
                itemSlots[i].style.backgroundImage = null;
            }

            // 根据是否选中删除应用遮罩颜色
            itemSlots[i].style.unityBackgroundImageTintColor = (isDeleteMode && itemsToDelete.Contains(i)) ? new Color(1f, 0.3f, 0.3f) : Color.white;
        }
    }

    private void UpdatePreviewPanel()
    {
        if (bindableData == null) return;

        // 更新UI文本渲染
        previewTitle.text = bindableData.previewTitle;
        previewDescription.text = bindableData.previewDescription;

        // 根据状态绑定 USS class 样式，文本数据已被 SetBinding 自动接管，不再需要手动赋值！
        if (bindableData.isPreviewVisible)
        {
            itemPreviewPanel.AddToClassList("visible");
        }
        else
        {
            itemPreviewPanel.RemoveFromClassList("visible");
        }
    }

    private void UpdateSortTabHighlight(int tabIndex)
    {
        sortByRarity.RemoveFromClassList("active-inventory-tab");
        sortByRecent.RemoveFromClassList("active-inventory-tab");

        if (tabIndex == 0)
            sortByRarity.AddToClassList("active-inventory-tab");
        else if (tabIndex == 1)
            sortByRecent.AddToClassList("active-inventory-tab");
    }

    private void UpdateCategoryTabHighlight(int categoryIndex)
    {
        for (int i = 0; i < 6; i++)
            categoryTabs[i].RemoveFromClassList("active-tab");

        int tabIdx = categoryIndex - 1;
        if (tabIdx >= 0 && tabIdx < 6)
            categoryTabs[tabIdx].AddToClassList("active-tab");
    }

    private void CancelDeleteMode()
    {
        if (!isDeleteMode) return;
        isDeleteMode = false;
        if (deleteItemButton != null)
            deleteItemButton.style.backgroundColor = new StyleColor(StyleKeyword.Null); // 恢复初始样式
        itemsToDelete.Clear();
        RefreshUI(); // 取消变红遮罩
    }

    private void OnBackButtonClicked() 
    {
        CancelDeleteMode();
        // 游戏背包退出：隐藏主UI或直接禁用物体
        if (root != null)
        {
            root.style.display = DisplayStyle.None;
        }
        // gameObject.SetActive(false); // 取决于你的系统如何管理弹窗页面
    }

    private void OnResetButtonClicked() 
    {
        CancelDeleteMode();
        Debug.Log("执行背包重置");
        if (viewModel != null)
            viewModel.ResetInventory();
    }

    private void OnDisplayButtonClicked() 
    {
        Debug.Log("显示设定 - 此处可以加入控制物品模型隐藏/显示的逻辑");
        // 如果有额外的面板，可以在此处 Toggle
    }

    private void OnCategoryTabClicked(int categoryIndex)
    {
        CancelDeleteMode();
        if (viewModel != null)
            viewModel.ChangeCategory(categoryIndex);
    }

    private void OnSortByRarityClicked()
    {
        CancelDeleteMode();
        if (viewModel != null)
            viewModel.SetActiveSortTab(0);
    }

    private void OnSortByRecentClicked()
    {
        CancelDeleteMode();
        if (viewModel != null)
            viewModel.SetActiveSortTab(1);
    }

    private void OnUISwitchButtonClicked() 
    {
        Debug.Log("界面设置被点击 - 呼出游戏系统菜单");
    }

    private void OnDeleteItemButtonClicked()
    {
        if (!isDeleteMode)
        {
            // 第一次点击：进入删除模式
            isDeleteMode = true;
            deleteItemButton.style.backgroundColor = new StyleColor(new Color(0.8f, 0.2f, 0.2f, 1f)); // 按钮变红高亮
            itemsToDelete.Clear();
        }
        else
        {
            // 第二次点击：执行真实删除并退出删除模式
            if (viewModel != null && itemsToDelete.Count > 0)
            {
                viewModel.DeleteItems(itemsToDelete);
            }
            CancelDeleteMode();
        }
    }

    private void OnSortButtonClicked() 
    {
        CancelDeleteMode();
        if (viewModel != null)
            viewModel.SortInventory();
    }

    private void OnUseItemButtonClicked()
    {
        CancelDeleteMode();
        if (viewModel != null && bindableData.selectedSlotIndex >= 0)
        {
            viewModel.UseItem(bindableData.selectedSlotIndex);
        }
    }

    // 移除悬停相关方法，因为不再使用


    private void OnItemSlotClicked(int index)
    {
        if (isDeleteMode)
        {
            // 删除模式：负责选中与反选变红
            if (viewModel == null) return;
            var slot = viewModel.GetSlotAt(index);
            if (slot.IsEmpty) return; // 不选择空格子

            if (itemsToDelete.Contains(index))
            {
                itemsToDelete.Remove(index);
                itemSlots[index].style.unityBackgroundImageTintColor = Color.white; // 恢复原样
            }
            else
            {
                itemsToDelete.Add(index);
                itemSlots[index].style.unityBackgroundImageTintColor = new Color(1f, 0.3f, 0.3f); // 图片变红
            }
        }
        else
        {
            // 正常模式：负责正常展示物品
            if (viewModel != null)
                viewModel.SelectItem(index);
        }
    }
}
